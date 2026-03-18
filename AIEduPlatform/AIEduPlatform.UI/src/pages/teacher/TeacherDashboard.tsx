import { useQuery } from '@tanstack/react-query';
import { usersApi } from '@/api/users.api';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { StatCardSkeleton } from '@/components/ui/Skeleton';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { AnimatedCounter } from '@/components/ui/AnimatedCounter';
import { useNavigate } from 'react-router-dom';
import {
  BookOpen,
  Users,
  FileText,
  AlertCircle,
  Star,
  TrendingUp,
  Calendar,
  GraduationCap,
  BarChart3,
  DollarSign,
} from 'lucide-react';
import { motion } from 'framer-motion';
import { staggerContainer, fadeInUp } from '@/utils/motion';
import { formatDate } from '@/utils/formatters';
import type { TeacherDashboard as TeacherDashboardType } from '@/types';

const statCards = [
  { key: 'total', icon: BookOpen, label: 'Total Courses', color: 'from-primary/20 to-primary/5', iconColor: 'text-primary' },
  { key: 'published', icon: GraduationCap, label: 'Published', color: 'from-success/20 to-success/5', iconColor: 'text-success' },
  { key: 'students', icon: Users, label: 'Students', color: 'from-info/20 to-info/5', iconColor: 'text-info' },
  { key: 'exams', icon: FileText, label: 'Exams', color: 'from-accent/20 to-accent/5', iconColor: 'text-accent' },
] as const;

