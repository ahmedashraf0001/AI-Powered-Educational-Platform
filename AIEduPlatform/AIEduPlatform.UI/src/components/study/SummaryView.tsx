import { useMutation } from '@tanstack/react-query';
import { studySessionsApi } from '@/api/studySessions.api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Spinner } from '@/components/ui/Spinner';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import remarkMath from 'remark-math';
import rehypeKatex from 'rehype-katex';
import 'katex/dist/katex.min.css';
import { useState, useMemo } from 'react';
import { FileText, CheckCircle2, BookOpen, Sparkles } from 'lucide-react';
import { toast } from 'sonner';
import { renderTextWithRefs, type MaterialInfo } from './SourceReference';
import type { SummaryDto } from '@/types';
import { preprocessMath } from '@/utils/mathUtils';

interface SummaryViewProps {
  sessionId: string;
  lectureIds: string[];
  materialIds: string[];
  materials?: MaterialInfo[];
  onOpenMaterial?: (materialId: string, page?: number, timestamp?: number) => void;
}

export function SummaryView({ sessionId, lectureIds, materialIds, materials = [], onOpenMaterial }: SummaryViewProps) {
  const [summaryData, setSummaryData] = useState<SummaryDto | null>(null);
  const [topic, setTopic] = useState('');

  const generateMutation = useMutation({
    mutationFn: () =>
      studySessionsApi.generateSummary(sessionId, { topic: topic || '', lectureIds, materialIds }),
    onSuccess: (res) => {
      const data = res.data.data;
      if (!data) return;
      // Handle both string and object responses
      const parsed = typeof data === 'string' ? (() => { try { return JSON.parse(data); } catch { return { summary: data }; } })() : data;
      setSummaryData({
        summary: parsed.summary || parsed.content || '',
        keyPoints: parsed.keyPoints || [],
        keyTerms: parsed.keyTerms || {},
      });
    },
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
  });

  const keyTerms = summaryData?.keyTerms;
  const keyTermEntries = keyTerms
    ? (typeof keyTerms === 'object' && !Array.isArray(keyTerms)
        ? Object.entries(keyTerms)
        : Array.isArray(keyTerms)
          ? (keyTerms as string[]).map((t) => [t, ''] as [string, string])
          : [])
    : [];

  // Custom markdown renderers for clickable source references
  const markdownComponents = useMemo(() => {
    if (!onOpenMaterial || materials.length === 0) return undefined;
    return {
      p: ({ children, ...props }: any) => {
        const processChild = (child: any): any => {
          if (typeof child === 'string') {
            return renderTextWithRefs(child, materials, onOpenMaterial);
          }
          return child;
        };
        const processed = Array.isArray(children) ? children.map(processChild) : processChild(children);
        return <p {...props}>{processed}</p>;
      },
      li: ({ children, ...props }: any) => {
        const processChild = (child: any): any => {
          if (typeof child === 'string') {
            return renderTextWithRefs(child, materials, onOpenMaterial);
          }
          return child;
        };
        const processed = Array.isArray(children) ? children.map(processChild) : processChild(children);
        return <li {...props}>{processed}</li>;
      },
    };
  }, [materials, onOpenMaterial]);

  return (
    <div className="p-5 space-y-5">
      {/* Header */}
      <div className="flex items-center justify-between gap-3 flex-wrap">
        <div className="flex items-center gap-2">
          <div className="p-2 rounded-lg bg-primary/10">
            <FileText className="h-5 w-5 text-primary" />
          </div>
          <h3 className="font-bold text-lg">Summary</h3>
        </div>
        <div className="flex gap-2">
          <Input
            placeholder="Topic (optional)"
            value={topic}
            onChange={(e) => setTopic(e.target.value)}
            className="w-48"
          />
          <Button
            onClick={() => generateMutation.mutate()}
            loading={generateMutation.isPending}
            variant="gradient"
          >
            <Sparkles className="h-4 w-4" />
            Generate
          </Button>
        </div>
      </div>

      {generateMutation.isPending && (
        <div className="flex flex-col items-center justify-center py-16 gap-3">
          <Spinner />
          <span className="text-muted-foreground">Generating summary...</span>
        </div>
      )}

      {summaryData && (
        <div className="space-y-5">
          {/* Main summary */}
          <div className="rounded-xl border bg-card p-6 shadow-sm">
            <div className="prose-ai">
              <ReactMarkdown
                remarkPlugins={[remarkGfm, remarkMath]}
                rehypePlugins={[rehypeKatex]}
                components={markdownComponents}
              >
                {preprocessMath(summaryData.summary || '')}
              </ReactMarkdown>
            </div>
          </div>

          {/* Key Points */}
          {summaryData.keyPoints && summaryData.keyPoints.length > 0 && (
            <div className="rounded-xl border bg-card p-5 shadow-sm">
              <div className="flex items-center gap-2 mb-4">
                <CheckCircle2 className="h-5 w-5 text-success" />
                <h4 className="font-semibold text-base">Key Points</h4>
              </div>
              <ul className="space-y-2.5">
                {summaryData.keyPoints.map((point, idx) => (
                  <li key={idx} className="flex items-start gap-3 p-2.5 rounded-lg bg-success/5 border border-success/10">
                    <span className="flex-shrink-0 w-6 h-6 rounded-full bg-success/15 text-success text-xs font-bold flex items-center justify-center mt-0.5">
                      {idx + 1}
                    </span>
                    <span className="text-sm leading-relaxed">{point}</span>
                  </li>
                ))}
              </ul>
            </div>
          )}

          {/* Key Terms */}
          {keyTermEntries.length > 0 && (
            <div className="rounded-xl border bg-card p-5 shadow-sm">
              <div className="flex items-center gap-2 mb-4">
                <BookOpen className="h-5 w-5 text-primary" />
                <h4 className="font-semibold text-base">Key Terms</h4>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                {keyTermEntries.map(([term, definition], idx) => (
                  <div key={idx} className="p-3 rounded-lg bg-primary/5 border border-primary/10">
                    <span className="font-semibold text-sm text-primary">{term}</span>
                    {definition && (
                      <p className="text-xs text-muted-foreground mt-1 leading-relaxed">{definition}</p>
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {!summaryData && !generateMutation.isPending && (
        <div className="flex flex-col items-center justify-center py-16 gap-3 text-muted-foreground">
          <FileText className="h-12 w-12 opacity-30" />
          <p className="text-sm">Generate a summary of your course materials</p>
          <p className="text-xs">Select references and click Generate</p>
        </div>
      )}
    </div>
  );
}

