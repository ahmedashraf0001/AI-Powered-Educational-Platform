import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { lecturesApi } from '@/api/lectures.api';
import { studySessionsApi } from '@/api/studySessions.api';
import { MaterialViewer } from '@/components/viewer/MaterialViewer';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { Button } from '@/components/ui/Button';

import { Badge } from '@/components/ui/Badge';
import { PageSpinner } from '@/components/ui/Spinner';
import { useEffect, useState } from 'react';
import { toast } from 'sonner';
import {
  ArrowLeft,
  Brain,
  FileText,
  Video,
  Music,
  Image as ImageIcon,
  PlayCircle,
  Layout,
  BookOpen
} from 'lucide-react';

const materialTypeIcon = (type: string | number) => {
  const t = typeof type === 'string' ? type.toLowerCase() : type;
  if (t === 'video' || t === 0) return <Video className="h-4 w-4" />;
  if (t === 'document' || t === 1) return <FileText className="h-4 w-4" />;
  if (t === 'audio' || t === 2) return <Music className="h-4 w-4" />;
  if (t === 'image' || t === 3) return <ImageIcon className="h-4 w-4" />;
  return <FileText className="h-4 w-4" />;
};

const materialTypeLabel = (type: string | number) => {
  const t = typeof type === 'string' ? type.toLowerCase() : type;
  if (t === 'video' || t === 0) return 'Video';
  if (t === 'document' || t === 1) return 'Document';
  if (t === 'audio' || t === 2) return 'Audio';
  if (t === 'image' || t === 3) return 'Image';
  return 'File';
};

const getApiErrorMessage = (error: unknown, fallback: string) => {
  const responseMessage = (error as any)?.response?.data?.message;
  if (typeof responseMessage === 'string' && responseMessage.trim().length > 0) {
    return responseMessage;
  }

  const message = (error as any)?.message;
  if (typeof message === 'string' && message.trim().length > 0) {
    return message;
  }

  return fallback;
};