export default function TeacherDashboard() {
  const navigate = useNavigate();

  const { data: dashboard, isLoading } = useQuery({
    queryKey: ['teacher-dashboard'],
    queryFn: () => usersApi.getTeacherDashboard(),
    select: (res) => res.data.data as TeacherDashboardType,
  });

  const getStatValue = (key: string) => {
    if (!dashboard) return 0;
    switch (key) {
      case 'total': return dashboard.totalCourses ?? 0;
      case 'published': return dashboard.publishedCourses ?? 0;
      case 'students': return dashboard.totalStudents ?? 0;
      case 'exams': return dashboard.totalExamsCreated ?? 0;
      default: return 0;
    }
  };

  return (
    <AnimatedPage>
      <div className="max-w-7xl mx-auto px-4 py-8">
        <motion.h1
          className="text-3xl font-bold mb-8"
          initial={{ opacity: 0, x: -20 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.4 }}
        >
          Teacher Dashboard
        </motion.h1>

        {/* Stats Grid */}
        {isLoading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
            {Array.from({ length: 4 }).map((_, i) => <StatCardSkeleton key={i} />)}
          </div>
        ) : (
          <motion.div
            className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-8"
            variants={staggerContainer}
            initial="hidden"
            animate="visible"
          >
            {statCards.map((stat) => (
              <motion.div key={stat.key} variants={fadeInUp}>
                <Card variant="glass">
                  <CardContent className="p-6 flex items-center gap-4">
                    <div className={`h-12 w-12 rounded-xl bg-gradient-to-br ${stat.color} flex items-center justify-center`}>
                      <stat.icon className={`h-6 w-6 ${stat.iconColor}`} />
                    </div>
                    <div>
                      <AnimatedCounter
                        target={getStatValue(stat.key)}
                        className="text-2xl font-bold"
                      />
                      <p className="text-sm text-muted-foreground">{stat.label}</p>
                    </div>
                  </CardContent>
                </Card>
              </motion.div>
            ))}
          </motion.div>
        )}

        {/* Alerts */}
        {((dashboard?.ungradedSubmissions ?? 0) > 0 || (dashboard?.pendingGradeApprovals ?? 0) > 0) && (
          <motion.div className="mb-8 space-y-3" initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.2 }}>
            {(dashboard?.ungradedSubmissions ?? 0) > 0 && (
              <div className="flex items-center gap-3 p-4 border rounded-xl bg-warning/10 border-warning/20">
                <AlertCircle className="h-5 w-5 text-warning" />
                <span className="text-sm">{dashboard?.ungradedSubmissions} ungraded submission(s)</span>
                <Button size="sm" variant="outline" className="ml-auto" onClick={() => navigate('/teacher/grading')}>
                  Grade Now
                </Button>
              </div>
            )}
            {(dashboard?.pendingGradeApprovals ?? 0) > 0 && (
              <div className="flex items-center gap-3 p-4 border rounded-xl bg-info/10 border-info/20">
                <AlertCircle className="h-5 w-5 text-info" />
                <span className="text-sm">{dashboard?.pendingGradeApprovals} pending AI grade approval(s)</span>
                <Button size="sm" variant="outline" className="ml-auto" onClick={() => navigate('/teacher/grading')}>
                  Review
                </Button>
              </div>
            )}
          </motion.div>
        )}

        {dashboard && (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Left column: Course Performance */}
            <div className="lg:col-span-2 space-y-6">
              {dashboard.coursePerformance && dashboard.coursePerformance.length > 0 && (
                <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.15 }}>
                  <h2 className="text-lg font-bold mb-3">Course Performance</h2>
                  <div className="space-y-3">
                    {dashboard.coursePerformance.map((cp) => (
                      <Card key={cp.courseId} className="hover:shadow-sm transition-shadow cursor-pointer" onClick={() => navigate(`/teacher/courses`)}>
                        <CardContent className="p-4">
                          <div className="flex items-center justify-between mb-2">
                            <h3 className="font-semibold text-sm truncate flex-1 mr-2">{cp.title}</h3>
                            <Badge variant="outline" className="text-xs shrink-0">
                              {cp.enrollmentCount} students
                            </Badge>
                          </div>
                          <div className="grid grid-cols-3 gap-4 text-sm">
                            <div className="flex items-center gap-1.5">
                              <Star className="h-3.5 w-3.5 text-warning" />
                              <span className="text-muted-foreground">Rating:</span>
                              <span className="font-medium">{cp.averageRating > 0 ? cp.averageRating.toFixed(1) : 'N/A'}</span>
                            </div>
                            <div className="flex items-center gap-1.5">
                              <TrendingUp className="h-3.5 w-3.5 text-success" />
                              <span className="text-muted-foreground">Completion:</span>
                              <span className="font-medium">{cp.completionRate.toFixed(0)}%</span>
                            </div>
                            <div className="flex items-center gap-1.5">
                              <DollarSign className="h-3.5 w-3.5 text-accent" />
                              <span className="text-muted-foreground">Revenue:</span>
                              <span className="font-medium">${cp.revenue.toFixed(0)}</span>
                            </div>
                          </div>
                        </CardContent>
                      </Card>
                    ))}
                  </div>
                </motion.div>
              )}

              {/* Enrollment Trend */}
              {dashboard.enrollmentTrend && dashboard.enrollmentTrend.length > 0 && (
                <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.2 }}>
                  <h2 className="text-lg font-bold mb-3">Enrollment Trend</h2>
                  <Card>
                    <CardContent className="p-4">
                      <div className="flex items-end gap-2 h-32">
                        {dashboard.enrollmentTrend.slice(-8).map((item, i) => {
                          const max = Math.max(...dashboard.enrollmentTrend.slice(-8).map((t) => t.count), 1);
                          const height = (item.count / max) * 100;
                          return (
                            <div key={i} className="flex-1 flex flex-col items-center gap-1">
                              <span className="text-xs font-medium">{item.count}</span>
                              <div className="w-full rounded-t-md bg-primary/20 relative" style={{ height: `${Math.max(height, 4)}%` }}>
                                <div className="absolute inset-0 rounded-t-md bg-primary" style={{ height: `${Math.max(height, 4)}%` }} />
                              </div>
                              <span className="text-[10px] text-muted-foreground">{item.month.slice(-2)}</span>
                            </div>
                          );
                        })}
                      </div>
                    </CardContent>
                  </Card>
                </motion.div>
              )}
            </div>

            {/* Right column: Stats + Recent Enrollments + Quick Actions */}
            <div className="space-y-6">
              {/* Additional Stats */}
              <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.15 }}>
                <h2 className="text-lg font-bold mb-3">Overview</h2>
                <Card>
                  <CardContent className="p-4 space-y-3">
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-muted-foreground flex items-center gap-1.5">
                        <BarChart3 className="h-3.5 w-3.5" /> Total Lectures
                      </span>
                      <span className="text-sm font-semibold">{dashboard.totalLectures}</span>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-muted-foreground flex items-center gap-1.5">
                        <Users className="h-3.5 w-3.5" /> Total Enrollments
                      </span>
                      <span className="text-sm font-semibold">{dashboard.totalEnrollments}</span>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-muted-foreground flex items-center gap-1.5">
                        <Star className="h-3.5 w-3.5" /> Avg Rating
                      </span>
                      <span className="text-sm font-semibold">{dashboard.averageRating > 0 ? dashboard.averageRating.toFixed(1) : 'N/A'}</span>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-muted-foreground flex items-center gap-1.5">
                        <TrendingUp className="h-3.5 w-3.5" /> Completion Rate
                      </span>
                      <span className="text-sm font-semibold">{dashboard.completionRate.toFixed(0)}%</span>
                    </div>
                    {dashboard.totalRevenue > 0 && (
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-muted-foreground flex items-center gap-1.5">
                          <DollarSign className="h-3.5 w-3.5" /> Revenue
                        </span>
                        <span className="text-sm font-semibold">${dashboard.totalRevenue.toFixed(0)}</span>
                      </div>
                    )}
                  </CardContent>
                </Card>
              </motion.div>

              {/* Recent Enrollments */}
              {dashboard.recentEnrollments && dashboard.recentEnrollments.length > 0 && (
                <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.2 }}>
                  <h2 className="text-lg font-bold mb-3">Recent Enrollments</h2>
                  <Card>
                    <CardContent className="p-4 space-y-2.5">
                      {dashboard.recentEnrollments.slice(0, 5).map((re, i) => (
                        <div key={i} className="flex items-start gap-2">
                          <Users className="h-3.5 w-3.5 text-muted-foreground mt-0.5 shrink-0" />
                          <div className="text-xs min-w-0">
                            <p className="font-medium truncate">{re.studentName}</p>
                            <p className="text-muted-foreground flex items-center gap-1">
                              {re.courseName}
                              <span className="flex items-center gap-0.5">
                                <Calendar className="h-3 w-3" />
                                {formatDate(re.enrolledAt)}
                              </span>
                            </p>
                          </div>
                        </div>
                      ))}
                    </CardContent>
                  </Card>
                </motion.div>
              )}

              {/* Quick Actions */}
              <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }}>
                <h2 className="text-lg font-bold mb-3">Quick Actions</h2>
                <div className="flex flex-wrap gap-2">
                  <Button variant="gradient" size="sm" onClick={() => navigate('/teacher/courses/create')}>Create Course</Button>
                  <Button variant="outline" size="sm" onClick={() => navigate('/teacher/courses')}>My Courses</Button>
                  <Button variant="outline" size="sm" onClick={() => navigate('/teacher/exams')}>Manage Exams</Button>
                  <Button variant="outline" size="sm" onClick={() => navigate('/teacher/grading')}>Grading</Button>
                </div>
              </motion.div>
            </div>
          </div>
        )}
      </div>
    </AnimatedPage>
  );
}
