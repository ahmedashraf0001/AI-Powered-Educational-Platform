import { useState, useRef, useCallback } from 'react';
import { Button } from '@/components/ui/Button';
import { BookOpen } from 'lucide-react';

interface Section {
  id: string;
  title: string;
  startSeconds: number | null;
  endSeconds: number | null;
}

interface ChapterProgressBarProps {
  currentTime: number;
  duration: number;
  sections: Section[];
  onSeek: (time: number) => void;
  onSummarizeSection?: (sectionId: string) => void;
  disabled?: boolean;
}

const SEGMENT_COLORS = [
  'bg-blue-500',
  'bg-purple-500',
  'bg-green-500',
  'bg-orange-500',
  'bg-pink-500',
  'bg-cyan-500',
  'bg-amber-500',
  'bg-rose-500',
];

export function ChapterProgressBar({
  currentTime,
  duration,
  sections,
  onSeek,
  onSummarizeSection,
  disabled = false,
}: ChapterProgressBarProps) {
  const [hoveredSection, setHoveredSection] = useState<Section | null>(null);
  const [hoverPosition, setHoverPosition] = useState({ x: 0, y: 0 });
  const barRef = useRef<HTMLDivElement>(null);

  // If no sections or duration, render a simple progress bar
  const hasSections = sections.length > 0 && duration > 0;

  const getSectionWidth = useCallback(
    (section: Section, index: number) => {
      if (!duration) return 0;
      const start = section.startSeconds ?? 0;
      const nextSection = sections[index + 1];
      const end = section.endSeconds ?? nextSection?.startSeconds ?? duration;
      return ((end - start) / duration) * 100;
    },
    [duration, sections]
  );

  const handleBarClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (disabled || !barRef.current || !duration) return;
    const rect = barRef.current.getBoundingClientRect();
    const clickX = e.clientX - rect.left;
    const percentage = clickX / rect.width;
    const seekTime = percentage * duration;
    onSeek(Math.max(0, Math.min(seekTime, duration)));
  };

  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>, section: Section) => {
    if (!barRef.current) return;
    const rect = barRef.current.getBoundingClientRect();
    setHoverPosition({
      x: e.clientX - rect.left,
      y: rect.top,
    });
    setHoveredSection(section);
  };

  const handleMouseLeave = () => {
    setHoveredSection(null);
  };

  const progressPercent = duration > 0 ? (currentTime / duration) * 100 : 0;

  // Simple progress bar when no sections
  if (!hasSections) {
    return (
      <div className="relative w-full group">
        <div
          ref={barRef}
          className="h-1.5 bg-secondary rounded-full cursor-pointer group-hover:h-2.5 transition-all"
          onClick={handleBarClick}
        >
          <div
            className="h-full bg-primary rounded-full relative transition-all"
            style={{ width: `${progressPercent}%` }}
          >
            <div className="absolute right-0 top-1/2 -translate-y-1/2 w-3 h-3 bg-primary rounded-full shadow-md opacity-0 group-hover:opacity-100 transition-opacity" />
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="relative w-full group">
      {/* Hover tooltip */}
      {hoveredSection && (
        <div
          className="absolute z-20 bottom-full mb-3 -translate-x-1/2 pointer-events-none"
          style={{ left: hoverPosition.x }}
        >
          <div className="bg-card border rounded-lg shadow-xl p-3 min-w-[180px] pointer-events-auto">
            <p className="text-xs font-medium text-foreground mb-2 line-clamp-2">
              {hoveredSection.title}
            </p>
            <div className="flex items-center justify-between text-[10px] text-muted-foreground mb-2">
              <span>
                {formatTime(hoveredSection.startSeconds ?? 0)} -{' '}
                {formatTime(hoveredSection.endSeconds ?? duration)}
              </span>
            </div>
            {onSummarizeSection && (
              <Button
                size="sm"
                variant="ghost"
                className="w-full h-7 text-xs"
                onClick={(e) => {
                  e.stopPropagation();
                  onSummarizeSection(hoveredSection.id);
                  setHoveredSection(null);
                }}
              >
                <BookOpen className="h-3 w-3 mr-1.5" />
                Summarize
              </Button>
            )}
          </div>
        </div>
      )}

      {/* Progress bar with segments */}
      <div
        ref={barRef}
        className="h-1.5 flex rounded-full overflow-hidden cursor-pointer group-hover:h-2.5 transition-all"
        onClick={handleBarClick}
      >
        {sections.map((section, index) => {
          const width = getSectionWidth(section, index);
          const start = section.startSeconds ?? 0;
          const end = section.endSeconds ?? sections[index + 1]?.startSeconds ?? duration;
          const isActive = currentTime >= start && currentTime < end;
          const segmentProgress =
            currentTime >= end
              ? 100
              : currentTime >= start
              ? ((currentTime - start) / (end - start)) * 100
              : 0;

          return (
            <div
              key={section.id}
              className={`relative h-full ${SEGMENT_COLORS[index % SEGMENT_COLORS.length]} opacity-30 hover:opacity-50 transition-opacity`}
              style={{ width: `${width}%` }}
              onMouseMove={(e) => handleMouseMove(e, section)}
              onMouseLeave={handleMouseLeave}
            >
              {/* Filled portion */}
              <div
                className={`absolute inset-y-0 left-0 ${SEGMENT_COLORS[index % SEGMENT_COLORS.length]} opacity-100`}
                style={{ width: `${segmentProgress}%` }}
              />
              {/* Segment divider */}
              {index < sections.length - 1 && (
                <div className="absolute right-0 top-0 bottom-0 w-0.5 bg-background/50" />
              )}
              {/* Active indicator */}
              {isActive && (
                <div
                  className="absolute top-1/2 -translate-y-1/2 w-3 h-3 bg-white rounded-full shadow-md opacity-0 group-hover:opacity-100 transition-opacity"
                  style={{ left: `${segmentProgress}%`, transform: 'translate(-50%, -50%)' }}
                />
              )}
            </div>
          );
        })}
      </div>

      {/* Section labels (on hover) */}
      <div className="hidden group-hover:flex absolute -bottom-5 left-0 right-0 text-[9px] text-muted-foreground">
        {sections.map((section, index) => {
          const width = getSectionWidth(section, index);
          return (
            <div
              key={section.id}
              className="truncate px-0.5 text-center"
              style={{ width: `${width}%` }}
            >
              {section.title}
            </div>
          );
        })}
      </div>
    </div>
  );
}

function formatTime(seconds: number): string {
  const mins = Math.floor(seconds / 60);
  const secs = Math.floor(seconds % 60);
  return `${mins}:${secs.toString().padStart(2, '0')}`;
}
