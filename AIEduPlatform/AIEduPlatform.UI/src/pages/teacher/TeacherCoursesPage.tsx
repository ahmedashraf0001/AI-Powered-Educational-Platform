import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { coursesApi } from '@/api/courses.api';
import { Card, CardContent, CardFooter } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { PageSpinner } from '@/components/ui/Spinner';
import { EmptyState } from '@/components/ui/Feedback';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { BookOpen, Plus, Pencil, Trash2, Globe, Search, Users, Video } from 'lucide-react';
import { Modal } from '@/components/ui/Modal';
import { Pagination } from '@/components/ui/Pagination';
import { useState, useMemo, useEffect } from 'react';
import { motion } from 'framer-motion';
import { resolveUrl } from '@/utils/url';

export default function TeacherCoursesPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [deleteId, setDeleteId] = useState<string | null>(null);

  // Filters
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [sortBy, setSortBy] = useState('newest');

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const ITEMS_PER_PAGE = 6;

  // Reset page to 1 when filters change
  useEffect(() => {
    setCurrentPage(1);
  }, [searchQuery, statusFilter, sortBy]);

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
    onError: (error: any) => toast.error(error?.userMessage ?? 'Failed to publish course'),
  });

  const deleteMutation = useMutation({
    mutationFn: (courseId: string) => coursesApi.delete(courseId),
    onSuccess: (res) => {
      const message = res?.data?.message || res?.data?.data?.message || 'Course removed';
      toast.success(message);
      queryClient.invalidateQueries({ queryKey: ['my-courses'] });
      setDeleteId(null);
    },
    onError: (error: any) => toast.error(error?.userMessage ?? 'Failed to delete course'),
  });

  // Filter & Sort Logic
  const processedCourses = useMemo(() => {
    if (!courses) return [];

    let result = [...courses];

    // Search filter
    if (searchQuery.trim()) {
      const q = searchQuery.toLowerCase();
      result = result.filter(c => c.title?.toLowerCase().includes(q));
    }

    // Status filter
    if (statusFilter === 'published') {
      result = result.filter(c => c.isPublished);
    } else if (statusFilter === 'draft') {
      result = result.filter(c => !c.isPublished);
    }

    // Sort
    result.sort((a, b) => {
      switch (sortBy) {
        case 'newest':
          return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
        case 'oldest':
          return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
        case 'title-asc':
          return (a.title || '').localeCompare(b.title || '');
        case 'title-desc':
          return (b.title || '').localeCompare(a.title || '');
        case 'students':
          return (b.enrollmentCount || 0) - (a.enrollmentCount || 0);
        default:
          return 0;
      }
    });

    return result;
  }, [courses, searchQuery, statusFilter, sortBy]);

  const totalPages = Math.ceil((processedCourses?.length || 0) / ITEMS_PER_PAGE);
  const paginatedCourses = useMemo(() => {
    return processedCourses.slice(
      (currentPage - 1) * ITEMS_PER_PAGE,
      currentPage * ITEMS_PER_PAGE
    );
  }, [processedCourses, currentPage]);

  if (isLoading) return <PageSpinner />;

  return (
    <AnimatedPage>
    <div className="max-w-6xl mx-auto px-4 py-8 space-y-8">
      
      {/* Header */}
      <div className="flex flex-col md:flex-row items-baseline md:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">My Courses</h1>
          <p className="text-muted-foreground mt-1">Manage, publish, and track your created course materials.</p>
        </div>
        <Button size="lg" className="w-full md:w-auto shadow-sm" onClick={() => navigate('/teacher/courses/create')}>
          <Plus className="h-5 w-5 mr-2" /> Create Course
        </Button>
      </div>

      {/* Stats Quick Look */}
      {courses && courses.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <Card variant="glass" className="bg-primary/5 border-primary/10">
            <CardContent className="p-4 flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Total Courses</p>
                <p className="text-2xl font-bold">{courses.length}</p>
              </div>
              <div className="p-3 bg-primary/10 rounded-full text-primary">
                <BookOpen className="h-6 w-6" />
              </div>
            </CardContent>
          </Card>
          <Card variant="glass" className="bg-success/5 border-success/10">
            <CardContent className="p-4 flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Published</p>
                <p className="text-2xl font-bold">{courses.filter(c => c.isPublished).length}</p>
              </div>
              <div className="p-3 bg-success/10 rounded-full text-success">
                <Globe className="h-6 w-6" />
              </div>
            </CardContent>
          </Card>
          <Card variant="glass" className="bg-secondary/5 border-secondary/10">
            <CardContent className="p-4 flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Total Students</p>
                <p className="text-2xl font-bold">{courses.reduce((acc, c) => acc + (c.enrollmentCount || 0), 0)}</p>
              </div>
              <div className="p-3 bg-secondary/10 rounded-full text-secondary">
                <Users className="h-6 w-6" />
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Filters */}
      <Card variant="glass" className="p-4 flex flex-col md:flex-row gap-4 relative z-50">
        <div className="relative w-full md:flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input 
            placeholder="Search courses by title..." 
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-9 w-full"
          />
        </div>
        <div className="flex gap-4 w-full md:w-auto">
          <Select
            className="w-full md:w-40"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            options={[
              { value: 'all', label: 'All Statuses' },
              { value: 'published', label: 'Published' },
              { value: 'draft', label: 'Draft' }
            ]}
          />
          <Select
            className="w-full md:w-48"
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value)}
            options={[
              { value: 'newest', label: 'Newest First' },
              { value: 'oldest', label: 'Oldest First' },
              { value: 'students', label: 'Most Students' },
              { value: 'title-asc', label: 'Title (A-Z)' },
              { value: 'title-desc', label: 'Title (Z-A)' }
            ]}
          />
        </div>
      </Card>

      {/* Course List / Grid */}
      {!courses || courses.length === 0 ? (
        <EmptyState
          icon={<BookOpen className="h-12 w-12" />}
          title="No courses yet"
          description="Create your first course to get started on your teaching journey"
          action={
            <Button onClick={() => navigate('/teacher/courses/create')}>
              Create Course
            </Button>
          }
        />
      ) : processedCourses.length === 0 ? (
        <EmptyState
          icon={<Search className="h-12 w-12" />}
          title="No matches found"
          description="Try adjusting your filters or search query"
          action={
            <Button variant="outline" onClick={() => { setSearchQuery(''); setStatusFilter('all'); }}>
              Clear Filters
            </Button>
          }
        />
      ) : (
        <motion.div 
          className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
          initial="hidden"
          animate="show"
          variants={{
            hidden: { opacity: 0 },
            show: {
              opacity: 1,
              transition: { staggerChildren: 0.1 }
            }
          }}
        >
          {paginatedCourses.map((course: any) => (
            <motion.div
              key={course.courseId}
              variants={{
                hidden: { opacity: 0, y: 20 },
                show: { opacity: 1, y: 0 }
              }}
            >
              <Card className="h-full flex flex-col group hover:shadow-lg transition-all duration-300 border-border/50">
                <div className="relative overflow-hidden rounded-t-xl bg-muted aspect-video">
                  <img
                    src={resolveUrl(course.thumbnailUrl) ?? '/placeholders/course-thumbnail.svg'}
                    alt={course.title}
                    className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
                    onError={(e) => {
                      e.currentTarget.src = '/placeholders/course-thumbnail.svg';
                    }}
                  />
                  <div className="absolute inset-0 bg-linear-to-t from-black/35 via-black/10 to-transparent" />
                  <div className="absolute top-4 right-4">
                    <Badge variant={course.isPublished ? 'success' : 'default'} className="shadow-sm">
                      {course.isPublished ? 'Published' : 'Draft'}
                    </Badge>
                  </div>
                </div>

                <CardContent className="p-5 flex-1 -mt-8 relative z-10 flex flex-col">
                  <div className="h-12 w-12 rounded-xl bg-background border shadow-sm flex items-center justify-center mb-4 overflow-hidden">
                    <img
                      src={resolveUrl(course.thumbnailUrl) ?? '/placeholders/course-thumbnail.svg'}
                      alt={course.title}
                      className="h-full w-full object-cover"
                      onError={(e) => {
                        e.currentTarget.src = '/placeholders/course-thumbnail.svg';
                      }}
                    />
                  </div>
                  
                  <h3 className="font-semibold text-lg leading-tight mb-2 line-clamp-2">
                    {course.title}
                  </h3>
                  
                  <div className="mt-auto pt-4 space-y-2 text-sm text-muted-foreground">
                    <div className="flex items-center justify-between">
                      <span className="flex items-center gap-1.5"><Video className="h-4 w-4" /> {course.lectureCount} Lectures</span>
                      <span className="flex items-center gap-1.5"><Users className="h-4 w-4" /> {course.enrollmentCount} Students</span>
                    </div>
                    <div className="flex items-center justify-between pt-2">
                      <span className="font-semibold text-primary">${course.price?.toFixed(2) || '0.00'}</span>
                      <span className="text-xs">
                        {new Date(course.createdAt).toLocaleDateString()}
                      </span>
                    </div>
                  </div>
                </CardContent>

                <CardFooter className="p-5 pt-0 gap-3">
                  <div className="flex-1 flex gap-2">
                    <Button
                      variant="primary"
                      size="sm"
                      className="flex-1 shadow-sm"
                      onClick={() => navigate(`/teacher/courses/${course.courseId}`)}
                    >
                      <Pencil className="h-4 w-4 mr-1.5" /> Manage
                    </Button>
                    {!course.isPublished && (
                      <Button
                        variant="outline"
                        size="sm"
                        className="flex-1"
                        onClick={() => publishMutation.mutate(course.courseId)}
                        loading={publishMutation.isPending && publishMutation.variables === course.courseId}
                      >
                        <Globe className="h-4 w-4 mr-1.5" /> Publish
                      </Button>
                    )}
                  </div>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="text-muted-foreground hover:text-destructive hover:bg-destructive/10 shrink-0"
                    onClick={() => setDeleteId(course.courseId)}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </CardFooter>
              </Card>
            </motion.div>
          ))}
        </motion.div>
      )}

      {totalPages > 1 && (
        <div className="flex justify-center mt-8">
          <Pagination
            page={currentPage}
            totalPages={totalPages}
            onPageChange={setCurrentPage}
            hasNext={currentPage < totalPages}
            hasPrevious={currentPage > 1}
          />
        </div>
      )}

      {/* Delete Modal */}
      <Modal
        open={!!deleteId}
        onClose={() => setDeleteId(null)}
        title="Delete Course"
      >
        <div className="p-1">
          <p className="mb-6 text-muted-foreground text-sm">
            Are you sure you want to delete this course? This action cannot be undone and will remove all associated materials and lectures.
          </p>
          <div className="flex gap-3 justify-end">
            <Button variant="outline" onClick={() => setDeleteId(null)} disabled={deleteMutation.isPending}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={() => deleteId && deleteMutation.mutate(deleteId)}
              loading={deleteMutation.isPending}
            >
              Delete Course
            </Button>
          </div>
        </div>
      </Modal>
    </div>
    </AnimatedPage>
  );
}