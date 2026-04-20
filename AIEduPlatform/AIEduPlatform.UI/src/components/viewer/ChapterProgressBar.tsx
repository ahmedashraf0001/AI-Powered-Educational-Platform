import { useState, useRef, useCallback } from 'react';
import { Button } from '@/components/ui/Button';
import { BookOpen, FileQuestion, Lightbulb } from 'lucide-react';

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
  videoUrl?: string;
  onSeek: (time: number) => void;
  onSectionAction?: (type: 'quiz' | 'summary' | 'flashcards', sectionId: string) => void;
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
  videoUrl,
  onSeek,
  onSectionAction,
  disabled = false,
}: ChapterProgressBarProps) {
  const [hoveredSection, setHoveredSection] = useState<Section | null>(null);
  const [hoverTime, setHoverTime] = useState<number | null>(null);
  const [hoverPosition, setHoverPosition] = useState({ x: 0, y: 0 });
  const barRef = useRef<HTMLDivElement>(null);
  const hoverVideoRef = useRef<HTMLVideoElement>(null);

  // If no sections or duration, render a simple progress bar
  const hasSections = sections.length > 0 && duration > 0;

  const getSectionStyles = useCallback(
    (section: Section, index: number) => {
      if (!duration) return { left: 0, width: 0 };
      const start = section.startSeconds ?? 0;
      const end = section.endSeconds ?? sections[index + 1]?.startSeconds ?? duration;
      const left = (start / duration) * 100;
      const width = ((end - start) / duration) * 100;
      return { left: `${left}%`, width: `${width}%` };
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

  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>, section?: Section) => {
    if (!barRef.current || !duration) return;
    const rect = barRef.current.getBoundingClientRect();
    const xOffset = e.clientX - rect.left;
    
    // clamp x between 0 and rect.width
    const clampedX = Math.max(0, Math.min(xOffset, rect.width));
    const rawTime = (clampedX / rect.width) * duration;
    setHoverTime(rawTime);

    setHoverPosition({
      x: clampedX,
      y: rect.top,
    });
    
    if (section) {
      setHoveredSection(section);
    } else {
      // Find the section that matches this time
      const foundSection = sections.find(s => {
        const start = s.startSeconds ?? 0;
        const end = s.endSeconds ?? duration;
        return rawTime >= start && rawTime < end;
      });
      setHoveredSection(foundSection || null);
    }

    if (hoverVideoRef.current && hoverVideoRef.current.readyState >= 1) {
      hoverVideoRef.current.currentTime = rawTime;
    }
  };

  const handleMouseLeave = () => {
    setHoveredSection(null);
    setHoverTime(null);
  };

  const progressPercent = duration > 0 ? (currentTime / duration) * 100 : 0;

  // IF there's video URL, we want to render the hover tooltip even if no sections exist.
  const renderTooltip = () => {
    if (hoverPosition.x === 0 && !hoveredSection) return null;

    let displayTime = formatTime(hoverTime ?? 0);
    if (hoveredSection && hasSections) {
      displayTime = `${formatTime(hoveredSection.startSeconds ?? 0)} - ${formatTime(hoveredSection.endSeconds ?? duration)}`;
    }

    return (
      <div
        className="absolute z-20 bottom-full pb-3 pointer-events-auto"
        style={{ left: hoverPosition.x, transform: 'translateX(-50%)' }}
      >
        <div className="bg-background/95 backdrop-blur-sm border border-white/20 rounded-lg shadow-xl p-1.5 w-56 flex flex-col overflow-hidden">
          {videoUrl && (
            <div className="w-full bg-black rounded-sm overflow-hidden aspect-video mb-1.5 border border-white/10">
              <video
                ref={hoverVideoRef}
                src={videoUrl}
                muted
                preload="metadata"
                className="w-full h-full object-cover"
              />
            </div>
          )}

          <div className="flex justify-between items-center text-[10px] text-muted-foreground">
            <span className="font-semibold text-foreground truncate mr-2 flex-1">
              {hoveredSection ? hoveredSection.title : displayTime}
            </span>
            <span className="bg-secondary px-1.5 py-0.5 rounded font-mono shrink-0">
               {formatTime(hoverTime ?? 0)}
            </span>
          </div>

          {onSectionAction && hoveredSection && hasSections && (
            <div className="flex gap-1.5 mt-1.5 justify-between">
              <Button
                size="sm"
                variant="ghost"
                title="Summary"
                className="flex-1 h-7 text-[10px] bg-primary/10 hover:bg-primary/20 px-0"
                onClick={(e) => {
                  e.stopPropagation();
                  onSectionAction('summary', hoveredSection.id);
                  setHoveredSection(null);
                  setHoverTime(null);
                }}
              >
                <BookOpen className="h-3.5 w-3.5" />
              </Button>
              <Button
                size="sm"
                variant="ghost"
                title="Quiz"
                className="flex-1 h-7 text-[10px] bg-primary/10 hover:bg-primary/20 px-0"
                onClick={(e) => {
                  e.stopPropagation();
                  onSectionAction('quiz', hoveredSection.id);
                  setHoveredSection(null);
                  setHoverTime(null);
                }}
              >
                <FileQuestion className="h-3.5 w-3.5" />
              </Button>
              <Button
                size="sm"
                variant="ghost"
                title="Flashcards"
                className="flex-1 h-7 text-[10px] bg-primary/10 hover:bg-primary/20 px-0"
                onClick={(e) => {
                  e.stopPropagation();
                  onSectionAction('flashcards', hoveredSection.id);
                  setHoveredSection(null);
                  setHoverTime(null);
                }}
              >
                <Lightbulb className="h-3.5 w-3.5" />
              </Button>
            </div>
          )}
        </div>
      </div>
    );
  };

  // Simple progress bar when no sections
  if (!hasSections) {
    return (
      <div className="relative w-full group" onMouseLeave={handleMouseLeave}>
        {hoverTime !== null && renderTooltip()}
        <div
          ref={barRef}
          className="h-1.5 bg-secondary rounded-full cursor-pointer group-hover:h-2.5 transition-all"
          onClick={handleBarClick}
          onMouseMove={(e) => handleMouseMove(e)}
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
    <div className="relative w-full group" onMouseLeave={handleMouseLeave}>
      {hoverTime !== null && renderTooltip()}

      {/* Progress bar with segments */}
      <div
        ref={barRef}
        className="h-1.5 flex rounded-full overflow-hidden cursor-pointer group-hover:h-2.5 transition-all bg-secondary w-full"
        onClick={handleBarClick}
      >
        <div className="relative w-full h-full text-[0] leading-none" onMouseMove={(e) => handleMouseMove(e)}>
          <div
            className="absolute top-0 bottom-0 left-0 bg-primary transition-all z-0"
            style={{ width: `${progressPercent}%` }}
          />

          {sections.map((section, index) => {
            const { left, width } = getSectionStyles(section, index);
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
                className={`absolute top-0 bottom-0 z-10 ${SEGMENT_COLORS[index % SEGMENT_COLORS.length]} hover:opacity-70 transition-opacity`}
                style={{ left, width, opacity: isActive ? '0.6' : '0.3' }}
              >
                <div
                  className={`absolute inset-y-0 left-0 bg-white/20`}
                  style={{ width: `${segmentProgress}%` }}
                />
                {index < sections.length - 1 && (
                  <div className="absolute right-0 top-0 bottom-0 w-0.5 bg-background/50" />
                )}
                {isActive && (
                  <div
                    className="absolute top-1/2 -translate-y-1/2 w-3 h-3 bg-white rounded-full shadow-md opacity-0 group-hover:opacity-100 transition-opacity"
                    style={{ left: `${segmentProgress}%`, transform: 'translate(-50%, -50%)', zIndex: 20 }}
                  />
                )}
              </div>
            );
          })}
        </div>
      </div>

      {/* Section labels (on hover) */}
      <div className="hidden group-hover:flex absolute -bottom-5 left-0 right-0 h-4 pointer-events-none text-muted-foreground text-[9px]">
        {sections.map((section, index) => {
          const { left, width } = getSectionStyles(section, index);
          return (
            <div
              key={section.id}
              className="absolute truncate px-0.5 text-center"
              style={{ left, width }}
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
