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
import { PageSpinner } from '@/components/ui/Spinner';
import { Pagination } from '@/components/ui/Pagination';
import { Textarea } from '@/components/ui/Textarea';
import { useState } from 'react';
import { toast } from 'sonner';
import { BookOpen, Users, ShoppingCart, Pencil, Trash2, AlertTriangle } from 'lucide-react';
import { formatDate } from '@/utils/formatters';
import { resolveUrl } from '@/utils/url';
import { Link } from 'react-router-dom';

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
  const [thumbnailFailed, setThumbnailFailed] = useState(false);

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
    onError: () => toast.error('Failed to enroll'),
  });

  const addToCartMutation = useMutation({
    mutationFn: () => cartApi.addItem(courseId!),
    onSuccess: () => {
      toast.success('Added to cart!');
      queryClient.invalidateQueries({ queryKey: ['cart'] });
    },
    onError: () => toast.error('Failed to add to cart'),
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
    onError: () => toast.error('Failed to update review'),
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
    onError: () => toast.error('Failed to delete review'),
  });

  if (isLoading) return <PageSpinner />;
  if (!course) return <div className="p-8 text-center">Course not found</div>;

  const isOwner = isTeacher() && course.teacherId === userId();
  const thumbnailUrl = resolveUrl(course.thumbnailUrl);

  const renderEnrollButton = () => {
    if (!isAuthenticated) {
      return <Button onClick={() => navigate('/login')}>Login to Enroll</Button>;
    }
    if (isOwner) {
      return <Button onClick={() => navigate(`/teacher/courses/${courseId}`)}>Manage Course</Button>;
    }
    if (course.isEnrolled) {
      return <Button onClick={() => navigate(`/courses/${courseId}/learn`)}>Go to Course</Button>;
    }
    if (course!.price === 0) {
      return (
        <Button onClick={() => enrollMutation.mutate()} loading={enrollMutation.isPending}>
          Enroll Now
        </Button>
      );
    }
    return (
      <Button onClick={() => addToCartMutation.mutate()} loading={addToCartMutation.isPending}>
        <ShoppingCart className="h-4 w-4 mr-2" /> Add to Cart — ${course!.price.toFixed(2)}
      </Button>
    );
  };

  return (
    <AnimatedPage>
    <div className="max-w-5xl mx-auto px-4 py-8">
      {/* Header */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-10">
        <div className="md:col-span-2 space-y-4">
          <h1 className="text-3xl font-bold">{course.title}</h1>
          <p className="text-muted-foreground">{course.description}</p>
          <div className="flex flex-wrap items-center gap-4 text-sm text-muted-foreground">
            <span className="flex items-center gap-1 group">
              By <Link to={`/instructor/${course.teacherId}`} className="text-primary hover:underline font-medium">{course.teacherName}</Link>
            </span>
            <span className="flex items-center gap-1"><BookOpen className="h-4 w-4" />{course.lectureCount} lectures</span>
            <span className="flex items-center gap-1"><Users className="h-4 w-4" />{course.enrollmentCount} students</span>
          </div>
          <div className="flex items-center gap-2">
            <StarRating rating={course.averageRating} />
            <span className="text-sm text-muted-foreground">({course.reviewCount} reviews)</span>
          </div>
          <div className="flex flex-wrap gap-2">
            {course.categories?.map((cat) => (
              <Badge key={cat.id} variant="outline">{cat.name}</Badge>
            ))}
          </div>
        </div>
        <div className="border rounded-lg p-6 space-y-4">
          {thumbnailUrl && !thumbnailFailed ? (
            <img
              src={thumbnailUrl}
              alt={course.title}
              className="w-full rounded"
              onError={() => setThumbnailFailed(true)}
            />
          ) : (
            <div className="w-full h-40 bg-gradient-to-br from-primary/20 to-accent/20 rounded flex items-center justify-center">
              <BookOpen className="h-12 w-12 text-primary/40" />
            </div>
          )}
          <div className="text-2xl font-bold">
            {course.price === 0 ? 'Free' : `$${course.price.toFixed(2)}`}
          </div>
          {renderEnrollButton()}
        </div>
      </div>

      {/* Lectures */}
      {course.lectures && course.lectures.length > 0 && (
        <div className="mb-10">
          <h2 className="text-xl font-bold mb-4">Course Content</h2>
          <div className="border rounded-lg divide-y">
            {course.lectures.map((lecture, idx) => (
              <div key={lecture.id} className="p-4 flex items-center gap-3">
                <span className="text-sm text-muted-foreground w-8">{idx + 1}</span>
                <div>
                  <p className="font-medium">{lecture.title}</p>
                  {lecture.description && (
                    <p className="text-sm text-muted-foreground">{lecture.description}</p>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Rating Summary */}
      {rating && (
        <div className="mb-10">
          <h2 className="text-xl font-bold mb-4">Ratings</h2>
          <div className="flex items-center gap-6">
            <div className="text-center">
              <div className="text-4xl font-bold">{rating.averageRating?.toFixed(1) ?? '—'}</div>
              <StarRating rating={rating.averageRating ?? 0} />
            </div>
          </div>
        </div>
      )}

      {/* Reviews */}
      <div className="mb-10">
        <h2 className="text-xl font-bold mb-4">Reviews</h2>

        {/* Write a Review form */}
        {isAuthenticated && course.isEnrolled && !isOwner && (
          <Card className="mb-6">
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
                <Card key={review.id}>
                  <CardContent className="p-5">
                    {editingReview?.id === review.id && editingReview ? (
                      <div className="space-y-5">
                        <h4 className="text-base font-semibold">Edit Review</h4>
                        <div>
                          <label className="block text-sm font-medium text-foreground mb-2">Rating</label>
                          <StarRating rating={editingReview.rating} interactive onChange={(r) => setEditingReview({ ...editingReview, rating: r })} />
                        </div>
                        <Textarea
                          label="Comment"
                          placeholder="Update your review..."
                          value={editingReview.comment}
                          onChange={(e) => setEditingReview({ ...editingReview, comment: e.target.value })}
                        />
                        <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
                          <Button
                            onClick={() => updateReviewMutation.mutate({ reviewId: editingReview.id, data: { rating: editingReview.rating, comment: editingReview.comment } })}
                            loading={updateReviewMutation.isPending}
                            disabled={editingReview.rating === 0}
                          >
                            Save
                          </Button>
                          <Button variant="outline" onClick={() => setEditingReview(null)}>Cancel</Button>
                        </div>
                      </div>
                    ) : (
                      <>
                        <div className="flex items-center justify-between mb-2">
                          <div className="flex items-center gap-2">
                            <span className="font-medium">{review.studentName}</span>
                            <StarRating rating={review.rating} size="sm" />
                          </div>
                          <div className="flex items-center gap-2">
                            {isAuthenticated && review.studentId === userId() && (
                              <>
                                <Button
                                  variant="ghost"
                                  size="icon"
                                  onClick={() => setEditingReview({ id: review.id ?? '', rating: review.rating ?? 0, comment: review.comment ?? '' })}
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
                              </>
                            )}
                            <span className="text-xs text-muted-foreground">{formatDate(review.createdAt)}</span>
                          </div>
                        </div>
                        <p className="text-sm">{review.comment}</p>
                      </>
                    )}
                  </CardContent>
                </Card>
              ))}
            </div>
            <div className="mt-4">
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
          <p className="text-muted-foreground">No reviews yet.</p>
        )}
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
