import { useQuery } from '@tanstack/react-query';
import { usersApi } from '@/api/users.api';
import { coursesApi } from '@/api/courses.api';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { StatCardSkeleton } from '@/components/ui/Skeleton';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { AnimatedCounter } from '@/components/ui/AnimatedCounter';
import { Button } from '@/components/ui/Button';
import { useNavigate } from 'react-router-dom';
import {
  BookOpen,
  Clock,
  Trophy,
  GraduationCap,
  Activity,
  FileText,
  Calendar,
  TrendingUp,
  BarChart3,
  Eye,
  Zap,
} from 'lucide-react';
import { motion } from 'framer-motion';
import { staggerContainer, fadeInUp } from '@/utils/motion';
import { formatDate } from '@/utils/formatters';
import type { StudentDashboard as StudentDashboardType } from '@/types';

const statCards = [
  { key: 'enrolled', icon: BookOpen, label: 'Courses', color: 'from-primary/20 to-primary/5', iconColor: 'text-primary' },
  { key: 'completed', icon: GraduationCap, label: 'Completed', color: 'from-success/20 to-success/5', iconColor: 'text-success' },
  { key: 'progress', icon: TrendingUp, label: 'Overall Progress', color: 'from-accent/20 to-accent/5', iconColor: 'text-accent' },
  { key: 'score', icon: Trophy, label: 'Avg Score', color: 'from-warning/20 to-warning/5', iconColor: 'text-warning' },
] as const;

