import { useParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { questionsApi } from '@/api/questions.api';
import { examsApi } from '@/api/exams.api';
import { Input } from '@/components/ui/Input';
import { Textarea } from '@/components/ui/Textarea';
import { Select } from '@/components/ui/Select';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Modal } from '@/components/ui/Modal';
import { PageSpinner } from '@/components/ui/Spinner';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { useForm, Controller } from 'react-hook-form';
import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import { DragDropContext, Droppable, Draggable, type DropResult } from '@hello-pangea/dnd';
import {
  Plus,
  Trash2,
  Sparkles,
  GripVertical,
  CheckCircle2,
  HelpCircle,
  FileText,
  Copy,
} from 'lucide-react';
import { generateId } from '@/utils/id';

const QUESTION_TYPE_OPTIONS = [
  { value: 'MultipleChoice', label: 'Multiple Choice' },
  { value: 'TrueFalse', label: 'True / False' },
  { value: 'ShortAnswer', label: 'Short Answer' },
  { value: 'Essay', label: 'Essay' },
  { value: 'FillInTheBlank', label: 'Fill in the Blank' },
] as const;

type QuestionTypeValue = (typeof QUESTION_TYPE_OPTIONS)[number]['value'];

interface BulkQuestion {
  id: string;
  text: string;
  type: QuestionTypeValue;
  optionsList: string[];
  correctAnswer: string;
  points: number;
}

interface AIGenerateForm {
  focusTopics: string;
  difficulty: string;
  numberOfQuestions: number;
  questionTypes: QuestionTypeValue[];
}

const createEmptyQuestion = (): BulkQuestion => ({
  id: generateId(),
  text: '',
  type: 'MultipleChoice',
  optionsList: ['', '', '', ''],
  correctAnswer: '',
  points: 1,
});

