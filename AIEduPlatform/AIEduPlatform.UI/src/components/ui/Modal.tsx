import type { ReactNode } from 'react';
import { cn } from '@/utils/cn';
import { X } from 'lucide-react';
import { Button } from './Button';
import { AnimatePresence, motion } from 'framer-motion';

interface ModalProps {
  open: boolean;
  onClose: () => void;
  title?: string;
  description?: string;
  children: ReactNode;
  className?: string;
}

export function Modal({ open, onClose, title, description, children, className }: ModalProps) {
  return (
    <AnimatePresence>
      {open && (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
          <motion.div
            className="fixed inset-0 bg-black/60 backdrop-blur-sm"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={onClose}
          />
          <motion.div
            className={cn(
              'relative z-50 w-full max-w-lg rounded-2xl bg-card p-0 shadow-2xl border border-border mx-4 flex flex-col max-h-[90vh]',
              className
            )}
            initial={{ opacity: 0, scale: 0.95, y: 10 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.95, y: 10 }}
            transition={{ duration: 0.2, ease: 'easeOut' }}
          >
            <div className="flex-shrink-0 flex items-start justify-between px-6 pt-6 pb-2">
              <div>
                {title && <h2 className="text-lg font-semibold">{title}</h2>}
                {description && <p className="text-sm text-muted-foreground mt-0.5">{description}</p>}
              </div>
              <Button variant="ghost" size="icon" onClick={onClose} className="ml-auto -mt-1 -mr-2 shrink-0">
                <X className="h-4 w-4" />
              </Button>
            </div>
            <div className="px-6 pb-6 pt-2 overflow-y-auto min-h-0">
              {children}
            </div>
          </motion.div>
        </div>
      )}
    </AnimatePresence>
  );
}
