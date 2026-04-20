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
import { ProgressBar } from '@/components/ui/ProgressBar';
import { toast } from 'sonner';
import {
  Brain,
  CheckCircle,
  Clock,
  Calendar,
  ArrowRight,
  PlayCircle,
  Layers3,
} from 'lucide-react';
import { formatDate } from '@/utils/formatters';
import { useEffect, useRef } from 'react';
import { FaVideo, FaFilePdf, FaClipboardCheck } from 'react-icons/fa6';
import Skeleton from 'react-loading-skeleton';

function getApiErrorMessage(error: unknown, fallback: string) {
  const responseMessage = (error as any)?.response?.data?.message;
  if (typeof responseMessage === 'string' && responseMessage.trim().length > 0) {
    return responseMessage;
  }

  const message = (error as any)?.message;
  if (typeof message === 'string' && message.trim().length > 0) {
    return message;
  }

  return fallback;
}

function getLectureTypeMeta(lecture: any) {
  const firstMaterialType = String(
    lecture.materials?.[0]?.materialType ?? lecture.materials?.[0]?.type ?? ''
  ).toLowerCase();

  if (firstMaterialType.includes('video')) {
    return {
      label: 'Video',
      icon: <FaVideo className="h-4 w-4 text-blue-500" />,
      chipClass: 'bg-blue-500/10 text-blue-500 border-blue-500/20',
    };
  }

  if (firstMaterialType.includes('quiz')) {
    return {
      label: 'Quiz',
      icon: <FaClipboardCheck className="h-4 w-4 text-amber-500" />,
      chipClass: 'bg-amber-500/10 text-amber-500 border-amber-500/20',
    };
  }

  return {
    label: 'Document',
    icon: <FaFilePdf className="h-4 w-4 text-rose-500" />,
    chipClass: 'bg-rose-500/10 text-rose-500 border-rose-500/20',
  };
}

