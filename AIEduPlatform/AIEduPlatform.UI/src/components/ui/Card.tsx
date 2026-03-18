import type { ReactNode } from 'react';
import { cn } from '@/utils/cn';

interface CardProps {
  children: ReactNode;
  className?: string;
  variant?: 'default' | 'glass' | 'gradient-border';
  onClick?: () => void;
}

export function Card({ children, className, variant = 'default', onClick }: CardProps) {
  if (variant === 'gradient-border') {
    return (
      <div className="bg-gradient-to-r from-primary to-accent p-[1px] rounded-xl">
        <div
          className={cn(
            'rounded-[calc(0.75rem-1px)] bg-card text-card-foreground',
            onClick && 'cursor-pointer',
            className
          )}
          onClick={onClick}
        >
          {children}
        </div>
      </div>
    );
  }

  return (
    <div
      className={cn(
        'rounded-xl border border-border text-card-foreground shadow-sm',
        variant === 'glass' ? 'glass' : 'bg-card',
        onClick && 'cursor-pointer hover:shadow-lg hover:shadow-primary/5 transition-all duration-300',
        className
      )}
      onClick={onClick}
    >
      {children}
    </div>
  );
}

export function CardHeader({ children, className }: { children: ReactNode; className?: string }) {
  return <div className={cn('flex flex-col space-y-1.5 p-6', className)}>{children}</div>;
}

export function CardTitle({ children, className }: { children: ReactNode; className?: string }) {
  return <h3 className={cn('text-lg font-semibold leading-none tracking-tight', className)}>{children}</h3>;
}

export function CardDescription({ children, className }: { children: ReactNode; className?: string }) {
  return <p className={cn('text-sm text-muted-foreground', className)}>{children}</p>;
}

export function CardContent({ children, className }: { children: ReactNode; className?: string }) {
  return <div className={cn('p-6 pt-0', className)}>{children}</div>;
}

export function CardFooter({ children, className }: { children: ReactNode; className?: string }) {
  return <div className={cn('flex items-center p-6 pt-0', className)}>{children}</div>;
}
