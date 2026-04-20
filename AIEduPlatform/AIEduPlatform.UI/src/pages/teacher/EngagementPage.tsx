import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { coursesApi } from '@/api/courses.api';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Textarea } from '@/components/ui/Textarea';
import { PageSpinner } from '@/components/ui/Spinner';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { AnimatedCounter } from '@/components/ui/AnimatedCounter';
import { cn } from '@/utils/cn';
import { useState } from 'react';
import { toast } from 'sonner';
import { AlertTriangle, Send, Users, Activity, TrendingUp, ArrowLeft, MailWarning } from 'lucide-react';
import { motion } from 'framer-motion';
import { staggerContainer, fadeInUp } from '@/utils/motion';
import { formatDate } from '@/utils/formatters';

export default function EngagementPage() {
  const { courseId } = useParams<{ courseId: string }>();
  const navigate = useNavigate();
  const [alertMsg, setAlertMsg] = useState('');

  const { data, isLoading } = useQuery({
    queryKey: ['engagement', courseId],
    queryFn: () => coursesApi.getEngagement(courseId!),
    enabled: !!courseId,
    select: (res) => res.data.data,
  });

  const alertMutation = useMutation({
    mutationFn: ({ studentIds, customMessage }: { studentIds: string[]; customMessage: string }) =>
      coursesApi.sendEngagementAlerts(courseId!, { studentIds, customMessage }),
    onSuccess: () => {
      toast.success('Alert(s) sent successfully!');
      setAlertMsg('');
    },
    onError: (error: any) => toast.error(error?.userMessage ?? 'Failed to send alerts'),
  });

  if (isLoading) return <PageSpinner />;

  const students = data?.students || [];
  const summary = data;
  const atRiskIds = students
    .filter((s: any) => (s.engagementScore ?? 0) <= 25)
    .map((s: any) => s.studentId);

  const getLevel = (score: number) => {
    if (score <= 25) return { label: 'Critical', variant: 'destructive' as const };
    if (score <= 50) return { label: 'Low', variant: 'warning' as const };
    if (score <= 75) return { label: 'Moderate', variant: 'outline' as const };
    return { label: 'High', variant: 'success' as const };
  };

  const statCards = [
    { key: 'total', icon: Users, label: 'Total Enrolled', value: summary?.totalEnrolled ?? students.length, color: 'from-primary/20 to-primary/5', iconColor: 'text-primary' },
    { key: 'active', icon: Activity, label: 'Active Students', value: summary?.activeStudents ?? 0, color: 'from-info/20 to-info/5', iconColor: 'text-info' },
    { key: 'risk', icon: AlertTriangle, label: 'At Risk', value: summary?.atRiskStudents ?? atRiskIds.length, color: 'from-destructive/20 to-destructive/5', iconColor: 'text-destructive' },
    { key: 'avg', icon: TrendingUp, label: 'Avg Engagement', value: summary?.averageEngagementScore ?? 0, suffix: '%', color: 'from-success/20 to-success/5', iconColor: 'text-success' },
  ] as const;

  return (
    <AnimatedPage>
      <div className="max-w-7xl mx-auto px-4 py-8">
        <div className="flex items-center gap-4 mb-8">
          <Button variant="outline" size="icon" onClick={() => navigate(-1)} className="shrink-0 h-9 w-9">
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <motion.h1
            className="text-2xl font-bold flex-1"
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.4 }}
          >
            Student Engagement Tracker
          </motion.h1>
        </div>

        {/* Summary Stats */}
        {summary ? (
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
                    <div className={`h-12 w-12 rounded-xl bg-gradient-to-br ${stat.color} flex items-center justify-center shrink-0`}>
                      <stat.icon className={`h-6 w-6 ${stat.iconColor}`} />
                    </div>
                    <div>
                      <div className="flex items-baseline gap-0.5">
                        <AnimatedCounter
                          target={Math.round(stat.value)}
                          className="text-2xl font-bold"
                        />
                        {('suffix' in stat && stat.suffix) && <span className="text-xl font-bold">{stat.suffix}</span>}
                      </div>
                      <p className="text-sm text-muted-foreground">{stat.label}</p>
                    </div>
                  </CardContent>
                </Card>
              </motion.div>
            ))}
          </motion.div>
        ) : null}

        {/* Bulk Alert Section */}
        {atRiskIds.length > 0 && (
          <motion.div 
            initial={{ opacity: 0, y: 10 }} 
            animate={{ opacity: 1, y: 0 }} 
            transition={{ delay: 0.2 }}
            className="mb-8"
          >
            <Card className="border-destructive/30 bg-destructive/5 shadow-sm">
              <CardContent className="p-5 flex flex-col md:flex-row items-start md:items-center gap-4">
                <div className="flex items-center gap-3 flex-1">
                  <div className="h-10 w-10 rounded-full bg-destructive/20 flex items-center justify-center shrink-0">
                    <MailWarning className="h-5 w-5 text-destructive" />
                  </div>
                  <div>
                    <h3 className="font-semibold text-destructive">Needs Attention</h3>
                    <p className="text-sm text-muted-foreground">{atRiskIds.length} student(s) have critical engagement levels.</p>
                  </div>
                </div>
                <div className="flex flex-1 w-full md:w-auto items-center gap-3">
                  <Textarea
                    placeholder="Custom alert message..."
                    value={alertMsg}
                    onChange={(e) => setAlertMsg(e.target.value)}
                    className="flex-1 min-h-[40px] h-10 py-2 resize-none bg-background text-sm"
                    rows={1}
                  />
                  <Button
                    variant="destructive"
                    onClick={() =>
                      alertMutation.mutate({ studentIds: atRiskIds, customMessage: alertMsg || 'We noticed your engagement has dropped. Please reach out if you need help with the course material.' })
                    }
                    loading={alertMutation.isPending}
                    className="shrink-0"
                  >
                    Alert All
                  </Button>
                </div>
              </CardContent>
            </Card>
          </motion.div>
        )}

        {/* Student Table */}
        <motion.div 
          initial={{ opacity: 0, y: 20 }} 
          animate={{ opacity: 1, y: 0 }} 
          transition={{ delay: 0.3 }}
        >
          <Card>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-secondary/30 border-b border-border/50">
                    <th className="text-left font-semibold p-4 text-xs uppercase tracking-wider text-muted-foreground">Student Name</th>
                    <th className="text-left font-semibold p-4 text-xs uppercase tracking-wider text-muted-foreground w-1/4">Engagement</th>
                    <th className="text-left font-semibold p-4 text-xs uppercase tracking-wider text-muted-foreground">Level</th>
                    <th className="text-left font-semibold p-4 text-xs uppercase tracking-wider text-muted-foreground">Last Active</th>
                    <th className="text-left font-semibold p-4 text-xs uppercase tracking-wider text-muted-foreground hidden sm:table-cell">Submissions</th>
                    <th className="text-left font-semibold p-4 text-xs uppercase tracking-wider text-muted-foreground">Avg Grade</th>
                    <th className="text-center font-semibold p-4 text-xs uppercase tracking-wider text-muted-foreground">Action</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border/50">
                  {students.length > 0 ? (
                    students
                      .sort((a: any, b: any) => (a.engagementScore ?? 0) - (b.engagementScore ?? 0))
                      .map((student: any) => {
                        const level = getLevel(student.engagementScore ?? 0);
                        const isCritical = (student.engagementScore ?? 0) <= 25;
                        return (
                          <tr key={student.studentId} className={cn("hover:bg-secondary/10 transition-colors", isCritical && "bg-destructive/5 hover:bg-destructive/10")}>
                            <td className="p-4 font-medium">{student.studentName || student.name}</td>
                            <td className="p-4">
                              <div className="flex items-center gap-3">
                                <div className="h-2 flex-1 bg-secondary rounded-full overflow-hidden">
                                  <div
                                    className={cn("h-full rounded-full transition-all duration-500", isCritical ? "bg-destructive" : "bg-primary")}
                                    style={{ width: `${student.engagementScore ?? 0}%` }}
                                  />
                                </div>
                                <span className={cn("font-semibold text-xs w-8 text-right", isCritical ? "text-destructive" : "text-muted-foreground")}>
                                  {student.engagementScore?.toFixed(0) ?? 0}%
                                </span>
                              </div>
                            </td>
                            <td className="p-4">
                              <Badge variant={level.variant} className="text-[10px] uppercase font-bold py-0 h-5">
                                {level.label}
                              </Badge>
                            </td>
                            <td className="p-4 text-xs text-muted-foreground">
                              {student.lastStudySessionDate ? formatDate(student.lastStudySessionDate) : 'Never'}
                            </td>
                            <td className="p-4 text-xs hidden sm:table-cell">
                              {student.examsAvailable > 0
                                ? `${Math.round((student.examsTaken / student.examsAvailable) * 100)}%`
                                : '0%'}
                            </td>
                            <td className="p-4 text-xs">
                              <span className={cn("font-medium", student.examsTaken > 0 && student.averageExamScore < 50 ? "text-destructive" : "")}>
                                {student.examsTaken > 0 ? student.averageExamScore?.toFixed(1) : '—'}
                              </span>
                            </td>
                            <td className="p-4 text-center">
                              <Button
                                variant={isCritical ? "destructive" : "ghost"}
                                size="icon"
                                className={cn("h-8 w-8", !isCritical && "text-muted-foreground hover:text-foreground")}
                                title="Send reminder"
                                onClick={() =>
                                  alertMutation.mutate({
                                    studentIds: [student.studentId],
                                    customMessage: alertMsg || 'Just checking in! Let me know if you need help with the course material.',
                                  })
                                }
                              >
                                <Send className="h-4 w-4" />
                              </Button>
                            </td>
                          </tr>
                        );
                      })
                  ) : (
                    <tr>
                      <td colSpan={7} className="p-8 text-center text-muted-foreground">
                        No students enrolled yet.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </Card>
        </motion.div>
      </div>
    </AnimatedPage>
  );
}
