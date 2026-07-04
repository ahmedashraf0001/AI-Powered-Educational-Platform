import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { lecturesApi } from '@/api/lectures.api';
import { studySessionsApi } from '@/api/studySessions.api';
import { StudioChat } from '@/components/study/StudioChat';
import { FlashcardsView } from '@/components/study/FlashcardsView';
import { MindMapView } from '@/components/study/MindMapView';
import { QuizView } from '@/components/study/QuizView';
import { DialogueAudioView } from '@/components/study/DialogueAudioView';
import { MaterialViewer } from '@/components/viewer/MaterialViewer';
import { AiProviderSettingsModal } from '@/components/settings/AiProviderSettingsModal';
import { VoiceSettingsModal } from '@/components/settings/VoiceSettingsModal';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { PageSpinner } from '@/components/ui/Spinner';
import { cn } from '@/utils/cn';
import { useState, useMemo, useCallback, useRef, useEffect } from 'react';
import { toast } from 'sonner';
import type { MaterialInfo } from '@/components/study/SourceReference';
import type { StudioChatRef } from '@/components/study/StudioChat';
import { Panel, Group as PanelGroup, Separator as PanelResizeHandle } from 'react-resizable-panels';
import {
  MessageSquare,
  Lightbulb,
  GitBranch,
  FileQuestion,
  Mic,
  X,
  BookOpen,
  PanelLeftOpen,
  FileVideo,
  FileAudio,
  Image as ImageIcon,
  File,
  GripVertical,
  ChevronDown,
  ChevronRight,
} from 'lucide-react';

