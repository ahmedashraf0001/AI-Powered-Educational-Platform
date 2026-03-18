import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { coursesApi } from '@/api/courses.api';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { PageSpinner } from '@/components/ui/Spinner';
import { EmptyState } from '@/components/ui/Feedback';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { Modal } from '@/components/ui/Modal';
import { useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { toast } from 'sonner';
import {
  GraduationCap,
  BookOpen,
  PlayCircle,
  CalendarDays,
  Clock,
  AlertTriangle,
  XCircle,
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

  if (isLoading) return <PageSpinner />;

  const statusLabel = (status: EnrollmentStatus) => {
    switch (status) {
      case EnrollmentStatus.Active:
        return 'Active';
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
      <div className="max-w-5xl mx-auto px-4 py-8">
        {/* Page Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold">My Enrollments</h1>
          <p className="text-muted-foreground mt-1">
            Track your courses and continue where you left off
          </p>
        </div>

        {!enrollments || enrollments.length === 0 ? (
          <EmptyState
            icon={<GraduationCap className="h-12 w-12" />}
            title="No enrollments yet"
            description="Browse courses and start learning"
            action={
              <Button onClick={() => navigate('/courses')}>
                Browse Courses
              </Button>
            }
          />
        ) : (
          <div className="space-y-4">
            {enrollments.map((enrollment: any) => {
              const progress = enrollment.progressPercentage ?? 0;
              const isActive = enrollment.status === EnrollmentStatus.Active;
              const isCompleted =
                enrollment.status === EnrollmentStatus.Completed;
              const isDropped = enrollment.status === EnrollmentStatus.Dropped;

              return (
                <Card
                  key={enrollment.id}
                  className="hover:shadow-md transition-shadow"
                >
                  <CardContent className="p-5">
                    <div className="flex flex-col sm:flex-row sm:items-start gap-4">
                      {/* Course Info */}
                      <div className="flex-1 min-w-0">
                        <div className="flex flex-wrap items-center gap-2 mb-2">
                          <h3 className="font-semibold text-base truncate">
                            {enrollment.courseTitle}
                          </h3>
                          <Badge variant={statusVariant(enrollment.status)}>
                            {statusLabel(enrollment.status)}
                          </Badge>
                        </div>

                        {/* Meta Info */}
                        <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-sm text-muted-foreground mb-3">
                          <span className="inline-flex items-center gap-1">
                            <BookOpen className="h-3.5 w-3.5" />
                            {enrollment.completedLectures}/
                            {enrollment.totalLectures} lectures
                          </span>
                          {enrollment.enrolledAt && (
                            <span className="inline-flex items-center gap-1">
                              <CalendarDays className="h-3.5 w-3.5" />
                              Enrolled {formatDate(enrollment.enrolledAt)}
                            </span>
                          )}
                          {enrollment.lastAccessedAt && (
                            <span className="inline-flex items-center gap-1">
                              <Clock className="h-3.5 w-3.5" />
                              Last accessed{' '}
                              {formatDate(enrollment.lastAccessedAt)}
                            </span>
                          )}
                        </div>

                        {/* Progress Bar */}
                        <div className="max-w-md">
                          <div className="flex items-center justify-between mb-1.5">
                            <span className="text-xs font-medium text-muted-foreground">
                              Progress
                            </span>
                            <span
                              className={`text-xs font-semibold ${
                                isCompleted
                                  ? 'text-success'
                                  : progress > 0
                                    ? 'text-primary'
                                    : 'text-muted-foreground'
                              }`}
                            >
                              {progress}%
                            </span>
                          </div>
                          <div className="w-full h-2.5 bg-secondary rounded-full overflow-hidden">
                            <div
                              className={`h-full rounded-full transition-all duration-500 ${
                                isCompleted
                                  ? 'bg-success'
                                  : 'bg-primary'
                              }`}
                              style={{ width: `${progress}%` }}
                            />
                          </div>
                        </div>
                      </div>

                      {/* Action Buttons */}
                      <div className="flex items-center gap-2 sm:flex-col sm:items-end shrink-0">
                        {!isDropped && (
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() =>
                              navigate(`/courses/${enrollment.courseId}/learn`)
                            }
                          >
                            <BookOpen className="h-4 w-4 mr-1.5" />
                            Go to Course
                          </Button>
                        )}

                        {isActive && (
                          <Button
                            size="sm"
                            onClick={() =>
                              navigate(
                                `/courses/${enrollment.courseId}/learn`
                              )
                            }
                          >
                            <PlayCircle className="h-4 w-4 mr-1.5" />
                            Continue
                          </Button>
                        )}

                        {isActive && (
                          <Button
                            variant="ghost"
                            size="sm"
                            className="text-destructive hover:text-destructive hover:bg-destructive/10"
                            onClick={() =>
                              setUnenrollTarget({
                                courseId: enrollment.courseId,
                                title: enrollment.courseTitle,
                              })
                            }
                          >
                            <XCircle className="h-4 w-4 mr-1.5" />
                            Unenroll
                          </Button>
                        )}
                      </div>
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
        title="Confirm Unenroll"
        description="This action cannot be undone."
      >
        <div className="space-y-5">
          <div className="flex items-start gap-3 p-4 rounded-lg bg-destructive/5 border border-destructive/20">
            <AlertTriangle className="h-5 w-5 text-destructive shrink-0 mt-0.5" />
            <div className="text-sm">
              <p className="font-medium text-destructive mb-1">
                You are about to unenroll from:
              </p>
              <p className="font-semibold text-foreground">
                {unenrollTarget?.title}
              </p>
              <p className="text-muted-foreground mt-2">
                Your progress will be permanently lost and you will need to
                re-enroll if you want to access this course again.
              </p>
            </div>
          </div>

          <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
            <Button
              variant="outline"
              className="flex-1"
              onClick={() => setUnenrollTarget(null)}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              className="flex-1"
              loading={unenrollMutation.isPending}
              onClick={() =>
                unenrollTarget &&
                unenrollMutation.mutate(unenrollTarget.courseId)
              }
            >
              Unenroll
            </Button>
          </div>
        </div>
      </Modal>
    </AnimatedPage>
  );
}
