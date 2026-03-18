import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { lecturesApi } from '@/api/lectures.api';
import { studySessionsApi } from '@/api/studySessions.api';
import { StudioChat } from '@/components/study/StudioChat';
import { FlashcardsView } from '@/components/study/FlashcardsView';
import { MindMapView } from '@/components/study/MindMapView';
import { QuizView } from '@/components/study/QuizView';
import { SummaryView } from '@/components/study/SummaryView';
import { DialogueAudioView } from '@/components/study/DialogueAudioView';
import { MaterialViewer } from '@/components/viewer/MaterialViewer';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { PageSpinner } from '@/components/ui/Spinner';
import { cn } from '@/utils/cn';
import { useState, useMemo, useCallback } from 'react';
import { toast } from 'sonner';
import type { MaterialInfo } from '@/components/study/SourceReference';
import {
  MessageSquare,
  Lightbulb,
  GitBranch,
  FileQuestion,
  FileText,
  Mic,
  X,
  ChevronRight,
  ChevronLeft,
  BookOpen,
  PanelLeftClose,
  PanelLeftOpen,
  FileVideo,
  FileAudio,
  Image as ImageIcon,
  File,
} from 'lucide-react';

type StudioTab = 'chat' | 'flashcards' | 'mindmap' | 'quiz' | 'summary' | 'dialogue';

function getMaterialIcon(type?: string) {
  switch (type?.toLowerCase()) {
    case 'video': return <FileVideo className="h-3.5 w-3.5 text-blue-500" />;
    case 'audio': return <FileAudio className="h-3.5 w-3.5 text-purple-500" />;
    case 'image': return <ImageIcon className="h-3.5 w-3.5 text-green-500" />;
    default: return <File className="h-3.5 w-3.5 text-orange-500" />;
  }
}

