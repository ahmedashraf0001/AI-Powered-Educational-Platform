import { useEffect, useRef, useState, useCallback } from 'react';
import { useMutation } from '@tanstack/react-query';
import { materialsApi } from '@/api/materials.api';
import { sectionsApi } from '@/api/sections.api';
import { useAuthStore } from '@/stores/authStore';
import { useMediaStream } from '@/hooks/useMediaStream';
import { Spinner } from '@/components/ui/Spinner';
import { Button } from '@/components/ui/Button';
import { Download, FileQuestion, BookOpen, Lightbulb, AlertCircle, Maximize2, Minimize2 } from 'lucide-react';

interface MaterialViewerProps {
  materialId: string;
  sessionId?: string;
  onSectionResult?: (type: string, data: any) => void;
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
  onSectionResult,
}: MaterialViewerProps) {
  const [sections, setSections] = useState<Section[]>([]);
  const [projection, setProjection] = useState<Projection | null>(null);
  const [loading, setLoading] = useState(true);
  const { blobUrl: streamUrl, loading: streamLoading, error: streamError } = useMediaStream(materialId);
  const lastTracked = useRef(0);
  const timerRef = useRef<ReturnType<typeof setInterval>>(undefined);
  const videoRef = useRef<HTMLVideoElement>(null);
  const audioRef = useRef<HTMLAudioElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const [isFullscreen, setIsFullscreen] = useState(false);

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
            lastPosition: 0,
            materialType: String(projData.materialType ?? 'PDF'),
          } : null);
          setSections((secsData as any[]) || []);
          lastTracked.current = 0;
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

  // Progress tracking for video/audio (every 30s)
  useEffect(() => {
    if (!projection || (projection.materialType !== 'Video' && projection.materialType !== 'Audio')) return;

    timerRef.current = setInterval(() => {
      const el = projection.materialType === 'Video' ? videoRef.current : audioRef.current;
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

  // Resume playback
  useEffect(() => {
    if (!projection || !streamUrl) return;
    const resume = projection.lastPosition || 0;
    if (projection.materialType === 'Video' && videoRef.current) {
      videoRef.current.currentTime = resume;
    } else if (projection.materialType === 'Audio' && audioRef.current) {
      audioRef.current.currentTime = resume;
    }
  }, [streamUrl, projection]);

  const sectionQuizMutation = useMutation({
    mutationFn: (sectionId: string) =>
      sectionsApi.generateQuiz(sessionId!, sectionId),
    onSuccess: (res) => onSectionResult?.('quiz', res.data.data),
  });

  const sectionSummarizeMutation = useMutation({
    mutationFn: (sectionId: string) =>
      sectionsApi.summarize(sessionId!, sectionId),
    onSuccess: (res) => onSectionResult?.('summary', res.data.data),
  });

  const sectionFlashcardsMutation = useMutation({
    mutationFn: (sectionId: string) =>
      sectionsApi.generateFlashcards(sessionId!, sectionId),
    onSuccess: (res) => onSectionResult?.('flashcards', res.data.data),
  });

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

  return (
    <div ref={containerRef} className={`h-full flex flex-col ${isFullscreen ? 'bg-background' : ''}`}>
      {/* Toolbar */}
      <div className="flex items-center justify-between p-2 border-b bg-secondary/30">
        <span className="text-sm font-medium">{materialType}</span>
        <div className="flex items-center gap-1">
          <Button
            variant="ghost"
            size="sm"
            onClick={handleDownload}
          >
            <Download className="h-4 w-4 mr-1" /> Download
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={toggleFullscreen}
            title={isFullscreen ? 'Exit fullscreen' : 'Fullscreen'}
          >
            {isFullscreen ? <Minimize2 className="h-4 w-4" /> : <Maximize2 className="h-4 w-4" />}
          </Button>
        </div>
      </div>

      {/* Content Area */}
      <div className={`flex-1 overflow-auto ${isFullscreen ? 'h-[calc(100vh-48px)]' : ''}`}>
        {materialType === 'Video' && streamUrl && (
          <video
            ref={videoRef}
            src={streamUrl}
            controls
            className={`w-full ${isFullscreen ? 'max-h-[calc(100vh-48px)]' : 'max-h-[60vh]'}`}
          />
        )}

        {materialType === 'Audio' && streamUrl && (
          <div className="p-8 flex items-center justify-center">
            <audio ref={audioRef} src={streamUrl} controls className="w-full max-w-lg" />
          </div>
        )}

        {materialType === 'Image' && streamUrl && (
          <img src={streamUrl} alt="Material" className="max-w-full mx-auto" />
        )}

        {(materialType === 'PDF' || materialType === 'Document') && streamUrl && (
          <object data={streamUrl} type="application/pdf" className={`w-full ${isFullscreen ? 'h-[calc(100vh-48px)]' : 'h-full min-h-[600px]'}`}>
            <div className="flex flex-col items-center justify-center h-full py-16 gap-3">
              <p className="text-muted-foreground">Unable to display PDF inline.</p>
              <Button variant="outline" size="sm" onClick={handleDownload}>
                <Download className="h-4 w-4 mr-1" /> Download to view
              </Button>
            </div>
          </object>
        )}
      </div>

      {/* Sections */}
      {sections.length > 0 && (
        <div className="border-t max-h-60 overflow-y-auto">
          <h4 className="text-sm font-medium p-2 bg-secondary/30">Sections</h4>
          {sections.map((section) => (
            <div key={section.id} className="p-3 border-b flex items-start gap-3">
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium truncate">{section.title}</p>
                <p className="text-xs text-muted-foreground line-clamp-2">
                  {section.summary}
                </p>
              </div>
              {sessionId && (
              <div className="flex gap-1">
                <Button
                  variant="ghost"
                  size="sm"
                  title="Make Quiz"
                  onClick={() => sectionQuizMutation.mutate(section.id)}
                  loading={
                    sectionQuizMutation.isPending &&
                    sectionQuizMutation.variables === section.id
                  }
                >
                  <FileQuestion className="h-3 w-3" />
                </Button>
                <Button
                  variant="ghost"
                  size="sm"
                  title="Summarize"
                  onClick={() => sectionSummarizeMutation.mutate(section.id)}
                  loading={
                    sectionSummarizeMutation.isPending &&
                    sectionSummarizeMutation.variables === section.id
                  }
                >
                  <BookOpen className="h-3 w-3" />
                </Button>
                <Button
                  variant="ghost"
                  size="sm"
                  title="Flashcards"
                  onClick={() => sectionFlashcardsMutation.mutate(section.id)}
                  loading={
                    sectionFlashcardsMutation.isPending &&
                    sectionFlashcardsMutation.variables === section.id
                  }
                >
                  <Lightbulb className="h-3 w-3" />
                </Button>
              </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
