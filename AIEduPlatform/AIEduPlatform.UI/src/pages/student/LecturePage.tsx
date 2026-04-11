import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { lecturesApi } from '@/api/lectures.api';
import { studySessionsApi } from '@/api/studySessions.api';
import { MaterialViewer } from '@/components/viewer/MaterialViewer';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { PageSpinner } from '@/components/ui/Spinner';
import { useState } from 'react';
import { toast } from 'sonner';
import {
  ArrowLeft,
  Brain,
  FileText,
  Video,
  Music,
  Image as ImageIcon,
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

export default function LecturePage() {
  const { courseId, lectureId } = useParams<{ courseId: string; lectureId: string }>();
  const navigate = useNavigate();
  const [selectedMaterialId, setSelectedMaterialId] = useState<string | null>(null);

  const { data: lecture, isLoading } = useQuery({
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
    onError: () => toast.error('Failed to start study session'),
  });

  if (isLoading) return <PageSpinner />;
  if (!lecture) return <div className="p-8 text-center">Lecture not found</div>;

  // Backend returns materialsByType: { "Video": [...], "Document": [...] }
  // Flatten into a single array, carrying the type from the dictionary key
  const materials = (lecture as any).materialsByType
    ? Object.entries((lecture as any).materialsByType as Record<string, any[]>).flatMap(
        ([type, mats]) => mats.map((mat: any) => ({ ...mat, type }))
      )
    : [];

  return (
    <AnimatedPage>
      <div className="w-full max-w-[95vw] 2xl:max-w-[1800px] mx-auto px-4 py-6">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-3">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => navigate(`/courses/${courseId}/learn`)}
            >
              <ArrowLeft className="h-4 w-4 mr-1" /> Back
            </Button>
            <div>
              <h1 className="text-2xl font-bold">{lecture.title}</h1>
              {lecture.description && (
                <p className="text-sm text-muted-foreground">{lecture.description}</p>
              )}
            </div>
          </div>
          <Button
            onClick={() => startSessionMutation.mutate()}
            loading={startSessionMutation.isPending}
          >
            <Brain className="h-4 w-4 mr-2" /> AI Study Session
          </Button>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-4 xl:grid-cols-5 gap-6">
          {/* Materials list */}
          <div className="lg:col-span-1 space-y-2">
            <h2 className="text-lg font-semibold mb-3">Materials ({materials.length})</h2>
            {materials.length === 0 ? (
              <p className="text-sm text-muted-foreground">No materials uploaded yet.</p>
            ) : (
              materials.map((mat: any) => (
                <Card
                  key={mat.id}
                  className={`cursor-pointer transition-all hover:shadow-md ${
                    selectedMaterialId === mat.id
                      ? 'ring-2 ring-primary border-primary'
                      : ''
                  }`}
                  onClick={() => setSelectedMaterialId(mat.id)}
                >
                  <CardContent className="p-3 flex items-center gap-3">
                    <div className="p-2 rounded-md bg-primary/10 text-primary">
                      {materialTypeIcon(mat.type)}
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="font-medium text-sm truncate">{mat.title}</p>
                      <Badge variant="outline" className="text-xs mt-1">
                        {materialTypeLabel(mat.type)}
                      </Badge>
                    </div>
                  </CardContent>
                </Card>
              ))
            )}
          </div>

          {/* Material viewer */}
          <div className="lg:col-span-3 xl:col-span-4 h-[calc(100vh-12rem)] min-h-[600px]">
            {selectedMaterialId ? (
              <div className="border rounded-lg overflow-hidden h-full flex flex-col bg-background shadow-sm">
                <MaterialViewer materialId={selectedMaterialId} />
              </div>
            ) : (
              <div className="border rounded-lg h-full flex items-center justify-center text-muted-foreground bg-secondary/10">
                <div className="text-center">
                  <FileText className="h-12 w-12 mx-auto mb-3 opacity-40 text-primary" />
                  <p>Select a material to view</p>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </AnimatedPage>
  );
}