export default function StudioPage() {
  const { courseId, sessionId } = useParams<{ courseId: string; sessionId: string }>();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState<StudioTab>('chat');
  const [selectedMaterialId, setSelectedMaterialId] = useState<string | null>(null);
  const [selectedLectureIds, setSelectedLectureIds] = useState<string[]>([]);
  const [selectedMaterialIds, setSelectedMaterialIds] = useState<string[]>([]);
  const [showEndConfirm, setShowEndConfirm] = useState(false);
  const [showMaterialsPanel, setShowMaterialsPanel] = useState(true);

  const { data: lectures, isLoading } = useQuery({
    queryKey: ['course-lectures-materials', courseId],
    queryFn: () => lecturesApi.getCourseLectures(courseId!),
    enabled: !!courseId,
    select: (res) => res.data.data,
  });

  const endSessionMutation = useMutation({
    mutationFn: () => studySessionsApi.end(sessionId!),
    onSuccess: () => {
      toast.success('Session ended');
      navigate(`/courses/${courseId}/learn`);
    },
    onError: () => toast.error('Failed to end session'),
  });

  // Build flat list of all materials for source reference linking
  const allMaterials: MaterialInfo[] = useMemo(() => {
    if (!lectures) return [];
    return lectures.flatMap((lecture: any) =>
      (lecture.materials || []).map((mat: any) => ({
        id: mat.id,
        title: mat.title,
        materialType: mat.materialType,
      }))
    );
  }, [lectures]);

  // Handler to open a material at a specific page or timestamp
  const handleOpenMaterialRef = useCallback((materialId: string, page?: number, timestamp?: number) => {
    setSelectedMaterialId(materialId);
    setShowMaterialsPanel(true);
    if (!selectedMaterialIds.includes(materialId)) {
      setSelectedMaterialIds((prev) => [...prev, materialId]);
    }
    // For PDF page navigation, we use a URL hash approach via the PDF viewer
    // The page/timestamp will be handled by the MaterialViewer via a query param or ref
    if (page) {
      // PDF viewers support #page=N in the URL
      setTimeout(() => {
        const pdfObj = document.querySelector('object[data]') as HTMLObjectElement;
        if (pdfObj && pdfObj.data) {
          const baseUrl = pdfObj.data.split('#')[0];
          pdfObj.data = `${baseUrl}#page=${page}`;
        }
      }, 500);
    }
    if (timestamp) {
      setTimeout(() => {
        const video = document.querySelector('video') as HTMLVideoElement;
        const audio = document.querySelector('audio') as HTMLAudioElement;
        const media = video || audio;
        if (media) {
          media.currentTime = timestamp;
          media.play().catch(() => {});
        }
      }, 500);
    }
  }, [selectedMaterialIds]);

  if (isLoading || !sessionId) return <PageSpinner />;

  const tabs: { key: StudioTab; label: string; icon: React.ReactNode }[] = [
    { key: 'chat', label: 'Chat', icon: <MessageSquare className="h-4 w-4" /> },
    { key: 'flashcards', label: 'Flashcards', icon: <Lightbulb className="h-4 w-4" /> },
    { key: 'mindmap', label: 'Mind Map', icon: <GitBranch className="h-4 w-4" /> },
    { key: 'quiz', label: 'Quiz', icon: <FileQuestion className="h-4 w-4" /> },
    { key: 'summary', label: 'Summary', icon: <FileText className="h-4 w-4" /> },
    { key: 'dialogue', label: 'Dialogue', icon: <Mic className="h-4 w-4" /> },
  ];

  const toggleLecture = (lectureId: string) => {
    setSelectedLectureIds((prev) =>
      prev.includes(lectureId)
        ? prev.filter((id) => id !== lectureId)
        : [...prev, lectureId]
    );
  };

  const toggleMaterial = (materialId: string) => {
    setSelectedMaterialIds((prev) =>
      prev.includes(materialId)
        ? prev.filter((id) => id !== materialId)
        : [...prev, materialId]
    );
  };

  return (
    <AnimatedPage>
    <div className="h-[calc(100vh-3.5rem)] flex">
      {/* Left Panel: Materials & References */}
      {showMaterialsPanel && (
        <div className="flex flex-col border-r bg-card/50 min-w-0" style={{ width: selectedMaterialId ? '50%' : '280px', maxWidth: selectedMaterialId ? '60%' : '320px' }}>
          {/* Materials panel header */}
          <div className="flex items-center justify-between px-3 py-2.5 border-b bg-secondary/30">
            <div className="flex items-center gap-2">
              <BookOpen className="h-4 w-4 text-primary" />
              <span className="text-sm font-semibold">Materials</span>
            </div>
            <div className="flex items-center gap-1">
              {selectedMaterialId && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setSelectedMaterialId(null)}
                  title="Close viewer"
                >
                  <X className="h-3.5 w-3.5" />
                </Button>
              )}
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setShowMaterialsPanel(false)}
                title="Hide materials panel"
              >
                <PanelLeftClose className="h-4 w-4" />
              </Button>
            </div>
          </div>

          {selectedMaterialId ? (
            /* Material Viewer */
            <div className="flex-1 flex flex-col min-h-0">
              <div className="flex-1 overflow-auto">
                <MaterialViewer
                  materialId={selectedMaterialId}
                  sessionId={sessionId}
                />
              </div>
              {/* Compact material list below viewer */}
              <div className="border-t max-h-48 overflow-y-auto bg-secondary/10">
                <div className="px-3 py-2 text-xs font-semibold text-muted-foreground uppercase tracking-wider border-b">
                  References
                </div>
                {lectures?.map((lecture: any) => (
                  <div key={lecture.id}>
                    <label className="flex items-center gap-2 px-3 py-1.5 hover:bg-secondary/50 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={selectedLectureIds.includes(lecture.id)}
                        onChange={() => toggleLecture(lecture.id)}
                        className="accent-primary h-3.5 w-3.5"
                      />
                      <span className="text-xs font-medium truncate">{lecture.title}</span>
                    </label>
                    {lecture.materials?.map((mat: any) => (
                      <div key={mat.id} className="flex items-center gap-1.5 pl-7 pr-3 py-1">
                        <input
                          type="checkbox"
                          checked={selectedMaterialIds.includes(mat.id)}
                          onChange={() => toggleMaterial(mat.id)}
                          className="accent-primary h-3 w-3"
                        />
                        {getMaterialIcon(mat.materialType)}
                        <button
                          className={cn(
                            'text-xs truncate hover:underline',
                            selectedMaterialId === mat.id ? 'text-primary font-semibold' : 'text-muted-foreground hover:text-foreground'
                          )}
                          onClick={() => {
                            setSelectedMaterialId(mat.id);
                            if (!selectedMaterialIds.includes(mat.id)) toggleMaterial(mat.id);
                          }}
                        >
                          {mat.title}
                        </button>
                      </div>
                    ))}
                  </div>
                ))}
              </div>
            </div>
          ) : (
            /* Full material list */
            <div className="flex-1 overflow-y-auto">
              {lectures?.map((lecture: any) => (
                <div key={lecture.id} className="border-b last:border-b-0">
                  <label className="flex items-center gap-2.5 px-3 py-2.5 hover:bg-secondary/50 cursor-pointer transition-colors">
                    <input
                      type="checkbox"
                      checked={selectedLectureIds.includes(lecture.id)}
                      onChange={() => toggleLecture(lecture.id)}
                      className="accent-primary h-4 w-4"
                    />
                    <span className="text-sm font-semibold truncate">{lecture.title}</span>
                  </label>
                  {lecture.materials?.map((mat: any) => (
                    <div key={mat.id} className="flex items-center gap-2 pl-8 pr-3 py-2 hover:bg-secondary/30 transition-colors">
                      <input
                        type="checkbox"
                        checked={selectedMaterialIds.includes(mat.id)}
                        onChange={() => toggleMaterial(mat.id)}
                        className="accent-primary h-3.5 w-3.5"
                      />
                      {getMaterialIcon(mat.materialType)}
                      <button
                        className="text-sm text-primary hover:underline truncate text-left flex-1"
                        onClick={() => {
                          setSelectedMaterialId(mat.id);
                          if (!selectedMaterialIds.includes(mat.id)) toggleMaterial(mat.id);
                        }}
                      >
                        {mat.title}
                      </button>
                    </div>
                  ))}
                </div>
              ))}
              {(!lectures || lectures.length === 0) && (
                <div className="flex flex-col items-center justify-center py-12 text-muted-foreground">
                  <BookOpen className="h-8 w-8 opacity-30 mb-2" />
                  <p className="text-sm">No materials found</p>
                </div>
              )}
            </div>
          )}
        </div>
      )}

      {/* Right: Studio Panel */}
      <div className="flex-1 flex flex-col min-w-0">
        {/* Tab bar */}
        <div className="flex items-center border-b bg-card/80 backdrop-blur-sm">
          {/* Toggle materials panel button */}
          {!showMaterialsPanel && (
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setShowMaterialsPanel(true)}
              className="ml-1"
              title="Show materials panel"
            >
              <PanelLeftOpen className="h-4 w-4" />
            </Button>
          )}

          <div className="flex items-center overflow-x-auto">
            {tabs.map((tab) => (
              <button
                key={tab.key}
                onClick={() => setActiveTab(tab.key)}
                className={cn(
                  'flex items-center gap-1.5 px-4 py-3 text-sm whitespace-nowrap border-b-2 transition-all duration-200',
                  activeTab === tab.key
                    ? 'border-primary text-primary font-semibold bg-primary/5'
                    : 'border-transparent text-muted-foreground hover:text-foreground hover:bg-secondary/50'
                )}
              >
                {tab.icon}
                <span className="hidden sm:inline">{tab.label}</span>
              </button>
            ))}
          </div>

          <div className="ml-auto px-3 flex items-center">
            <Button
              variant="destructive"
              size="sm"
              onClick={() => setShowEndConfirm(true)}
            >
              End Session
            </Button>
          </div>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-auto min-h-0">
          {activeTab === 'chat' && (
            <StudioChat
              sessionId={sessionId}
              lectureIds={selectedLectureIds}
              materialIds={selectedMaterialIds}
              materials={allMaterials}
              onOpenMaterial={handleOpenMaterialRef}
            />
          )}
          {activeTab === 'flashcards' && (
            <FlashcardsView
              sessionId={sessionId}
              lectureIds={selectedLectureIds}
              materialIds={selectedMaterialIds}
            />
          )}
          {activeTab === 'mindmap' && (
            <MindMapView
              sessionId={sessionId}
              lectureIds={selectedLectureIds}
              materialIds={selectedMaterialIds}
            />
          )}
          {activeTab === 'quiz' && (
            <QuizView
              sessionId={sessionId}
              lectureIds={selectedLectureIds}
              materialIds={selectedMaterialIds}
              materials={allMaterials}
              onOpenMaterial={handleOpenMaterialRef}
            />
          )}
          {activeTab === 'summary' && (
            <SummaryView
              sessionId={sessionId}
              lectureIds={selectedLectureIds}
              materialIds={selectedMaterialIds}
              materials={allMaterials}
              onOpenMaterial={handleOpenMaterialRef}
            />
          )}
          {activeTab === 'dialogue' && (
            <DialogueAudioView
              sessionId={sessionId}
              lectureIds={selectedLectureIds}
              materialIds={selectedMaterialIds}
            />
          )}
        </div>
      </div>

      <Modal
        open={showEndConfirm}
        onClose={() => setShowEndConfirm(false)}
        title="End Study Session"
      >
        <p className="mb-4">Are you sure you want to end this study session?</p>
        <div className="flex gap-2 justify-end">
          <Button variant="outline" onClick={() => setShowEndConfirm(false)}>Cancel</Button>
          <Button
            variant="destructive"
            onClick={() => endSessionMutation.mutate()}
            loading={endSessionMutation.isPending}
          >
            End Session
          </Button>
        </div>
      </Modal>
    </div>
    </AnimatedPage>
  );
}
