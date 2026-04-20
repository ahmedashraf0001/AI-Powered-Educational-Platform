import { useQuery } from '@tanstack/react-query';
import { usersApi } from '@/api/users.api';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Pagination } from '@/components/ui/Pagination';
import { StatCardSkeleton } from '@/components/ui/Skeleton';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { AnimatedCounter } from '@/components/ui/AnimatedCounter';
import { useState } from 'react';
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
  Trophy
} from 'lucide-react';
import { motion } from 'framer-motion';
import { staggerContainer, fadeInUp } from '@/utils/motion';
import { formatDate } from '@/utils/formatters';
import type { TeacherDashboard as TeacherDashboardType, StudentDashboard as StudentDashboardType } from '@/types';

const statCards = [
  { key: 'total', icon: BookOpen, label: 'Total Courses', color: 'from-primary/20 to-primary/5', iconColor: 'text-primary' },
  { key: 'published', icon: GraduationCap, label: 'Published', color: 'from-success/20 to-success/5', iconColor: 'text-success' },
  { key: 'students', icon: Users, label: 'Students', color: 'from-info/20 to-info/5', iconColor: 'text-info' },
  { key: 'exams', icon: FileText, label: 'Exams', color: 'from-accent/20 to-accent/5', iconColor: 'text-accent' },
] as const;

const COURSE_PERFORMANCE_PAGE_SIZE = 5;

