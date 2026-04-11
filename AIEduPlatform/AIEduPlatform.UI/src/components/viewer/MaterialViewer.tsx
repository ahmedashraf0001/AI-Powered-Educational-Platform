import { useEffect, useRef, useState, useCallback } from 'react';
import { useMutation } from '@tanstack/react-query';
import { materialsApi } from '@/api/materials.api';
import { sectionsApi } from '@/api/sections.api';
import { useAuthStore } from '@/stores/authStore';
import { useMediaStream } from '@/hooks/useMediaStream';
import { Spinner } from '@/components/ui/Spinner';
import { Button } from '@/components/ui/Button';
import { PdfViewer } from './PdfViewer';
import { VideoPlayer } from './VideoPlayer';
import { Download, AlertCircle, Maximize2, Minimize2 } from 'lucide-react';

interface MaterialViewerProps {
  materialId: string;
  sessionId?: string;
  initialPage?: number;
  initialTimestamp?: number;
  scrollTrigger?: number;
  onSectionResult?: (type: string, data: any) => void;
  onSectionSummarize?: (sectionTitle: string) => void;
}

interface Section {
  id: string;
  title: string;
  summary: string;
  startSeconds: number | null;
  endSeconds: number | null;
  startPage: number | null;
  endPage: number | null;
  orderIndex: number;
}

interface Projection {
  lastPosition: number;
  materialType: string;
}

