import { useQuery } from '@tanstack/react-query';
import { submissionsApi } from '@/api/submissions.api';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Pagination } from '@/components/ui/Pagination';
import { PageSpinner, Spinner } from '@/components/ui/Spinner';
import { EmptyState } from '@/components/ui/Feedback';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { Modal } from '@/components/ui/Modal';
import { useState } from 'react';
import {
  FileText,
  Calendar,
  BookOpen,
  Trophy,
  Eye,
  CheckCircle2,
  XCircle,
  MessageSquare,
  Sparkles,
} from 'lucide-react';
import { formatDate } from '@/utils/formatters';
import type { SubmissionDetailDto, SubmissionAnswerDto } from '@/types';

export default function MySubmissionsPage() {
  const [page, setPage] = useState(1);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ['my-submissions', page],
    queryFn: () => submissionsApi.getMine({ page }),
    select: (res) => res.data.data,
  });

  const { data: detail, isLoading: detailLoading } = useQuery({
    queryKey: ['submission-detail', selectedId],
    queryFn: () => submissionsApi.getById(selectedId!),
    enabled: !!selectedId,
    select: (res) => res.data.data as SubmissionDetailDto,
  });

  if (isLoading) return <PageSpinner />;

  const isCorrect = (a: SubmissionAnswerDto) => {
    // If no correct answer available (not graded yet), return null
    if (!a.correctAnswer || a.correctAnswer.trim() === '') return null;
    return a.answer.trim().toLowerCase() === a.correctAnswer.trim().toLowerCase();
  };

  // Check if correct answers are available (submission is graded)
  const hasCorrectAnswer = (a: SubmissionAnswerDto) => {
    return a.correctAnswer && a.correctAnswer.trim() !== '';
  };

  return (
    <AnimatedPage>
      <div className="max-w-5xl mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold mb-8">My Submissions</h1>

        {!data || data.items.length === 0 ? (
          <EmptyState
            icon={<FileText className="h-12 w-12" />}
            title="No submissions yet"
            description="Take an exam to see your submissions here"
          />
        ) : (
          <>
            <div className="space-y-3">
              {data.items.map((sub: any) => (
                <Card key={sub.id} className="hover:shadow-sm transition-shadow">
                  <CardContent className="p-4 flex items-center justify-between gap-4">
                    <div className="min-w-0 flex-1">
                      <h3 className="font-semibold truncate">{sub.examTitle || 'Exam'}</h3>
                      <p className="text-sm text-muted-foreground flex items-center gap-3 flex-wrap mt-0.5">
                        {sub.courseName && (
                          <span className="flex items-center gap-1">
                            <BookOpen className="h-3.5 w-3.5" />
                            {sub.courseName}
                          </span>
                        )}
                        <span className="flex items-center gap-1">
                          <Calendar className="h-3.5 w-3.5" />
                          {formatDate(sub.submittedAt)}
                        </span>
                      </p>
                    </div>
                    <div className="flex items-center gap-3 shrink-0">
                      {sub.isGraded && sub.score != null && (
                        <span className="flex items-center gap-1 text-sm font-semibold">
                          <Trophy className="h-4 w-4 text-warning" />
                          {sub.score.toFixed(1)}%
                        </span>
                      )}
                      <Badge variant={sub.isGraded ? 'success' : 'outline'}>
                        {sub.isGraded ? 'Graded' : 'Pending'}
                      </Badge>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => setSelectedId(sub.id)}
                      >
                        <Eye className="h-4 w-4" />
                      </Button>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
            <div className="mt-6">
              <Pagination
                page={data.page}
                totalPages={data.totalPages}
                onPageChange={setPage}
                hasPrevious={data.page > 1}
                hasNext={data.page < data.totalPages}
              />
            </div>
          </>
        )}
      </div>

      {/* Submission Detail Modal */}
      <Modal
        open={!!selectedId}
        onClose={() => setSelectedId(null)}
        title={detail?.examTitle || 'Submission Details'}
        description={detail?.courseName}
        className="max-w-2xl max-h-[85vh] flex flex-col"
      >
        {detailLoading ? (
          <div className="flex justify-center py-8">
            <Spinner />
          </div>
        ) : detail ? (
          <div className="space-y-4 overflow-y-auto max-h-[60vh] pr-1">
            {/* Grade Summary */}
            {detail.grade && (
              <div className="p-3 rounded-lg border bg-muted/30">
                <div className="flex items-center justify-between mb-2">
                  <span className="font-semibold flex items-center gap-1.5">
                    <Trophy className="h-4 w-4 text-warning" />
                    Score: {detail.grade.score.toFixed(1)}%
                  </span>
                  <div className="flex items-center gap-2">
                    {detail.grade.isAiGraded && (
                      <Badge variant="outline" className="text-xs">
                        <Sparkles className="h-3 w-3 mr-1" /> AI Graded
                      </Badge>
                    )}
                    <Badge variant={detail.grade.isApproved ? 'success' : 'outline'} className="text-xs">
                      {detail.grade.isApproved ? 'Approved' : 'Pending Approval'}
                    </Badge>
                  </div>
                </div>
                {detail.grade.feedback && (
                  <p className="text-sm text-muted-foreground flex items-start gap-1.5">
                    <MessageSquare className="h-3.5 w-3.5 mt-0.5 shrink-0" />
                    {detail.grade.feedback}
                  </p>
                )}
              </div>
            )}

            {/* Answers */}
            {detail.answers.map((a, i) => {
              const correct = isCorrect(a);
              const showCorrect = hasCorrectAnswer(a);
              return (
                <div key={a.questionId} className="p-3 rounded-lg border">
                  <div className="flex items-start gap-2 mb-2">
                    <span className="text-xs font-bold text-muted-foreground mt-0.5">
                      Q{i + 1}
                    </span>
                    <div className="flex-1">
                      <p className="text-sm font-medium">{a.questionText}</p>
                      <div className="flex items-center gap-2 mt-0.5">
                        <Badge variant="outline" className="text-[10px]">
                          {a.questionType}
                        </Badge>
                        <span className="text-[10px] text-muted-foreground">
                          {a.points} pt{a.points !== 1 ? 's' : ''}
                        </span>
                      </div>
                    </div>
                  </div>

                  {/* MCQ options */}
                  {a.options.length > 0 ? (
                    <div className="space-y-1 ml-6">
                      {a.options.map((opt, oi) => {
                        const isStudentAnswer = a.answer === opt;
                        const isCorrectAnswer = showCorrect && a.correctAnswer === opt;
                        return (
                          <div
                            key={oi}
                            className={`text-sm px-2.5 py-1.5 rounded flex items-center gap-2 ${
                              showCorrect && isCorrectAnswer
                                ? 'bg-success/10 text-success'
                                : isStudentAnswer && showCorrect && !isCorrectAnswer
                                  ? 'bg-destructive/10 text-destructive line-through'
                                  : isStudentAnswer && !showCorrect
                                    ? 'bg-primary/10 border border-primary/20'
                                    : 'text-muted-foreground'
                            }`}
                          >
                            {showCorrect && isCorrectAnswer && <CheckCircle2 className="h-3.5 w-3.5 shrink-0" />}
                            {showCorrect && isStudentAnswer && !isCorrectAnswer && (
                              <XCircle className="h-3.5 w-3.5 shrink-0" />
                            )}
                            <span>{opt}</span>
                            {isStudentAnswer && !showCorrect && (
                              <span className="text-[10px] text-muted-foreground ml-auto">(your answer)</span>
                            )}
                          </div>
                        );
                      })}
                    </div>
                  ) : (
                    <div className="ml-6 space-y-1.5">
                      <div className="text-sm">
                        <span className="text-xs text-muted-foreground">Your answer: </span>
                        <span className={correct === false ? 'text-destructive' : correct === true ? 'text-success' : ''}>
                          {a.answer || '(no answer)'}
                        </span>
                      </div>
                      {showCorrect && a.correctAnswer && (
                        <div className="text-sm">
                          <span className="text-xs text-muted-foreground">Correct answer: </span>
                          <span className="text-success">{a.correctAnswer}</span>
                        </div>
                      )}
                      {!showCorrect && (
                        <p className="text-xs text-muted-foreground italic">
                          Correct answer will be shown after grading
                        </p>
                      )}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        ) : null}
      </Modal>
    </AnimatedPage>
  );
}