export default function TeacherDashboard() {
  const navigate = useNavigate();
  const [coursePerformancePage, setCoursePerformancePage] = useState(1);
  const [showAllCoursePerformance, setShowAllCoursePerformance] = useState(false);

  const { data: dashboard, isLoading } = useQuery({
    queryKey: ['teacher-dashboard'],
    queryFn: () => usersApi.getTeacherDashboard(),
    select: (res) => res.data.data as TeacherDashboardType,
  });

  const { data: studentDashboard, isLoading: isStudentLoading } = useQuery({
    queryKey: ['student-dashboard'],
    queryFn: () => usersApi.getStudentDashboard(),
    select: (res) => res.data.data as StudentDashboardType,
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

  const coursePerformance = dashboard?.coursePerformance ?? [];
  const hasCoursePerformanceOverflow = coursePerformance.length > COURSE_PERFORMANCE_PAGE_SIZE;
  const totalCoursePerformancePages = Math.max(
    1,
    Math.ceil(coursePerformance.length / COURSE_PERFORMANCE_PAGE_SIZE)
  );
  const visibleCoursePerformance = showAllCoursePerformance
    ? coursePerformance
    : coursePerformance.slice(
        (coursePerformancePage - 1) * COURSE_PERFORMANCE_PAGE_SIZE,
        coursePerformancePage * COURSE_PERFORMANCE_PAGE_SIZE
      );

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
        {isLoading || isStudentLoading ? (
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
                  <div className="mb-3 flex items-center justify-between gap-2">
                    <h2 className="text-lg font-bold">Course Performance</h2>
                    {hasCoursePerformanceOverflow && (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => {
                          setShowAllCoursePerformance((prev) => !prev);
                          setCoursePerformancePage(1);
                        }}
                      >
                        {showAllCoursePerformance ? 'Show paged' : 'View all'}
                      </Button>
                    )}
                  </div>
                  <div className="space-y-3">
                    {visibleCoursePerformance.map((cp) => (
                      <Card key={cp.courseId} className="hover:shadow-md hover:border-primary/40 transition-all duration-200 cursor-pointer group hover:-translate-y-0.5" onClick={() => navigate(`/teacher/courses/${cp.courseId}`)}>
                        <CardContent className="p-4">
                          <div className="flex items-center justify-between mb-3">
                            <h3 className="font-semibold text-sm truncate flex-1 mr-2 group-hover:text-primary transition-colors">{cp.title}</h3>
                            <Badge variant="outline" className="text-xs shrink-0 bg-secondary/50 group-hover:bg-primary/20 transition-colors border-border/50">
                              <Users className="h-3 w-3 mr-1 inline-block text-muted-foreground group-hover:text-primary transition-colors" /> {cp.enrollmentCount} students
                            </Badge>
                          </div>
                          <div className="grid grid-cols-3 gap-3 text-xs">
                            <div className="flex flex-col items-center justify-center gap-1 bg-secondary/20 p-2 rounded-lg border border-border/40 group-hover:border-warning/30 transition-colors">
                              <div className="flex items-center gap-1.5 text-muted-foreground">
                                <Star className="h-3.5 w-3.5 text-warning" />
                                <span className="hidden sm:inline">Rating</span>
                              </div>
                              <span className="font-medium text-sm">{cp.averageRating > 0 ? cp.averageRating.toFixed(1) : 'N/A'}</span>
                            </div>
                            <div className="flex flex-col items-center justify-center gap-1 bg-secondary/20 p-2 rounded-lg border border-border/40 group-hover:border-success/30 transition-colors">
                              <div className="flex items-center gap-1.5 text-muted-foreground">
                                <TrendingUp className="h-3.5 w-3.5 text-success" />
                                <span className="hidden sm:inline">Completion</span>
                              </div>
                              <span className="font-medium text-sm">{cp.completionRate.toFixed(0)}%</span>
                            </div>
                            <div className="flex flex-col items-center justify-center gap-1 bg-secondary/20 p-2 rounded-lg border border-border/40 group-hover:border-accent/30 transition-colors">
                              <div className="flex items-center gap-1.5 text-muted-foreground">
                                <DollarSign className="h-3.5 w-3.5 text-accent" />
                                <span className="hidden sm:inline">Revenue</span>
                              </div>
                              <span className="font-medium text-sm text-foreground">$${cp.revenue.toFixed(0)}</span>
                            </div>
                          </div>
                        </CardContent>
                      </Card>
                    ))}

                    {!showAllCoursePerformance && hasCoursePerformanceOverflow && (
                      <Pagination
                        page={coursePerformancePage}
                        totalPages={totalCoursePerformancePages}
                        onPageChange={setCoursePerformancePage}
                        hasPrevious={coursePerformancePage > 1}
                        hasNext={coursePerformancePage < totalCoursePerformancePages}
                      />
                    )}
                  </div>
                </motion.div>
              )}

              {/* Enrollment Trend */}
              {dashboard.enrollmentTrend && dashboard.enrollmentTrend.length > 0 && (
                <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.2 }}>
                  <div className="flex justify-between items-end mb-3">
                    <div>
                      <h2 className="text-lg font-bold">Enrollment Trend (Last 6 Months)</h2>
                      <p className="text-sm text-muted-foreground mt-1">
                        Total of <span className="font-semibold text-foreground">{dashboard.enrollmentTrend.reduce((acc, t) => acc + t.count, 0)}</span> students enrolled across your courses during this period.
                      </p>
                    </div>
                  </div>
                  <Card>
                    <CardContent className="p-4">
                      <div className="flex items-end gap-2 h-32">
                        {dashboard.enrollmentTrend.map((item, i) => {
                          const max = Math.max(...dashboard.enrollmentTrend.map((t) => t.count), 1);
                          const height = (item.count / max) * 100;
                          return (
                            <div key={i} className="flex-1 flex flex-col items-center h-full group relative">
                              <span className="text-xs font-medium mb-1">{item.count}</span>
                              <div className="flex-1 w-full relative bg-primary/10 rounded-t-md hover:bg-primary/20 transition-colors">
                                <div className="absolute bottom-0 w-full rounded-t-md bg-primary transition-all duration-300 group-hover:bg-primary/90" style={{ height: `${Math.max(height, 4)}%` }} />
                              </div>
                              <span className="text-xs font-medium text-muted-foreground mt-2 truncate w-full text-center">{item.month}</span>
                            </div>
                          );
                        })}
                      </div>
                    </CardContent>
                  </Card>
                </motion.div>
              )}

              {/* Student Course Progress */}
              {studentDashboard && studentDashboard.courseProgress && studentDashboard.courseProgress.length > 0 && (
                <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.25 }}>
                  <h2 className="text-lg font-bold mb-3">My Learning Progress</h2>
                  <div className="space-y-3">
                    {studentDashboard.courseProgress.map((cp) => (
                      <Card key={cp.courseId} className="hover:shadow-sm transition-shadow">
                        <CardContent className="p-4">
                          <div className="flex items-center justify-between mb-2">
                            <h3 className="font-semibold text-sm truncate flex-1 mr-2">{cp.courseTitle}</h3>
                            <Badge variant={cp.status === 'Completed' ? 'success' : 'outline'} className="text-xs shrink-0">
                              {cp.status}
                            </Badge>
                          </div>
                          <div className="w-full h-2 bg-secondary rounded-full mb-2 overflow-hidden">
                            <motion.div
                              className="h-full bg-gradient-to-r from-primary to-accent rounded-full"
                              initial={{ width: 0 }}
                              animate={{ width: `${cp.progressPercentage}%` }}
                              transition={{ duration: 1, ease: 'easeOut', delay: 0.3 }}
                            />
                          </div>
                          <div className="flex items-center justify-between text-xs text-muted-foreground">
                            <span>{cp.completedMaterials}/{cp.totalMaterials} materials</span>
                            <span>{cp.progressPercentage}%</span>
                          </div>
                          <div className="mt-2 flex justify-end">
                            <Button size="sm" onClick={() => navigate(`/courses/${cp.courseId}/learn`)}>
                              Continue
                            </Button>
                          </div>
                        </CardContent>
                      </Card>
                    ))}
                  </div>
                </motion.div>
              )}

              {/* Student Recent Submissions */}
              {studentDashboard && studentDashboard.submissionHistory && studentDashboard.submissionHistory.length > 0 && (
                <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }}>
                  <h2 className="text-lg font-bold mb-3">My Recent Submissions</h2>
                  <div className="space-y-2">
                    {studentDashboard.submissionHistory.slice(0, 5).map((sub) => (
                      <Card key={sub.submissionId}>
                        <CardContent className="p-3 flex items-center justify-between gap-3">
                          <div className="min-w-0 flex-1">
                            <p className="text-sm font-medium truncate">{sub.examTitle}</p>
                            <p className="text-xs text-muted-foreground flex items-center gap-2 mt-0.5">
                              <span>{sub.courseName}</span>
                              <span className="flex items-center gap-0.5">
                                <Calendar className="h-3 w-3" />
                                {formatDate(sub.submittedAt)}
                              </span>
                            </p>
                          </div>
                          <div className="flex items-center gap-2 shrink-0">
                            {sub.isGraded && sub.score != null && (
                              <span className="text-sm font-semibold flex items-center gap-1">
                                <Trophy className="h-3.5 w-3.5 text-warning" />
                                {sub.score.toFixed(1)}%
                              </span>
                            )}
                            <Badge variant={sub.isGraded ? 'success' : 'outline'} className="text-xs">
                              {sub.isGraded ? 'Graded' : 'Pending'}
                            </Badge>
                          </div>
                        </CardContent>
                      </Card>
                    ))}
                  </div>
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

              {/* Student Exam Performance */}
              {studentDashboard && studentDashboard.performance && studentDashboard.performance.examsTaken > 0 && (
                <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }}>
                  <h2 className="text-lg font-bold mb-3">My Exam Performance</h2>
                  <Card>
                    <CardContent className="p-4 space-y-3">
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-muted-foreground flex items-center gap-1.5">
                          <BarChart3 className="h-3.5 w-3.5" /> Exams Taken
                        </span>
                        <span className="text-sm font-semibold">{studentDashboard.performance.examsTaken}</span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-muted-foreground">Average</span>
                        <span className="text-sm font-semibold">{studentDashboard.performance.averageScore.toFixed(1)}%</span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-muted-foreground">Highest</span>
                        <span className="text-sm font-semibold text-success">{studentDashboard.performance.highestScore.toFixed(1)}%</span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-muted-foreground">Lowest</span>
                        <span className="text-sm font-semibold text-destructive">{studentDashboard.performance.lowestScore.toFixed(1)}%</span>
                      </div>
                    </CardContent>
                  </Card>
                </motion.div>
              )}

              {/* Quick Actions */}
              <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.35 }}>
                <h2 className="text-lg font-bold mb-3">Quick Actions</h2>
                <div className="flex flex-wrap gap-2">
                  <Button variant="gradient" size="sm" onClick={() => navigate('/teacher/courses/create')}>Create Course</Button>
                  <Button variant="outline" size="sm" onClick={() => navigate('/teacher/courses')}>My Courses</Button>
                  <Button variant="outline" size="sm" onClick={() => navigate('/teacher/exams')}>Manage Exams</Button>
                  <Button variant="outline" size="sm" onClick={() => navigate('/teacher/grading')}>Grading</Button>
                  <Button variant="outline" size="sm" onClick={() => navigate('/courses')}>Browse Courses</Button>
                  <Button variant="outline" size="sm" onClick={() => navigate('/my-enrollments')}>My Enrollments</Button>
                </div>
              </motion.div>
            </div>
          </div>
        )}
      </div>
    </AnimatedPage>
  );
}
