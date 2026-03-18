import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { coursesApi } from '@/api/courses.api';
import { lecturesApi } from '@/api/lectures.api';
import { examsApi } from '@/api/exams.api';
import { studySessionsApi } from '@/api/studySessions.api';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { PageSpinner } from '@/components/ui/Spinner';
import { toast } from 'sonner';
import { Brain, FileText, ChevronRight, CheckCircle, Clock, Calendar } from 'lucide-react';
import { formatDate } from '@/utils/formatters';
import { useEffect, useRef } from 'react';

export default function CourseLearningPage() {
  const { courseId } = useParams<{ courseId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const autoCompleteTriggered = useRef(false);

  const { data: course, isLoading } = useQuery({
    queryKey: ['course', courseId],
    queryFn: () => coursesApi.getById(courseId!),
    enabled: !!courseId,
    select: (res) => res.data.data,
  });

  const { data: lectures } = useQuery({
    queryKey: ['course-lectures', courseId],
    queryFn: () => lecturesApi.getCourseLectures(courseId!),
    enabled: !!courseId,
    select: (res) => res.data.data,
  });

  const { data: progress } = useQuery({
    queryKey: ['course-progress', courseId],
    queryFn: () => coursesApi.getProgress(courseId!),
    enabled: !!courseId,
    select: (res) => res.data.data,
  });

  const { data: availableExams } = useQuery({
    queryKey: ['available-exams', courseId],
    queryFn: () => examsApi.getActive(courseId!, { page: 1, pageSize: 50 }),
    enabled: !!courseId,
    select: (res) => res.data.data?.items ?? [],
  });

  const { data: upcomingExams } = useQuery({
    queryKey: ['upcoming-exams', courseId],
    queryFn: () => examsApi.getUpcoming(courseId!, { page: 1, pageSize: 50 }),
    enabled: !!courseId,
    select: (res) => res.data.data?.items ?? [],
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

  const completeMutation = useMutation({
    mutationFn: () => coursesApi.complete(courseId!),
    onSuccess: () => {
      toast.success('Congratulations! You have completed this course!');
      queryClient.invalidateQueries({ queryKey: ['course', courseId] });
      queryClient.invalidateQueries({ queryKey: ['course-progress', courseId] });
    },
    onError: () => {},
  });

  // Auto-complete course when progress reaches 100%
  useEffect(() => {
    if (
      progress &&
      progress.progressPercentage >= 100 &&
      !autoCompleteTriggered.current
    ) {
      autoCompleteTriggered.current = true;
      completeMutation.mutate();
    }
  }, [progress?.progressPercentage]);

  if (isLoading) return <PageSpinner />;
  if (!course) return <div className="p-8 text-center">Course not found</div>;

  const isCompleted = progress && progress.progressPercentage >= 100;

  return (
    <AnimatedPage>
    <div className="max-w-5xl mx-auto px-4 py-8">
      <div className="flex items-start justify-between mb-8">
        <div>
          <div className="flex items-center gap-3 mb-2">
            <h1 className="text-3xl font-bold">{course.title}</h1>
            {isCompleted && (
              <Badge variant="default" className="bg-green-600 text-white">
                <CheckCircle className="h-3 w-3 mr-1" /> Completed
              </Badge>
            )}
          </div>
          <p className="text-muted-foreground">{course.description}</p>
          {progress && (
            <div className="mt-3 max-w-md">
              <div className="flex justify-between text-sm mb-1">
                <span>Progress</span>
                <span>{progress.progressPercentage}%</span>
              </div>
              <div className="w-full h-2 bg-secondary rounded-full">
                <div
                  className={`h-full rounded-full transition-all duration-500 ${
                    isCompleted ? 'bg-green-500' : 'bg-primary'
                  }`}
                  style={{ width: `${progress.progressPercentage}%` }}
                />
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                {progress.completedLessons} / {progress.totalLessons} materials completed
              </p>
            </div>
          )}
        </div>
        <Button
          onClick={() => startSessionMutation.mutate()}
          loading={startSessionMutation.isPending}
        >
          <Brain className="h-4 w-4 mr-2" /> AI Study
        </Button>
      </div>

      {/* Lectures */}
      <div className="mb-8">
        <h2 className="text-xl font-bold mb-4">Lectures</h2>
        {lectures && lectures.length > 0 ? (
          <div className="space-y-3">
            {lectures.map((lecture: any, idx: number) => (
              <Card
                key={lecture.id}
                className="hover:shadow-md hover:border-primary/30 transition-all cursor-pointer"
                onClick={() => navigate(`/courses/${courseId}/lectures/${lecture.id}`)}
              >
                <CardContent className="p-4 flex items-center gap-4">
                  <span className="text-lg font-bold text-muted-foreground w-8">
                    {idx + 1}
                  </span>
                  <div className="flex-1">
                    <h3 className="font-semibold">{lecture.title}</h3>
                    {lecture.description && (
                      <p className="text-sm text-muted-foreground">{lecture.description}</p>
                    )}
                    {lecture.materialCount > 0 && (
                      <p className="text-xs text-muted-foreground mt-1">
                        {lecture.materialCount} material(s)
                      </p>
                    )}
                  </div>
                  <ChevronRight className="h-5 w-5 text-muted-foreground" />
                </CardContent>
              </Card>
            ))}
          </div>
        ) : (
          <p className="text-muted-foreground">No lectures yet.</p>
        )}
      </div>

      {/* Available Exams */}
      {availableExams && availableExams.length > 0 && (
        <div className="mb-8">
          <h2 className="text-xl font-bold mb-4">Available Exams</h2>
          <div className="space-y-3">
            {availableExams.map((exam: any) => (
              <Card key={exam.id}>
                <CardContent className="p-4 flex items-center justify-between">
                  <div>
                    <h3 className="font-semibold">{exam.title}</h3>
                    <p className="text-sm text-muted-foreground">
                      {exam.durationMinutes} minutes
                    </p>
                  </div>
                  <Button onClick={() => navigate(`/exams/${exam.id}/take`)}>
                    <FileText className="h-4 w-4 mr-2" /> Take Exam
                  </Button>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      )}

      {/* Upcoming Exams */}
      {upcomingExams && upcomingExams.length > 0 && (
        <div>
          <h2 className="text-xl font-bold mb-4">Upcoming Exams</h2>
          <div className="space-y-3">
            {upcomingExams.map((exam: any) => (
              <Card key={exam.id} className="border-l-4 border-l-info">
                <CardContent className="p-4 flex items-center justify-between">
                  <div>
                    <h3 className="font-semibold">{exam.title}</h3>
                    <div className="flex items-center gap-4 mt-1">
                      <span className="text-sm text-muted-foreground flex items-center gap-1">
                        <Calendar className="h-3.5 w-3.5" />
                        Starts {formatDate(exam.startTime)}
                      </span>
                      <span className="text-sm text-muted-foreground flex items-center gap-1">
                        <Clock className="h-3.5 w-3.5" />
                        {exam.durationMinutes} min
                      </span>
                    </div>
                  </div>
                  <Badge variant="outline">Upcoming</Badge>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      )}
    </div>
    </AnimatedPage>
  );
}
