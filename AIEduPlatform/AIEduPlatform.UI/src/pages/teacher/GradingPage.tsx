import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { submissionsApi } from '@/api/submissions.api';
import { gradesApi } from '@/api/grades.api';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Textarea } from '@/components/ui/Textarea';
import { PageSpinner } from '@/components/ui/Spinner';
import { EmptyState } from '@/components/ui/Feedback';
import { Modal } from '@/components/ui/Modal';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { useState, useEffect, useMemo } from 'react';
import { toast } from 'sonner';
import {
  ClipboardList,
  Check,
  CheckCircle2,
  XCircle,
  Eye,
  User,
  Calendar,
  AlertTriangle,
} from 'lucide-react';
import { formatDate } from '@/utils/formatters';
import type { SubmissionAnswerDto, QuestionType } from '@/types';

export default function GradingPage() {
  const queryClient = useQueryClient();
  const [gradeSubId, setGradeSubId] = useState<string | null>(null);
  const [viewSubId, setViewSubId] = useState<string | null>(null);
  const [questionGrades, setQuestionGrades] = useState<Record<string, number>>({});
  const [feedback, setFeedback] = useState('');

  // Reset question grades when opening a new submission
  useEffect(() => {
    if (gradeSubId) {
      setQuestionGrades({});
      setFeedback('');
    }
  }, [gradeSubId]);

  const { data: ungraded, isLoading: ungradedLoading } = useQuery({
    queryKey: ['ungraded-submissions'],
    queryFn: () => submissionsApi.getUngraded(),
    select: (res) => res.data.data,
  });

  const { data: pendingApproval, isLoading: pendingLoading } = useQuery({
    queryKey: ['pending-approval'],
    queryFn: () => gradesApi.getPendingApproval(),
    select: (res) => res.data.data,
  });

  const activeSubId = viewSubId || gradeSubId;
  const { data: submissionDetail, isLoading: detailLoading } = useQuery({
    queryKey: ['submission-detail', activeSubId],
    queryFn: () => submissionsApi.getById(activeSubId!),
    enabled: !!activeSubId,
    select: (res) => res.data.data,
  });

  const invalidateAll = () => {
    queryClient.invalidateQueries({ queryKey: ['ungraded-submissions'] });
    queryClient.invalidateQueries({ queryKey: ['pending-approval'] });
  };

  // Check if a question type is objective (auto-graded)
  const isObjectiveType = (type: string | QuestionType): boolean => {
    const t = typeof type === 'string' ? type : String(type);
    return ['MultipleChoice', 'TrueFalse', 'FillInTheBlank'].includes(t);
  };

  // Calculate scores for display
  const gradeCalculation = useMemo(() => {
    if (!submissionDetail?.answers) return { total: 0, max: 0, percentage: 0 };

    let total = 0;
    let max = 0;

    for (const a of submissionDetail.answers) {
      max += a.points;

      if (isObjectiveType(a.questionType)) {
        // Auto-calculate objective questions
        const isCorrect = a.answer?.trim().toLowerCase() === a.correctAnswer?.trim().toLowerCase();
        if (isCorrect) total += a.points;
      } else {
        // Use teacher's grade for written questions
        const grade = questionGrades[a.questionId] ?? 0;
        total += Math.min(grade, a.points);
      }
    }

    return {
      total,
      max,
      percentage: max > 0 ? (total / max) * 100 : 0,
    };
  }, [submissionDetail?.answers, questionGrades]);

  const manualGradeMutation = useMutation({
    mutationFn: () =>
      gradesApi.gradeManual(gradeSubId!, { feedback, questionGrades }),
    onSuccess: () => {
      toast.success('Graded successfully');
      invalidateAll();
      setGradeSubId(null);
    },
    onError: () => toast.error('Failed to grade'),
  });

  const approveMutation = useMutation({
    mutationFn: (gradeId: string) => gradesApi.approve(gradeId),
    onSuccess: () => {
      toast.success('Grade approved');
      invalidateAll();
    },
    onError: () => toast.error('Failed to approve'),
  });

  if (ungradedLoading || pendingLoading) return <PageSpinner />;

  const ungradedItems = Array.isArray(ungraded) ? ungraded : ungraded?.items || [];
  const pendingItems = Array.isArray(pendingApproval) ? pendingApproval : pendingApproval?.items || [];

  const renderAnswerItem = (a: SubmissionAnswerDto) => {
    const isObjective = isObjectiveType(a.questionType);
    const isCorrect = isObjective
      ? a.answer?.trim().toLowerCase() === a.correctAnswer?.trim().toLowerCase()
      : null;

    return (
      <div key={a.questionId} className="rounded-lg border border-border p-3 space-y-2">
        <div className="flex items-start justify-between gap-2">
          <p className="text-sm font-medium flex-1">Q{a.order}. {a.questionText}</p>
          <div className="flex items-center gap-1.5 shrink-0">
            <Badge variant="outline" className="text-xs">{a.points} pts</Badge>
            {isCorrect !== null && (
              isCorrect
                ? <CheckCircle2 className="h-4 w-4 text-success" />
                : <XCircle className="h-4 w-4 text-destructive" />
            )}
          </div>
        </div>

        {a.options && a.options.length > 0 && (
          <div className="text-xs text-muted-foreground space-y-0.5 ml-2">
            {a.options.map((opt: string, i: number) => (
              <p
                key={i}
                className={`${opt === a.correctAnswer ? 'text-success font-medium' : ''} ${opt === a.answer && opt !== a.correctAnswer ? 'text-destructive line-through' : ''}`}
              >
                {String.fromCharCode(65 + i)}. {opt}
                {opt === a.correctAnswer && ' ✓'}
              </p>
            ))}
          </div>
        )}

        <div className={`grid ${isObjective && a.correctAnswer ? 'grid-cols-2' : 'grid-cols-1'} gap-3 text-sm`}>
          <div>
            <span className="text-xs text-muted-foreground block mb-0.5">Student Answer</span>
            <p className={`text-sm ${!a.answer ? 'text-muted-foreground italic' : ''}`}>
              {a.answer || 'No answer provided'}
            </p>
          </div>
          {isObjective && a.correctAnswer && !(a.options && a.options.length > 0) && (
            <div>
              <span className="text-xs text-muted-foreground block mb-0.5">Correct Answer</span>
              <p className="text-sm text-success">{a.correctAnswer}</p>
            </div>
          )}
        </div>
      </div>
    );
  };

  const renderGradingAnswerItem = (a: SubmissionAnswerDto) => {
    const isObjective = isObjectiveType(a.questionType);
    const isCorrect = isObjective
      ? a.answer?.trim().toLowerCase() === a.correctAnswer?.trim().toLowerCase()
      : null;
    const autoScore = isCorrect ? a.points : 0;

    return (
      <div key={a.questionId} className="rounded-lg border border-border p-3 space-y-2">
        <div className="flex items-start justify-between gap-2">
          <div className="flex-1">
            <p className="text-sm font-medium">Q{a.order}. {a.questionText}</p>
            <Badge variant="outline" className="text-[10px] mt-1">{a.questionType}</Badge>
          </div>
          <Badge variant={isObjective ? (isCorrect ? 'success' : 'destructive') : 'default'} className="text-xs shrink-0">
            {isObjective ? `${autoScore}/${a.points} pts` : `${a.points} pts`}
          </Badge>
        </div>

        <div className="text-sm bg-muted/30 rounded p-2">
          <span className="text-xs text-muted-foreground">Student Answer:</span>
          <p className={`mt-0.5 ${!a.answer ? 'text-muted-foreground italic' : ''}`}>
            {a.answer || 'No answer provided'}
          </p>
        </div>

        {isObjective ? (
          // Show auto-graded result for objective questions
          <div className="flex items-center gap-2 text-sm">
            {isCorrect ? (
              <div className="flex items-center gap-1 text-success">
                <CheckCircle2 className="h-4 w-4" />
                <span>Correct ({autoScore}/{a.points} pts)</span>
              </div>
            ) : (
              <div className="flex items-center gap-1 text-destructive">
                <XCircle className="h-4 w-4" />
                <span>Incorrect (0/{a.points} pts)</span>
                {a.correctAnswer && (
                  <span className="text-muted-foreground ml-2">
                    Expected: <span className="text-success">{a.correctAnswer}</span>
                  </span>
                )}
              </div>
            )}
          </div>
        ) : (
          // Show grading input for written questions
          <div className="space-y-2">
            {a.correctAnswer && (
              <div className="text-sm bg-success/10 rounded p-2">
                <span className="text-xs text-success">Model Answer:</span>
                <p className="mt-0.5 text-success">{a.correctAnswer}</p>
              </div>
            )}
            <div className="flex items-center gap-2">
              <Input
                type="number"
                min={0}
                max={a.points}
                step={0.5}
                className="w-24"
                placeholder="0"
                value={questionGrades[a.questionId] ?? ''}
                onChange={(e) => {
                  const val = parseFloat(e.target.value) || 0;
                  setQuestionGrades(prev => ({
                    ...prev,
                    [a.questionId]: Math.min(Math.max(0, val), a.points)
                  }));
                }}
              />
              <span className="text-sm text-muted-foreground">/ {a.points} pts</span>
            </div>
          </div>
        )}
      </div>
    );
  };

  return (
    <AnimatedPage>
      <div className="max-w-5xl mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold mb-8">Grading</h1>

        {/* Submissions Requiring Manual Review */}
        <section className="mb-10">
          <h2 className="text-xl font-bold mb-4">Submissions Requiring Review</h2>
          {ungradedItems.length === 0 ? (
            <EmptyState
              icon={<ClipboardList className="h-12 w-12" />}
              title="All caught up!"
              description="No submissions require manual review"
            />
          ) : (
            <div className="space-y-3">
              {ungradedItems.map((sub: any) => (
                <Card key={sub.id}>
                  <CardContent className="p-4 flex items-center justify-between gap-4">
                    <div className="min-w-0 flex-1">
                      <div className="flex items-center gap-2">
                        <h3 className="font-semibold truncate">{sub.examTitle || 'Exam'}</h3>
                        <Badge variant="outline" className="text-xs flex items-center gap-1">
                          <AlertTriangle className="h-3 w-3" />
                          Needs Review
                        </Badge>
                      </div>
                      <p className="text-sm text-muted-foreground flex items-center gap-3 flex-wrap mt-0.5">
                        <span className="flex items-center gap-1">
                          <User className="h-3.5 w-3.5" />
                          {sub.studentName || sub.studentId?.slice(0, 8)}
                        </span>
                        {sub.courseName && <span>{sub.courseName}</span>}
                        <span className="flex items-center gap-1">
                          <Calendar className="h-3.5 w-3.5" />
                          {formatDate(sub.submittedAt)}
                        </span>
                      </p>
                    </div>
                    <div className="flex gap-2 shrink-0">
                      <Button variant="ghost" size="sm" onClick={() => setViewSubId(sub.id)} title="View answers">
                        <Eye className="h-4 w-4" />
                      </Button>
                      <Button size="sm" onClick={() => setGradeSubId(sub.id)}>
                        Grade
                      </Button>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </section>

        {/* Pending AI Grade Approvals */}
        <section>
          <h2 className="text-xl font-bold mb-4">Pending AI Grade Approvals</h2>
          {pendingItems.length === 0 ? (
            <p className="text-muted-foreground">No pending approvals</p>
          ) : (
            <div className="space-y-3">
              {pendingItems.map((grade: any) => (
                <Card key={grade.id}>
                  <CardContent className="p-4 flex items-center justify-between gap-4">
                    <div className="min-w-0 flex-1">
                      <div className="flex items-center gap-2">
                        <h3 className="font-semibold">AI Grade Review</h3>
                        <Badge variant="outline" className="text-xs">
                          Score: {typeof grade.score === 'number' ? `${grade.score.toFixed(1)}%` : grade.score}
                        </Badge>
                      </div>
                      <p className="text-sm text-muted-foreground mt-0.5 line-clamp-2">
                        {grade.feedback || 'No feedback'}
                      </p>
                    </div>
                    <div className="flex gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setGradeSubId(grade.submissionId)}
                      >
                        Review & Edit
                      </Button>
                      <Button
                        size="sm"
                        onClick={() => approveMutation.mutate(grade.id)}
                        loading={approveMutation.isPending && approveMutation.variables === grade.id}
                      >
                        <Check className="h-4 w-4 mr-1" /> Approve
                      </Button>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </section>

        {/* View Answers Modal */}
        <Modal
          open={!!viewSubId}
          onClose={() => setViewSubId(null)}
          title="Submission Answers"
          description={submissionDetail ? `${submissionDetail.studentName || 'Student'} · ${submissionDetail.examTitle || 'Exam'}` : ''}
          className="max-w-2xl"
        >
          {detailLoading ? (
            <div className="py-8 text-center text-muted-foreground">Loading answers...</div>
          ) : submissionDetail?.answers?.length ? (
            <div className="space-y-3 max-h-[60vh] overflow-y-auto pr-1">
              {[...submissionDetail.answers]
                .sort((a: SubmissionAnswerDto, b: SubmissionAnswerDto) => a.order - b.order)
                .map(renderAnswerItem)}
            </div>
          ) : (
            <p className="text-muted-foreground py-4">No answers found.</p>
          )}
        </Modal>

        {/* Manual Grade Modal */}
        <Modal
          open={!!gradeSubId}
          onClose={() => setGradeSubId(null)}
          title="Grade Submission"
          description={submissionDetail ? `${submissionDetail.studentName || 'Student'} · ${submissionDetail.examTitle || 'Exam'}` : ''}
          className="max-w-3xl"
        >
          {detailLoading ? (
            <div className="py-8 text-center text-muted-foreground">Loading answers...</div>
          ) : (
            <form
              onSubmit={(e) => { e.preventDefault(); manualGradeMutation.mutate(); }}
              className="space-y-5"
            >
              {submissionDetail?.answers?.length ? (
                <div className="space-y-3 max-h-[50vh] overflow-y-auto pr-1 pb-3">
                  {[...submissionDetail.answers]
                    .sort((a: SubmissionAnswerDto, b: SubmissionAnswerDto) => a.order - b.order)
                    .map(renderGradingAnswerItem)}
                </div>
              ) : null}

              {/* Score Summary */}
              <div className="border-t border-border pt-4">
                <div className="flex items-center justify-between text-lg font-semibold">
                  <span>Total Score:</span>
                  <span className={gradeCalculation.percentage >= 50 ? 'text-success' : 'text-destructive'}>
                    {gradeCalculation.total.toFixed(1)} / {gradeCalculation.max} pts ({gradeCalculation.percentage.toFixed(1)}%)
                  </span>
                </div>
              </div>

              <Textarea
                label="Feedback"
                placeholder="Provide constructive feedback for the student..."
                hint="Detailed feedback helps students understand their performance."
                value={feedback}
                onChange={(e) => setFeedback(e.target.value)}
              />

              <div className="flex items-center gap-3 pt-4 border-t border-border">
                <Button type="submit" loading={manualGradeMutation.isPending}>
                  Submit Grade
                </Button>
                <Button variant="outline" type="button" onClick={() => setGradeSubId(null)}>Cancel</Button>
              </div>
            </form>
          )}
        </Modal>
      </div>
    </AnimatedPage>
  );
}
