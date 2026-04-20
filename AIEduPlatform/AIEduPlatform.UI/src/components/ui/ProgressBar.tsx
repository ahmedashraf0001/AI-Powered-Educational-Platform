import { cn } from '@/utils/cn';

interface ProgressBarProps {
  progress: number;
  className?: string;
  fillClassName?: string;
}

function clampProgress(value: number) {
  if (!Number.isFinite(value)) return 0;
  return Math.max(0, Math.min(100, value));
}

export function ProgressBar({ progress, className, fillClassName }: ProgressBarProps) {
  const clampedProgress = clampProgress(progress);

  return (
    <div
      className={cn('w-full overflow-hidden rounded-full', className)}
      style={{ width: '100%', height: '8px', backgroundColor: '#e5e7eb', borderRadius: '4px' }}
      aria-label="Progress"
      role="progressbar"
      aria-valuemin={0}
      aria-valuemax={100}
      aria-valuenow={Math.round(clampedProgress)}
    >
      <div
        className={cn('h-full rounded-full bg-primary transition-all duration-500', fillClassName)}
        style={{
          width: `${clampedProgress}%`,
          height: '100%',
          borderRadius: '4px',
          ...(fillClassName ? {} : { backgroundColor: '#3b82f6' }),
        }}
      />
    </div>
  );
}
