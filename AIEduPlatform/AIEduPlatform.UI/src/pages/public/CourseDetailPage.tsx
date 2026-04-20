import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { coursesApi } from '@/api/courses.api';
import { reviewsApi } from '@/api/reviews.api';
import { cartApi } from '@/api/cart.api';
import { useAuthStore } from '@/stores/authStore';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { Modal } from '@/components/ui/Modal';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { StarRating } from '@/components/ui/StarRating';
import { Pagination } from '@/components/ui/Pagination';
import { Textarea } from '@/components/ui/Textarea';
import { useState } from 'react';
import { toast } from 'sonner';
import {
  BookOpen,
  Users,
  ShoppingCart,
  Pencil,
  Trash2,
  AlertTriangle,
  Star,
  Layers,
  GraduationCap,
  CheckCircle2,
} from 'lucide-react';
import { formatDate } from '@/utils/formatters';
import { resolveUrl } from '@/utils/url';
import { Link } from 'react-router-dom';
import Skeleton from 'react-loading-skeleton';

export default function CourseDetailPage() {
  const { courseId } = useParams<{ courseId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { isAuthenticated, userId, isTeacher } = useAuthStore();
  const [reviewPage, setReviewPage] = useState(1);
  const [reviewRating, setReviewRating] = useState(0);
  const [reviewComment, setReviewComment] = useState('');
  const [editingReview, setEditingReview] = useState<{ id: string; rating: number; comment: string } | null>(null);
  const [deleteReviewId, setDeleteReviewId] = useState<string | null>(null);

  const { data: course, isLoading } = useQuery({
    queryKey: ['course', courseId],
    queryFn: () => coursesApi.getById(courseId!),
    select: (res) => res.data.data,
    enabled: !!courseId,
  });

  const { data: rating } = useQuery({
    queryKey: ['course-rating', courseId],
    queryFn: () => reviewsApi.getRating(courseId!),
    select: (res) => res.data.data,
    enabled: !!courseId,
  });

  const { data: reviews } = useQuery({
    queryKey: ['course-reviews', courseId, reviewPage],
    queryFn: () => reviewsApi.getByCourse(courseId!, { page: reviewPage }),
    select: (res) => res.data.data,
    enabled: !!courseId,
  });

  const enrollMutation = useMutation({
    mutationFn: () => coursesApi.enroll(courseId!),
    onSuccess: () => {
      toast.success('Enrolled successfully!');
      queryClient.invalidateQueries({ queryKey: ['course', courseId] });
    },
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
  });

  const addToCartMutation = useMutation({
    mutationFn: () => cartApi.addItem(courseId!),
    onSuccess: () => {
      toast.success('Added to cart!');
      queryClient.invalidateQueries({ queryKey: ['cart'] });
    },
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
  });

  const addReviewMutation = useMutation({
    mutationFn: () => reviewsApi.add(courseId!, { rating: reviewRating, comment: reviewComment }),
    onSuccess: () => {
      toast.success('Review submitted!');
      setReviewRating(0);
      setReviewComment('');
      queryClient.invalidateQueries({ queryKey: ['course', courseId] });
      queryClient.invalidateQueries({ queryKey: ['course-reviews', courseId] });
      queryClient.invalidateQueries({ queryKey: ['course-rating', courseId] });
    },
    onError: (err: any) => {
      const msg = err?.response?.status === 409
        ? 'You have already reviewed this course'
        : 'Failed to submit review';
      toast.error(msg);
    },
  });

  const updateReviewMutation = useMutation({
    mutationFn: ({ reviewId, data }: { reviewId: string; data: { rating: number; comment: string } }) =>
      reviewsApi.update(reviewId, data),
    onSuccess: () => {
      toast.success('Review updated!');
      setEditingReview(null);
      queryClient.invalidateQueries({ queryKey: ['course', courseId] });
      queryClient.invalidateQueries({ queryKey: ['course-reviews', courseId] });
      queryClient.invalidateQueries({ queryKey: ['course-rating', courseId] });
    },
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
  });

  const deleteReviewMutation = useMutation({
    mutationFn: (reviewId: string) => reviewsApi.delete(reviewId),
    onSuccess: () => {
      toast.success('Review deleted');
      setDeleteReviewId(null);
      queryClient.invalidateQueries({ queryKey: ['course', courseId] });
      queryClient.invalidateQueries({ queryKey: ['course-reviews', courseId] });
      queryClient.invalidateQueries({ queryKey: ['course-rating', courseId] });
    },
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
  });

  if (isLoading) {
    return (
      <AnimatedPage>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="grid gap-8 lg:grid-cols-12">
            <div className="lg:col-span-8 space-y-8">
              <Skeleton height={280} borderRadius={20} />
              <Skeleton height={260} borderRadius={16} />
              <Skeleton height={220} borderRadius={16} />
              <Skeleton height={380} borderRadius={16} />
            </div>
            <div className="lg:col-span-4 space-y-4">
              <Skeleton height={420} borderRadius={16} />
              <Skeleton height={180} borderRadius={16} />
            </div>
          </div>
        </div>
      </AnimatedPage>
    );
  }
  if (!course) return <div className="p-8 text-center">Course not found</div>;

  const isOwner = isTeacher() && course.teacherId === userId();
  const thumbnailUrl = resolveUrl(course.thumbnailUrl) ?? '/placeholders/course-thumbnail.svg';
  const sortedLectures = [...(course.lectures ?? [])].sort((a, b) => a.orderIndex - b.orderIndex);
  const totalMaterials = sortedLectures.reduce(
    (sum, lecture) => sum + lecture.materialCount,
    0
  );
  const averageRating = rating?.averageRating ?? course.averageRating ?? 0;
  const totalReviews = rating?.totalReviews ?? course.reviewCount ?? 0;
  const ratingDistribution = rating?.ratingDistribution ?? {};
  const ratingBreakdown = [5, 4, 3, 2, 1].map((stars) => {
    const count = Number(ratingDistribution[String(stars)] ?? 0);
    const percentage = totalReviews > 0 ? (count / totalReviews) * 100 : 0;

    return {
      stars,
      count,
      percentage,
    };
  });

  const renderEnrollButton = () => {
    const buttonClass = 'w-full';

    if (!isAuthenticated) {
      return (
        <Button className={buttonClass} variant="gradient" onClick={() => navigate('/login')}>
          Login to Enroll
        </Button>
      );
    }

    if (isOwner) {
      return (
        <Button className={buttonClass} onClick={() => navigate(`/teacher/courses/${courseId}`)}>
          Manage Course
        </Button>
      );
    }

    if (course.isEnrolled) {
      return (
        <Button className={buttonClass} onClick={() => navigate(`/courses/${courseId}/learn`)}>
          Continue Learning
        </Button>
      );
    }

    if (course!.price === 0) {
      return (
        <Button
          className={buttonClass}
          variant="gradient"
          onClick={() => enrollMutation.mutate()}
          loading={enrollMutation.isPending}
        >
          Enroll for Free
        </Button>
      );
    }

    return (
      <Button
        className={buttonClass}
        variant="gradient"
        onClick={() => addToCartMutation.mutate()}
        loading={addToCartMutation.isPending}
      >
        <ShoppingCart className="h-4 w-4 mr-2" /> Add to Cart — ${course!.price.toFixed(2)}
      </Button>
    );
  };

  return (
    <AnimatedPage>
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid gap-8 lg:grid-cols-12">
          <div className="lg:col-span-8 space-y-8">
            <section className="relative overflow-hidden rounded-3xl border border-border bg-card p-6 md:p-8">
              <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_top_right,rgba(59,130,246,0.12),transparent_45%),radial-gradient(circle_at_bottom_left,rgba(6,182,212,0.12),transparent_45%)]" />

              <div className="relative space-y-5">
                <div className="flex flex-wrap items-center gap-2">
                  <Badge variant={course.isFree ? 'success' : 'info'}>
                    {course.isFree ? 'Free Course' : 'Premium Course'}
                  </Badge>
                  {course.isPublished && <Badge variant="outline">Published</Badge>}
                </div>

                <h1 className="text-3xl md:text-4xl font-extrabold tracking-tight leading-tight">
                  {course.title}
                </h1>

                <p className="text-base text-muted-foreground leading-relaxed">
                  {course.description}
                </p>

                <div className="flex flex-wrap items-center gap-4 text-sm text-muted-foreground">
                  <span className="flex items-center gap-1.5">
                    By
                    <Link
                        to={`/profile/${course.teacherId}`}
                        className="flex items-center gap-2 group"
                      >
                        <div className="h-6 w-6 rounded-full overflow-hidden border border-border bg-muted flex items-center justify-center">
                           <img src={`https://api.dicebear.com/7.x/initials/svg?seed=${course.teacherName}`} alt={course.teacherName || 'Instructor'} className="h-full w-full object-cover" />
                        </div>
                        <span className="text-primary group-hover:underline font-medium">
                          {course.teacherName || 'Instructor unavailable'}
                        </span>                      </Link>
                  </span>                  <span className="flex items-center gap-1.5">
                    <Star className="h-4 w-4 text-warning" />
                    {averageRating > 0 ? averageRating.toFixed(1) : 'No rating yet'}
                  </span>

                  <span className="text-sm text-muted-foreground">
                    {totalReviews} reviews
                  </span>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                  <div className="rounded-xl border border-border bg-background/70 p-3">
                    <p className="text-xs uppercase tracking-wide text-muted-foreground">Lectures</p>
                    <p className="mt-1 text-xl font-semibold">{sortedLectures.length}</p>
                  </div>
                  <div className="rounded-xl border border-border bg-background/70 p-3">
                    <p className="text-xs uppercase tracking-wide text-muted-foreground">Materials</p>
                    <p className="mt-1 text-xl font-semibold">{totalMaterials}</p>
                  </div>
                  <div className="rounded-xl border border-border bg-background/70 p-3">
                    <p className="text-xs uppercase tracking-wide text-muted-foreground">Students</p>
                    <p className="mt-1 text-xl font-semibold">{course.enrollmentCount}</p>
                  </div>
                </div>

                <div className="flex flex-wrap gap-2">
                  {course.categories?.map((cat) => (
                    <Badge key={cat.id} variant="outline">
                      {cat.name}
                    </Badge>
                  ))}
                </div>
              </div>
            </section>

            <section className="overflow-hidden rounded-2xl border border-border bg-card">
              <div className="flex items-center justify-between gap-4 border-b border-border px-6 py-5">
                <h2 className="text-xl font-bold">Course Content</h2>
                <Badge variant="info">{sortedLectures.length} lectures</Badge>
              </div>

              {sortedLectures.length > 0 ? (
                <div className="divide-y divide-border">
                  {sortedLectures.map((lecture, idx) => {
                    const materialCount = lecture.materialCount;

                    return (
                      <div key={lecture.id} className="flex items-start gap-4 px-6 py-4">
                        <div className="mt-0.5 h-8 w-8 rounded-full bg-secondary text-secondary-foreground text-sm font-semibold flex items-center justify-center shrink-0">
                          {idx + 1}
                        </div>

                        <div className="min-w-0 flex-1">
                          <p className="font-semibold leading-snug">{lecture.title}</p>
                          <p className="text-sm text-muted-foreground mt-1 line-clamp-2">
                            {lecture.description?.trim() || 'No description provided yet.'}
                          </p>
                        </div>

                        <Badge variant="outline" className="shrink-0 whitespace-nowrap">
                          {materialCount} materials
                        </Badge>
                      </div>
                    );
                  })}
                </div>
              ) : (
                <div className="px-6 py-10 text-sm text-muted-foreground">
                  This course does not have published lectures yet.
                </div>
              )}
            </section>

            <section className="rounded-2xl border border-border bg-card p-6">
              <h2 className="text-xl font-bold">Ratings</h2>

              <div className="mt-5 grid gap-6 md:grid-cols-[220px_1fr]">
                <div className="rounded-xl border border-border bg-background/60 p-5 text-center">
                  <p className="text-5xl font-extrabold leading-none">
                    {averageRating > 0 ? averageRating.toFixed(1) : '—'}
                  </p>
                  <div className="mt-3 flex items-center justify-center">
                    <StarRating rating={averageRating} />
                  </div>
                  <p className="mt-2 text-sm text-muted-foreground">
                    {totalReviews} total review{totalReviews === 1 ? '' : 's'}
                  </p>
                </div>

                <div className="space-y-3">
                  {ratingBreakdown.map((row) => (
                    <div key={row.stars} className="flex items-center gap-3">
                      <span className="w-12 text-sm font-medium text-muted-foreground">
                        {row.stars} star
                      </span>

                      <div className="h-2.5 flex-1 rounded-full bg-secondary overflow-hidden">
                        <div
                          className="h-full rounded-full bg-primary"
                          style={{ width: `${row.percentage}%` }}
                        />
                      </div>

                      <span className="w-10 text-right text-sm text-muted-foreground">
                        {row.count}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            </section>

            <section className="rounded-2xl border border-border bg-card p-6">
              <div className="flex items-center justify-between gap-4 mb-5">
                <h2 className="text-xl font-bold">Reviews</h2>
                <span className="text-sm text-muted-foreground">{totalReviews} total</span>
              </div>

              {isAuthenticated && course.isEnrolled && !isOwner && (
                <Card className="mb-6 border-border">
                  <CardHeader>
                    <CardTitle>Write a Review</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-5">
                      <div>
                        <label className="block text-sm font-medium text-foreground mb-2">Your rating</label>
                        <StarRating rating={reviewRating} interactive onChange={setReviewRating} />
                      </div>

                      <Textarea
                        label="Your review"
                        placeholder="Share your experience with this course... What did you learn? Would you recommend it?"
                        hint="Your review helps other students make informed decisions"
                        value={reviewComment}
                        onChange={(e) => setReviewComment(e.target.value)}
                      />

                      <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
                        <Button
                          variant="gradient"
                          onClick={() => addReviewMutation.mutate()}
                          loading={addReviewMutation.isPending}
                          disabled={reviewRating === 0}
                        >
                          Submit Review
                        </Button>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              )}

              {reviews && reviews.items.length > 0 ? (
                <>
                  <div className="space-y-4">
                    {reviews.items.map((review: any) => (
                      <Card key={review.id} className="border-border">
                        <CardContent className="p-5">
                          {editingReview?.id === review.id && editingReview ? (
                            <div className="space-y-5">
                              <h4 className="text-base font-semibold">Edit Review</h4>
                              <div>
                                <label className="block text-sm font-medium text-foreground mb-2">Rating</label>
                                <StarRating
                                  rating={editingReview.rating}
                                  interactive
                                  onChange={(r) => setEditingReview({ ...editingReview, rating: r })}
                                />
                              </div>

                              <Textarea
                                label="Comment"
                                placeholder="Update your review..."
                                value={editingReview.comment}
                                onChange={(e) => setEditingReview({ ...editingReview, comment: e.target.value })}
                              />

                              <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
                                <Button
                                  onClick={() =>
                                    updateReviewMutation.mutate({
                                      reviewId: editingReview.id,
                                      data: {
                                        rating: editingReview.rating,
                                        comment: editingReview.comment,
                                      },
                                    })
                                  }
                                  loading={updateReviewMutation.isPending}
                                  disabled={editingReview.rating === 0}
                                >
                                  Save
                                </Button>
                                <Button variant="outline" onClick={() => setEditingReview(null)}>
                                  Cancel
                                </Button>
                              </div>
                            </div>
                          ) : (
                            <>
                              <div className="flex items-start justify-between gap-3 mb-3">
                                <div className="min-w-0 flex items-start gap-3">
                                  <Link to={`/profile/${review.studentId}`} className="shrink-0 flex items-center justify-center">
                                    <div className="h-10 w-10 rounded-full border border-border bg-muted overflow-hidden flex items-center justify-center">
                                      <img src={review.studentAvatarUrl || `https://api.dicebear.com/7.x/initials/svg?seed=${review.studentName}`} alt={review.studentName} className="h-full w-full object-cover" />
                                    </div>
                                  </Link>
                                  <div>
                                    <div className="flex items-center gap-2 flex-wrap">
                                      <Link to={`/profile/${review.studentId}`} className="font-medium hover:underline hover:text-primary transition-colors">
                                        {review.studentName}
                                      </Link>
                                      <StarRating rating={review.rating} size="sm" />
                                    </div>
                                    <span className="text-xs text-muted-foreground">
                                      {formatDate(review.createdAt)}
                                    </span>
                                  </div>
                                </div>

                                {isAuthenticated && review.studentId === userId() && (
                                  <div className="flex items-center gap-2 shrink-0">
                                    <Button
                                      variant="ghost"
                                      size="icon"
                                      onClick={() =>
                                        setEditingReview({
                                          id: review.id ?? '',
                                          rating: review.rating ?? 0,
                                          comment: review.comment ?? '',
                                        })
                                      }
                                    >
                                      <Pencil className="h-4 w-4" />
                                    </Button>

                                    <Button
                                      variant="ghost"
                                      size="icon"
                                      onClick={() => setDeleteReviewId(review.id)}
                                    >
                                      <Trash2 className="h-4 w-4 text-destructive" />
                                    </Button>
                                  </div>
                                )}
                              </div>

                              <p className="text-sm text-foreground/90">
                                {review.comment?.trim() || 'No written feedback provided.'}
                              </p>
                            </>
                          )}
                        </CardContent>
                      </Card>
                    ))}
                  </div>

                  <div className="mt-5">
                    <Pagination
                      page={reviews.page}
                      totalPages={reviews.totalPages}
                      onPageChange={setReviewPage}
                      hasPrevious={reviews.hasPrevious}
                      hasNext={reviews.hasNext}
                    />
                  </div>
                </>
              ) : (
                <div className="rounded-xl border border-dashed border-border p-6 text-sm text-muted-foreground text-center">
                  No reviews yet. Be the first to share your feedback.
                </div>
              )}
            </section>
          </div>

          <aside className="lg:col-span-4">
            <div className="space-y-4 lg:sticky lg:top-20">
              <Card className="overflow-hidden border-border">
                <img
                  src={thumbnailUrl}
                  alt={course.title}
                  className="w-full aspect-video object-cover"
                  onError={(event) => {
                    event.currentTarget.src = '/placeholders/course-thumbnail.svg';
                  }}
                />

                <CardContent className="p-6 space-y-5">
                  <div className="flex items-end justify-between gap-4">
                    <p className="text-3xl font-extrabold leading-none">
                      {course.price === 0 ? 'Free' : `$${course.price.toFixed(2)}`}
                    </p>

                    {course.isEnrolled && (
                      <Badge variant="success" className="inline-flex items-center gap-1">
                        <CheckCircle2 className="h-3.5 w-3.5" />
                        Enrolled
                      </Badge>
                    )}
                  </div>

                  {renderEnrollButton()}

                  <p className="text-xs text-muted-foreground leading-relaxed">
                    {course.isFree
                      ? 'Enroll instantly and start learning right away.'
                      : 'Secure checkout, lifetime access, and progress tracking included.'}
                  </p>
                </CardContent>
              </Card>

              <Card className="border-border">
                <CardHeader className="pb-3">
                  <CardTitle className="text-base">At a Glance</CardTitle>
                </CardHeader>

                <CardContent className="space-y-3 text-sm">
                  <div className="flex items-center justify-between">
                    <span className="inline-flex items-center gap-2 text-muted-foreground">
                      <BookOpen className="h-4 w-4" /> Lectures
                    </span>
                    <span className="font-medium">{sortedLectures.length}</span>
                  </div>

                  <div className="flex items-center justify-between">
                    <span className="inline-flex items-center gap-2 text-muted-foreground">
                      <Layers className="h-4 w-4" /> Materials
                    </span>
                    <span className="font-medium">{totalMaterials}</span>
                  </div>

                  <div className="flex items-center justify-between">
                    <span className="inline-flex items-center gap-2 text-muted-foreground">
                      <Users className="h-4 w-4" /> Enrolled
                    </span>
                    <span className="font-medium">{course.enrollmentCount}</span>
                  </div>

                  <div className="flex items-center justify-between">
                    <span className="inline-flex items-center gap-2 text-muted-foreground">
                      <GraduationCap className="h-4 w-4" /> Instructor
                    </span>
                  <Link
                    to={`/profile/${course.teacherId}`}
                    className="flex items-center gap-2 text-foreground hover:text-primary transition-colors"
                  >
                    <div className="h-6 w-6 rounded-full border border-border bg-muted overflow-hidden flex items-center justify-center">
                       <img src={`https://api.dicebear.com/7.x/initials/svg?seed=${course.teacherName}`} alt={course.teacherName || 'Instructor'} className="h-full w-full object-cover" />    
                    </div>
                    <span className="font-medium max-w-[150px] truncate group-hover:underline" title={course.teacherName}>
                      {course.teacherName || 'Instructor unavailable'}
                    </span>
                  </Link>
                  </div>
                </CardContent>
              </Card>
            </div>
          </aside>
        </div>
      </div>

      <Modal open={!!deleteReviewId} onClose={() => setDeleteReviewId(null)} title="Delete Review">
        <div className="space-y-4">
          <div className="flex items-start gap-3 rounded-lg bg-destructive/10 border border-destructive/20 p-4">
            <AlertTriangle className="h-5 w-5 text-destructive shrink-0 mt-0.5" />
            <div>
              <p className="text-sm font-medium text-destructive">This action cannot be undone</p>
              <p className="text-sm text-muted-foreground mt-1">
                Are you sure you want to permanently delete this review?
              </p>
            </div>
          </div>
          <div className="flex items-center justify-end gap-3 pt-4 border-t border-border">
            <Button variant="outline" onClick={() => setDeleteReviewId(null)}>Cancel</Button>
            <Button
              variant="destructive"
              loading={deleteReviewMutation.isPending}
              onClick={() => deleteReviewId && deleteReviewMutation.mutate(deleteReviewId)}
            >
              Delete Review
            </Button>
          </div>
        </div>
      </Modal>
    </AnimatedPage>
  );
}

