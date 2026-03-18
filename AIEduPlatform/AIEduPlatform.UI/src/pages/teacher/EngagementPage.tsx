import { useParams } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { coursesApi } from '@/api/courses.api';
import { Card, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Textarea } from '@/components/ui/Textarea';
import { PageSpinner } from '@/components/ui/Spinner';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { cn } from '@/utils/cn';
import { useState } from 'react';
import { toast } from 'sonner';
import { AlertTriangle, Send } from 'lucide-react';
import { formatDate } from '@/utils/formatters';

export default function EngagementPage() {
  const { courseId } = useParams<{ courseId: string }>();
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
      toast.success('Alert sent!');
      setAlertMsg('');
    },
    onError: () => toast.error('Failed to send alert'),
  });

  if (isLoading) return <PageSpinner />;

  const students = data?.students || [];
  const summary = data;
  const atRiskIds = students
    .filter((s: any) => (s.engagementScore ?? 0) <= 25)
    .map((s: any) => s.studentId);

  const getLevel = (score: number) => {
    if (score <= 25) return { label: 'Critical', color: 'text-destructive bg-destructive/10' };
    if (score <= 50) return { label: 'Low', color: 'text-warning bg-warning/10' };
    if (score <= 75) return { label: 'Moderate', color: 'text-warning bg-warning/10' };
    return { label: 'High', color: 'text-success bg-success/10' };
  };

  return (
    <AnimatedPage>
    <div className="max-w-6xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-8">Student Engagement</h1>

      {/* Summary */}
      {summary && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
          <Card>
            <CardContent className="p-4 text-center">
              <p className="text-2xl font-bold">{summary.totalEnrolled ?? students.length}</p>
              <p className="text-sm text-muted-foreground">Total Enrolled</p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4 text-center">
              <p className="text-2xl font-bold">{summary.activeStudents ?? '—'}</p>
              <p className="text-sm text-muted-foreground">Active</p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4 text-center">
              <p className="text-2xl font-bold text-destructive">{summary.atRiskStudents ?? atRiskIds.length}</p>
              <p className="text-sm text-muted-foreground">At Risk</p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4 text-center">
              <p className="text-2xl font-bold">{summary?.averageEngagementScore?.toFixed(0) ?? '—'}%</p>
              <p className="text-sm text-muted-foreground">Avg Engagement</p>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Bulk Alert */}
      {atRiskIds.length > 0 && (
        <div className="flex items-center gap-3 mb-6 p-4 border rounded-lg bg-destructive/10 border-destructive/20">
          <AlertTriangle className="h-5 w-5 text-destructive" />
          <span className="text-sm flex-1">{atRiskIds.length} at-risk student(s)</span>
          <Textarea
            placeholder="Alert message..."
            value={alertMsg}
            onChange={(e) => setAlertMsg(e.target.value)}
            className="max-w-xs"
            rows={1}
          />
          <Button
            size="sm"
            onClick={() =>
              alertMutation.mutate({ studentIds: atRiskIds, customMessage: alertMsg || 'Please engage more with the course!' })
            }
            loading={alertMutation.isPending}
          >
            Alert All At-Risk
          </Button>
        </div>
      )}

      {/* Student Table */}
      <div className="border rounded-lg overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-secondary/50">
              <th className="text-left p-3">Student</th>
              <th className="text-left p-3">Engagement</th>
              <th className="text-left p-3">Level</th>
              <th className="text-left p-3">Last Active</th>
              <th className="text-left p-3">Submission Rate</th>
              <th className="text-left p-3">Avg Grade</th>
              <th className="text-left p-3">Action</th>
            </tr>
          </thead>
          <tbody>
            {(Array.isArray(students) ? students : [])
              .sort((a: any, b: any) => (a.engagementScore ?? 0) - (b.engagementScore ?? 0))
              .map((student: any) => {
                const level = getLevel(student.engagementScore ?? 0);
                return (
                  <tr key={student.studentId} className="border-t hover:bg-secondary/20">
                    <td className="p-3 font-medium">{student.studentName || student.name}</td>
                    <td className="p-3">
                      <div className="flex items-center gap-2">
                        <div className="w-24 h-2 bg-secondary rounded-full">
                          <div
                            className="h-full bg-primary rounded-full"
                            style={{ width: `${student.engagementScore ?? 0}%` }}
                          />
                        </div>
                        <span>{student.engagementScore?.toFixed(0) ?? 0}%</span>
                      </div>
                    </td>
                    <td className="p-3">
                      <span className={cn('px-2 py-1 rounded text-xs font-medium', level.color)}>
                        {level.label}
                      </span>
                    </td>
                    <td className="p-3 text-muted-foreground">
                      {student.lastStudySessionDate ? formatDate(student.lastStudySessionDate) : '—'}
                    </td>
                    <td className="p-3">
                      {student.examsAvailable > 0
                        ? `${Math.round((student.examsTaken / student.examsAvailable) * 100)}%`
                        : '—'}
                    </td>
                    <td className="p-3">{student.examsTaken > 0 ? student.averageExamScore?.toFixed(1) : '—'}</td>
                    <td className="p-3">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() =>
                          alertMutation.mutate({
                            studentIds: [student.studentId],
                            customMessage: alertMsg || 'Please engage more with the course!',
                          })
                        }
                      >
                        <Send className="h-3 w-3" />
                      </Button>
                    </td>
                  </tr>
                );
              })}
          </tbody>
        </table>
      </div>
    </div>
    </AnimatedPage>
  );
}
