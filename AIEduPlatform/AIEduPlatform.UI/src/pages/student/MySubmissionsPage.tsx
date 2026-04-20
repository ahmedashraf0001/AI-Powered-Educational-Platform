import { useQuery } from '@tanstack/react-query';
import { submissionsApi } from '@/api/submissions.api';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Pagination } from '@/components/ui/Pagination';
import { PageSpinner, Spinner } from '@/components/ui/Spinner';
import { EmptyState } from '@/components/ui/Feedback';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { Modal } from '@/components/ui/Modal';
import { useState } from 'react';
import {
  FileText
} from 'lucide-react';
import { formatDate } from '@/utils/formatters';
import type { SubmissionDetailDto } from '@/types';

export default function MySubmissionsPage() {
  const [page, setPage] = useState(1);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ['my-submissions', page],
    queryFn: () => submissionsApi.getMine({ page }),
    select: (res) => res.data.data,
  });

  const { data: detail, isLoading: detailLoading } = useQuery({
    queryKey: ['submission-detail', selectedId],
    queryFn: () => submissionsApi.getById(selectedId!),
    enabled: !!selectedId,
    select: (res) => res.data.data as SubmissionDetailDto,
  });

  if (isLoading) return <PageSpinner />;

  return (
    <AnimatedPage>
      <div className="max-w-4xl mx-auto px-4 py-8">
        <div className="flex items-center gap-3 mb-8">
          <FileText className="h-8 w-8 text-primary" />
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Submission History</h1>
            <p className="text-sm text-muted-foreground mt-1">A log of all exams and assignments you have successfully submitted.</p>
          </div>
        </div>

        {!data || data.items.length === 0 ? (
          <EmptyState
            icon={<FileText className="h-12 w-12" />}
            title="No submissions yet"
            description="When you take an exam, your submission receipts will appear here."
          />
        ) : (
          <>
            <div className="bg-card border rounded-lg overflow-hidden">
              <div className="grid grid-cols-12 gap-4 p-4 bg-muted/50 border-b text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                <div className="col-span-5 md:col-span-4">Exam</div>
                <div className="hidden md:block col-span-4">Course</div>
                <div className="col-span-4 md:col-span-2">Date</div>
                <div className="col-span-3 md:col-span-2 text-right">Receipt</div>
              </div>
              <div className="divide-y">
                {data.items.map((sub: any) => (
                  <div key={sub.id} className="grid grid-cols-12 gap-4 p-4 items-center hover:bg-muted/10 transition-colors">
                    <div className="col-span-5 md:col-span-4 min-w-0">
                      <h3 className="font-medium truncate text-sm">{sub.examTitle || 'Exam'}</h3>
                    </div>
                    <div className="hidden md:block col-span-4 min-w-0">
                      <p className="text-sm text-muted-foreground truncate">{sub.courseName || 'Unknown Course'}</p>
                    </div>
                    <div className="col-span-4 md:col-span-2 text-sm text-muted-foreground">
                      {formatDate(sub.submittedAt)}
                    </div>
                    <div className="col-span-3 md:col-span-2 flex justify-end gap-2 items-center">
                      <Badge variant={sub.isGraded ? 'success' : 'outline'} className={sub.isGraded ? 'opacity-80' : 'text-muted-foreground'}>
                        {sub.isGraded ? 'Graded' : 'Received'}
                      </Badge>
                      <Button
                        variant="ghost"
                        size="sm"
                        className="h-8 w-8 p-0"
                        onClick={() => setSelectedId(sub.id)}
                        title="View Receipt"
                      >
                        <FileText className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            </div>
            <div className="mt-6">
              <Pagination
                page={data.page}
                totalPages={data.totalPages}
                onPageChange={setPage}
                hasPrevious={data.page > 1}
                hasNext={data.page < data.totalPages}
              />
            </div>
          </>
        )}
      </div>

      {/* Submission Detail Modal */}
      <Modal
        open={!!selectedId}
        onClose={() => setSelectedId(null)}
        title="Submission Receipt"
        description="A record of the answers you submitted"
        className="max-w-2xl max-h-[85vh] flex flex-col"
      >
        {detailLoading ? (
          <div className="flex justify-center py-8">
            <Spinner />
          </div>
        ) : detail ? (
          <div className="space-y-3 overflow-y-auto max-h-[60vh] pr-1">
            {/* Submission Summary */}
            <div className="bg-muted/30 border border-border/50 rounded-lg p-3 grid grid-cols-2 md:grid-cols-4 gap-3 text-xs">
              <div>
                <p className="text-muted-foreground text-[10px] uppercase tracking-wider font-semibold mb-0.5">Exam</p>
                <p className="font-medium truncate">{detail.examTitle}</p>
              </div>
              <div>
                <p className="text-muted-foreground text-[10px] uppercase tracking-wider font-semibold mb-0.5">Course</p>
                <p className="font-medium truncate">{detail.courseName}</p>
              </div>
              <div>
                <p className="text-muted-foreground text-[10px] uppercase tracking-wider font-semibold mb-0.5">Submitted</p>
                <p className="font-medium">{formatDate(detail.submittedAt)}</p>
              </div>
              <div>
                <p className="text-muted-foreground text-[10px] uppercase tracking-wider font-semibold mb-0.5">Status</p>
                <Badge variant={detail.isGraded ? 'success' : 'outline'} className="mt-0.5 text-[9px] px-1.5 py-0">
                  {detail.isGraded ? 'Graded' : 'Pending Evaluation'}
                </Badge>
              </div>
            </div>
  
            {/* Read-only Answers */}
            <div className="space-y-2.5">
              {detail.answers.map((a, i) => (
                <div key={a.questionId} className="p-3 rounded-lg border bg-card">
                  <div className="flex items-start gap-2.5 mb-2.5">
                    <span className="text-[10px] font-bold text-muted-foreground mt-0.5 bg-secondary/30 px-1.5 py-0.5 rounded">
                      Q{i + 1}
                    </span>
                    <div className="flex-1">
                      <p className="text-xs font-medium">{a.questionText}</p>
                      <div className="flex items-center gap-2 mt-1 flex-wrap">
                        <Badge variant="outline" className="text-[9px] px-1.5 py-0">
                          {a.questionType}
                        </Badge>
                        <span className="text-[9px] text-muted-foreground">
                          {a.points} pt{a.points !== 1 ? 's' : ''}
                        </span>
                      </div>
                    </div>
                  </div>

                  {/* Options or Answer display */}
                  <div className="ml-7">
                    {a.options && a.options.length > 0 ? (
                      <div className="space-y-1.5">
                        {a.options.map((opt, oi) => {
                          const isStudentAnswer = a.answer === opt;
                          return (
                            <div
                              key={oi}
                              className={`text-xs px-2.5 py-1.5 rounded-md flex items-center gap-2 border transition-colors ${
                                isStudentAnswer
                                  ? 'bg-primary/5 border-primary/30 text-foreground font-medium'
                                  : 'border-transparent text-muted-foreground'
                              }`}
                            >
                              <div className={`w-3.5 h-3.5 shrink-0 rounded-full border flex items-center justify-center ${isStudentAnswer ? 'border-primary' : 'border-muted-foreground/30'}`}>
                                {isStudentAnswer && <div className="w-1.5 h-1.5 rounded-full bg-primary" />}
                              </div>
                              <span>{opt}</span>
                            </div>
                          );
                        })}
                      </div>
                    ) : (
                      <div className="text-xs bg-muted/30 p-2.5 rounded-md border border-border/50">
                        <span className="text-[10px] text-muted-foreground uppercase tracking-wider block mb-1">Your Submission:</span>
                        <div className="whitespace-pre-wrap font-medium text-foreground">
                          {a.answer || <span className="italic text-muted-foreground font-normal">No answer provided</span>}
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        ) : null}
      </Modal>
    </AnimatedPage>
  );
}
