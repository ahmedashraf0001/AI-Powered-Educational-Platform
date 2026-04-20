import type { CourseListDto } from '@/types';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { StarRating } from '@/components/ui/StarRating';
import { Button } from '@/components/ui/Button';
import { Link } from 'react-router-dom';
import { Users, BookOpen, ShoppingCart } from 'lucide-react';
import { motion } from 'framer-motion';
import { resolveUrl } from '@/utils/url';
import { useEffect, useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { cartApi } from '@/api/cart.api';
import { toast } from 'sonner';
import { useAuthStore } from '@/stores/authStore';

interface CourseCardProps {
  course: CourseListDto;
}

export function CourseCard({ course }: CourseCardProps) {
  const thumbnailUrl = resolveUrl(course.thumbnailUrl) ?? '/placeholders/course-thumbnail.svg';
  const [imageSrc, setImageSrc] = useState(thumbnailUrl);
  const queryClient = useQueryClient();
  const { isAuthenticated } = useAuthStore();

  const addToCartMutation = useMutation({
    mutationFn: () => cartApi.addItem(course.courseId),
    onSuccess: () => {
      toast.success('Added to cart!');
      queryClient.invalidateQueries({ queryKey: ['cart'] });
    },
    onError: (error: any) => {
      const msg = error?.response?.data?.message || 'Failed to add to cart';
      toast.error(msg);
    },
  });

  const handleAddToCart = (e: React.MouseEvent) => {
    e.preventDefault(); // Prevent navigating to course details
    e.stopPropagation();

    if (!isAuthenticated) {
      toast.error('Please login to add items to cart');
      return;
    }

    addToCartMutation.mutate();
  };

  useEffect(() => {
    setImageSrc(thumbnailUrl);
  }, [thumbnailUrl]);

  return (
    <Link to={`/courses/${course.courseId}`} className="group">
      <motion.div
        whileHover={{ y: -4 }}
        transition={{ type: 'spring', stiffness: 300, damping: 20 }}
      >
        <Card className="h-full hover:shadow-xl hover:shadow-primary/5 transition-all duration-300 cursor-pointer overflow-hidden">
          <div className="overflow-hidden">
            <img
              src={imageSrc}
              alt={course.title}
              className="w-full h-40 object-cover transition-transform duration-500 group-hover:scale-105"
              onError={() => setImageSrc('/placeholders/course-thumbnail.svg')}
            />
          </div>
          <CardContent className="p-4 space-y-3">
            <h3 className="font-semibold line-clamp-2">{course.title}</h3>
            <p className="text-sm text-muted-foreground">{course.teacherName}</p>
            <div className="flex items-center gap-2">
              <StarRating rating={course.averageRating} size="sm" />
              <span className="text-xs text-muted-foreground">
                ({course.reviewCount})
              </span>
            </div>
            <div className="flex items-center gap-4 text-xs text-muted-foreground">
              <span className="flex items-center gap-1">
                <BookOpen className="h-3 w-3" />
                {course.lectureCount} lectures
              </span>
              <span className="flex items-center gap-1">
                <Users className="h-3 w-3" />
                {course.enrollmentCount}
              </span>
            </div>
            <div className="flex items-center justify-between pt-1">
              <span className="font-bold text-lg">
                {course.price === 0 ? (
                  <span className="text-success">Free</span>
                ) : (
                  `$${course.price.toFixed(2)}`
                )}
              </span>
              <div className="flex items-center gap-2">
                {course.isEnrolled ? (
                  <Badge variant="success">Enrolled</Badge>
                ) : course.price > 0 ? (
                  <Button
                    size="sm"
                    variant="gradient"
                    className="h-8 w-8 p-0 rounded-full"
                    onClick={handleAddToCart}
                    disabled={addToCartMutation.isPending}
                    title="Add to Cart"
                  >
                    <ShoppingCart className="h-4 w-4" />
                  </Button>
                ) : null}
              </div>
            </div>
          </CardContent>
        </Card>
      </motion.div>
    </Link>
  );
}