export default function StudentDashboard() {
  const navigate = useNavigate();

  const { data: dashboard, isLoading } = useQuery({
    queryKey: ['student-dashboard'],
    queryFn: () => usersApi.getStudentDashboard(),
    select: (res) => res.data.data as StudentDashboardType,
  });

  const { data: continueLearningData } = useQuery({
    queryKey: ['continue-learning'],
    queryFn: () => coursesApi.continueLearning(),
    select: (res) => res.data.data,
  });

  const handleContinueCourse = (courseId: string) => {
    if (!continueLearningData) {
      navigate(`/courses/${courseId}/learn`);
      return;
    }
    const continueData = continueLearningData.find((c) => c.courseId === courseId);
    if (continueData?.lectureId && continueData.lastMaterialId) {
      navigate(`/courses/${courseId}/lectures/${continueData.lectureId}?materialId=${continueData.lastMaterialId}`);
    } else {
      navigate(`/courses/${courseId}/learn`);
    }
  };

  const getStatValue = (key: string) => {
    if (!dashboard) return 0;
    switch (key) {
      case 'enrolled': return dashboard.totalEnrolledCourses ?? 0;
      case 'completed': return dashboard.completedCourses ?? 0;
      case 'progress': return dashboard.overallProgressPercentage ?? 0;
      case 'score': return dashboard.performance?.averageScore ?? 0;
      default: return 0;
    }
  };

  const formatTime = (minutes: number) => {
    if (minutes < 60) return `${Math.round(minutes)}m`;
    const h = Math.floor(minutes / 60);
    const m = Math.round(minutes % 60);
    return m > 0 ? `${h}h ${m}m` : `${h}h`;
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
          Dashboard
        </motion.h1>

        {/* Stats */}
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
                        suffix={stat.key === 'score' || stat.key === 'progress' ? '%' : ''}
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

        {dashboard && (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Left column: Course progress + Engagement */}
            <div className="lg:col-span-2 space-y-6">
              {/* Course Progress */}
              {dashboard.courseProgress && dashboard.courseProgress.length > 0 && (
                <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.1 }}>
                  <h2 className="text-lg font-bold mb-3">Course Progress</h2>
                  <div className="space-y-3">
                    {dashboard.courseProgress.map((cp) => (
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
                            <Button size="sm" onClick={() => {
                              if (cp.status === 'Completed' || cp.progressPercentage === 100) {
                                navigate(`/courses/${cp.courseId}/learn`);
                              } else {
                                handleContinueCourse(cp.courseId);
                              }
                            }}>
                              {cp.status === 'Completed' || cp.progressPercentage === 100 ? 'Go to Course' : 'Continue'}
                            </Button>
                          </div>
                        </CardContent>
                      </Card>
                    ))}
                  </div>
                </motion.div>
              )}

              {/* Submission History */}
              {dashboard.submissionHistory && dashboard.submissionHistory.length > 0 && (
                <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.2 }}>
                  <h2 className="text-lg font-bold mb-3">Recent Submissions</h2>
                  <div className="space-y-2">
                    {dashboard.submissionHistory.slice(0, 5).map((sub) => (
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

            {/* Right column: Engagement + Performance + Quick Actions */}
            <div className="space-y-6">
              {/* Engagement */}
              {dashboard.engagement && (
                <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.15 }}>
                  <h2 className="text-lg font-bold mb-3">Engagement</h2>
                  <Card>
                    <CardContent className="p-4 space-y-3">
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-muted-foreground flex items-center gap-1.5">
                          <Activity className="h-3.5 w-3.5" /> Study Sessions
                        </span>
                        <span className="text-sm font-semibold">{dashboard.engagement.totalStudySessions}</span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-muted-foreground flex items-center gap-1.5">
                          <Clock className="h-3.5 w-3.5" /> Time Spent
                        </span>
                        <span className="text-sm font-semibold">{formatTime(dashboard.engagement.totalTimeSpentMinutes)}</span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-muted-foreground flex items-center gap-1.5">
                          <Eye className="h-3.5 w-3.5" /> Materials Viewed
                        </span>
                        <span className="text-sm font-semibold">{dashboard.engagement.totalMaterialsViewed}</span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-muted-foreground flex items-center gap-1.5">
                          <Zap className="h-3.5 w-3.5" /> Quizzes Taken
                        </span>
                        <span className="text-sm font-semibold">{dashboard.engagement.totalQuizzesGenerated}</span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-muted-foreground flex items-center gap-1.5">
                          <FileText className="h-3.5 w-3.5" /> Flashcards
                        </span>
                        <span className="text-sm font-semibold">{dashboard.engagement.totalFlashcardsGenerated}</span>
                      </div>
                    </CardContent>
                  </Card>
                </motion.div>
              )}

              {/* Performance */}
              {dashboard.performance && dashboard.performance.examsTaken > 0 && (
                <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.2 }}>
                  <h2 className="text-lg font-bold mb-3">Exam Performance</h2>
                  <Card>
                    <CardContent className="p-4 space-y-3">
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-muted-foreground flex items-center gap-1.5">
                          <BarChart3 className="h-3.5 w-3.5" /> Exams Taken
                        </span>
                        <span className="text-sm font-semibold">{dashboard.performance.examsTaken}</span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-muted-foreground">Average</span>
                        <span className="text-sm font-semibold">{dashboard.performance.averageScore.toFixed(1)}%</span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-muted-foreground">Highest</span>
                        <span className="text-sm font-semibold text-success">{dashboard.performance.highestScore.toFixed(1)}%</span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-muted-foreground">Lowest</span>
                        <span className="text-sm font-semibold text-destructive">{dashboard.performance.lowestScore.toFixed(1)}%</span>
                      </div>
                    </CardContent>
                  </Card>
                </motion.div>
              )}

              {/* Recent Activity */}
              {dashboard.recentActivity && dashboard.recentActivity.length > 0 && (
                <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.25 }}>
                  <h2 className="text-lg font-bold mb-3">Recent Activity</h2>
                  <Card>
                    <CardContent className="p-4 space-y-2.5">
                      {dashboard.recentActivity.slice(0, 5).map((act, i) => (
                        <div key={i} className="flex items-start gap-2">
                          <BookOpen className="h-3.5 w-3.5 text-muted-foreground mt-0.5 shrink-0" />
                          <div className="text-xs min-w-0">
                            <p className="font-medium truncate">{act.lectureTitle}</p>
                            <p className="text-muted-foreground">{act.courseTitle} {act.completedAt ? `· ${formatDate(act.completedAt)}` : ''}</p>
                          </div>
                        </div>
                      ))}
                    </CardContent>
                  </Card>
                </motion.div>
              )}

              {/* Quick Actions */}
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.3 }}
              >
                <h2 className="text-lg font-bold mb-3">Quick Actions</h2>
                <div className="flex flex-wrap gap-2">
                  <Button variant="outline" size="sm" onClick={() => navigate('/courses')}>Browse Courses</Button>
                  <Button variant="outline" size="sm" onClick={() => navigate('/my-enrollments')}>My Enrollments</Button>
                  <Button variant="outline" size="sm" onClick={() => navigate('/my-submissions')}>My Submissions</Button>
                  <Button variant="outline" size="sm" onClick={() => navigate('/my-grades')}>My Grades</Button>
                </div>
              </motion.div>
            </div>
          </div>
        )}
      </div>
    </AnimatedPage>
  );
}
