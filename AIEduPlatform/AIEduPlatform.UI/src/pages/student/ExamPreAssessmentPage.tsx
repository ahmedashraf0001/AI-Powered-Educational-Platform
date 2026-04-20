import { useMemo } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { examsApi } from '@/api/exams.api';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { Button } from '@/components/ui/Button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { PageSpinner } from '@/components/ui/Spinner';
import { AlertTriangle, CalendarDays, Clock3, ListChecks, PlayCircle } from 'lucide-react';

function formatDate(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;

  return date.toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export default function ExamPreAssessmentPage() {
  const { examId } = useParams<{ examId: string }>();
  const navigate = useNavigate();

  const { data: exam, isLoading } = useQuery({
    queryKey: ['exam', examId],
    queryFn: () => examsApi.getById(examId!),
    enabled: !!examId,
    select: (res) => res.data.data,
  });

  const { data: totalPoints } = useQuery({
    queryKey: ['exam-total-points', examId],
    queryFn: () => examsApi.getTotalPoints(examId!),
    enabled: !!examId,
    select: (res) => res.data.data,
  });

  const now = Date.now();
  const hasStarted = useMemo(() => {
    if (!exam?.startTime) return false;
    return new Date(exam.startTime).getTime() <= now;
  }, [exam?.startTime, now]);

  const hasEnded = useMemo(() => {
    if (!exam?.endTime) return false;
    return new Date(exam.endTime).getTime() < now;
  }, [exam?.endTime, now]);

  const hasSubmitted = Boolean(exam?.hasSubmitted);
  const canStart = Boolean(exam) && hasStarted && !hasEnded && !hasSubmitted;

  if (isLoading) {
    return <PageSpinner />;
  }

  if (!exam) {
    return (
      <AnimatedPage>
        <div className="max-w-2xl mx-auto px-4 py-12">
          <Card>
            <CardContent className="p-8 text-center space-y-4">
              <h1 className="text-2xl font-bold">Exam not found</h1>
              <p className="text-muted-foreground">This exam may no longer be available.</p>
              <Button onClick={() => navigate('/dashboard')}>Back to Dashboard</Button>
            </CardContent>
          </Card>
        </div>
      </AnimatedPage>
    );
  }

  return (
    <AnimatedPage>
      <div className="max-w-4xl mx-auto px-4 py-8 space-y-6">
        <div>
          <p className="text-sm font-medium text-primary uppercase tracking-wide">Pre Assessment</p>
          <h1 className="text-3xl font-bold mt-1">{exam.title}</h1>
          <p className="text-muted-foreground mt-2">
            Review exam details before you begin.
          </p>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Exam Overview</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="rounded-lg border p-4 space-y-1">
              <div className="text-xs text-muted-foreground flex items-center gap-2">
                <Clock3 className="h-4 w-4" /> Duration
              </div>
              <div className="text-lg font-semibold">{exam.durationMinutes} minutes</div>
            </div>
            <div className="rounded-lg border p-4 space-y-1">
              <div className="text-xs text-muted-foreground flex items-center gap-2">
                <ListChecks className="h-4 w-4" /> Questions
              </div>
              <div className="text-lg font-semibold">{exam.questions?.length ?? 0}</div>
            </div>
            <div className="rounded-lg border p-4 space-y-1">
              <div className="text-xs text-muted-foreground flex items-center gap-2">
                <CalendarDays className="h-4 w-4" /> Start time
              </div>
              <div className="text-sm font-medium">{formatDate(exam.startTime)}</div>
            </div>
            <div className="rounded-lg border p-4 space-y-1">
              <div className="text-xs text-muted-foreground flex items-center gap-2">
                <CalendarDays className="h-4 w-4" /> End time
              </div>
              <div className="text-sm font-medium">{formatDate(exam.endTime)}</div>
            </div>
            <div className="rounded-lg border p-4 space-y-1 sm:col-span-2">
              <div className="text-xs text-muted-foreground">Total points</div>
              <div className="text-lg font-semibold">{totalPoints ?? '—'}</div>
            </div>
          </CardContent>
        </Card>

        {(hasSubmitted || hasEnded || !hasStarted) && (
          <Card>
            <CardContent className="p-4 text-sm flex items-start gap-3">
              <AlertTriangle className="h-4 w-4 mt-0.5 text-warning" />
              <div className="space-y-1">
                {hasSubmitted && <p>You have already submitted this exam.</p>}
                {!hasSubmitted && hasEnded && <p>This exam window has ended.</p>}
                {!hasSubmitted && !hasEnded && !hasStarted && (
                  <p>This exam is not open yet. You can start when the start time is reached.</p>
                )}
              </div>
            </CardContent>
          </Card>
        )}

        <div className="flex flex-wrap gap-3">
          <Button
            onClick={() => navigate(`/exams/${exam.id}/take`)}
            disabled={!canStart}
            className="min-w-40"
          >
            <PlayCircle className="h-4 w-4 mr-2" />
            Start Exam
          </Button>
          <Button
            variant="outline"
            onClick={() => navigate(`/courses/${exam.courseId}/learn`)}
          >
            Back to Course
          </Button>
        </div>
      </div>
    </AnimatedPage>
  );
}