export function MaterialViewer({
  materialId,
  sessionId,
  initialPage,
  initialTimestamp,
  scrollTrigger,
  onSectionResult,
  onSectionSummarize,
}: MaterialViewerProps) {
  const [sections, setSections] = useState<Section[]>([]);
  const [projection, setProjection] = useState<Projection | null>(null);
  const [loading, setLoading] = useState(true);
  const { blobUrl: streamUrl, loading: streamLoading, error: streamError } = useMediaStream(materialId);
  const lastTracked = useRef(0);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const audioRef = useRef<HTMLAudioElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [loadingSection, setLoadingSection] = useState<{ type: string; sectionId: string } | null>(null);

  const toggleFullscreen = useCallback(async () => {
    if (!containerRef.current) return;
    if (!document.fullscreenElement) {
      await containerRef.current.requestFullscreen();
    } else {
      await document.exitFullscreen();
    }
  }, []);

  useEffect(() => {
    const onFsChange = () => setIsFullscreen(!!document.fullscreenElement);
    document.addEventListener('fullscreenchange', onFsChange);
    return () => document.removeEventListener('fullscreenchange', onFsChange);
  }, []);

  // Load projection and sections
  useEffect(() => {
    let cancelled = false;

    const loadMeta = async () => {
      try {
        const [projRes, secsRes] = await Promise.all([
          materialsApi.getProjection(materialId),
          sectionsApi.getByMaterial(materialId),
        ]);
        if (!cancelled) {
          const projData = projRes.data.data;
          const secsData = secsRes.data.data;
          setProjection(projData ? {
            lastPosition: projData.progress?.current ?? 0,
            materialType: String(projData.materialType ?? 'PDF'),
          } : null);
          setSections((secsData as any[]) || []);
          lastTracked.current = projData?.progress?.current ?? 0;
        }
      } catch {
        // ignore
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    loadMeta();
    return () => { cancelled = true; };
  }, [materialId]);

  // Progress tracking for audio (every 30s)
  useEffect(() => {
    if (!projection || projection.materialType !== 'Audio') return;

    timerRef.current = setInterval(() => {
      const el = audioRef.current;
      if (!el) return;
      const currentTime = Math.floor(el.currentTime);
      if (currentTime > lastTracked.current) {
        lastTracked.current = currentTime;
        materialsApi.updateProgress(materialId, currentTime).catch(() => {});
      }
    }, 30000);

    return () => {
      if (timerRef.current) clearInterval(timerRef.current);
    };
  }, [projection, materialId]);

  // Handle audio initial timestamp and scroll triggers
  useEffect(() => {
    if (audioRef.current && initialTimestamp !== undefined && initialTimestamp > 0) {
      audioRef.current.currentTime = initialTimestamp;
    }
  }, [initialTimestamp, scrollTrigger, streamUrl]);

  // Section mutations (for quiz and flashcards)
  const sectionQuizMutation = useMutation({
    mutationFn: (sectionId: string) => {
      setLoadingSection({ type: 'quiz', sectionId });
      return sectionsApi.generateQuiz(sessionId!, sectionId);
    },
    onSuccess: (res) => {
      setLoadingSection(null);
      onSectionResult?.('quiz', res.data.data);
    },
    onError: () => setLoadingSection(null),
  });

  const sectionFlashcardsMutation = useMutation({
    mutationFn: (sectionId: string) => {
      setLoadingSection({ type: 'flashcards', sectionId });
      return sectionsApi.generateFlashcards(sessionId!, sectionId);
    },
    onSuccess: (res) => {
      setLoadingSection(null);
      onSectionResult?.('flashcards', res.data.data);
    },
    onError: () => setLoadingSection(null),
  });

  // Handle section action - summary goes through chat, quiz/flashcards use API
  const handleSectionAction = useCallback((type: 'quiz' | 'summary' | 'flashcards', sectionId: string) => {
    if (!sessionId) return;

    // Find the section to get its title
    const section = sections.find(s => s.id === sectionId);

    if (type === 'summary') {
      // Use chat-based summarization for streaming response
      if (section && onSectionSummarize) {
        onSectionSummarize(section.title);
      }
    } else if (type === 'quiz') {
      sectionQuizMutation.mutate(sectionId);
    } else if (type === 'flashcards') {
      sectionFlashcardsMutation.mutate(sectionId);
    }
  }, [sessionId, sections, onSectionSummarize, sectionQuizMutation, sectionFlashcardsMutation]);

  // Handle video section summarize (from progress bar hover)
  const handleVideoSectionSummarize = useCallback((sectionId: string) => {
    const section = sections.find(s => s.id === sectionId);
    if (section && onSectionSummarize) {
      onSectionSummarize(section.title);
    }
  }, [sections, onSectionSummarize]);

  const handleVideoTimeUpdate = useCallback((time: number) => {
    const currentTime = Math.floor(time);
    if (currentTime > lastTracked.current + 30) {
      lastTracked.current = currentTime;
      materialsApi.updateProgress(materialId, currentTime).catch(() => {});
    }
  }, [materialId]);

  const handlePdfPageChange = useCallback((pageNumber: number) => {
    // Only update progress if we move to a new page
    if (pageNumber > lastTracked.current) {
      lastTracked.current = pageNumber;
      materialsApi.updateProgress(materialId, pageNumber).catch(() => {});
    }
  }, [materialId]);

  const handleDownload = async () => {
    const url = materialsApi.getDownloadUrl(materialId);
    const token = useAuthStore.getState().accessToken;
    const res = await fetch(url, {
      headers: token ? { Authorization: `Bearer ${token}` } : {},
    });
    const blob = await res.blob();
    const blobUrl = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = blobUrl;
    a.download = '';
    a.click();
    URL.revokeObjectURL(blobUrl);
  };

  if (loading || streamLoading) {
    return (
      <div className="flex items-center justify-center h-full py-16">
        <Spinner />
        <span className="ml-2 text-muted-foreground">Loading material...</span>
      </div>
    );
  }

  if (streamError) {
    return (
      <div className="flex flex-col items-center justify-center h-full py-16 gap-3">
        <AlertCircle className="h-8 w-8 text-destructive" />
        <p className="text-muted-foreground">{streamError}</p>
      </div>
    );
  }

  const materialType = projection?.materialType || 'PDF';

  return (
    <div ref={containerRef} className={`h-full flex flex-col ${isFullscreen ? 'bg-background' : ''}`}>
      {/* Video */}
      {materialType === 'Video' && streamUrl && (
        <VideoPlayer
          url={streamUrl}
          sections={sections}
          initialTime={initialTimestamp ?? projection?.lastPosition ?? 0}
          scrollTrigger={scrollTrigger}
          onTimeUpdate={handleVideoTimeUpdate}
          onSectionSummarize={sessionId && onSectionSummarize ? handleVideoSectionSummarize : undefined}
          isFullscreen={isFullscreen}
          onToggleFullscreen={toggleFullscreen}
        />
      )}

      {/* Audio */}
      {materialType === 'Audio' && streamUrl && (
        <div className="flex flex-col h-full">
          <div className="flex items-center justify-between p-2 border-b bg-secondary/30">
            <span className="text-sm font-medium">Audio</span>
            <div className="flex items-center gap-1">
              <Button variant="ghost" size="sm" onClick={handleDownload}>
                <Download className="h-4 w-4 mr-1" /> Download
              </Button>
              <Button variant="ghost" size="sm" onClick={toggleFullscreen}>
                {isFullscreen ? <Minimize2 className="h-4 w-4" /> : <Maximize2 className="h-4 w-4" />}
              </Button>
            </div>
          </div>
          <div className="flex-1 flex items-center justify-center p-8">
            <audio ref={audioRef} src={streamUrl} controls className="w-full max-w-lg" />
          </div>
        </div>
      )}

      {/* Image */}
      {materialType === 'Image' && streamUrl && (
        <div className="flex flex-col h-full">
          <div className="flex items-center justify-between p-2 border-b bg-secondary/30">
            <span className="text-sm font-medium">Image</span>
            <div className="flex items-center gap-1">
              <Button variant="ghost" size="sm" onClick={handleDownload}>
                <Download className="h-4 w-4 mr-1" /> Download
              </Button>
              <Button variant="ghost" size="sm" onClick={toggleFullscreen}>
                {isFullscreen ? <Minimize2 className="h-4 w-4" /> : <Maximize2 className="h-4 w-4" />}
              </Button>
            </div>
          </div>
          <div className="flex-1 overflow-auto p-4">
            <img src={streamUrl} alt="Material" className="max-w-full mx-auto" />
          </div>
        </div>
      )}

      {/* PDF / Document */}
      {(materialType === 'PDF' || materialType === 'Document') && streamUrl && (
        <PdfViewer
          url={streamUrl}
          sections={sections}
          initialPage={initialPage ?? (projection?.lastPosition || 1)}
            scrollTrigger={scrollTrigger}
            onDownload={handleDownload}
            onSectionAction={sessionId ? handleSectionAction : undefined}
            isFullscreen={isFullscreen}
            onToggleFullscreen={toggleFullscreen}
            loadingSection={loadingSection}
            onPageChange={handlePdfPageChange}
        />
      )}
    </div>
  );
}

