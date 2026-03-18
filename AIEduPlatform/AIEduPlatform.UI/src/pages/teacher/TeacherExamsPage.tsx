import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { examsApi } from '@/api/exams.api';
import { coursesApi } from '@/api/courses.api';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Modal } from '@/components/ui/Modal';
import { PageSpinner } from '@/components/ui/Spinner';
import { EmptyState } from '@/components/ui/Feedback';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { useState } from 'react';
import { toast } from 'sonner';
import { Plus, FileText, Pencil } from 'lucide-react';

export default function TeacherExamsPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [showCreate, setShowCreate] = useState(false);

  const { data: courses, isLoading: coursesLoading } = useQuery({
    queryKey: ['my-courses'],
    queryFn: () => coursesApi.getMyCourses(),
    select: (res) => res.data.data?.items ?? [],
  });

  const examForm = useForm<{
    title: string;
    durationMinutes: number;
    startTime: string;
    endTime: string;
    courseId: string;
  }>();

  // Get exams for all courses
  const courseIds = (courses || []).map((c: any) => c.courseId);
  const { data: allExams, isLoading: examsLoading } = useQuery({
    queryKey: ['teacher-exams', courseIds],
    queryFn: async () => {
      const results = await Promise.all(
        courseIds.map((id: string) =>
          examsApi.getByCourse(id)
            .then((res) => res.data.data?.items ?? [])
            .catch(() => [])
        )
      );
      return results.flat();
    },
    enabled: courseIds.length > 0,
  });

  const createMutation = useMutation({
    mutationFn: (data: any) => {
      const { courseId, startTime, endTime, ...examData } = data;
      return examsApi.create(courseId, {
        ...examData,
        startTime: new Date(startTime).toISOString(),
        endTime: new Date(endTime).toISOString(),
      });
    },
    onSuccess: (res) => {
      toast.success('Exam created!');
      setShowCreate(false);
      examForm.reset();
      queryClient.invalidateQueries({ queryKey: ['teacher-exams'] });
      const examId = res.data.data?.examId;
      if (examId) navigate(`/teacher/exams/${examId}/questions`);
    },
    onError: () => toast.error('Failed to create exam'),
  });

  if (coursesLoading || examsLoading) return <PageSpinner />;

  return (
    <AnimatedPage>
    <div className="max-w-5xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-3xl font-bold">Exam Management</h1>
        <Button onClick={() => setShowCreate(true)}>
          <Plus className="h-4 w-4 mr-2" /> Create Exam
        </Button>
      </div>

      {!allExams || allExams.length === 0 ? (
        <EmptyState
          icon={<FileText className="h-12 w-12" />}
          title="No exams yet"
          description="Create your first exam"
        />
      ) : (
        <div className="space-y-4">
          {allExams.map((exam: any) => (
            <Card key={exam.id}>
              <CardContent className="p-4 flex items-center justify-between">
                <div>
                  <div className="flex items-center gap-2 mb-1">
                    <h3 className="font-semibold">{exam.title}</h3>
                    <Badge variant="outline">{exam.durationMinutes} min</Badge>
                    {exam.questionCount != null && (
                      <Badge variant="outline">{exam.questionCount} questions</Badge>
                    )}
                  </div>
                  <p className="text-sm text-muted-foreground">
                    {exam.courseName || exam.courseTitle || ''}
                  </p>
                </div>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => navigate(`/teacher/exams/${exam.id}/questions`)}
                  >
                    <Pencil className="h-4 w-4 mr-1" /> Questions
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => navigate(`/teacher/exams/${exam.id}`)}
                  >
                    Manage
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      <Modal open={showCreate} onClose={() => setShowCreate(false)} title="Create Exam">
        <form
          onSubmit={examForm.handleSubmit((data) => createMutation.mutate(data))}
          className="space-y-5"
        >
          <Select
            label="Course"
            placeholder="Select a course..."
            hint="The exam will be associated with the selected course."
            {...examForm.register('courseId', { required: true })}
            options={
              (courses || []).map((c: any) => ({ value: c.courseId, label: c.title }))
            }
          />
          <Input
            label="Title"
            placeholder="e.g. Midterm Exam, Final Quiz"
            {...examForm.register('title', { required: true })}
          />
          <Input
            label="Duration (minutes)"
            type="number"
            placeholder="60"
            hint="How long students have to complete the exam once started."
            {...examForm.register('durationMinutes', { valueAsNumber: true, required: true })}
          />
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <Input
              label="Start Time"
              type="datetime-local"
              hint="When the exam becomes available."
              {...examForm.register('startTime', { required: true })}
            />
            <Input
              label="End Time"
              type="datetime-local"
              hint="When the exam closes."
              {...examForm.register('endTime', { required: true })}
            />
          </div>
          <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
            <Button type="submit" loading={createMutation.isPending}>Create Exam</Button>
            <Button variant="outline" type="button" onClick={() => setShowCreate(false)}>Cancel</Button>
          </div>
        </form>
      </Modal>
    </div>
    </AnimatedPage>
  );
}