export default function QuestionEditorPage() {
  const { examId } = useParams<{ examId: string }>();
  const queryClient = useQueryClient();
  const [showAdd, setShowAdd] = useState(false);
  const [showAI, setShowAI] = useState(false);
  const [bulkQuestions, setBulkQuestions] = useState<BulkQuestion[]>([createEmptyQuestion()]);
  const [activeQuestionIdx, setActiveQuestionIdx] = useState(0);

  const [localQuestions, setLocalQuestions] = useState<any[]>([]);

  const { data: exam, isLoading: examLoading } = useQuery({
    queryKey: ['exam', examId],
    queryFn: () => examsApi.getById(examId!),
    enabled: !!examId,
    select: (res) => res.data.data,
  });

  const { data: questions, isLoading } = useQuery({
    queryKey: ['exam-questions', examId],
    queryFn: () => questionsApi.getByExam(examId!),
    enabled: !!examId,
    select: (res) => res.data.data,
  });

  useEffect(() => {
    if (questions) {
      setLocalQuestions([...questions].sort((a, b) => (a.order || 0) - (b.order || 0)));
    }
  }, [questions]);

  const aiForm = useForm<AIGenerateForm>({
    defaultValues: {
      difficulty: 'medium',
      numberOfQuestions: 5,
      focusTopics: '',
      questionTypes: [],
    },
  });

  const buildPayload = (q: BulkQuestion) => {
    const payload: any = {
      text: q.text,
      type: q.type,
      points: Number(q.points),
    };
    if (q.type === 'MultipleChoice') {
      payload.options = q.optionsList.filter((o) => o.trim());
      payload.correctAnswer = q.correctAnswer;
    } else if (q.type === 'TrueFalse') {
      payload.options = ['True', 'False'];
      payload.correctAnswer = q.correctAnswer;
    } else if (q.type === 'FillInTheBlank') {
      payload.correctAnswer = q.correctAnswer;
    } else {
      payload.correctAnswer = q.correctAnswer || '';
    }
    return payload;
  };

  const bulkAddMutation = useMutation({
    mutationFn: () => {
      const payloads = bulkQuestions
        .filter((q) => q.text.trim())
        .map(buildPayload);
      if (payloads.length === 0) throw new Error('No questions to add');
      if (payloads.length === 1) {
        return questionsApi.add(examId!, payloads[0]);
      }
      return questionsApi.addBulk(examId!, payloads);
    },
    onSuccess: () => {
      const count = bulkQuestions.filter((q) => q.text.trim()).length;
      toast.success(`${count} question${count > 1 ? 's' : ''} added`);
      queryClient.invalidateQueries({ queryKey: ['exam-questions', examId] });
      setShowAdd(false);
      setBulkQuestions([createEmptyQuestion()]);
      setActiveQuestionIdx(0);
    },
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
  });

  const generateMutation = useMutation({
    mutationFn: (data: AIGenerateForm) => {
      const payload: any = {
        numberOfQuestions: Number(data.numberOfQuestions),
        difficulty: (data.difficulty || '').toLowerCase(),
        focusTopics: data.focusTopics
          ? data.focusTopics.split(',').map((t: string) => t.trim()).filter(Boolean)
          : undefined,
      };
      if (data.questionTypes && data.questionTypes.length > 0) {
        payload.questionTypes = data.questionTypes;
      }
      return questionsApi.generateAI(examId!, payload);
    },
    onSuccess: () => {
      toast.success('AI questions generated!');
      queryClient.invalidateQueries({ queryKey: ['exam-questions', examId] });
      setShowAI(false);
      aiForm.reset({ difficulty: 'medium', numberOfQuestions: 5, focusTopics: '', questionTypes: [] });
    },
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
  });

  const reorderMutation = useMutation({
    mutationFn: (orders: Record<string, number>) => questionsApi.reorder(examId!, orders),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['exam-questions', examId] });
      toast.success('Questions reordered');
    },
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
  });

  const handleDragEnd = (result: DropResult) => {
    if (!result.destination) return;

    const items = Array.from(localQuestions);
    const [reorderedItem] = items.splice(result.source.index, 1);
    items.splice(result.destination.index, 0, reorderedItem);

    setLocalQuestions(items);

    const orders: Record<string, number> = {};
    items.forEach((item: any, index: number) => {
      orders[item.id] = index + 1;
    });

    reorderMutation.mutate(orders);
  };

  const deleteMutation = useMutation({
    mutationFn: (questionId: string) => questionsApi.delete(questionId),
    onSuccess: () => {
      toast.success('Question deleted');
      queryClient.invalidateQueries({ queryKey: ['exam-questions', examId] });
    },
  });

  const updateBulkQuestion = (idx: number, updates: Partial<BulkQuestion>) => {
    setBulkQuestions((prev) =>
      prev.map((q, i) => (i === idx ? { ...q, ...updates } : q))
    );
  };

  const addAnotherQuestion = () => {
    setBulkQuestions((prev) => [...prev, createEmptyQuestion()]);
    setActiveQuestionIdx(bulkQuestions.length);
  };

  const duplicateQuestion = (idx: number) => {
    const q = bulkQuestions[idx];
    const dup = { ...q, id: generateId() };
    setBulkQuestions((prev) => [...prev.slice(0, idx + 1), dup, ...prev.slice(idx + 1)]);
    setActiveQuestionIdx(idx + 1);
  };

  const removeBulkQuestion = (idx: number) => {
    if (bulkQuestions.length <= 1) return;
    setBulkQuestions((prev) => prev.filter((_, i) => i !== idx));
    setActiveQuestionIdx((prev) => Math.min(prev, bulkQuestions.length - 2));
  };

  if (isLoading || examLoading) return <PageSpinner />;

  const questionTypeLabel = (type: string) => {
    const opt = QUESTION_TYPE_OPTIONS.find((o) => o.value === type);
    return opt?.label ?? type;
  };

  const activeQ = bulkQuestions[activeQuestionIdx];
  const validCount = bulkQuestions.filter((q) => q.text.trim()).length;
  
  const existingHasWritten = (questions || []).some(
    (q: any) => q.type === 'Essay' || q.type === 'ShortAnswer'
  );
  const newHasWritten = bulkQuestions
    .filter((q) => q.text.trim())
    .some((q) => q.type === 'Essay' || q.type === 'ShortAnswer');

  return (
    <AnimatedPage>
      <div className="max-w-6xl mx-auto px-4 py-8 space-y-8">
        
        {/* Header */}
        <div className="flex flex-col md:flex-row items-start md:items-center justify-between gap-4 bg-primary/5 p-6 rounded-2xl border border-primary/10">
          <div className="flex gap-4 items-center">
            <div className="h-12 w-12 rounded-xl bg-primary/20 flex items-center justify-center text-primary shrink-0">
              <FileText className="h-6 w-6" />
            </div>
            <div>
              <h1 className="text-3xl font-bold tracking-tight">Question Editor</h1>
              <p className="text-muted-foreground mt-1">Manage and organize questions for <span className="font-semibold text-foreground">{exam?.title || 'Exam'}</span>.</p>
            </div>
          </div>
          <div className="flex items-center gap-3 shrink-0">
            <Button variant="outline" className="bg-background/50 hover:bg-background" onClick={() => setShowAI(true)}>
              <Sparkles className="h-4 w-4 mr-1.5 text-primary" /> Generate AI
            </Button>
            <Button className="shadow-sm" onClick={() => {
              setBulkQuestions([createEmptyQuestion()]);
              setActiveQuestionIdx(0);
              setShowAdd(true);
            }}>
              <Plus className="h-4 w-4 mr-1.5" /> Add Questions
            </Button>
          </div>
        </div>

        {/* Existing questions content */}
        <div className="space-y-6">
          {(!localQuestions || localQuestions.length === 0) && (
            <Card variant="glass" className="border-border/50">
              <CardContent className="p-12 text-center flex flex-col items-center justify-center border-2 border-dashed border-border/50 rounded-xl bg-background/30 m-4">
                <HelpCircle className="h-12 w-12 text-muted-foreground/40 mb-3" />
                <p className="font-medium text-lg">No questions yet</p>
                <p className="text-sm text-muted-foreground mt-1 mb-4">
                  Add questions manually or let AI generate them based on your topics.
                </p>
                <div className="flex gap-3 mt-4">
                  <Button variant="outline" onClick={() => setShowAI(true)}>
                    <Sparkles className="h-4 w-4 mr-1.5" /> Generate
                  </Button>
                  <Button onClick={() => setShowAdd(true)}>
                    <Plus className="h-4 w-4 mr-1.5" /> Manual Add
                  </Button>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Written question notice */}
          {existingHasWritten && localQuestions.length > 0 && (
            <div className="flex items-center gap-3 bg-info/5 border border-info/20 p-4 rounded-xl mb-6">
              <div className="h-8 w-8 bg-info/10 rounded-full flex items-center justify-center shrink-0">
                <Sparkles className="h-4 w-4 text-info" />
              </div>
              <p className="text-sm text-muted-foreground">
                This exam contains essay/short answer questions. These will be <span className="font-medium text-foreground">AI graded by default</span> during submissions. You can also review and manually override scores.
              </p>
            </div>
          )}

          {localQuestions.length > 0 && (
            <DragDropContext onDragEnd={handleDragEnd}>
              <Droppable droppableId="questions-list">
                {(provided) => (
                  <div 
                    className="space-y-4"
                    {...provided.droppableProps}
                    ref={provided.innerRef}
                  >
                    {localQuestions.map((q: any, idx: number) => {
                      const parsedOptions = typeof q.options === 'string' ? JSON.parse(q.options || '[]') : (q.options || []);
                      const hasOptions = Array.isArray(parsedOptions) && parsedOptions.length > 0;
                    
                    return (
                      <Draggable key={q.id} draggableId={q.id} index={idx}>
                        {(provided, snapshot) => (
                          <div
                            ref={provided.innerRef}
                            {...provided.draggableProps}
                            style={{
                              ...provided.draggableProps.style,
                              opacity: snapshot.isDragging ? 0.9 : 1
                            }}
                          >
                            <Card className={`transition-all ${snapshot.isDragging ? 'shadow-md border-primary ring-1 ring-primary/20' : 'hover:shadow-sm'}`}>
                              <CardContent className="p-4">
                                <div className="flex items-start gap-3">
                                  <div {...provided.dragHandleProps} className="mt-1 cursor-grab active:cursor-grabbing hover:bg-secondary rounded p-1">
                                    <GripVertical className="h-5 w-5 text-muted-foreground shrink-0" />
                                  </div>
                                  <div className="flex-1 min-w-0">
                                    <div className="flex flex-wrap items-center gap-2 mb-1.5">
                                      <span className="font-medium text-sm">Q{idx + 1}. {q.text}</span>
                                    </div>
                                    <div className="flex flex-wrap items-center gap-1.5 mb-2">
                                      <Badge variant="outline" className="text-xs">{questionTypeLabel(q.type)}</Badge>
                                      <Badge variant="outline" className="text-xs">{q.points} {q.points === 1 ? 'pt' : 'pts'}</Badge>
                                    </div>
                                    {q.type === 'TrueFalse' ? (
                                      <div className="text-sm text-muted-foreground ml-1 space-y-0.5">
                                        {['True', 'False'].map((opt: string, i: number) => (
                                          <p
                                            key={i}
                                            className={`flex items-center gap-1.5 ${opt === q.correctAnswer ? 'text-success font-medium' : ''}`}
                                          >
                                            {opt === q.correctAnswer && <CheckCircle2 className="h-3.5 w-3.5" />}
                                            {String.fromCharCode(65 + i)}. {opt}
                                          </p>
                                        ))}
                                      </div>
                                    ) : hasOptions ? (
                                      <div className="text-sm text-muted-foreground ml-1 space-y-0.5">
                                        {parsedOptions.map(
                                          (opt: string, i: number) => (
                                            <p
                                              key={i}
                                              className={`flex items-center gap-1.5 ${opt === q.correctAnswer ? 'text-success font-medium' : ''}`}
                                            >
                                              {opt === q.correctAnswer && <CheckCircle2 className="h-3.5 w-3.5" />}
                                              {String.fromCharCode(65 + i)}. {opt}
                                            </p>
                                          )
                                        )}
                                      </div>
                                    ) : q.correctAnswer ? (
                                      <p className="text-sm text-success flex items-center gap-1.5 ml-1 mt-1">
                                        <CheckCircle2 className="h-3.5 w-3.5" />
                                        {q.type === 'TrueFalse' ? q.correctAnswer : `Answer: ${q.correctAnswer}`}
                                      </p>
                                    ) : null}
                                  </div>
                                  <Button
                                    variant="ghost"
                                    size="icon"
                                    onClick={() => deleteMutation.mutate(q.id)}
                                    className="shrink-0 hover:bg-destructive/10"
                                  >
                                    <Trash2 className="h-4 w-4 text-destructive" />
                                  </Button>
                                </div>
                              </CardContent>
                            </Card>
                          </div>
                        )}
                      </Draggable>
                    );
                  })}
                  {provided.placeholder}
                </div>
              )}
            </Droppable>
          </DragDropContext>
          )}
        </div>

        {/* ─── Bulk Add Questions Modal ─── */}
        <Modal
          open={showAdd}
          onClose={() => setShowAdd(false)}
          title="Add Questions"
          description={`${validCount} question${validCount !== 1 ? 's' : ''} ready to add`}
          className="max-w-2xl"
        >
          <div className="space-y-4">
            {/* Question Tabs */}
            <div className="flex items-center gap-2 overflow-x-auto pb-1">
              {bulkQuestions.map((q, idx) => (
                <button
                  key={q.id}
                  onClick={() => setActiveQuestionIdx(idx)}
                  className={`shrink-0 px-3 py-1.5 rounded-lg text-sm font-medium transition-all border cursor-pointer ${
                    idx === activeQuestionIdx
                      ? 'border-primary bg-primary/10 text-primary'
                      : q.text.trim()
                        ? 'border-success/30 bg-success/5 text-success hover:bg-success/10'
                        : 'border-border bg-secondary/50 text-muted-foreground hover:bg-secondary'
                  }`}
                >
                  Q{idx + 1}
                </button>
              ))}
              <button
                onClick={addAnotherQuestion}
                className="shrink-0 px-2.5 py-1.5 rounded-lg text-sm border border-dashed border-border text-muted-foreground hover:border-primary hover:text-primary transition-all cursor-pointer"
              >
                <Plus className="h-3.5 w-3.5" />
              </button>
            </div>

            {/* Active Question Form */}
            {activeQ && (
              <div className="space-y-4">
                <div className="rounded-lg border border-border p-4 space-y-4">
                  <div className="flex items-center justify-between">
                    <h4 className="text-sm font-medium flex items-center gap-1.5">
                      <FileText className="h-4 w-4 text-muted-foreground" />
                      Question {activeQuestionIdx + 1}
                    </h4>
                    <div className="flex items-center gap-1">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => duplicateQuestion(activeQuestionIdx)}
                        title="Duplicate question"
                      >
                        <Copy className="h-3.5 w-3.5" />
                      </Button>
                      {bulkQuestions.length > 1 && (
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => removeBulkQuestion(activeQuestionIdx)}
                          className="hover:bg-destructive/10"
                          title="Remove question"
                        >
                          <Trash2 className="h-3.5 w-3.5 text-destructive" />
                        </Button>
                      )}
                    </div>
                  </div>

                  <Textarea
                    label="Question Text"
                    placeholder="Enter your question here..."
                    value={activeQ.text}
                    onChange={(e) => updateBulkQuestion(activeQuestionIdx, { text: e.target.value })}
                  />

                  <div className="grid grid-cols-2 gap-4">
                    <Select
                      label="Question Type"
                      value={activeQ.type}
                      onChange={(e) =>
                        updateBulkQuestion(activeQuestionIdx, {
                          type: e.target.value as QuestionTypeValue,
                          optionsList: e.target.value === 'MultipleChoice' ? ['', '', '', ''] : [],
                          correctAnswer: '',
                        })
                      }
                      options={[...QUESTION_TYPE_OPTIONS]}
                    />
                    <Input
                      label="Points"
                      type="number"
                      placeholder="1"
                      hint="Score weight"
                      value={activeQ.points}
                      onChange={(e) => updateBulkQuestion(activeQuestionIdx, { points: Number(e.target.value) })}
                    />
                  </div>
                </div>

                <div className="rounded-lg border border-border p-4 space-y-4">
                  <h4 className="text-sm font-medium flex items-center gap-1.5">
                    <CheckCircle2 className="h-4 w-4 text-muted-foreground" />
                    Answer Configuration
                  </h4>

                  {activeQ.type === 'MultipleChoice' && (
                    <div className="space-y-3">
                      <label className="block text-sm font-medium text-foreground">
                        Options
                        <span className="text-xs text-muted-foreground ml-1.5 font-normal">Click the radio to mark correct answer</span>
                      </label>
                      {activeQ.optionsList.map((opt, optIdx) => (
                        <div key={optIdx} className="flex items-center gap-2">
                          <input
                            type="radio"
                            name={`correct-${activeQ.id}`}
                            checked={activeQ.correctAnswer === opt && opt.trim() !== ''}
                            onChange={() => updateBulkQuestion(activeQuestionIdx, { correctAnswer: opt })}
                            disabled={opt.trim() === ''}
                            className="h-4 w-4 accent-primary shrink-0"
                            title="Mark as correct answer"
                          />
                          <Input
                            placeholder={`Option ${String.fromCharCode(65 + optIdx)}`}
                            value={opt}
                            onChange={(e) => {
                              const newOptions = [...activeQ.optionsList];
                              const oldVal = newOptions[optIdx];
                              newOptions[optIdx] = e.target.value;
                              const updates: Partial<BulkQuestion> = { optionsList: newOptions };
                              if (activeQ.correctAnswer === oldVal) {
                                updates.correctAnswer = e.target.value;
                              }
                              updateBulkQuestion(activeQuestionIdx, updates);
                            }}
                            className={`flex-1 ${activeQ.correctAnswer === opt && opt.trim() !== '' ? 'border-success ring-1 ring-success/30' : ''}`}
                          />
                          {activeQ.optionsList.length > 2 && (
                            <Button
                              variant="ghost"
                              size="icon"
                              className="shrink-0 hover:bg-destructive/10"
                              onClick={() => {
                                const newOptions = activeQ.optionsList.filter((_, i) => i !== optIdx);
                                const updates: Partial<BulkQuestion> = { optionsList: newOptions };
                                if (activeQ.correctAnswer === opt) {
                                  updates.correctAnswer = '';
                                }
                                updateBulkQuestion(activeQuestionIdx, updates);
                              }}
                            >
                              <Trash2 className="h-3.5 w-3.5 text-destructive" />
                            </Button>
                          )}
                        </div>
                      ))}
                      {activeQ.optionsList.length < 8 && (
                        <Button
                          variant="outline"
                          size="sm"
                          type="button"
                          onClick={() => {
                            updateBulkQuestion(activeQuestionIdx, {
                              optionsList: [...activeQ.optionsList, ''],
                            });
                          }}
                        >
                          <Plus className="h-3.5 w-3.5 mr-1" /> Add Option
                        </Button>
                      )}
                      {activeQ.correctAnswer && (
                        <p className="text-xs text-success flex items-center gap-1">
                          <CheckCircle2 className="h-3 w-3" />
                          Correct: {activeQ.correctAnswer}
                        </p>
                      )}
                    </div>
                  )}

                  {activeQ.type === 'TrueFalse' && (
                    <div className="space-y-2">
                      <label className="block text-sm font-medium text-foreground">Correct Answer</label>
                      <div className="flex items-center gap-6">
                        {['True', 'False'].map((val) => (
                          <label key={val} className="flex items-center gap-2 cursor-pointer">
                            <input
                              type="radio"
                              name={`tf-${activeQ.id}`}
                              value={val}
                              checked={activeQ.correctAnswer === val}
                              onChange={() => updateBulkQuestion(activeQuestionIdx, { correctAnswer: val })}
                              className="h-4 w-4 accent-primary"
                            />
                            <span className="text-sm font-medium">{val}</span>
                          </label>
                        ))}
                      </div>
                    </div>
                  )}

                  {activeQ.type === 'FillInTheBlank' && (
                    <div className="space-y-3">
                      <div className="rounded-lg bg-info/5 border border-info/20 p-3">
                        <p className="text-xs text-info font-medium mb-1">How it works</p>
                        <p className="text-xs text-muted-foreground">
                          Use <code className="bg-secondary px-1 py-0.5 rounded text-foreground font-mono">___</code> in your question text to show where the blank goes.
                          Then enter the correct answer below.
                        </p>
                      </div>
                      {activeQ.text && !activeQ.text.includes('___') && (
                        <div className="rounded-lg bg-warning/5 border border-warning/20 p-3">
                          <p className="text-xs text-warning">
                            Tip: Add <code className="bg-secondary px-1 py-0.5 rounded font-mono">___</code> in your question text to mark the blank position.
                          </p>
                        </div>
                      )}
                      {activeQ.text && activeQ.text.includes('___') && (
                        <div className="rounded-lg border border-border p-3">
                          <p className="text-xs text-muted-foreground mb-1">Preview</p>
                          <p className="text-sm">
                            {activeQ.text.split('___').map((part, i, arr) => (
                              <span key={i}>
                                {part}
                                {i < arr.length - 1 && (
                                  <span className="inline-block min-w-[80px] border-b-2 border-primary text-primary font-medium text-center mx-1">
                                    {activeQ.correctAnswer || '?'}
                                  </span>
                                )}
                              </span>
                            ))}
                          </p>
                        </div>
                      )}
                      <Input
                        label="Correct Answer"
                        placeholder="Enter the word or phrase that fills the blank"
                        hint="This is what the student must type to get it right"
                        value={activeQ.correctAnswer}
                        onChange={(e) => updateBulkQuestion(activeQuestionIdx, { correctAnswer: e.target.value })}
                      />
                    </div>
                  )}

                  {(activeQ.type === 'ShortAnswer' || activeQ.type === 'Essay') && (
                    <Textarea
                      label="Model Answer (optional)"
                      placeholder={
                        activeQ.type === 'Essay'
                          ? 'Provide a model essay answer for reference...'
                          : 'Provide a model short answer for reference...'
                      }
                      hint="Used by the AI to grade student answers. Providing a model answer improves AI grading accuracy."
                      value={activeQ.correctAnswer}
                      onChange={(e) => updateBulkQuestion(activeQuestionIdx, { correctAnswer: e.target.value })}
                    />
                  )}
                </div>
              </div>
            )}

            {/* Written Question Warning */}
            {(newHasWritten || existingHasWritten) && (
              <div className="flex items-start gap-2.5 rounded-lg bg-info/5 border border-info/20 p-3">
                <Sparkles className="h-4 w-4 text-info shrink-0 mt-0.5" />
                <p className="text-xs text-muted-foreground">
                  <span className="font-medium text-info">AI graded by default.</span>{' '}
                  Essay and short answer questions will be automatically graded by AI when you use the "AI Grade" option. You can review and adjust the scores afterward.
                </p>
              </div>
            )}

            {/* Action Buttons */}
            <div className="flex items-center gap-3 pt-4 border-t border-border">
              <Button variant="outline" type="button" onClick={addAnotherQuestion}>
                <Plus className="h-4 w-4 mr-1.5" /> Add Another
              </Button>
              <div className="flex-1" />
              <Button variant="outline" type="button" onClick={() => setShowAdd(false)}>
                Cancel
              </Button>
              <Button
                onClick={() => bulkAddMutation.mutate()}
                loading={bulkAddMutation.isPending}
                disabled={validCount === 0}
              >
                Add {validCount} Question{validCount !== 1 ? 's' : ''}
              </Button>
            </div>
          </div>
        </Modal>

        {/* ─── AI Generate Modal ─── */}
        <Modal
          open={showAI}
          onClose={() => setShowAI(false)}
          title="Generate Questions with AI"
          description="AI will create questions based on the course materials"
        >
          <form
            onSubmit={aiForm.handleSubmit((d) => generateMutation.mutate(d))}
            className="space-y-5"
          >
            <div className="rounded-lg border border-border p-4 space-y-4">
              <h4 className="text-sm font-medium flex items-center gap-1.5">
                <Sparkles className="h-4 w-4 text-muted-foreground" />
                Generation Settings
              </h4>

              <Input
                label="Focus Topics"
                placeholder="e.g. Machine Learning, Neural Networks, Data Preprocessing"
                hint="Comma-separated list of topics to focus on (leave empty for all topics)"
                {...aiForm.register('focusTopics')}
              />

              <div className="grid grid-cols-2 gap-4">
                <Select
                  label="Difficulty"
                  {...aiForm.register('difficulty')}
                  options={[
                    { value: 'easy', label: 'Easy' },
                    { value: 'medium', label: 'Medium' },
                    { value: 'hard', label: 'Hard' },
                  ]}
                />
                <Input
                  label="Number of Questions"
                  type="number"
                  placeholder="5"
                  hint="How many questions to generate"
                  {...aiForm.register('numberOfQuestions', {
                    valueAsNumber: true,
                    min: 1,
                    max: 20,
                  })}
                  min={1}
                  max={20}
                />
              </div>
            </div>

            <div className="rounded-lg border border-border p-4 space-y-3">
              <h4 className="text-sm font-medium flex items-center gap-1.5">
                <HelpCircle className="h-4 w-4 text-muted-foreground" />
                Question Types
              </h4>
              <p className="text-xs text-muted-foreground">
                Select which question types to include. Leave all unchecked to let the AI decide.
              </p>

              <Controller
                control={aiForm.control}
                name="questionTypes"
                render={({ field }) => (
                  <div className="grid grid-cols-2 gap-2">
                    {QUESTION_TYPE_OPTIONS.map((opt) => {
                      const checked = (field.value || []).includes(opt.value);
                      return (
                        <label
                          key={opt.value}
                          className={`flex items-center gap-2.5 p-2.5 rounded-lg border cursor-pointer transition-all duration-150 ${
                            checked
                              ? 'border-primary bg-primary/5'
                              : 'border-border hover:border-primary/30 hover:bg-secondary/50'
                          }`}
                        >
                          <input
                            type="checkbox"
                            checked={checked}
                            onChange={(e) => {
                              const current = field.value || [];
                              if (e.target.checked) {
                                field.onChange([...current, opt.value]);
                              } else {
                                field.onChange(current.filter((v: string) => v !== opt.value));
                              }
                            }}
                            className="h-4 w-4 rounded accent-primary"
                          />
                          <span className="text-sm font-medium">{opt.label}</span>
                        </label>
                      );
                    })}
                  </div>
                )}
              />
            </div>

            <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
              <Button variant="outline" type="button" className="flex-1" onClick={() => setShowAI(false)}>
                Cancel
              </Button>
              <Button type="submit" className="flex-1" loading={generateMutation.isPending}>
                <Sparkles className="h-4 w-4 mr-1.5" />
                Generate
              </Button>
            </div>
          </form>
        </Modal>
      </div>
    </AnimatedPage>
  );
}

