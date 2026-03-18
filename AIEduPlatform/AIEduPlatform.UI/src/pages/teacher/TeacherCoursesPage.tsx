import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { coursesApi } from '@/api/courses.api';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { PageSpinner } from '@/components/ui/Spinner';
import { EmptyState } from '@/components/ui/Feedback';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { BookOpen, Plus, Pencil, Trash2, Globe } from 'lucide-react';
import { Modal } from '@/components/ui/Modal';
import { useState } from 'react';

export default function TeacherCoursesPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const { data: courses, isLoading } = useQuery({
    queryKey: ['my-courses'],
    queryFn: () => coursesApi.getMyCourses(),
    select: (res) => res.data.data?.items ?? [],
  });

  const publishMutation = useMutation({
    mutationFn: (courseId: string) => coursesApi.publish(courseId),
    onSuccess: () => {
      toast.success('Course published!');
      queryClient.invalidateQueries({ queryKey: ['my-courses'] });
    },
    onError: () => toast.error('Failed to publish'),
  });

  const deleteMutation = useMutation({
    mutationFn: (courseId: string) => coursesApi.delete(courseId),
    onSuccess: () => {
      toast.success('Course deleted');
      queryClient.invalidateQueries({ queryKey: ['my-courses'] });
      setDeleteId(null);
    },
    onError: () => toast.error('Failed to delete'),
  });

  if (isLoading) return <PageSpinner />;

  return (
    <AnimatedPage>
    <div className="max-w-5xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-3xl font-bold">My Courses</h1>
        <Button onClick={() => navigate('/teacher/courses/create')}>
          <Plus className="h-4 w-4 mr-2" /> Create Course
        </Button>
      </div>

      {!courses || courses.length === 0 ? (
        <EmptyState
          icon={<BookOpen className="h-12 w-12" />}
          title="No courses yet"
          description="Create your first course to get started"
          action={
            <Button onClick={() => navigate('/teacher/courses/create')}>
              Create Course
            </Button>
          }
        />
      ) : (
        <div className="space-y-4">
          {courses.map((course: any) => (
            <Card key={course.courseId}>
              <CardContent className="p-4 flex items-center gap-4">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-1">
                    <h3 className="font-semibold">{course.title}</h3>
                    <Badge variant={course.isPublished ? 'success' : 'outline'}>
                      {course.isPublished ? 'Published' : 'Draft'}
                    </Badge>
                  </div>
                  <p className="text-sm text-muted-foreground">
                    {course.lectureCount} lectures · {course.enrollmentCount} students · ${course.price?.toFixed(2) || '0.00'}
                  </p>
                </div>
                <div className="flex gap-2">
                  {!course.isPublished && (
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => publishMutation.mutate(course.courseId)}
                    >
                      <Globe className="h-4 w-4 mr-1" /> Publish
                    </Button>
                  )}
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => navigate(`/teacher/courses/${course.courseId}`)}
                  >
                    <Pencil className="h-4 w-4 mr-1" /> Manage
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => setDeleteId(course.courseId)}
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      <Modal
        open={!!deleteId}
        onClose={() => setDeleteId(null)}
        title="Delete Course"
      >
        <p className="mb-4">Are you sure? This action cannot be undone.</p>
        <div className="flex gap-2 justify-end">
          <Button variant="outline" onClick={() => setDeleteId(null)}>Cancel</Button>
          <Button
            variant="destructive"
            onClick={() => deleteId && deleteMutation.mutate(deleteId)}
            loading={deleteMutation.isPending}
          >
            Delete
          </Button>
        </div>
      </Modal>
    </div>
    </AnimatedPage>
  );
}