export default function LecturePage() {
  const { courseId, lectureId } = useParams<{ courseId: string; lectureId: string }>();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [selectedMaterialId, setSelectedMaterialId] = useState<string | null>(searchParams.get('materialId'));

  const {
    data: lecture,
    isLoading,
    isError,
    error,
  } = useQuery({
    queryKey: ['lecture-detail', lectureId],
    queryFn: () => lecturesApi.getById(lectureId!),
    enabled: !!lectureId,
    select: (res) => res.data.data,
  });

  const startSessionMutation = useMutation({
    mutationFn: () => studySessionsApi.start(courseId!),
    onSuccess: (res) => {
      const sessionId = res.data.data?.sessionId;
      if (sessionId) {
        navigate(`/courses/${courseId}/studio/${sessionId}`);
      } else {
        toast.error('No session ID returned');
      }
    },
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
  });

  // Backend returns materialsByType: { "Video": [...], "Document": [...] }
  // Flatten into a single array, carrying the type from the dictionary key
  const materials = (lecture as any)?.materialsByType
    ? Object.entries((lecture as any).materialsByType as Record<string, any[]>).flatMap(
        ([type, mats]) => mats.map((mat: any) => ({ ...mat, type }))
      )
    : [];
  const hasSelectedMaterial = materials.some((material: any) => material.id === selectedMaterialId);

  useEffect(() => {
    if (materials.length === 0) {
      return;
    }

    if (!selectedMaterialId || !hasSelectedMaterial) {
      setSelectedMaterialId(materials[0].id);
    }
  }, [materials, selectedMaterialId, hasSelectedMaterial]);

  if (isLoading) return <PageSpinner />;
  if (isError) {
    return (
      <div className="p-8 text-center text-sm text-muted-foreground">
        {getApiErrorMessage(error, 'Failed to load lecture details.')}
      </div>
    );
  }
  if (!lecture) return <div className="p-8 text-center">Lecture not found</div>;

  return (
    <AnimatedPage>
      <div className="min-h-screen bg-secondary/20 pb-12">
        {/* Top Navigation Bar / Header */}
        <div className="bg-background border-b sticky top-0 z-10 shadow-sm">
          <div className="max-w-[1600px] mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
            <div className="flex items-center gap-4">
              <Button
                variant="ghost"
                size="icon"
                className="rounded-full hover:bg-secondary"
                onClick={() => navigate(`/courses/${courseId}/learn`)}
              >
                <ArrowLeft className="h-5 w-5" />
              </Button>
              <div className="h-6 w-px bg-border mx-1 hidden sm:block"></div>
              <div className="hidden sm:block">
                <h1 className="text-lg font-semibold tracking-tight line-clamp-1">{lecture.title}</h1>
                <div className="flex items-center gap-2 text-xs text-muted-foreground mt-0.5">
                  <span className="flex items-center gap-1"><BookOpen className="h-3 w-3"/> Lecture</span>
                  {lecture.description && (
                    <>
                      <span>•</span>
                      <span className="line-clamp-1 max-w-md">{lecture.description}</span>
                    </>
                  )}
                </div>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <Button
                size="sm"
                className="shadow-sm rounded-full px-5"
                onClick={() => startSessionMutation.mutate()}
                loading={startSessionMutation.isPending}
              >
                <Brain className="h-4 w-4 mr-2" /> AI Study Session
              </Button>
            </div>
          </div>
        </div>

        {/* Main Content */}
        <div className="max-w-[1600px] mx-auto px-4 sm:px-6 lg:px-8 mt-6">
          <div className="flex flex-col lg:flex-row gap-6">
            
            {/* Left Column: Material Viewer */}
            <div className="flex-1 min-w-0">
              {selectedMaterialId ? (
                <div className="bg-background sm:rounded-xl overflow-hidden shadow-md border h-[60vh] lg:h-[calc(100vh-8rem)] min-h-[500px] flex flex-col relative group">
                  <MaterialViewer materialId={selectedMaterialId} />
                </div>
              ) : (
                <div className="bg-background sm:rounded-xl overflow-hidden shadow-md border h-[60vh] lg:h-[calc(100vh-8rem)] min-h-[500px] flex flex-col items-center justify-center text-muted-foreground relative group">
                  <div className="h-20 w-20 rounded-full bg-secondary/40 flex items-center justify-center mb-4">
                    <Layout className="h-10 w-10 opacity-50 text-foreground" />
                  </div>
                  <h3 className="text-lg font-medium text-foreground mb-1">No material selected</h3>
                  <p className="text-sm">Select an item from the course content to view it</p>
                </div>
              )}
            </div>

            {/* Right Column: Course Content Playlist */}
            <div className="w-full lg:w-[400px] shrink-0">
              <div className="bg-background sm:rounded-xl border shadow-md flex flex-col h-[60vh] lg:h-[calc(100vh-8rem)] overflow-hidden">
                <div className="p-4 border-b bg-muted/20">
                  <h2 className="font-semibold text-lg flex items-center gap-2">
                    Course Content
                  </h2>
                  <p className="text-sm text-muted-foreground mt-1">
                    {materials.length} {materials.length === 1 ? 'item' : 'items'}
                  </p>
                </div>
                
                <div className="flex-1 overflow-y-auto p-2 space-y-1 custom-scrollbar">
                  {materials.length === 0 ? (
                    <div className="p-8 text-center text-sm text-muted-foreground flex flex-col items-center justify-center h-full">
                      <FileText className="h-10 w-10 mb-3 opacity-20" />
                      <p>No materials uploaded yet.</p>
                    </div>
                  ) : (
                    materials.map((mat: any, index: number) => {
                      const isSelected = selectedMaterialId === mat.id;
                      return (
                        <div
                          key={mat.id}
                          className={`group flex items-start gap-3 p-3 rounded-lg cursor-pointer transition-all ${
                            isSelected
                              ? 'bg-primary/10 text-primary hover:bg-primary/20'
                              : 'hover:bg-secondary/60 text-foreground'
                          }`}
                          onClick={() => setSelectedMaterialId(mat.id)}
                        >
                          <div className={`mt-0.5 flex-shrink-0 flex items-center justify-center h-5 w-5 ${isSelected ? 'text-primary' : 'text-muted-foreground group-hover:text-primary transition-colors'}`}>
                            {isSelected ? <PlayCircle className="h-5 w-5" /> : materialTypeIcon(mat.type)}
                          </div>
                          <div className="flex-1 min-w-0">
                            <p className={`text-sm font-medium leading-normal ${isSelected ? 'text-primary' : ''}`}>
                              {index + 1}. {mat.title}
                            </p>
                            <div className="flex items-center gap-2 mt-1">
                              <Badge variant={isSelected ? "default" : "outline"} className={`text-[10px] px-1.5 py-0 font-normal ${isSelected ? '' : 'opacity-80'}`}>
                                {materialTypeLabel(mat.type)}
                              </Badge>
                            </div>
                          </div>
                        </div>
                      );
                    })
                  )}
                </div>
              </div>
            </div>

          </div>
        </div>
      </div>
    </AnimatedPage>
  );
}

