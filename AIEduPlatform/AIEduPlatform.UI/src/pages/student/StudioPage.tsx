import { useParams, useNavigate } from 'react-router-dom';
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
  PanelLeftClose,
  PanelLeftOpen,
  FileVideo,
  FileAudio,
  Image as ImageIcon,
  File,
  GripVertical
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
  const [activeTab, setActiveTab] = useState<StudioTab>('chat');
  const [selectedMaterialId, setSelectedMaterialId] = useState<string | null>(null);
  const [materialPage, setMaterialPage] = useState<number | undefined>(undefined);
  const [materialTimestamp, setMaterialTimestamp] = useState<number | undefined>(undefined);
  const [scrollTrigger, setScrollTrigger] = useState<number>(0);
  const [selectedLectureIds, setSelectedLectureIds] = useState<string[]>([]);
  const [selectedMaterialIds, setSelectedMaterialIds] = useState<string[]>([]);
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
    
    setShowMaterialsPanel(true);
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

  const toggleMaterial = (materialId: string) => {
    setSelectedMaterialIds((prev) =>
      prev.includes(materialId)
        ? prev.filter((id) => id !== materialId)
        : [...prev, materialId]
    );
  };

  return (
    <AnimatedPage>
    <div className="h-[calc(100vh-3.5rem)] bg-background">
      <PanelGroup orientation="horizontal">
        {/* Left Panel: Materials & References */}
        {showMaterialsPanel && (
          <>
            <Panel defaultSize={65} minSize={30} className="flex flex-col min-w-0 bg-card">
          {/* Materials panel header */}
          <div className="flex items-center justify-between px-4 py-3 border-b bg-gradient-to-r from-primary/5 to-transparent">
            <div className="flex items-center gap-2.5">
              <div className="p-1.5 rounded-lg bg-primary/10">
                <BookOpen className="h-4 w-4 text-primary" />
              </div>
              <span className="text-sm font-semibold">Materials</span>
            </div>
            <div className="flex items-center gap-1">
              {selectedMaterialId && (
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-7 w-7"
                  onClick={() => {
                    setSelectedMaterialId(null);
                    setMaterialPage(undefined);
                    setMaterialTimestamp(undefined);
                  }}
                  title="Close viewer"
                >
                  <X className="h-3.5 w-3.5" />
                </Button>
              )}
              <Button
                variant="ghost"
                size="icon"
                className="h-7 w-7"
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
              <div className="flex-1 overflow-hidden">
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
              {/* Compact material list below viewer */}
              <div className="border-t max-h-40 overflow-y-auto bg-secondary/5">
                <div className="px-3 py-2 text-[10px] font-semibold text-muted-foreground uppercase tracking-wider border-b bg-secondary/30 sticky top-0">
                  References
                </div>
                {lectures?.map((lecture: any) => (
                  <div key={lecture.id}>
                    <label className="flex items-center gap-2 px-3 py-1.5 hover:bg-secondary/50 cursor-pointer transition-colors">
                      <input
                        type="checkbox"
                        checked={selectedLectureIds.includes(lecture.id)}
                        onChange={() => toggleLecture(lecture.id)}
                        className="accent-primary h-3 w-3 rounded"
                      />
                      <span className="text-xs font-medium truncate">{lecture.title}</span>
                    </label>
                    {lecture.materials?.map((mat: any) => (
                      <div key={mat.id} className="flex items-center gap-1.5 pl-6 pr-3 py-1 hover:bg-secondary/30 transition-colors">
                        <input
                          type="checkbox"
                          checked={selectedMaterialIds.includes(mat.id)}
                          onChange={() => toggleMaterial(mat.id)}
                          className="accent-primary h-2.5 w-2.5 rounded"
                        />
                        {getMaterialIcon(mat.materialType)}
                        <button
                          className={cn(
                            'text-xs truncate hover:underline text-left flex-1',
                            selectedMaterialId === mat.id ? 'text-primary font-semibold' : 'text-muted-foreground hover:text-foreground'
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
                <div key={lecture.id} className="border-b border-border/50 last:border-b-0">
                  <label className="flex items-center gap-3 px-4 py-3 hover:bg-secondary/40 cursor-pointer transition-colors group">
                    <input
                      type="checkbox"
                      checked={selectedLectureIds.includes(lecture.id)}
                      onChange={() => toggleLecture(lecture.id)}
                      className="accent-primary h-4 w-4 rounded"
                    />
                    <div className="flex-1 min-w-0">
                      <span className="text-sm font-semibold truncate block group-hover:text-primary transition-colors">
                        {lecture.title}
                      </span>
                      <span className="text-xs text-muted-foreground">
                        {lecture.materials?.length || 0} materials
                      </span>
                    </div>
                  </label>
                  <div className="pb-1">
                    {lecture.materials?.map((mat: any) => (
                      <div
                        key={mat.id}
                        className="flex items-center gap-2.5 pl-10 pr-4 py-2 hover:bg-secondary/30 transition-colors rounded-lg mx-2 my-0.5"
                      >
                        <input
                          type="checkbox"
                          checked={selectedMaterialIds.includes(mat.id)}
                          onChange={() => toggleMaterial(mat.id)}
                          className="accent-primary h-3.5 w-3.5 rounded"
                        />
                        <div className="p-1 rounded bg-secondary/50">
                          {getMaterialIcon(mat.materialType)}
                        </div>
                        <button
                          className="text-sm text-foreground/80 hover:text-primary hover:underline truncate text-left flex-1 transition-colors"
                          onClick={() => {
                            setSelectedMaterialId(mat.id);
                            setMaterialPage(undefined);
                            setMaterialTimestamp(undefined);
                            if (!selectedMaterialIds.includes(mat.id)) toggleMaterial(mat.id);
                          }}
                        >
                          {mat.title}
                        </button>
                      </div>
                    ))}
                  </div>
                </div>
              ))}
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
          )}
        </Panel>
        
        {/* Resize Handle */}
        <PanelResizeHandle className="w-1.5 flex flex-col justify-center items-center cursor-col-resize bg-border/50 hover:bg-primary/50 transition-colors z-10 group">
          <div className="h-8 w-1 rounded-full bg-border group-hover:bg-primary transition-colors flex items-center justify-center">
            <GripVertical className="h-4 w-4 text-muted-foreground opacity-0 group-hover:opacity-100 transition-opacity absolute" />
          </div>
        </PanelResizeHandle>
        </>
      )}

      {/* Right: Studio Panel */}
        <Panel defaultSize={35} minSize={25} className="flex flex-col min-w-0 bg-background">
        {/* Modern Tab bar - Compact dynamically shrinking layout */}
        <div className="flex items-center justify-between gap-2 px-2 py-2 border-b bg-background z-10 sticky top-0 h-[52px]">
          <div className="flex flex-1 items-center min-w-0 h-full">
            {!showMaterialsPanel && (
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










