import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { coursesApi } from '@/api/courses.api';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { PageSpinner } from '@/components/ui/Spinner';
import { EmptyState } from '@/components/ui/Feedback';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { Modal } from '@/components/ui/Modal';
import { ProgressBar } from '@/components/ui/ProgressBar';
import { useNavigate } from 'react-router-dom';
import { useState, useMemo } from 'react';
import { toast } from 'sonner';
import {
  GraduationCap,
  BookOpen,
  PlayCircle,
  CalendarDays,
  Clock,
  AlertTriangle,
  LogOut,
  Trophy,
  Activity,
  Archive,
} from 'lucide-react';
import { EnrollmentStatus } from '@/types';

export default function MyEnrollmentsPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [unenrollTarget, setUnenrollTarget] = useState<{
    courseId: string;
    title: string;
  } | null>(null);

  const unenrollMutation = useMutation({
    mutationFn: (courseId: string) => coursesApi.unenroll(courseId),
    onSuccess: () => {
      toast.success('Unenrolled successfully');
      queryClient.invalidateQueries({ queryKey: ['enrolled-courses'] });
      queryClient.invalidateQueries({ queryKey: ['courses'] });
      queryClient.invalidateQueries({ queryKey: ['course'] });
      setUnenrollTarget(null);
    },
    onError: () => toast.error('Failed to unenroll'),
  });

  const { data: enrollments, isLoading } = useQuery({
    queryKey: ['enrolled-courses'],
    queryFn: () => coursesApi.getEnrolled(),
    select: (res) => res.data.data?.items ?? [],
  });

  const stats = useMemo(() => {
    if (!enrollments) return { total: 0, inProgress: 0, completed: 0 };
    return {
      total: enrollments.length,
      inProgress: enrollments.filter((e: any) => e.status === EnrollmentStatus.Active).length,
      completed: enrollments.filter((e: any) => e.status === EnrollmentStatus.Completed).length,
    };
  }, [enrollments]);

  if (isLoading) return <PageSpinner />;

  const statusLabel = (status: EnrollmentStatus) => {
    switch (status) {
      case EnrollmentStatus.Active:
        return 'In Progress';
      case EnrollmentStatus.Completed:
        return 'Completed';
      case EnrollmentStatus.Dropped:
        return 'Dropped';
      case EnrollmentStatus.Pending:
        return 'Pending';
      default:
        return 'Unknown';
    }
  };

  const statusVariant = (status: EnrollmentStatus) => {
    switch (status) {
      case EnrollmentStatus.Active:
        return 'default' as const;
      case EnrollmentStatus.Completed:
        return 'success' as const;
      case EnrollmentStatus.Dropped:
        return 'destructive' as const;
      default:
        return 'outline' as const;
    }
  };

  const formatDate = (dateStr: string | null | undefined) => {
    if (!dateStr) return null;
    return new Date(dateStr).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  return (
    <AnimatedPage>
      <div className="max-w-6xl mx-auto px-4 py-8">
        {/* Page Header */}
        <div className="flex flex-col md:flex-row md:items-end justify-between mb-10 gap-6">
          <div>
            <h1 className="text-3xl font-bold tracking-tight">My Enrollments</h1>
            <p className="text-muted-foreground mt-2 text-lg">
              Track your courses, pick up where you left off, and review your progress.
            </p>
          </div>
        </div>

        {/* Summarized Statistics */}
        {enrollments && enrollments.length > 0 && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-10">
            <Card className="bg-primary/5 border-primary/20">
              <CardContent className="p-6 flex items-center gap-4">
                <div className="p-3 bg-primary/10 rounded-xl">
                  <Archive className="h-6 w-6 text-primary" />
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Total Enrollments</p>
                  <p className="text-3xl font-bold text-foreground">{stats.total}</p>
                </div>
              </CardContent>
            </Card>
            <Card className="bg-blue-500/5 border-blue-500/20">
              <CardContent className="p-6 flex items-center gap-4">
                <div className="p-3 bg-blue-500/10 rounded-xl">
                  <Activity className="h-6 w-6 text-blue-500" />
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">In Progress</p>
                  <p className="text-3xl font-bold text-foreground">{stats.inProgress}</p>
                </div>
              </CardContent>
            </Card>
            <Card className="bg-success/5 border-success/20">
              <CardContent className="p-6 flex items-center gap-4">
                <div className="p-3 bg-success/10 rounded-xl">
                  <Trophy className="h-6 w-6 text-success" />
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Completed</p>
                  <p className="text-3xl font-bold text-foreground">{stats.completed}</p>
                </div>
              </CardContent>
            </Card>
          </div>
        )}

        {!enrollments || enrollments.length === 0 ? (
          <div className="mt-12">
            <EmptyState
              icon={<GraduationCap className="h-14 w-14 text-muted-foreground/60" />}
              title="No enrollments yet"
              description="Browse our collection of courses and start learning today."
              action={
                <Button onClick={() => navigate('/courses')} size="lg">
                  Explore Courses
                </Button>
              }
            />
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {enrollments.map((enrollment: any) => {
              const progress = enrollment.progressPercentage ?? 0;
              const isActive = enrollment.status === EnrollmentStatus.Active;
              const isCompleted = enrollment.status === EnrollmentStatus.Completed;
              const isDropped = enrollment.status === EnrollmentStatus.Dropped;

              return (
                <Card
                  key={enrollment.id}
                  className="flex flex-col hover:shadow-lg transition-all duration-300 border border-border group"
                >
                  <CardContent className="p-6 flex-1 flex flex-col">
                    <div className="flex justify-between items-start gap-4 mb-4">
                      <h3 className="font-semibold text-lg line-clamp-2 group-hover:text-primary transition-colors">
                        {enrollment.courseTitle}
                      </h3>
                      <Badge variant={statusVariant(enrollment.status)} className="shrink-0">
                        {statusLabel(enrollment.status)}
                      </Badge>
                    </div>

                    <div className="space-y-2 text-sm text-muted-foreground mb-6">
                      <div className="flex items-center gap-2">
                        <BookOpen className="h-4 w-4" />
                        <span>
                          {enrollment.completedLectures} / {enrollment.totalLectures} lectures
                        </span>
                      </div>
                      {enrollment.enrolledAt && (
                        <div className="flex items-center gap-2">
                          <CalendarDays className="h-4 w-4" />
                          <span>Enrolled: {formatDate(enrollment.enrolledAt)}</span>
                        </div>
                      )}
                      {enrollment.lastAccessedAt && (
                        <div className="flex items-center gap-2">
                          <Clock className="h-4 w-4" />
                          <span>Accessed: {formatDate(enrollment.lastAccessedAt)}</span>
                        </div>
                      )}
                    </div>

                    <div className="mt-auto mb-6">
                      <div className="flex items-center justify-between mb-2">
                        <span className="text-xs font-semibold text-muted-foreground">Course Progress</span>
                        <span
                          className={`text-xs font-bold ${
                            isCompleted ? 'text-success' : progress > 0 ? 'text-primary' : 'text-muted-foreground'
                          }`}
                        >
                          {progress}%
                        </span>
                      </div>
                      <ProgressBar 
                        progress={progress} 
                        fillClassName={isCompleted ? '!bg-success' : ''} 
                        className="h-2"
                      />
                    </div>

                    <div className="flex items-center justify-between mt-2 pt-4 border-t border-border">
                      <div className="flex-1">
                        {!isDropped && isActive && (
                          <Button
                            className="w-full shadow-sm"
                            onClick={() => navigate(`/courses/${enrollment.courseId}/learn`)}
                          >
                            <PlayCircle className="h-4 w-4 mr-2" />
                            Continue Learning
                          </Button>
                        )}
                        {!isDropped && isCompleted && (
                          <Button
                            variant="secondary"
                            className="w-full"
                            onClick={() => navigate(`/courses/${enrollment.courseId}/learn`)}
                          >
                            <BookOpen className="h-4 w-4 mr-2" />
                            Review Course
                          </Button>
                        )}
                      </div>

                      {isActive && (
                        <Button
                          variant="ghost"
                          size="icon"
                          className="ml-2 text-muted-foreground/60 hover:text-destructive hover:bg-destructive/10 transition-colors"
                          title="Drop Course"
                          onClick={() =>
                            setUnenrollTarget({
                              courseId: enrollment.courseId,
                              title: enrollment.courseTitle,
                            })
                          }
                        >
                          <LogOut className="h-4 w-4" />
                        </Button>
                      )}
                    </div>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        )}
      </div>

      {/* Unenroll Confirmation Modal */}
      <Modal
        open={!!unenrollTarget}
        onClose={() => setUnenrollTarget(null)}
        title="Drop Course"
        description="Are you sure you want to drop this course?"
      >
        <div className="space-y-6 mt-2">
          <div className="flex items-start gap-4 p-4 rounded-xl bg-destructive/10 border border-destructive/20">
            <div className="p-2 bg-destructive/20 rounded-full shrink-0">
              <AlertTriangle className="h-5 w-5 text-destructive" />
            </div>
            <div>
              <p className="font-semibold text-destructive mb-1">
                You are about to drop:
              </p>
              <p className="font-bold text-foreground text-lg mb-2">
                {unenrollTarget?.title}
              </p>
              <p className="text-sm text-destructive/80 leading-relaxed">
                Dropping out means you'll lose access to course materials. Depending on your time of enrollment and progress, a refund policy may apply.
              </p>
            </div>
          </div>

          <div className="flex items-center justify-end gap-3 pt-4 border-t border-border/50">
            <Button
              variant="ghost"
              onClick={() => setUnenrollTarget(null)}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              loading={unenrollMutation.isPending}
              onClick={() =>
                unenrollTarget && unenrollMutation.mutate(unenrollTarget.courseId)
              }
            >
              Confirm Drop
            </Button>
          </div>
        </div>
      </Modal>
    </AnimatedPage>
  );
}
