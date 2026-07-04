import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { gradesApi } from '@/api/grades.api';
import { submissionsApi } from '@/api/submissions.api';
import { Card } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { PageSpinner, Spinner } from '@/components/ui/Spinner';
import { EmptyState } from '@/components/ui/Feedback';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { BookOpen, Bot, User, CheckCircle, Clock, ChevronDown, ChevronUp, Search, XCircle, Trophy, CheckCircle2, AlertCircle } from 'lucide-react';
import type { QuestionResultDto } from '@/types';
import { cn } from '@/utils/cn';

function SubmissionDetailsModal({ submissionId, isOpen, onClose }: { submissionId: string | null; isOpen: boolean; onClose: () => void }) {
  const { data: submission, isLoading, isError } = useQuery({
    queryKey: ['submission-details', submissionId],
    queryFn: () => submissionId ? submissionsApi.getById(submissionId) : Promise.reject('No ID'),
    select: (res) => res.data.data,
    enabled: !!submissionId && isOpen,
  });

  return (
    <Modal
      open={isOpen}
      onClose={onClose}
      title="Detailed Grade Report"
      description={submission ? `${submission.examTitle} - ${submission.courseName}` : undefined}
      className="max-w-4xl"
    >
      <div className="p-6 overflow-y-auto min-h-[50vh] max-h-[70vh]">
        {isLoading ? (
          <div className="flex flex-col items-center justify-center py-12">
            <Spinner size="lg" />
            <p className="text-muted-foreground mt-4">Loading report details...</p>
          </div>
        ) : isError || !submission ? (
          <div className="text-center py-12">
            <XCircle className="h-12 w-12 mx-auto text-destructive mb-3" />
            <p className="text-destructive font-medium">Failed to load submission details.</p>
          </div>
        ) : (
          <div className="space-y-6">
            <div className="flex flex-wrap items-center justify-between gap-4 bg-muted/30 p-4 rounded-xl mb-6">
              <div>
                <p className="text-sm text-muted-foreground uppercase tracking-wider mb-1 font-semibold">Total Score</p>
                <div className="text-2xl font-bold">{submission.grade?.score?.toFixed(1) ?? 'N/A'}%</div>
              </div>
              <div className="text-right">
                <p className="text-sm text-muted-foreground uppercase tracking-wider mb-1 font-semibold">Graded Items</p>
                <div className="text-xl font-semibold">{submission.answers.length} <span className="text-muted-foreground text-sm font-medium">Questions</span></div>
              </div>
            </div>

            <div className="space-y-4">
              {submission.answers.map((a, i) => {
                // Check structured questionResults first (AI-graded ShortAnswer/Essay)
                const questionResult = submission.grade?.questionResults?.find(
                  (qr: QuestionResultDto) => qr.questionId === a.questionId
                );

                let correct: boolean;
                let isPartial = false;
                let qFeedback: string | null = null;
                let qScore: string | null = null;

                if (questionResult) {
                  const s = questionResult.score;
                  const m = questionResult.maxScore;
                  correct = s >= m;
                  isPartial = s > 0 && s < m;
                  qScore = s.toString();
                  qFeedback = questionResult.feedback;
                } else {
                  correct = a.answer?.trim().toLowerCase() === a.correctAnswer?.trim().toLowerCase();

                  if (submission.grade?.feedback) {
                    const prefixMatch = `Q${i + 1} | `;
                    const prefixMatchOld = `Q${i + 1}: `;
                    const lines = submission.grade.feedback.split('\n');
                    const matchLine = lines.find(line => line.trim().startsWith(prefixMatch) || line.trim().startsWith(prefixMatchOld));
                    if (matchLine) {
                      if (matchLine.trim().startsWith(prefixMatch)) {
                        const parts = matchLine.split(' | ');
                        if (parts.length >= 3) {
                          qScore = parts[1].replace('Score:', '').split('/')[0].trim();
                          qFeedback = parts.slice(2).join(' | ').trim();
                        }
                      } else {
                        qFeedback = matchLine.trim().replace(prefixMatchOld, '').trim();

                        // Fallback inference for old grades
                        if (qFeedback === 'Correct!') qScore = a.points.toString();
                        if (qFeedback.startsWith('Incorrect') || qFeedback.includes('failed')) qScore = '0';
                      }
                    }
                  }

                  // Absolute fallback inferencing if parsing didn't work but we know correctness (like auto-graded ones)
                  if (qScore === null && correct !== null) {
                    qScore = correct ? a.points.toString() : '0';
                  }
                }

                return (
                  <div key={a.questionId} className="p-4 rounded-lg border border-border/60 bg-card">
                    <div className="flex items-start gap-3 mb-4">
                      <div className="flex flex-col items-center gap-2 shrink-0">
                        <span className="text-xs font-bold text-muted-foreground mt-0.5">
                          Q{i + 1}
                        </span>
                        {correct ? (
                          <CheckCircle className="w-4 h-4 text-success" />
                        ) : isPartial ? (
                          <AlertCircle className="w-4 h-4 text-warning" />
                        ) : (
                          <XCircle className="w-4 h-4 text-destructive" />
                        )}
                      </div>
                      <div className="flex-1 w-full overflow-hidden min-w-0">
                        <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4">
                          <div className="flex-1 min-w-0">
                            <p className="text-sm font-medium break-words">{a.questionText}</p>
                            <div className="flex items-center gap-2 mt-1.5 flex-wrap">
                              <Badge variant="outline" className="text-[10px]">
                                {a.questionType}
                              </Badge>
                              <span className="text-[10px] text-muted-foreground">
                                {a.points} pt{a.points !== 1 ? 's' : ''}
                              </span>
                            </div>
                          </div>
                          
                          <div className="shrink-0 mt-1 sm:mt-0">
                            <span className="text-xs font-semibold text-muted-foreground bg-secondary/30 px-2.5 py-1.5 rounded-md border text-nowrap">
                              Score: <span className="text-foreground">{qScore} / {a.points}</span>
                            </span>
                          </div>
                        </div>
                      </div>
                    </div>

                    <div className="flex flex-col gap-3">
                      {/* Options or Answer display */}
                      <div className="w-full">
                            {a.options && a.options.length > 0 ? (
                              <div className="space-y-1.5">
                                {a.options.map((opt, oi) => {
                                  const isStudentAnswer = a.answer === opt;
                                  const isCorrectAnswer = a.correctAnswer === opt;
                                  
                                  let optionClass = "text-muted-foreground border-transparent";
                                  
                                  if (isCorrectAnswer) {
                                    optionClass = "bg-success/10 text-success";
                                  } else if (isStudentAnswer && !isCorrectAnswer) {
                                    optionClass = "bg-destructive/10 text-destructive line-through";
                                  } else if (isStudentAnswer) {
                                    optionClass = "bg-primary/10 border border-primary/20";
                                  }

                                  return (
                                    <div
                                      key={oi}
                                      className={cn("text-sm px-3 py-2 rounded-md flex items-center gap-2.5", optionClass)}
                                    >
                                      {isCorrectAnswer ? (
                                        <CheckCircle2 className="h-4 w-4 shrink-0 text-success" />
                                      ) : isStudentAnswer && !isCorrectAnswer ? (
                                        <XCircle className="h-4 w-4 shrink-0 text-destructive" />
                                      ) : (
                                        <div className="w-4 h-4 shrink-0 rounded-full border border-muted-foreground/30" />
                                      )}
                                      <span className="flex-1 break-words">{opt}</span>
                                      {isStudentAnswer && !isCorrectAnswer && (
                                        <span className="text-[10px] uppercase font-bold text-destructive/80 ml-2 shrink-0">(your answer)</span>
                                      )}
                                    </div>
                                  );
                                })}
                              </div>
                            ) : (
                              <div className="space-y-2">
                                <div className="text-sm bg-secondary/10 p-3 rounded-md border border-border/50">
                                  <span className="text-xs font-semibold text-muted-foreground uppercase tracking-wider block mb-1">Your answer: </span>
                                  <span className={cn(correct ? 'text-success font-medium' : isPartial ? 'text-warning font-medium' : 'text-destructive font-medium')}>
                                    {a.answer || '(no answer)'}
                                  </span>
                                </div>
                                {a.correctAnswer && (
                                  <div className="text-sm bg-success/5 p-3 rounded-md border border-success/20">
                                    <span className="text-xs font-semibold text-success/80 uppercase tracking-wider block mb-1">Correct answer: </span>
                                    <span className="text-success font-medium">{a.correctAnswer}</span>
                                  </div>
                                )}
                              </div>
                            )}

                            {qFeedback && (
                              <div className="mt-4 p-3 rounded-md bg-secondary/15 border-l-[3px] border-primary text-sm flex flex-col gap-1.5 overflow-hidden">
                                <span className="text-xs font-bold text-primary flex items-center gap-1.5 uppercase tracking-wider">
                                  <Trophy className="h-3.5 w-3.5" />
                                  Instructor Feedback
                                </span>
                                <span className="text-sm leading-relaxed text-muted-foreground whitespace-pre-wrap">{qFeedback}</span>
                              </div>
                            )}
                          </div>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        )}
      </div>
      <div className="p-4 border-t bg-muted/10 flex justify-end">
        <Button variant="outline" onClick={onClose}>Close Report</Button>
      </div>
    </Modal>
  );
}

export default function MyGradesPage() {
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [selectedSubmissionId, setSelectedSubmissionId] = useState<string | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ['my-grades'],
    queryFn: () => gradesApi.getMyGrades(),
    select: (res) => res.data.data,
  });

  if (isLoading) return <PageSpinner />;

  const grades = Array.isArray(data) ? data : (data as any)?.items ?? [];

  // Helper function to color code scores
  const getScoreColor = (score: number) => {
    if (score >= 90) return 'text-green-600 dark:text-green-400 bg-green-100 dark:bg-green-900/30 border-green-500';
    if (score >= 70) return 'text-blue-600 dark:text-blue-400 bg-blue-100 dark:bg-blue-900/30 border-blue-500';
    if (score >= 50) return 'text-yellow-600 dark:text-yellow-400 bg-yellow-100 dark:bg-yellow-900/30 border-yellow-500';
    return 'text-red-600 dark:text-red-400 bg-red-100 dark:bg-red-900/30 border-red-500';
  };

  const getScoreBorder = (score: number) => {
    if (score >= 90) return 'border-l-green-500';
    if (score >= 70) return 'border-l-blue-500';
    if (score >= 50) return 'border-l-yellow-500';
    return 'border-l-red-500';
  };

  return (
    <AnimatedPage>
      <div className="max-w-5xl mx-auto px-4 py-8">
        <div className="flex items-center gap-3 mb-8">
          <Trophy className="h-8 w-8 text-primary" />
          <h1 className="text-3xl font-bold tracking-tight">Academic Record</h1>
        </div>

        {grades.length === 0 ? (
          <EmptyState
            icon={<BookOpen className="h-12 w-12" />}
            title="No grades yet"
            description="Complete some exams, and your grades along with detailed feedback will appear here."
          />
        ) : (
          <div className="grid gap-6">
            {grades.map((grade: any) => {
              const scoreNum = grade.score || 0;
              const scoreStyles = getScoreColor(scoreNum);
              const scoreBorder = getScoreBorder(scoreNum);
              const isExpanded = expandedId === grade.id;

              return (
                <Card 
                  key={grade.id} 
                  className={cn(
                    "overflow-hidden border-l-4 hover:bg-muted/30 transition-all cursor-pointer", 
                    scoreBorder
                  )}
                  onClick={() => setExpandedId(isExpanded ? null : grade.id)}
                >
                  <div className="flex flex-col sm:flex-row items-center justify-between p-4 sm:p-5 gap-3">
                    {/* Header Info */}
                    <div className="flex flex-1 items-center gap-4 w-full">
                      <div className={cn("hidden sm:flex flex-col items-center justify-center h-12 w-16 rounded-lg font-bold shadow-sm shrink-0", scoreStyles)}>
                        <span className="text-[10px] font-medium opacity-80 -mb-1 leading-none pt-1">Score</span>
                        <span className="text-lg leading-tight">{scoreNum.toFixed(0)}%</span>
                      </div>
                      
                      <div className="space-y-1 block flex-1">
                        <div className="flex items-center justify-between sm:justify-start gap-2">
                          <h3 className="text-base sm:text-lg font-semibold leading-tight line-clamp-1">{grade.examTitle || 'Unknown Exam'}</h3>
                          <div className={cn("sm:hidden flex items-center justify-center px-2 py-0.5 rounded-md text-xs font-bold shrink-0", scoreStyles)}>
                            {scoreNum.toFixed(0)}%
                          </div>
                        </div>
                        <div className="flex flex-wrap items-center gap-2 text-xs text-muted-foreground">
                          <span className="flex items-center gap-1.5 font-medium truncate max-w-[200px] sm:max-w-xs">
                            <BookOpen className="h-3.5 w-3.5" />
                            {grade.courseTitle || 'Unknown Course'}
                          </span>
                          <span className="hidden sm:inline text-muted-foreground/40">•</span>
                          {grade.isApproved ? (
                            <span className="flex items-center gap-1 text-green-600 dark:text-green-500 font-medium">
                              <CheckCircle className="h-3 w-3" /> Approved
                            </span>
                          ) : (
                            <span className="flex items-center gap-1 text-yellow-600 dark:text-yellow-500 font-medium">
                              <Clock className="h-3 w-3" /> Pending Review
                            </span>
                          )}
                        </div>
                      </div>
                    </div>

                    {/* Toggle Indicator */}
                    <div className="hidden sm:flex shrink-0 pr-2">
                      {isExpanded ? (
                        <ChevronUp className="h-5 w-5 text-muted-foreground" />
                      ) : (
                        <ChevronDown className="h-5 w-5 text-muted-foreground" />
                      )}
                    </div>
                  </div>

                  {/* Expanded Content */}
                  {isExpanded && (
                    <div className="px-5 pb-5 pt-2 border-t bg-muted/10 animate-in slide-in-from-top-2 duration-200">
                      <div className="flex flex-col md:flex-row gap-6 mt-4">
                        {/* Feedback Section */}
                        <div className="flex-1 space-y-2">
                          <h4 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
                            Feedback details
                          </h4>
                          {grade.feedback ? (
                            <div className="bg-background border rounded-lg p-4 text-sm text-foreground/90 whitespace-pre-wrap leading-relaxed shadow-sm">
                              {grade.feedback}
                            </div>
                          ) : (
                            <div className="bg-background border rounded-lg p-4 text-sm text-muted-foreground italic">
                              No detailed feedback provided yet.
                            </div>
                          )}
                        </div>

                        {/* Meta Sidebar */}
                        <div className="md:w-56 shrink-0 space-y-4">
                          <div className="space-y-1.5">
                            <span className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Grading Method</span>
                            <div className="flex items-center">
                              {grade.isAiGraded ? (
                                <Badge variant="outline" className="flex items-center gap-1.5 text-blue-600 border-blue-200 bg-blue-50/50 dark:border-blue-900 dark:text-blue-400 dark:bg-blue-900/20">
                                  <Bot className="h-3.5 w-3.5" /> Automatically Graded
                                </Badge>
                              ) : (
                                <Badge variant="outline" className="flex items-center gap-1.5">
                                  <User className="h-3.5 w-3.5" /> Instructor Graded
                                </Badge>
                              )}
                            </div>
                          </div>

                          <div className="pt-2 border-t">
                            <Button 
                              variant="outline" 
                              className="w-full justify-between hover:bg-primary/5 hover:text-primary transition-colors border-primary/20 hover:border-primary/50"
                              onClick={(e) => {
                                e.stopPropagation();
                                setSelectedSubmissionId(grade.submissionId);
                              }}
                            >
                              <span className="font-semibold text-sm">View Report</span>
                              <Search className="h-4 w-4 opacity-70" />
                            </Button>
                          </div>
                        </div>
                      </div>
                    </div>
                  )}
                </Card>
              );
            })}
          </div>
        )}
      </div>

      <SubmissionDetailsModal 
        submissionId={selectedSubmissionId} 
        isOpen={!!selectedSubmissionId} 
        onClose={() => setSelectedSubmissionId(null)} 
      />
    </AnimatedPage>
  );
}
