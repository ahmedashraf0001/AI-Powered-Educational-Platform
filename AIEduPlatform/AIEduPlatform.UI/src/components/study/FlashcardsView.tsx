import { useState, useEffect } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { studySessionsApi } from '@/api/studySessions.api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { toast } from 'sonner';
import { Spinner } from '@/components/ui/Spinner';
import { cn } from '@/utils/cn';
import { ChevronLeft, ChevronRight, RotateCcw, Lightbulb, Sparkles, Grid, Layers } from 'lucide-react';

interface FlashcardsViewProps {
  sessionId: string;
  lectureIds: string[];
  materialIds: string[];
  pendingData?: { timestamp: number; data: any } | null;
}

interface Flashcard {
  frontText: string;
  backText: string;
}

export function FlashcardsView({ sessionId, lectureIds, materialIds, pendingData }: FlashcardsViewProps) {
  const [cards, setCards] = useState<Flashcard[]>([]);
  const [flippedIdx, setFlippedIdx] = useState<Set<number>>(new Set());
  const [topic, setTopic] = useState('');
  const [currentIndex, setCurrentIndex] = useState(0);
  const [viewMode, setViewMode] = useState<'single' | 'grid'>('single');
  const [lastLoadedTimestamp, setLastLoadedTimestamp] = useState(0);
  const queryClient = useQueryClient();

  useEffect(() => {
    if (pendingData && pendingData.timestamp !== lastLoadedTimestamp) {
      const data = pendingData.data;
      setCards(Array.isArray(data) ? data.map((f: any) => ({ frontText: f.frontText, backText: f.backText })) : []);
      setFlippedIdx(new Set());
      setCurrentIndex(0);
      setLastLoadedTimestamp(pendingData.timestamp);
    }
  }, [pendingData, lastLoadedTimestamp]);

  const generateMutation = useMutation({
    mutationFn: () => {
      const promise = studySessionsApi.generateFlashcards(sessionId, { topic: topic || '', lectureIds, materialIds })
        .then((res) => {
          queryClient.invalidateQueries({ queryKey: ['flashcards-history', sessionId] });
          return res;
        });

      toast.promise(promise, {
        loading: 'Generating flashcards...',
        success: 'Flashcards generated successfully!',
        error: (err: any) => err?.userMessage || 'Failed to generate flashcards'
      });

      return promise;
    },
    onSuccess: (res) => {
      const data = res.data.data;
      setCards(Array.isArray(data) ? data.map((f: any) => ({ frontText: f.frontText, backText: f.backText })) : []);
      setFlippedIdx(new Set());
      setCurrentIndex(0);
    },
  });

  const { data: history } = useQuery({
    queryKey: ['flashcards-history', sessionId],
    queryFn: () => studySessionsApi.getFlashcards(sessionId),
    select: (res) => res.data.data?.items,
  });

  const toggle = (idx: number) => {
    setFlippedIdx((prev) => {
      const next = new Set(prev);
      if (next.has(idx)) next.delete(idx);
      else next.add(idx);
      return next;
    });
  };

  const goNext = () => {
    if (currentIndex < cards.length - 1) setCurrentIndex(currentIndex + 1);
  };

  const goPrev = () => {
    if (currentIndex > 0) setCurrentIndex(currentIndex - 1);
  };

  return (
    <div className="p-5 space-y-5">
      {/* Header */}
      <div className="flex items-center justify-between gap-3 flex-wrap">
        <div className="flex items-center gap-2">
          <div className="p-2 rounded-lg bg-amber-500/10">
            <Lightbulb className="h-5 w-5 text-amber-500" />
          </div>
          <h3 className="font-bold text-lg">Flashcards</h3>
          {cards.length > 0 && (
            <span className="text-xs text-muted-foreground bg-secondary px-2 py-1 rounded-full">
              {flippedIdx.size}/{cards.length} revealed
            </span>
          )}
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
          <span className="text-muted-foreground">Generating flashcards...</span>
        </div>
      )}

      {cards.length > 0 && (
        <>
          {/* View mode toggle */}
          <div className="flex items-center gap-2 justify-end">
            <Button
              variant={viewMode === 'single' ? 'primary' : 'outline'}
              size="sm"
              onClick={() => setViewMode('single')}
            >
              <Layers className="h-3.5 w-3.5" />
              <span className="text-xs">Single</span>
            </Button>
            <Button
              variant={viewMode === 'grid' ? 'primary' : 'outline'}
              size="sm"
              onClick={() => setViewMode('grid')}
            >
              <Grid className="h-3.5 w-3.5" />
              <span className="text-xs">Grid</span>
            </Button>
          </div>

          {viewMode === 'single' ? (
            /* Single card view */
            <div className="flex flex-col items-center gap-5">
              {/* Card counter */}
              <div className="flex items-center gap-2">
                {cards.map((_, idx) => (
                  <button
                    key={idx}
                    onClick={() => setCurrentIndex(idx)}
                    className={cn(
                      'w-2.5 h-2.5 rounded-full transition-all',
                      idx === currentIndex
                        ? 'bg-primary scale-125'
                        : flippedIdx.has(idx)
                          ? 'bg-primary/40'
                          : 'bg-border'
                    )}
                  />
                ))}
              </div>

              {/* 3D Flip card */}
              <div
                className="w-full max-w-lg cursor-pointer"
                style={{ perspective: '1000px' }}
                onClick={() => toggle(currentIndex)}
              >
                <div
                  className="relative w-full transition-transform duration-500"
                  style={{
                    transformStyle: 'preserve-3d',
                    transform: flippedIdx.has(currentIndex) ? 'rotateY(180deg)' : 'rotateY(0deg)',
                    minHeight: '220px',
                  }}
                >
                  {/* Front */}
                  <div
                    className="absolute inset-0 rounded-2xl border-2 border-primary/20 bg-gradient-to-br from-card to-secondary/50 p-8 flex flex-col items-center justify-center shadow-lg"
                    style={{ backfaceVisibility: 'hidden' }}
                  >
                    <span className="text-xs font-medium text-muted-foreground mb-1">
                      QUESTION {currentIndex + 1} of {cards.length}
                    </span>
                    <RotateCcw className="h-4 w-4 text-muted-foreground/40 mb-3" />
                    <p className="text-center font-medium text-lg leading-relaxed">
                      {cards[currentIndex].frontText}
                    </p>
                    <span className="text-xs text-muted-foreground/50 mt-4">Click to reveal</span>
                  </div>
                  {/* Back */}
                  <div
                    className="absolute inset-0 rounded-2xl border-2 border-success/20 bg-gradient-to-br from-success/5 to-success/10 p-8 flex flex-col items-center justify-center shadow-lg"
                    style={{ backfaceVisibility: 'hidden', transform: 'rotateY(180deg)' }}
                  >
                    <span className="text-xs font-medium text-success mb-1">
                      ANSWER {currentIndex + 1} of {cards.length}
                    </span>
                    <RotateCcw className="h-4 w-4 text-success/40 mb-3" />
                    <p className="text-center font-medium text-lg leading-relaxed">
                      {cards[currentIndex].backText}
                    </p>
                    <span className="text-xs text-success/50 mt-4">Click to flip back</span>
                  </div>
                </div>
              </div>

              {/* Navigation */}
              <div className="flex items-center gap-4">
                <Button
                  variant="outline"
                  size="icon"
                  onClick={goPrev}
                  disabled={currentIndex === 0}
                >
                  <ChevronLeft className="h-5 w-5" />
                </Button>
                <span className="text-sm font-medium text-muted-foreground">
                  {currentIndex + 1} / {cards.length}
                </span>
                <Button
                  variant="outline"
                  size="icon"
                  onClick={goNext}
                  disabled={currentIndex === cards.length - 1}
                >
                  <ChevronRight className="h-5 w-5" />
                </Button>
              </div>
            </div>
          ) : (
            /* Grid view */
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {cards.map((card, idx) => (
                <div
                  key={idx}
                  className="cursor-pointer"
                  style={{ perspective: '800px' }}
                  onClick={() => toggle(idx)}
                >
                  <div
                    className="relative w-full transition-transform duration-500"
                    style={{
                      transformStyle: 'preserve-3d',
                      transform: flippedIdx.has(idx) ? 'rotateY(180deg)' : 'rotateY(0deg)',
                      minHeight: '150px',
                    }}
                  >
                    {/* Front */}
                    <div
                      className="absolute inset-0 rounded-xl border bg-card hover:shadow-md transition-shadow p-5 flex flex-col items-center justify-center"
                      style={{ backfaceVisibility: 'hidden' }}
                    >
                      <span className="text-[10px] font-bold text-muted-foreground/60 mb-2 uppercase tracking-wider">
                        Question {idx + 1}
                      </span>
                      <p className="text-center text-sm font-medium leading-relaxed">
                        {card.frontText}
                      </p>
                    </div>
                    {/* Back */}
                    <div
                      className="absolute inset-0 rounded-xl border border-success/20 bg-success/5 p-5 flex flex-col items-center justify-center"
                      style={{ backfaceVisibility: 'hidden', transform: 'rotateY(180deg)' }}
                    >
                      <span className="text-[10px] font-bold text-success/60 mb-2 uppercase tracking-wider">
                        Answer {idx + 1}
                      </span>
                      <p className="text-center text-sm font-medium leading-relaxed">
                        {card.backText}
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </>
      )}

      {/* History */}
      {history && Array.isArray(history) && history.length > 0 && cards.length === 0 && !generateMutation.isPending && (
        <div className="border-t pt-4 mt-6">
          <h4 className="font-medium mb-3 text-sm text-muted-foreground">Previously Generated ({history.length} cards)</h4>
          <Button
            variant="outline"
            size="sm"
            onClick={() => {
              setCards(history.map((f: any) => ({ frontText: f.frontText ?? '', backText: f.backText ?? '' })));
              setFlippedIdx(new Set());
              setCurrentIndex(0);
            }}
          >
            Load Saved Flashcards
          </Button>
        </div>
      )}

      {cards.length === 0 && !generateMutation.isPending && (
        <div className="flex flex-col items-center justify-center py-16 gap-3 text-muted-foreground">
          <Lightbulb className="h-12 w-12 opacity-30" />
          <p className="text-sm">Generate flashcards from your course materials</p>
          <p className="text-xs">Select references and click Generate</p>
        </div>
      )}
    </div>
  );
}
