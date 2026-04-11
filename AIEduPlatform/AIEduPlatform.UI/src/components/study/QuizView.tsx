import { useState, useEffect } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { studySessionsApi } from '@/api/studySessions.api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Spinner } from '@/components/ui/Spinner';
import { Badge } from '@/components/ui/Badge';
import { cn } from '@/utils/cn';
import { toast } from 'sonner';
import { CheckCircle2, XCircle } from 'lucide-react';
import { SourceReference, type MaterialInfo } from './SourceReference';

interface QuizViewProps {
  sessionId: string;
  lectureIds: string[];
  materialIds: string[];
  materials?: MaterialInfo[];
  onOpenMaterial?: (materialId: string, page?: number, timestamp?: number) => void;
  pendingData?: { timestamp: number; data: any } | null;
}

interface QuizQuestion {
  questionText: string;
  questionType: string;
  options: string[];
  correctAnswer: string;
  explanation: string;
  difficulty: string;
}

export function QuizView({ sessionId, lectureIds, materialIds, materials = [], onOpenMaterial, pendingData }: QuizViewProps) {
  const [questions, setQuestions] = useState<QuizQuestion[]>([]);
  const [quizId, setQuizId] = useState<string | null>(null);
  const [answers, setAnswers] = useState<Record<number, string>>({});
  const [result, setResult] = useState<any>(null);
  const [topic, setTopic] = useState('');
  const [lastLoadedTimestamp, setLastLoadedTimestamp] = useState(0);

  useEffect(() => {
    if (pendingData && pendingData.timestamp !== lastLoadedTimestamp) {
      const data = pendingData.data;
      if (data) {
        const parsed =
          typeof data.questions === 'string'
            ? JSON.parse(data.questions)
            : data.questions;
        setQuestions(parsed || []);
        setQuizId(data.id);
        let parsedAnswers = {};
        let isSolved = false;
        if (data.studentAnswers && data.studentAnswers !== "null") {
          try {
            parsedAnswers = typeof data.studentAnswers === 'string' ? JSON.parse(data.studentAnswers) : data.studentAnswers;
            if (Object.keys(parsedAnswers).length > 0) isSolved = true;
          } catch(e) {}
        }
        setAnswers(parsedAnswers);
        setResult(isSolved ? { score: data.score, total: (parsed || []).length } : null);
        setLastLoadedTimestamp(pendingData.timestamp);
      }
    }
  }, [pendingData, lastLoadedTimestamp]);

  const generateMutation = useMutation({
    mutationFn: () =>
      studySessionsApi.generateQuiz(sessionId, { topic: topic || 'Key concepts', lectureIds, materialIds }),
    onSuccess: (res) => {
      const data = res.data.data;
      if (data) {
        const parsed =
          typeof data.questions === 'string'
            ? JSON.parse(data.questions)
            : data.questions;
        setQuestions(parsed || []);
        setQuizId(data.id);
        let parsedAnswers = {};
        let isSolved = false;
        if (data.studentAnswers && data.studentAnswers !== "null") {
          try {
            parsedAnswers = typeof data.studentAnswers === 'string' ? JSON.parse(data.studentAnswers) : data.studentAnswers;
            if (Object.keys(parsedAnswers).length > 0) isSolved = true;
          } catch(e) {}
        }
        setAnswers(parsedAnswers);
        setResult(isSolved ? { score: data.score, total: (parsed || []).length } : null);
      }
    },
  });

  const submitMutation = useMutation({
    mutationFn: () => {
      const answerMap: Record<string, string> = {};
      Object.entries(answers).forEach(([idx, answer]) => {
        answerMap[idx] = answer;
      });
      return studySessionsApi.submitQuiz(sessionId, quizId!, answerMap);
    },
    onSuccess: (res) => {
      const data = res.data.data;
      setResult(data);
      toast.success(`Quiz submitted! Score: ${data?.score ?? 'N/A'}%`);
    },
    onError: () => toast.error('Failed to submit quiz'),
  });

  const { data: history } = useQuery({
    queryKey: ['quizzes-history', sessionId],
    queryFn: () => studySessionsApi.getQuizzes(sessionId),
    select: (res) => res.data.data?.items,
  });

  return (
    <div className="p-4 space-y-4">
      <div className="flex items-center justify-between gap-2">
        <h3 className="font-bold text-lg">Quiz</h3>
        <div className="flex gap-2">
          <Input
            placeholder="Topic (optional)"
            value={topic}
            onChange={(e) => setTopic(e.target.value)}
            className="w-48"
          />
          <Button
            onClick={() => generateMutation.mutate()}
            loading={generateMutation.isPending}
          >
            Generate
          </Button>
        </div>
      </div>

      {generateMutation.isPending && (
        <div className="flex items-center justify-center py-12">
          <Spinner />
          <span className="ml-2 text-muted-foreground">Generating quiz...</span>
        </div>
      )}

      {questions.length > 0 && (
        <div className="space-y-6">
          {questions.map((q, idx) => (
            <div key={idx} className="border rounded-lg p-4">
              <div className="flex items-center justify-between mb-3">
                <span className="font-medium">
                  {idx + 1}. {q.questionText}
                </span>
                {q.difficulty && (
                  <Badge variant="outline">{q.difficulty}</Badge>
                )}
              </div>

              {q.options && q.options.length > 0 ? (
                <div className="space-y-2">
                  {q.options.map((opt, optIdx) => {
                    const isSelected = answers[idx] === opt;
                    const showCorrect = result && opt === q.correctAnswer;
                    const showWrong = result && isSelected && opt !== q.correctAnswer;

                    return (
                      <label
                        key={optIdx}
                        className={cn(
                          'flex items-center gap-3 p-2 rounded border cursor-pointer transition-colors',
                          isSelected && !result && 'border-primary bg-primary/5',
                          showCorrect && 'border-success bg-success/10',
                          showWrong && 'border-destructive bg-destructive/5'
                        )}
                      >
                        <input
                          type="radio"
                          name={`q-${idx}`}
                          checked={isSelected}
                          onChange={() =>
                            !result && setAnswers((prev) => ({ ...prev, [idx]: opt }))
                          }
                          disabled={!!result}
                          className="accent-primary"
                        />
                        <span className="flex-1">{opt}</span>
                        {showCorrect && <CheckCircle2 className="h-4 w-4 text-success" />}
                        {showWrong && <XCircle className="h-4 w-4 text-destructive" />}
                      </label>
                    );
                  })}
                </div>
              ) : (
                <textarea
                  className="w-full p-2 border rounded bg-background resize-y"
                  rows={3}
                  placeholder="Your answer..."
                  value={answers[idx] || ''}
                  onChange={(e) =>
                    !result && setAnswers((prev) => ({ ...prev, [idx]: e.target.value }))
                  }
                  disabled={!!result}
                />
              )}

              {result && q.explanation && (
                <div className="mt-2 text-sm text-muted-foreground bg-secondary p-2 rounded">
                  {onOpenMaterial && materials.length > 0 ? (
                    <SourceReference
                      text={q.explanation}
                      materials={materials}
                      onOpenMaterial={onOpenMaterial}
                    />
                  ) : (
                    q.explanation
                  )}
                </div>
              )}
            </div>
          ))}

          {!result && (
            <Button
              onClick={() => submitMutation.mutate()}
              loading={submitMutation.isPending}
              disabled={Object.keys(answers).length === 0}
            >
              Submit Quiz
            </Button>
          )}

          {result && (
            <div className="border rounded-lg p-4 bg-primary/5">
              <p className="text-lg font-bold">
                Score: {result.score}%
                <span className="text-sm font-normal text-muted-foreground ml-2">
                  ({Math.round((result.score / 100) * questions.length)}/{questions.length} correct)
                </span>
              </p>
            </div>
          )}
        </div>
      )}

      {history && Array.isArray(history) && history.length > 0 && (
        <div className="border-t pt-4 mt-6">
          <h4 className="font-medium mb-3">Previous Quizzes</h4>
          {history.map((quiz: any, idx: number) => (
            <Button
              key={quiz.id || idx}
              variant="outline"
              size="sm"
              className="mr-2 mb-2"
              onClick={() => {
                const parsed =
                  typeof quiz.questions === 'string'
                    ? JSON.parse(quiz.questions)
                    : quiz.questions;
                setQuestions(parsed || []);
                setQuizId(quiz.id);
                let parsedAnswers = {};
                let isSolved = false;
                if (quiz.studentAnswers && quiz.studentAnswers !== "null") {
                  try {
                    parsedAnswers = typeof quiz.studentAnswers === 'string' ? JSON.parse(quiz.studentAnswers) : quiz.studentAnswers;
                    if (Object.keys(parsedAnswers).length > 0) isSolved = true;
                  } catch(e) {}
                }
                setAnswers(parsedAnswers);
                setResult(isSolved ? { score: quiz.score, total: (parsed || []).length } : null);
              }}
            >
              {quiz.topic || `Quiz ${idx + 1}`}
              {quiz.studentAnswers && quiz.studentAnswers !== "null" && ` (${quiz.score}%)`}
            </Button>
          ))}
        </div>
      )}
    </div>
  );
}
