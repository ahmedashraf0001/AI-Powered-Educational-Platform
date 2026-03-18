import { useQuery } from '@tanstack/react-query';
import { gradesApi } from '@/api/grades.api';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { PageSpinner } from '@/components/ui/Spinner';
import { EmptyState } from '@/components/ui/Feedback';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { Trophy } from 'lucide-react';

export default function MyGradesPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['my-grades'],
    queryFn: () => gradesApi.getMyGrades(),
    select: (res) => res.data.data,
  });

  if (isLoading) return <PageSpinner />;

  const grades = Array.isArray(data) ? data : (data as any)?.items ?? [];

  return (
    <AnimatedPage>
    <div className="max-w-5xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-8">My Grades</h1>

      {grades.length === 0 ? (
        <EmptyState
          icon={<Trophy className="h-12 w-12" />}
          title="No grades yet"
          description="Your grades will appear here after your exams are graded"
        />
      ) : (
        <div className="space-y-4">
          {grades.map((grade: any) => (
            <Card key={grade.id}>
              <CardContent className="p-4 flex items-center justify-between">
                <div>
                  <h3 className="font-semibold">Grade</h3>
                  <p className="text-sm text-muted-foreground">
                    Score: {grade.score}
                  </p>
                  {grade.feedback && (
                    <p className="text-sm mt-1">{grade.feedback}</p>
                  )}
                </div>
                <div className="text-right">
                  <Badge variant={grade.isApproved ? 'success' : 'outline'}>
                    {grade.isApproved ? 'Approved' : 'Pending'}
                  </Badge>
                  <p className="text-xs text-muted-foreground mt-1">
                    {grade.isAiGraded ? 'AI Graded' : 'Manual'}
                  </p>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
    </AnimatedPage>
  );
}
