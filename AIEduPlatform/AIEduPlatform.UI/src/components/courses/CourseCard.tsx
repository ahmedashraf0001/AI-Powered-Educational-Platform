import type { CourseListDto } from '@/types';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { StarRating } from '@/components/ui/StarRating';
import { Link } from 'react-router-dom';
import { Users, BookOpen } from 'lucide-react';
import { motion } from 'framer-motion';
import { resolveUrl } from '@/utils/url';

interface CourseCardProps {
  course: CourseListDto;
}

export function CourseCard({ course }: CourseCardProps) {
  return (
    <Link to={`/courses/${course.courseId}`} className="group">
      <motion.div
        whileHover={{ y: -4 }}
        transition={{ type: 'spring', stiffness: 300, damping: 20 }}
      >
        <Card className="h-full hover:shadow-xl hover:shadow-primary/5 transition-all duration-300 cursor-pointer overflow-hidden">
          <div className="overflow-hidden">
            {course.thumbnailUrl ? (
              <img
                src={resolveUrl(course.thumbnailUrl)!}
                alt={course.title}
                className="w-full h-40 object-cover transition-transform duration-500 group-hover:scale-105"
              />
            ) : (
              <div className="w-full h-40 bg-gradient-to-br from-primary/20 to-accent/20 flex items-center justify-center">
                <BookOpen className="h-12 w-12 text-primary/40" />
              </div>
            )}
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
              {course.isEnrolled && <Badge variant="success">Enrolled</Badge>}
            </div>
          </CardContent>
        </Card>
      </motion.div>
    </Link>
  );
}
