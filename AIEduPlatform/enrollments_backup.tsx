import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { coursesApi } from '@/api/courses.api';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { Modal } from '@/components/ui/Modal';
import { ProgressBar } from '@/components/ui/ProgressBar';
import { useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { toast } from 'sonner';
import Skeleton from 'react-loading-skeleton';
import {
  BookOpen,
  AlertTriangle,
  XCircle,
} from 'lucide-react';
import { EnrollmentStatus } from '@/types';
import { resolveUrl } from '@/utils/url';

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
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
  });

  const { data: enrollments, isLoading } = useQuery({
    queryKey: ['enrolled-courses'],
    queryFn: () => coursesApi.getEnrolled(),
    select: (res) => res.data.data?.items ?? [],
  });

  if (isLoading) {
    return (
      <AnimatedPage>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="mb-8 max-w-md">
            <Skeleton height={36} borderRadius={8} />
            <div className="mt-2">
              <Skeleton height={18} borderRadius={8} />
            </div>
          </div>
          <div className="grid gap-4" style={{ gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))' }}>
            {Array.from({ length: 4 }).map((_, i) => (
              <Skeleton key={i} height={320} borderRadius={12} />
            ))}
          </div>
        </div>
      </AnimatedPage>
    );
  }

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
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Page Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold">My Enrollments</h1>
          <p className="text-muted-foreground mt-1">
            Track your courses and continue where you left off
          </p>
        </div>

        {!enrollments || enrollments.length === 0 ? (
          <div style={{ textAlign: 'center', padding: '64px 0', color: '#9ca3af' }}>
            <img
              src="/placeholders/empty-state.svg"
              alt="No courses"
              style={{ width: 120, margin: '0 auto 16px auto' }}
            />
            <p style={{ fontSize: '1.1rem' }}>You haven't enrolled in any courses yet.</p>
            <div className="mt-6">
              <Button onClick={() => navigate('/courses')}>Browse Courses</Button>
            </div>
          </div>
        ) : (
          <div
            className="grid gap-4"
            style={{ gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))' }}
          >
            {enrollments.map((enrollment: any) => {
              const totalLectures =
                enrollment.totalLectures
                ?? enrollment.totalLessons
                ?? enrollment.lectureCount
                ?? enrollment.progress?.totalLectures
                ?? enrollment.progress?.totalLessons
                ?? 0;
              const completedLectures =
                enrollment.completedLectures
                ?? enrollment.completedLessons
                ?? enrollment.progress?.completedLectures
                ?? enrollment.progress?.completedLessons
                ?? 0;
              const progressValue =
                enrollment.progressPercentage ??
                enrollment.progress?.progressPercentage ??
                (totalLectures > 0 ? (completedLectures / totalLectures) * 100 : 0);
              const progress = Number.isFinite(progressValue)
                ? Math.max(0, Math.min(100, Math.round(progressValue)))
                : 0;
              const isActive = enrollment.status === EnrollmentStatus.Active;
              const isCompleted =
                enrollment.status === EnrollmentStatus.Completed;
              const isDropped = enrollment.status === EnrollmentStatus.Dropped;
              const thumbnailUrl = resolveUrl(
                enrollment.thumbnailUrl ?? enrollment.courseThumbnailUrl ?? enrollment.course?.thumbnailUrl
              ) ?? '/placeholders/course-thumbnail.svg';
              const instructorName = [
                enrollment.teacherName,
                enrollment.instructorName,
                enrollment.course?.teacherName,
              ].find((name) => typeof name === 'string' && name.trim().length > 0)
                ?? 'Instructor unavailable';

              return (
                <Card
                  key={enrollment.id}
                  className="group cursor-pointer overflow-hidden transition-all duration-200 hover:shadow-lg hover:-translate-y-0.5"
                  onClick={() => navigate(`/courses/${enrollment.courseId}`)}
                >
                  <img
                    src={thumbnailUrl}
                    alt={enrollment.courseTitle}
                    className="h-40 w-full object-cover transition-transform duration-300 group-hover:scale-[1.03]"
                    onError={(event) => {
                      event.currentTarget.src = '/placeholders/course-thumbnail.svg';
                    }}
                  />
                  <CardContent className="p-4 space-y-3">
                    <div className="flex flex-wrap items-center gap-2">
                      <h3 className="font-semibold text-base line-clamp-2">{enrollment.courseTitle}</h3>
                      <Badge variant={statusVariant(enrollment.status)}>{statusLabel(enrollment.status)}</Badge>
                    </div>

                    <p className="text-sm text-muted-foreground">{instructorName}</p>

                    <div className="space-y-1.5">
                      <div className="flex items-center justify-between text-xs font-medium">
                        <span className="text-muted-foreground">Progress</span>
                        <span className={isCompleted ? 'text-success' : 'text-primary'}>{progress}%</span>
                      </div>
                      <ProgressBar
                        progress={progress}
                        className="h-2.5"
                        fillClassName={isCompleted ? 'bg-success' : 'bg-primary'}
                      />
                    </div>

                    <div className="flex items-center justify-between text-sm text-muted-foreground">
                      <span className="inline-flex items-center gap-1">
                        <BookOpen className="h-3.5 w-3.5" />
                        {completedLectures}/{totalLectures} lectures
                      </span>
                      {enrollment.enrolledAt && <span>Enrolled {formatDate(enrollment.enrolledAt)}</span>}
                    </div>

                    {isActive && !isDropped && (
                      <Button
                        variant="ghost"
                        size="sm"
                        className="text-destructive hover:text-destructive hover:bg-destructive/10"
                        onClick={(event) => {
                          event.stopPropagation();
                          setUnenrollTarget({
                            courseId: enrollment.courseId,
                            title: enrollment.courseTitle,
                          });
                        }}
                      >
                        <XCircle className="h-4 w-4 mr-1.5" />
                        Unenroll
                      </Button>
                    )}
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

