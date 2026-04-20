import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import { motion } from 'framer-motion';
import { staggerContainer, fadeInUp } from '@/utils/motion';
import { examsApi } from '@/api/exams.api';
import { submissionsApi } from '@/api/submissions.api';
import { gradesApi } from '@/api/grades.api';
import { Card, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Modal } from '@/components/ui/Modal';
import { PageSpinner } from '@/components/ui/Spinner';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import type { ExamDetailDto, ExamGradeStats } from '@/types';
import {
  Pencil,
  Trash2,
  FileText,
  Users,
  BarChart3,
  Clock,
  Target,
  TrendingUp,
  AlertTriangle,
} from 'lucide-react';

interface ExamFormValues {
  title: string;
  durationMinutes: number;
  startTime: string;
  endTime: string;
}

/** Convert an ISO date string to the `datetime-local` input format */
function toLocalInput(iso: string): string {
  if (!iso) return '';
  return iso.slice(0, 16); // "YYYY-MM-DDTHH:mm"
}

const GRADE_COLORS: Record<string, string> = {
  A: 'bg-success',
  B: 'bg-info',
  C: 'bg-warning',
  D: 'bg-accent',
  F: 'bg-destructive',
};

export default function TeacherExamDetailPage() {
  const { examId } = useParams<{ examId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [showEdit, setShowEdit] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  // ── Queries ──────────────────────────────────────────────────────────

  const {
    data: exam,
    isLoading: examLoading,
  } = useQuery({
    queryKey: ['exam', examId],
    queryFn: () => examsApi.getById(examId!),
    enabled: !!examId,
    select: (res) => res.data.data as ExamDetailDto,
  });

  const { data: submissionStats } = useQuery({
    queryKey: ['submission-stats', examId],
    queryFn: () => submissionsApi.getStats(examId!),
    enabled: !!examId,
    select: (res) => res.data.data,
  });

  const { data: gradeStats } = useQuery({
    queryKey: ['grade-stats', examId],
    queryFn: () => gradesApi.getExamStats(examId!),
    enabled: !!examId,
    select: (res) => res.data.data as ExamGradeStats,
  });

  const { data: gradeDistribution } = useQuery({
    queryKey: ['grade-distribution', examId],
    queryFn: () => gradesApi.getDistribution(examId!),
    enabled: !!examId,
    select: (res) => (res.data.data ?? {}) as Record<string, number>,
  });

  // ── Edit form ───────────────────────────────────────────────────────

  const editForm = useForm<ExamFormValues>();

  useEffect(() => {
    if (exam && showEdit) {
      editForm.reset({
        title: exam.title,
        durationMinutes: exam.durationMinutes,
        startTime: toLocalInput(exam.startTime),
        endTime: toLocalInput(exam.endTime),
      });
    }
  }, [exam, showEdit]);

  const updateMutation = useMutation({
    mutationFn: (data: ExamFormValues) => examsApi.update(examId!, data),
    onSuccess: () => {
      toast.success('Exam updated successfully');
      setShowEdit(false);
      queryClient.invalidateQueries({ queryKey: ['exam', examId] });
      queryClient.invalidateQueries({ queryKey: ['teacher-exams'] });
    },
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
  });

  // ── Delete ──────────────────────────────────────────────────────────

  const deleteMutation = useMutation({
    mutationFn: () => examsApi.delete(examId!),
    onSuccess: () => {
      toast.success('Exam deleted');
      queryClient.invalidateQueries({ queryKey: ['teacher-exams'] });
      navigate('/teacher/exams');
    },
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
  });

  // ── Loading ─────────────────────────────────────────────────────────

  if (examLoading) return <PageSpinner />;

  if (!exam) {
    return (
      <AnimatedPage>
        <div className="max-w-5xl mx-auto px-4 py-8 text-center">
          <h1 className="text-2xl font-bold mb-2">Exam not found</h1>
          <Button variant="outline" onClick={() => navigate('/teacher/exams')}>
            Back to Exams
          </Button>
        </div>
      </AnimatedPage>
    );
  }

  // ── Derived values ──────────────────────────────────────────────────

  const distributionEntries = Object.entries(gradeDistribution ?? {});
  const maxCount = Math.max(...distributionEntries.map(([, v]) => v), 1);

  const statCards = [
    {
      icon: Users,
      label: 'Total Submissions',
      value: submissionStats?.totalSubmissions ?? 0,
      color: 'from-primary/20 to-primary/5',
      iconColor: 'text-primary',
    },
    {
      icon: Target,
      label: 'Average Score',
      value: gradeStats?.averageScore != null ? `${gradeStats.averageScore.toFixed(1)}%` : '--',
      color: 'from-info/20 to-info/5',
      iconColor: 'text-info',
    },
    {
      icon: TrendingUp,
      label: 'Pass Rate',
      value: gradeStats?.passRate != null ? `${gradeStats.passRate.toFixed(0)}%` : '--',
      color: 'from-success/20 to-success/5',
      iconColor: 'text-success',
    },
    {
      icon: BarChart3,
      label: 'Total Graded',
      value: gradeStats?.totalGraded ?? 0,
      color: 'from-accent/20 to-accent/5',
      iconColor: 'text-accent',
    },
  ];

  // ── Render ──────────────────────────────────────────────────────────

  return (
    <AnimatedPage>
      <div className="max-w-5xl mx-auto px-4 py-8">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8">
          <div>
            <motion.h1
              className="text-3xl font-bold"
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ duration: 0.4 }}
            >
              {exam.title}
            </motion.h1>
            <p className="text-sm text-muted-foreground mt-1">
              <Clock className="inline h-4 w-4 mr-1" />
              {exam.durationMinutes} min &middot; {exam.questions?.length ?? 0} question(s)
            </p>
            <p className="text-sm text-muted-foreground">
              {new Date(exam.startTime).toLocaleString()} &ndash;{' '}
              {new Date(exam.endTime).toLocaleString()}
            </p>
          </div>

          <div className="flex gap-2 flex-wrap">
            <Button variant="outline" size="sm" onClick={() => setShowEdit(true)}>
              <Pencil className="h-4 w-4 mr-1" /> Edit
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => navigate(`/teacher/exams/${examId}/questions`)}
            >
              <FileText className="h-4 w-4 mr-1" /> Questions
            </Button>
            <Button
              variant="destructive"
              size="sm"
              onClick={() => setShowDeleteConfirm(true)}
            >
              <Trash2 className="h-4 w-4 mr-1" /> Delete
            </Button>
          </div>
        </div>

        {/* Stat Cards */}
        <motion.div
          className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-8"
          variants={staggerContainer}
          initial="hidden"
          animate="visible"
        >
          {statCards.map((stat) => (
            <motion.div key={stat.label} variants={fadeInUp}>
              <Card variant="glass">
                <CardContent className="p-6 flex items-center gap-4">
                  <div
                    className={`h-12 w-12 rounded-xl bg-gradient-to-br ${stat.color} flex items-center justify-center`}
                  >
                    <stat.icon className={`h-6 w-6 ${stat.iconColor}`} />
                  </div>
                  <div>
                    <p className="text-2xl font-bold">{stat.value}</p>
                    <p className="text-sm text-muted-foreground">{stat.label}</p>
                  </div>
                </CardContent>
              </Card>
            </motion.div>
          ))}
        </motion.div>

        {/* Grade Stats Detail */}
        {gradeStats && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.4, delay: 0.2 }}
            className="mb-8"
          >
            <Card>
              <CardContent className="p-6">
                <h2 className="text-lg font-semibold mb-4">Score Breakdown</h2>
                <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 text-center">
                  <div>
                    <p className="text-xl font-bold">{gradeStats.highestScore.toFixed(1)}%</p>
                    <p className="text-sm text-muted-foreground">Highest</p>
                  </div>
                  <div>
                    <p className="text-xl font-bold">{gradeStats.lowestScore.toFixed(1)}%</p>
                    <p className="text-sm text-muted-foreground">Lowest</p>
                  </div>
                  <div>
                    <p className="text-xl font-bold">{gradeStats.medianScore.toFixed(1)}%</p>
                    <p className="text-sm text-muted-foreground">Median</p>
                  </div>
                  <div>
                    <p className="text-xl font-bold">{gradeStats.averageScore.toFixed(1)}%</p>
                    <p className="text-sm text-muted-foreground">Average</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </motion.div>
        )}

        {/* Grade Distribution Chart */}
        {distributionEntries.length > 0 && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.4, delay: 0.3 }}
          >
            <Card>
              <CardContent className="p-6">
                <h2 className="text-lg font-semibold mb-4">Grade Distribution</h2>
                <div className="flex items-end gap-4 h-48">
                  {['A', 'B', 'C', 'D', 'F'].map((letter) => {
                    const count = gradeDistribution?.[letter] ?? 0;
                    const pct = maxCount > 0 ? (count / maxCount) * 100 : 0;
                    return (
                      <div key={letter} className="flex-1 flex flex-col items-center gap-1">
                        <span className="text-sm font-medium">{count}</span>
                        <div className="w-full flex items-end" style={{ height: '140px' }}>
                          <div
                            className={`w-full rounded-t-md ${GRADE_COLORS[letter] ?? 'bg-primary'}`}
                            style={{ height: `${Math.max(pct, 4)}%` }}
                          />
                        </div>
                        <span className="text-sm font-semibold">{letter}</span>
                      </div>
                    );
                  })}
                </div>
              </CardContent>
            </Card>
          </motion.div>
        )}

        {/* Edit Modal */}
        <Modal open={showEdit} onClose={() => setShowEdit(false)} title="Edit Exam">
          <form
            onSubmit={editForm.handleSubmit((data) => updateMutation.mutate(data))}
            className="space-y-5"
          >
            <Input
              label="Title"
              placeholder="Enter the exam title"
              {...editForm.register('title', { required: true })}
            />
            <Input
              label="Duration (minutes)"
              type="number"
              placeholder="60"
              hint="How long students have to complete the exam."
              {...editForm.register('durationMinutes', {
                valueAsNumber: true,
                required: true,
              })}
            />
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <Input
                label="Start Time"
                type="datetime-local"
                hint="When the exam becomes available."
                {...editForm.register('startTime', { required: true })}
              />
              <Input
                label="End Time"
                type="datetime-local"
                hint="When the exam closes."
                {...editForm.register('endTime', { required: true })}
              />
            </div>
            <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
              <Button type="submit" loading={updateMutation.isPending}>
                Save Changes
              </Button>
              <Button
                variant="outline"
                type="button"
                onClick={() => setShowEdit(false)}
              >
                Cancel
              </Button>
            </div>
          </form>
        </Modal>

        {/* Delete Confirmation Modal */}
        <Modal
          open={showDeleteConfirm}
          onClose={() => setShowDeleteConfirm(false)}
          title="Delete Exam"
        >
          <div className="space-y-5">
            <div className="flex items-start gap-4 rounded-lg border border-destructive/30 bg-destructive/5 p-4">
              <AlertTriangle className="h-5 w-5 text-destructive shrink-0 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-foreground mb-1">
                  This action cannot be undone.
                </p>
                <p className="text-sm text-muted-foreground">
                  Are you sure you want to delete <strong>{exam.title}</strong>? All questions,
                  submissions, and grades associated with this exam will be permanently removed.
                </p>
              </div>
            </div>
            <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
              <Button
                variant="destructive"
                onClick={() => deleteMutation.mutate()}
                loading={deleteMutation.isPending}
              >
                Delete Exam
              </Button>
              <Button
                variant="outline"
                onClick={() => setShowDeleteConfirm(false)}
              >
                Cancel
              </Button>
            </div>
          </div>
        </Modal>
      </div>
    </AnimatedPage>
  );
}


