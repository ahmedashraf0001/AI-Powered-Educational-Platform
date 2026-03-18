import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { examsApi } from '@/api/exams.api';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { Button } from '@/components/ui/Button';
import { Modal } from '@/components/ui/Modal';
import { Textarea } from '@/components/ui/Textarea';
import { PageSpinner } from '@/components/ui/Spinner';
import { useState, useEffect, useCallback, useRef } from 'react';
import { toast } from 'sonner';
import { cn } from '@/utils/cn';
import { Clock, AlertTriangle, CheckCircle2 } from 'lucide-react';
import { QuestionType } from '@/types';

// Auto-save interval in milliseconds
const AUTO_SAVE_INTERVAL = 30000; // 30 seconds

export default function ExamTakingPage() {
  const { examId } = useParams<{ examId: string }>();
  const navigate = useNavigate();
  const [currentQ, setCurrentQ] = useState(0);
  const [answers, setAnswers] = useState<Record<string, string>>({});
  const [showConfirm, setShowConfirm] = useState(false);
  const [timeLeft, setTimeLeft] = useState<number | null>(null);
  const [attemptStarted, setAttemptStarted] = useState(false);
  const submitted = useRef(false);
  const answersRef = useRef(answers);
  answersRef.current = answers;

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

  // Start or resume exam attempt
  const startAttemptMutation = useMutation({
    mutationFn: () => examsApi.startAttempt(examId!),
    onSuccess: (res) => {
      const attempt = res.data.data;
      if (attempt) {
        setTimeLeft(attempt.remainingSeconds);
        setAttemptStarted(true);
        // Load saved answers if any
        if (attempt.savedAnswers) {
          setAnswers(attempt.savedAnswers);
        }
      }
    },
    onError: (error: any) => {
      const message = error?.response?.data?.message || 'Failed to start exam';
      toast.error(message);
      if (message.includes('already submitted')) {
        navigate('/my-submissions');
      }
    },
  });

  // Auto-save answers mutation
  const saveAnswersMutation = useMutation({
    mutationFn: (ans: Record<string, string>) => examsApi.saveAnswers(examId!, ans),
  });

  // Start attempt when exam loads
  useEffect(() => {
    if (exam && examId && !attemptStarted && !startAttemptMutation.isPending) {
      startAttemptMutation.mutate();
    }
  }, [exam, examId, attemptStarted]);

  // Auto-save answers periodically
  useEffect(() => {
    if (!attemptStarted || !examId) return;

    const interval = setInterval(() => {
      if (Object.keys(answersRef.current).length > 0) {
        saveAnswersMutation.mutate(answersRef.current);
      }
    }, AUTO_SAVE_INTERVAL);

    return () => clearInterval(interval);
  }, [attemptStarted, examId]);

  // Timer countdown
  useEffect(() => {
    if (timeLeft === null || timeLeft <= 0) return;
    const interval = setInterval(() => {
      setTimeLeft((prev) => {
        if (prev === null) return null;
        if (prev <= 1) {
          // Auto-submit using ref to get latest answers
          if (!submitted.current) {
            submitted.current = true;
            submitMutation.mutate(answersRef.current);
          }
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(interval);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [timeLeft !== null && timeLeft > 0]);

  const submitMutation = useMutation({
    mutationFn: (answers: Record<string, string>) =>
      examsApi.submit(examId!, answers),
    onSuccess: () => {
      toast.success('Exam submitted successfully!');
      navigate('/my-submissions');
    },
    onError: () => toast.error('Failed to submit exam'),
  });

  const handleSubmit = useCallback(() => {
    if (submitted.current) return;
    submitted.current = true;
    submitMutation.mutate(answers);
  }, [answers, submitMutation]);

  if (isLoading) return <PageSpinner />;
  if (!exam) return <div className="p-8 text-center">Exam not found</div>;

  const questions = exam.questions || [];
  const question = questions[currentQ];
  const answeredCount = Object.keys(answers).length;

  const formatTime = (seconds: number) => {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
  };

  return (
    <AnimatedPage>
    <div className="max-w-5xl mx-auto px-4 py-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-6 sticky top-14 bg-background py-3 z-10 border-b">
        <div>
          <h1 className="text-xl font-bold">{exam.title}</h1>
          <p className="text-sm text-muted-foreground">
            {totalPoints !== undefined && `Total: ${totalPoints} points`}
          </p>
        </div>
        <div className="flex items-center gap-4">
          {timeLeft !== null && (
            <div className={cn(
              'flex items-center gap-2 text-lg font-mono font-bold',
              timeLeft < 60 && 'text-destructive animate-pulse'
            )}>
              <Clock className="h-5 w-5" />
              {formatTime(timeLeft)}
            </div>
          )}
          <Button onClick={() => setShowConfirm(true)}>Submit Exam</Button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        {/* Question Navigator */}
        <div className="md:col-span-1">
          <h3 className="font-medium mb-3">Questions</h3>
          <div className="grid grid-cols-5 md:grid-cols-3 gap-2">
            {questions.map((_: any, idx: number) => (
              <button
                key={idx}
                onClick={() => setCurrentQ(idx)}
                className={cn(
                  'w-10 h-10 rounded-lg text-sm font-medium transition-all duration-200 border',
                  idx === currentQ && 'ring-2 ring-primary ring-offset-2 ring-offset-background',
                  answers[questions[idx]?.id]
                    ? 'bg-success text-success-foreground border-success/50'
                    : 'bg-secondary border-border hover:bg-secondary/80'
                )}
              >
                {idx + 1}
              </button>
            ))}
          </div>
          <p className="text-xs text-muted-foreground mt-3">
            {answeredCount}/{questions.length} answered
          </p>
        </div>

        {/* Question Content */}
        <div className="md:col-span-3">
          {question && (
            <div className="border rounded-lg p-6">
              <div className="flex items-center justify-between mb-4">
                <span className="text-sm text-muted-foreground">
                  Question {currentQ + 1} of {questions.length}
                </span>
                <span className="text-sm font-medium">{question.points} pts</span>
              </div>
              <p className="text-lg mb-6">{question.text}</p>

              {question.type === QuestionType.MultipleChoice && question.options ? (
                <div className="space-y-3">
                  {(typeof question.options === 'string'
                    ? JSON.parse(question.options)
                    : question.options
                  ).map((option: string, idx: number) => (
                    <label
                      key={idx}
                      className={cn(
                        'flex items-center gap-3 p-3.5 rounded-lg border-2 cursor-pointer transition-all duration-200',
                        answers[question.id] === option
                          ? 'border-primary bg-primary/5 shadow-sm shadow-primary/10'
                          : 'border-border hover:bg-secondary hover:border-border/80'
                      )}
                    >
                      <input
                        type="radio"
                        name={question.id}
                        checked={answers[question.id] === option}
                        onChange={() =>
                          setAnswers((prev) => ({ ...prev, [question.id]: option }))
                        }
                        className="accent-primary"
                      />
                      <span>{option}</span>
                    </label>
                  ))}
                </div>
              ) : question.type === QuestionType.TrueFalse ? (
                <div className="space-y-3">
                  {['True', 'False'].map((opt) => (
                    <label
                      key={opt}
                      className={cn(
                        'flex items-center gap-3 p-3.5 rounded-lg border-2 cursor-pointer transition-all duration-200',
                        answers[question.id] === opt
                          ? 'border-primary bg-primary/5 shadow-sm shadow-primary/10'
                          : 'border-border hover:bg-secondary hover:border-border/80'
                      )}
                    >
                      <input
                        type="radio"
                        name={question.id}
                        checked={answers[question.id] === opt}
                        onChange={() =>
                          setAnswers((prev) => ({ ...prev, [question.id]: opt }))
                        }
                        className="accent-primary"
                      />
                      <span>{opt}</span>
                    </label>
                  ))}
                </div>
              ) : (
                <Textarea
                  placeholder="Type your answer here..."
                  hint="Write your response in detail"
                  value={answers[question.id] || ''}
                  onChange={(e) =>
                    setAnswers((prev) => ({ ...prev, [question.id]: e.target.value }))
                  }
                />
              )}

              <div className="flex justify-between mt-6">
                <Button
                  variant="outline"
                  onClick={() => setCurrentQ((p) => Math.max(0, p - 1))}
                  disabled={currentQ === 0}
                >
                  Previous
                </Button>
                <Button
                  onClick={() => setCurrentQ((p) => Math.min(questions.length - 1, p + 1))}
                  disabled={currentQ === questions.length - 1}
                >
                  Next
                </Button>
              </div>
            </div>
          )}
        </div>
      </div>

      <Modal open={showConfirm} onClose={() => setShowConfirm(false)} title="Submit Exam">
        <div className="space-y-5">
          {answeredCount < questions.length ? (
            <div className="flex items-start gap-3 rounded-lg bg-warning/10 border border-warning/20 p-4">
              <AlertTriangle className="h-5 w-5 text-warning shrink-0 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-warning">Unanswered questions</p>
                <p className="text-sm text-muted-foreground mt-1">
                  You still have {questions.length - answeredCount} unanswered question{questions.length - answeredCount > 1 ? 's' : ''}. You can go back and complete them before submitting.
                </p>
              </div>
            </div>
          ) : (
            <div className="flex items-start gap-3 rounded-lg bg-success/10 border border-success/20 p-4">
              <CheckCircle2 className="h-5 w-5 text-success shrink-0 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-success">All questions answered</p>
                <p className="text-sm text-muted-foreground mt-1">
                  You have answered all {questions.length} questions.
                </p>
              </div>
            </div>
          )}
          <p className="text-sm text-muted-foreground">
            You answered <span className="font-semibold text-foreground">{answeredCount}</span> out of <span className="font-semibold text-foreground">{questions.length}</span> questions. Are you ready to submit?
          </p>
          <div className="flex items-center justify-end gap-3 pt-4 border-t border-border">
            <Button variant="outline" onClick={() => setShowConfirm(false)}>
              Go Back
            </Button>
            <Button
              variant="gradient"
              onClick={() => {
                setShowConfirm(false);
                handleSubmit();
              }}
              loading={submitMutation.isPending}
            >
              Submit Exam
            </Button>
          </div>
        </div>
      </Modal>
    </div>
    </AnimatedPage>
  );
}