export default function CourseLearningPage() {
  const { courseId } = useParams<{ courseId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const autoCompleteTriggered = useRef(false);

  const {
    data: course,
    isLoading,
    isError: isCourseError,
    error: courseError,
  } = useQuery({
    queryKey: ['course', courseId],
    queryFn: () => coursesApi.getById(courseId!),
    enabled: !!courseId,
    select: (res) => res.data.data,
  });

  const {
    data: lectures,
    isLoading: isLecturesLoading,
    isError: isLecturesError,
    error: lecturesError,
  } = useQuery({
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
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
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
      !progress.isCompleted &&
      !autoCompleteTriggered.current
    ) {
      autoCompleteTriggered.current = true;
      completeMutation.mutate();
    }
  }, [progress, progress?.progressPercentage]);

  if (isLoading) {
    return (
      <AnimatedPage>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <Skeleton height={230} borderRadius={20} />
          <div className="mt-6 grid gap-6 xl:grid-cols-12">
            <div className="xl:col-span-8 space-y-3">
              {Array.from({ length: 5 }).map((_, i) => (
                <Skeleton key={i} height={84} borderRadius={14} />
              ))}
            </div>
            <div className="xl:col-span-4 space-y-3">
              <Skeleton height={210} borderRadius={14} />
              <Skeleton height={180} borderRadius={14} />
              <Skeleton height={180} borderRadius={14} />
            </div>
          </div>
        </div>
      </AnimatedPage>
    );
  }
  if (isCourseError) {
    return (
      <div className="p-8 text-center text-sm text-muted-foreground">
        {getApiErrorMessage(courseError, 'Failed to load this course.')}
      </div>
    );
  }
  if (!course) return <div className="p-8 text-center">Course not found</div>;

  const isCompleted = progress?.isCompleted || (progress && progress.progressPercentage >= 100);
  const sortedLectures = [...(lectures ?? [])].sort(
    (a: any, b: any) => (a.orderIndex ?? 0) - (b.orderIndex ?? 0)
  );
  const completedLectureIds = progress?.completedLectureIds ?? [];
  const completedLectureCount = sortedLectures.filter((lecture: any) =>
    completedLectureIds.includes(lecture.id)
  ).length;
  const nextLecture = sortedLectures.find(
    (lecture: any) => !completedLectureIds.includes(lecture.id)
  ) ?? sortedLectures[0];
  const materialsCompleted = progress?.completedLessons ?? 0;
  const materialsTotal = progress?.totalLessons ?? 0;

  return (
    <AnimatedPage>
      <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
        
        {/* Clean Dashboard Header (No heavy gradients) */}
        <header className="mb-8 border-b border-border/60 pb-6">
          <div className="flex flex-col md:flex-row md:items-end justify-between gap-4">
            <div>
              <div className="flex items-center gap-2 mb-3">
                {isCompleted ? (
                  <Badge variant="success" className="inline-flex items-center gap-1 bg-success/10 text-success border-success/20">
                    <CheckCircle className="h-3.5 w-3.5" /> Completed
                  </Badge>
                ) : (
                  <Badge variant="info" className="inline-flex items-center gap-1">
                    <PlayCircle className="h-3.5 w-3.5" /> Active Course
                  </Badge>
                )}
                <span className="text-sm font-medium text-muted-foreground">
                  Dashboard
                </span>
              </div>
              <h1 className="text-2xl md:text-3xl font-bold tracking-tight text-foreground">
                {course.title}
              </h1>
            </div>
            
            <div className="w-full md:w-64">
              <div className="flex items-center justify-between text-xs mb-1.5">
                <span className="text-muted-foreground font-medium uppercase tracking-wider">Overall Progress</span>
                <span className="font-bold">{progress?.progressPercentage ?? 0}%</span>
              </div>
              <ProgressBar
                progress={progress?.progressPercentage ?? 0}
                className="h-2 rounded-full"
                fillClassName={isCompleted ? 'bg-success' : 'bg-primary'}
              />
            </div>
          </div>
        </header>

        {/* Top Action Row */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
          {/* Main Action: Up Next */}
          <Card className="lg:col-span-2 bg-card border-primary/20 shadow-sm relative overflow-hidden group">
            <div className="absolute top-0 right-0 p-4 opacity-10 group-hover:opacity-20 transition-opacity">
              <PlayCircle className="w-24 h-24 text-primary" />
            </div>
            <CardContent className="p-6 flex flex-col justify-center h-full relative z-10">
              <Badge variant="outline" className="w-fit mb-3 border-primary/30 text-primary bg-primary/5">
                Up Next
              </Badge>
              {nextLecture ? (
                <>
                  <h3 className="text-xl font-bold mb-1 truncate">{nextLecture.title}</h3>
                  <p className="text-sm text-muted-foreground mb-4">Resume your learning journey right where you left off.</p>
                  <Button
                    onClick={() => navigate(`/courses/${courseId}/lectures/${nextLecture.id}`)}
                    className="w-fit shadow-md"
                  >
                    Continue Learning <ArrowRight className="h-4 w-4 ml-2" />
                  </Button>
                </>
              ) : isCompleted ? (
                <>
                  <h3 className="text-xl font-bold mb-1 text-success">Course Completed!</h3>
                  <p className="text-sm text-muted-foreground mb-4">You have finished all available lectures.</p>
                  <Button variant="outline" className="w-fit pointer-events-none opacity-50">
                    All Caught Up
                  </Button>
                </>
              ) : (
                 <>
                  <h3 className="text-xl font-bold mb-1">No upcoming lectures</h3>
                  <p className="text-sm text-muted-foreground mb-4">You are caught up with the current content.</p>
                </>
              )}
            </CardContent>
          </Card>

          {/* AI Session Action */}
          <Card className="bg-gradient-to-br from-indigo-500/10 via-purple-500/5 to-transparent border-indigo-500/20 shadow-sm relative overflow-hidden group">
            <CardContent className="p-6 flex flex-col justify-center h-full">
               <div className="h-10 w-10 rounded-xl bg-indigo-500/20 flex items-center justify-center mb-4">
                 <Brain className="h-5 w-5 text-indigo-600 dark:text-indigo-400" />
               </div>
               <h3 className="font-semibold mb-1">AI Tutor</h3>
               <p className="text-xs text-muted-foreground mb-4">Get personalized help on course concepts.</p>
               <Button
                  variant="outline"
                  size="sm"
                  className="w-full border-indigo-500/30 hover:bg-indigo-500/10 text-indigo-700 dark:text-indigo-400"
                  onClick={() => startSessionMutation.mutate()}
                  loading={startSessionMutation.isPending}
               >
                 Start Session
               </Button>
            </CardContent>
          </Card>

          {/* Quick Stats */}
          <div className="flex flex-col gap-4">
            <Card className="flex-1 shadow-sm flex items-center p-4">
              <div className="rounded-full bg-secondary w-10 h-10 flex items-center justify-center mr-4 shrink-0">
                <Layers3 className="h-4 w-4 text-muted-foreground" />
              </div>
              <div>
                <p className="text-xs text-muted-foreground font-medium uppercase tracking-wider">Lectures</p>
                <p className="text-lg font-bold leading-none mt-1">{completedLectureCount} / {sortedLectures.length}</p>
              </div>
            </Card>
            <Card className="flex-1 shadow-sm flex items-center p-4">
              <div className="rounded-full bg-secondary w-10 h-10 flex items-center justify-center mr-4 shrink-0">
                <FaFilePdf className="h-4 w-4 text-muted-foreground" />
              </div>
              <div>
                <p className="text-xs text-muted-foreground font-medium uppercase tracking-wider">Materials</p>
                <p className="text-lg font-bold leading-none mt-1">{materialsCompleted} / {materialsTotal}</p>
              </div>
            </Card>
          </div>
        </div>

        {/* Main Content Split */}
        <div className="flex flex-col lg:flex-row gap-8">
          
          {/* Left Column: Course Contents */}
          <div className="flex-1">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-bold">Course Contents</h2>
            </div>
            
            <div className="bg-card rounded-xl border border-border overflow-hidden shadow-sm">
              {isLecturesLoading ? (
                <div className="p-4 space-y-4">
                  {Array.from({ length: 4 }).map((_, i) => (
                    <Skeleton key={i} height={60} borderRadius={8} />
                  ))}
                </div>
              ) : sortedLectures.length > 0 ? (
                <div className="divide-y divide-border/50">
                  {sortedLectures.map((lecture: any, idx: number) => {
                    const materialCount = Array.isArray(lecture.materials)
                      ? lecture.materials.length
                      : (typeof lecture.materialCount === 'number' ? lecture.materialCount : 0);
                    const typeMeta = getLectureTypeMeta(lecture);
                    const isCompletedLecture = completedLectureIds.includes(lecture.id);

                    return (
                      <div
                        key={lecture.id}
                        onClick={() => navigate(`/courses/${courseId}/lectures/${lecture.id}`)}
                        className="group flex items-center gap-4 p-4 hover:bg-secondary/40 transition-colors cursor-pointer"
                      >
                        <div className="w-8 flex justify-center shrink-0">
                          {isCompletedLecture ? (
                            <CheckCircle className="h-5 w-5 text-success" />
                          ) : (
                            <span className="text-sm font-medium text-muted-foreground group-hover:text-foreground transition-colors">
                              {(idx + 1).toString().padStart(2, '0')}
                            </span>
                          )}
                        </div>
                        
                        <div className="h-8 w-8 rounded bg-background border border-border flex items-center justify-center shrink-0 shadow-sm">
                          {typeMeta.icon}
                        </div>

                        <div className="flex-1 min-w-0">
                          <h4 className="font-medium text-sm truncate group-hover:text-primary transition-colors">
                            {lecture.title}
                          </h4>
                        </div>

                        <div className="hidden sm:flex items-center gap-3 shrink-0">
                          <span className="text-xs text-muted-foreground">
                            {materialCount} materials
                          </span>
                        </div>
                      </div>
                    );
                  })}
                </div>
              ) : isLecturesError ? (
                <div className="p-8 text-center text-muted-foreground">
                  {getApiErrorMessage(lecturesError, 'Unable to load lectures.')}
                </div>
              ) : (
                <div className="p-8 text-center text-muted-foreground">
                  No lectures have been added yet.
                </div>
              )}
            </div>
          </div>

          {/* Right Column: Exams & Assessments */}
          <div className="w-full lg:w-80 shrink-0 space-y-6">
            
            {/* Actionable Exams */}
            <div>
              <h2 className="text-lg font-bold mb-4 flex items-center gap-2">
                <FaClipboardCheck className="h-4 w-4 text-primary" /> Assessments
              </h2>
              
              <div className="space-y-3">
                {availableExams && availableExams.length > 0 ? (
                  availableExams.map((exam: any) => (
                    <Card key={exam.id} className="border-border shadow-sm hover:border-primary/50 transition-colors">
                      <CardContent className="p-4">
                        <div className="flex justify-between items-start mb-2">
                          <p className="font-semibold text-sm leading-snug">{exam.title}</p>
                          {exam.hasSubmitted && (
                             <Badge variant="outline" className="text-[10px] h-5 py-0">Done</Badge>
                          )}
                        </div>
                        <div className="flex items-center gap-4 text-xs text-muted-foreground mb-4">
                          <span className="flex items-center gap-1">
                            <Clock className="h-3 w-3" /> {exam.durationMinutes}m
                          </span>
                        </div>
                        <Button
                          className="w-full h-8 text-xs"
                          onClick={() => navigate(`/exams/${exam.id}/take`)}
                          disabled={exam.hasSubmitted}
                          variant={exam.hasSubmitted ? 'secondary' : 'primary'}
                        >
                          {exam.hasSubmitted ? 'Review Submission' : 'Start Exam'}
                        </Button>
                      </CardContent>
                    </Card>
                  ))
                ) : (
                  <Card className="border-dashed bg-transparent shadow-none">
                    <CardContent className="p-4 text-center">
                      <p className="text-xs text-muted-foreground">No active assessments available.</p>
                    </CardContent>
                  </Card>
                )}
              </div>
            </div>

            {/* Upcoming Schedule */}
            {upcomingExams && upcomingExams.length > 0 && (
              <div>
                <h3 className="text-sm font-semibold mb-3 text-muted-foreground uppercase tracking-wider">
                  Upcoming Schedule
                </h3>
                <div className="space-y-3">
                  {upcomingExams.map((exam: any) => (
                    <div key={exam.id} className="rounded-lg border border-border bg-card p-3 relative overflow-hidden">
                      <div className="absolute left-0 top-0 bottom-0 w-1 bg-info/50" />
                      <p className="font-medium text-sm leading-snug ml-1">{exam.title}</p>
                      <div className="mt-2 ml-1 space-y-1">
                        <p className="text-[11px] text-muted-foreground flex items-center gap-1">
                          <Calendar className="h-3 w-3" /> {formatDate(exam.startTime)}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}
            
          </div>
        </div>

      </div>
    </AnimatedPage>
  );
}

