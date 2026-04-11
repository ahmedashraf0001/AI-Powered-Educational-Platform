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
              <CardContent className="p-4 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
                <div className="flex-1 w-full">
                  <h3 className="font-semibold text-lg">{grade.examTitle || 'Unknown Exam'}</h3>
                  <p className="text-sm text-muted-foreground mb-2">
                    {grade.courseTitle || 'Unknown Course'}
                  </p>
                  <div className="flex items-center gap-2 mb-3">
                    <span className="font-medium">Score:</span>
                    <Badge variant="info" className="text-sm">
                      {grade.score?.toFixed(1) ?? '0'}%
                    </Badge>
                  </div>
                  
                  {grade.feedback && (
                    <div className="mt-2 bg-secondary/20 p-3 rounded-md text-sm italic border-l-4 border-primary max-h-32 overflow-y-auto w-full prose prose-sm max-w-none dark:prose-invert">
                      <div className="font-medium not-italic mb-1 opacity-80">Feedback:</div>
                      <div className="whitespace-pre-wrap">{grade.feedback}</div>
                    </div>
                  )}
                </div>
                
                <div className="flex flex-row sm:flex-col items-center sm:items-end justify-between w-full sm:w-auto gap-2">
                  <Badge variant={grade.isApproved ? 'success' : 'outline'}>
                    {grade.isApproved ? 'Approved' : 'Pending'}
                  </Badge>
                  <p className="text-xs text-muted-foreground">
                    {grade.isAiGraded ? '🤖 AI Graded' : '👤 Manual'}
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