type StudioTab = 'chat' | 'flashcards' | 'mindmap' | 'quiz' | 'dialogue';

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
  const [searchParams] = useSearchParams();
  const initialMaterialId = searchParams.get('materialId');
  
  const [activeTab, setActiveTab] = useState<StudioTab>('chat');
  const [selectedMaterialId, setSelectedMaterialId] = useState<string | null>(initialMaterialId);
  const [materialPage, setMaterialPage] = useState<number | undefined>(undefined);
  const [materialTimestamp, setMaterialTimestamp] = useState<number | undefined>(undefined);
  const [scrollTrigger, setScrollTrigger] = useState<number>(0);
  const [selectedLectureIds, setSelectedLectureIds] = useState<string[]>([]);
  const [expandedLectureIds, setExpandedLectureIds] = useState<string[]>([]);
  const [selectedMaterialIds, setSelectedMaterialIds] = useState<string[]>(initialMaterialId ? [initialMaterialId] : []);
  const [showEndConfirm, setShowEndConfirm] = useState(false);
  const [showMaterialsPanel, setShowMaterialsPanel] = useState(true);
  const [pendingChatMessage, setPendingChatMessage] = useState<string | null>(null);
  const [pendingSectionId, setPendingSectionId] = useState<string | null>(null);
  const [pendingFlashcards, setPendingFlashcards] = useState<{timestamp: number, data: any} | null>(null);
  const [pendingQuiz, setPendingQuiz] = useState<{timestamp: number, data: any} | null>(null);
  const [showAiProviderSettings, setShowAiProviderSettings] = useState(false);
  const [showVoiceSettings, setShowVoiceSettings] = useState(false);
  const chatRef = useRef<StudioChatRef>(null);

  const queryClient = useQueryClient();

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
    setMaterialPage(page);
    setMaterialTimestamp(timestamp);
    setScrollTrigger((prev) => prev + 1);

    if (!selectedMaterialIds.includes(materialId)) {
      setSelectedMaterialIds((prev) => [...prev, materialId]);
    }
  }, [selectedMaterialIds]);

  // Handle sending pending chat message when chat tab is active
  useEffect(() => {
    if (activeTab === 'chat' && pendingChatMessage && chatRef.current) {
      const message = pendingChatMessage;
      const sectionId = pendingSectionId ?? undefined;
      setPendingChatMessage(null);
      setPendingSectionId(null);
      // Small delay to ensure component is fully mounted
      setTimeout(() => {
        chatRef.current?.sendMessage(message, { sectionId });
      }, 100);
    }
  }, [activeTab, pendingChatMessage, pendingSectionId]);

  // Handle section summarize - send to chat instead of using section API
  const handleSectionSummarize = useCallback((sectionId: string, sectionTitle: string, materialTitle?: string) => {
    const message = materialTitle
      ? `Summarize the section "${sectionTitle}" from "${materialTitle}". Please provide a detailed summary with key points.`
      : `Summarize the section "${sectionTitle}". Please provide a detailed summary with key points.`;

    setPendingChatMessage(message);
    setPendingSectionId(sectionId);
    setActiveTab('chat');
  }, []);

  // Handle section results from MaterialViewer (for quiz and flashcards)
  const handleSectionResult = useCallback((type: string, data: any) => {
    if (type === 'quiz') {
      setPendingQuiz({ timestamp: Date.now(), data });
      setActiveTab('quiz');
      toast.success('Quiz generated!');
      queryClient.invalidateQueries({ queryKey: ['quizzes-history', sessionId] });
    } else if (type === 'flashcards') {
      setPendingFlashcards({ timestamp: Date.now(), data });
      setActiveTab('flashcards');
      toast.success('Flashcards generated!');
      queryClient.invalidateQueries({ queryKey: ['flashcards-history', sessionId] });
    }
  }, [sessionId, queryClient]);

  // Default to first material on load
  useEffect(() => {
    if (allMaterials.length > 0 && !selectedMaterialId) {
      setSelectedMaterialId(allMaterials[0].id);
      setSelectedMaterialIds((prev) => 
        prev.includes(allMaterials[0].id) ? prev : [...prev, allMaterials[0].id]
      );
    }
  }, [allMaterials, selectedMaterialId]);

  // Expand (accordion) + auto-include-in-context the lecture containing the selected material
  useEffect(() => {
    if (lectures && selectedMaterialId) {
      const lectureWithMaterial = lectures.find((l: any) => 
        (l.materials || []).some((m: any) => m.id === selectedMaterialId)
      );
      if (lectureWithMaterial) {
        if (!selectedLectureIds.includes(lectureWithMaterial.id)) {
          setSelectedLectureIds(prev => [...prev, lectureWithMaterial.id]);
        }
        if (!expandedLectureIds.includes(lectureWithMaterial.id)) {
          setExpandedLectureIds(prev => [...prev, lectureWithMaterial.id]);
        }
      }
    }
  }, [lectures, selectedMaterialId, selectedLectureIds, expandedLectureIds]);

  if (isLoading || !sessionId) return <PageSpinner />;
  // Find material title for section summarize
  const currentMaterial = allMaterials.find(m => m.id === selectedMaterialId);

  const tabs: { key: StudioTab; label: string; icon: React.ReactNode }[] = [
    { key: 'chat', label: 'Chat', icon: <MessageSquare className="h-4 w-4" /> },
    { key: 'flashcards', label: 'Flashcards', icon: <Lightbulb className="h-4 w-4" /> },
    { key: 'mindmap', label: 'Mind Map', icon: <GitBranch className="h-4 w-4" /> },
    { key: 'quiz', label: 'Quiz', icon: <FileQuestion className="h-4 w-4" /> },
    { key: 'dialogue', label: 'Dialogue', icon: <Mic className="h-4 w-4" /> },
  ];

  const toggleLecture = (lectureId: string) => {
    setSelectedLectureIds((prev) =>
      prev.includes(lectureId)
        ? prev.filter((id) => id !== lectureId)
        : [...prev, lectureId]
    );
  };

  const toggleExpanded = (lectureId: string) => {
    setExpandedLectureIds((prev) =>
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
    <div className="h-[calc(100vh-3.5rem)] bg-background relative">

      {/* Backdrop for the materials drawer */}
      <div
        className={cn(
          'fixed inset-0 bg-black/30 z-30 transition-opacity duration-300',
          showMaterialsPanel ? 'opacity-100 pointer-events-auto' : 'opacity-0 pointer-events-none'
        )}
        onClick={() => setShowMaterialsPanel(false)}
      />

      {/* Materials drawer: slides over content, does not affect panel sizing */}
      <div
        className={cn(
          'fixed left-0 top-14 bottom-0 w-[320px] sm:w-[380px] bg-card border-r border-border shadow-2xl z-40 flex flex-col transition-transform duration-300 ease-out',
          showMaterialsPanel ? 'translate-x-0' : '-translate-x-full'
        )}
      >
        <div className="flex items-center justify-between px-4 py-3.5 border-b bg-linear-to-r from-primary/5 to-transparent shrink-0">
          <div className="flex items-center gap-2.5 min-w-0">
            <div className="p-1.5 rounded-lg bg-primary/10 shrink-0">
              <BookOpen className="h-4 w-4 text-primary" />
            </div>
            <div className="min-w-0">
              <div className="text-sm font-semibold leading-none truncate">Course Materials</div>
              <div className="text-[11px] text-muted-foreground mt-1">
                {selectedMaterialIds.length} selected for context
              </div>
            </div>
          </div>
          <Button
            variant="ghost"
            size="icon"
            className="h-7 w-7 shrink-0"
            onClick={() => setShowMaterialsPanel(false)}
            title="Close"
          >
            <X className="h-4 w-4" />
          </Button>
        </div>

        <div className="flex-1 overflow-y-auto bg-secondary/5">
          <div className="px-4 py-2 text-[11px] font-semibold text-muted-foreground uppercase tracking-wider sticky top-0 bg-card/95 backdrop-blur-sm border-b z-10">
            Select materials to reference
          </div>
          {lectures?.map((lecture: any) => {
            const isExpanded = expandedLectureIds.includes(lecture.id);
            const materials = lecture.materials || [];
            const selectedCount = materials.filter((m: any) => selectedMaterialIds.includes(m.id)).length;
            return (
              <div key={lecture.id} className="border-b border-border/40 last:border-b-0">
                <div className="flex items-center gap-2.5 px-3 py-2.5 hover:bg-secondary/40 transition-colors group">
                  <input
                    type="checkbox"
                    checked={selectedLectureIds.includes(lecture.id)}
                    onChange={() => toggleLecture(lecture.id)}
                    className="accent-primary h-4 w-4 rounded shrink-0 cursor-pointer"
                  />
                  <button
                    className="flex-1 min-w-0 flex items-center justify-between gap-2 text-left"
                    onClick={() => toggleExpanded(lecture.id)}
                  >
                    <div className="min-w-0">
                      <div className="text-sm font-semibold truncate group-hover:text-primary transition-colors">
                        {lecture.title}
                      </div>
                      <div className="text-[11px] text-muted-foreground">
                        {selectedCount > 0 ? `${selectedCount}/${materials.length} selected` : `${materials.length} materials`}
                      </div>
                    </div>
                    {isExpanded ? (
                      <ChevronDown className="h-4 w-4 text-muted-foreground shrink-0" />
                    ) : (
                      <ChevronRight className="h-4 w-4 text-muted-foreground shrink-0" />
                    )}
                  </button>
                </div>
                {isExpanded && (
                  <div className="pb-1.5">
                    {materials.map((mat: any) => {
                      const isActive = selectedMaterialId === mat.id;
                      return (
                        <div
                          key={mat.id}
                          className={cn(
                            'flex items-center gap-2.5 pl-8 pr-3 py-1.5 mx-2 my-0.5 rounded-lg transition-colors',
                            isActive ? 'bg-primary/10' : 'hover:bg-secondary/30'
                          )}
                        >
                          <input
                            type="checkbox"
                            checked={selectedMaterialIds.includes(mat.id)}
                            onChange={() => toggleMaterial(mat.id)}
                            className="accent-primary h-3.5 w-3.5 rounded shrink-0 cursor-pointer"
                          />
                          <span className="shrink-0">{getMaterialIcon(mat.materialType)}</span>
                          <button
                            className={cn(
                              'text-sm truncate text-left flex-1 transition-colors',
                              isActive ? 'text-primary font-semibold' : 'text-foreground/80 hover:text-primary'
                            )}
                            onClick={() => {
                              setSelectedMaterialId(mat.id);
                              setMaterialPage(undefined);
                              setMaterialTimestamp(undefined);
                              if (!selectedMaterialIds.includes(mat.id)) toggleMaterial(mat.id);
                            }}
                          >
                            {mat.title}
                          </button>
                          {isActive && <span className="h-1.5 w-1.5 rounded-full bg-primary shrink-0" />}
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            );
          })}
          {(!lectures || lectures.length === 0) && (
            <div className="flex flex-col items-center justify-center py-16 text-muted-foreground">
              <div className="p-4 rounded-2xl bg-secondary/30 mb-4">
                <BookOpen className="h-10 w-10 opacity-30" />
              </div>
              <p className="text-sm font-medium">No materials found</p>
              <p className="text-xs text-muted-foreground/70">Add materials to your course lectures</p>
            </div>
          )}
        </div>
      </div>

      <PanelGroup orientation="horizontal">
        {/* Center Panel: Material Viewer */}
        {selectedMaterialId && (
          <>
            <Panel defaultSize={72} minSize={30} className="flex flex-col min-w-0 bg-card">
              <div className="flex items-center justify-between px-3 py-2 border-b bg-background z-10 sticky top-0 h-13">
                <div className="flex items-center gap-2 truncate">
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => setShowMaterialsPanel(true)}
                    className="h-8 w-8 shrink-0 text-muted-foreground hover:text-foreground mr-1"
                    title="Show materials"
                  >
                    <PanelLeftOpen className="h-4 w-4" />
                  </Button>
                  <span className="text-sm font-semibold truncate">
                    {currentMaterial?.title || 'Material Viewer'}
                  </span>
                </div>
                <div className="flex items-center gap-1 shrink-0">
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-8 w-8"
                    onClick={() => {
                      setSelectedMaterialId(null);
                      setMaterialPage(undefined);
                      setMaterialTimestamp(undefined);
                    }}
                    title="Close viewer"
                  >
                    <X className="h-4 w-4" />
                  </Button>
                </div>
              </div>
              <div className="flex-1 overflow-hidden relative">
                <MaterialViewer
                  key={selectedMaterialId}
                  materialId={selectedMaterialId}
                  sessionId={sessionId}
                  initialPage={materialPage}
                  initialTimestamp={materialTimestamp}
                  scrollTrigger={scrollTrigger}
                  onSectionResult={handleSectionResult}
                  onSectionSummarize={(sectionId, sectionTitle) => handleSectionSummarize(sectionId, sectionTitle, currentMaterial?.title)}
                />
              </div>
            </Panel>
            
            <PanelResizeHandle className="w-1.5 flex flex-col justify-center items-center cursor-col-resize bg-border/50 hover:bg-primary/50 transition-colors z-10 group relative">
              <div className="h-8 w-1 rounded-full bg-border group-hover:bg-primary transition-colors flex items-center justify-center">
                <GripVertical className="h-4 w-4 text-muted-foreground opacity-0 group-hover:opacity-100 transition-opacity absolute" />
              </div>
            </PanelResizeHandle>
          </>
        )}

      {/* Right: Studio Panel */}
        <Panel defaultSize={24} minSize={20} className="flex flex-col min-w-0 bg-background">
        {/* Modern Tab bar - Compact dynamically shrinking layout */}
        <div className="flex items-center justify-between gap-2 px-2 py-2 border-b bg-background z-10 sticky top-0 h-13">
          <div className="flex flex-1 items-center min-w-0 h-full">
            {!selectedMaterialId && (
              <Button
                variant="outline"
                size="icon"
                onClick={() => setShowMaterialsPanel(true)}
                className="h-8 w-8 shrink-0 text-muted-foreground hover:text-foreground mr-2"
                title="Show materials panel"
              >
                <PanelLeftOpen className="h-4 w-4" />
              </Button>
            )}

            {/* Unified container that shrinks items evenly without wrapping */}
            <div className="flex flex-1 items-center p-1 bg-secondary/30 rounded-lg border border-border/50 min-w-0 h-full">
              {tabs.map((tab) => {
                const isActive = activeTab === tab.key;
                return (
                  <button
                    key={tab.key}
                    onClick={() => setActiveTab(tab.key)}
                    title={tab.label}
                    className={cn(
                      'flex flex-1 justify-center items-center gap-1.5 px-1 py-1 h-full rounded-md text-xs font-medium transition-all min-w-0 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 ring-offset-background',
                      isActive
                        ? 'bg-background text-foreground shadow-sm ring-1 ring-border/50'
                        : 'text-muted-foreground hover:text-foreground hover:bg-secondary/40'
                    )}
                  >
                    <span className={cn('shrink-0', isActive ? 'text-primary' : 'text-muted-foreground')}>
                      {tab.icon}
                    </span>
                    <span className="truncate">{tab.label}</span>
                  </button>
                );
              })}
            </div>
          </div>

          <div className="flex items-center shrink-0 gap-1.5 pl-2 pr-1">
            <Button
              variant="ghost"
              size="icon"
              onClick={() => setShowAiProviderSettings(true)}
              className="h-8 w-8 text-muted-foreground hover:text-foreground"
              title="AI Provider Settings"
            >
              <Lightbulb className="h-4 w-4" />
            </Button>
            <Button
              variant="ghost"
              size="icon"
              onClick={() => setShowVoiceSettings(true)}
              className="h-8 w-8 text-muted-foreground hover:text-foreground"
              title="Voice Settings"
            >
              <Mic className="h-4 w-4" />
            </Button>
            <div className="w-px h-4 bg-border/50 mx-0.5" />
            <Button
              variant="destructive"
              size="sm"
              onClick={() => setShowEndConfirm(true)}
              className="h-8 px-3 text-xs font-medium shadow-sm transition-transform active:scale-95 ml-0.5"
            >
              End Session
            </Button>
          </div>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-auto min-h-0 bg-secondary/5 relative">
          {activeTab === 'chat' && (
            <StudioChat
              ref={chatRef}
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
              pendingData={pendingFlashcards}
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
              pendingData={pendingQuiz}
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
      </Panel>
      </PanelGroup>

      <Modal
        open={showEndConfirm}
        onClose={() => setShowEndConfirm(false)}
        title="End Study Session"
      >
        <p className="mb-4 text-muted-foreground">Are you sure you want to end this study session? Your progress has been saved.</p>
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

      <AiProviderSettingsModal open={showAiProviderSettings} onClose={() => setShowAiProviderSettings(false)} />
      <VoiceSettingsModal open={showVoiceSettings} onClose={() => setShowVoiceSettings(false)} />
    </div>
    </AnimatedPage>
  );
}