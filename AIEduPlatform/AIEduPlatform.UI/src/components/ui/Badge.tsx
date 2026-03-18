import type { ReactNode } from 'react';
import { cn } from '@/utils/cn';

interface BadgeProps {
  children: ReactNode;
  variant?: 'default' | 'success' | 'warning' | 'destructive' | 'outline' | 'info';
  pulse?: boolean;
  className?: string;
}

const badgeVariants = {
  default: 'bg-primary/10 text-primary border-primary/20',
  success: 'bg-success/10 text-success border-success/20',
  warning: 'bg-warning/10 text-warning border-warning/20',
  destructive: 'bg-destructive/10 text-destructive border-destructive/20',
  outline: 'bg-transparent text-foreground border-border',
  info: 'bg-info/10 text-info border-info/20',
};

export function Badge({ children, variant = 'default', pulse, className }: BadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-medium transition-all duration-200',
        badgeVariants[variant],
        pulse && 'animate-pulse',
        className
      )}
    >
      {children}
    </span>
  );
}
