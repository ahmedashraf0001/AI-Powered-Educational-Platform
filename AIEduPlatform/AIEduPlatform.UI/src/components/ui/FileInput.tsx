import { forwardRef, useState, type InputHTMLAttributes } from 'react';
import { cn } from '@/utils/cn';
import { Upload, FileText } from 'lucide-react';

interface FileInputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label?: string;
  hint?: string;
  error?: string;
}

export const FileInput = forwardRef<HTMLInputElement, FileInputProps>(
  ({ className, label, hint, error, id, onChange, ...props }, ref) => {
    const [fileName, setFileName] = useState<string | null>(null);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      const files = e.target.files;
      if (files && files.length > 0) {
        setFileName(Array.from(files).map(f => f.name).join(', '));
      } else {
        setFileName(null);
      }
      onChange?.(e);
    };

    return (
      <div className="space-y-1.5">
        {label && (
          <label htmlFor={id} className="block text-sm font-medium text-foreground">
            {label}
          </label>
        )}
        <div
          className={cn(
            'relative flex items-center gap-3 w-full rounded-lg border border-dashed border-input bg-card px-3.5 py-3 text-sm transition-all duration-200 hover:border-primary/50 cursor-pointer',
            error && 'border-destructive',
            className
          )}
        >
          {fileName ? (
            <FileText className="h-5 w-5 text-primary shrink-0" />
          ) : (
            <Upload className="h-5 w-5 text-muted-foreground shrink-0" />
          )}
          <input
            ref={ref}
            id={id}
            type="file"
            onChange={handleChange}
            className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
            {...props}
          />
          <span className={cn("truncate", fileName ? "text-foreground font-medium" : "text-muted-foreground")}>
            {fileName || `Choose file${props.multiple ? 's' : ''}...`}
          </span>
        </div>
        {hint && !error && <p className="text-xs text-muted-foreground">{hint}</p>}
        {error && <p className="text-xs text-destructive mt-1">{error}</p>}
      </div>
    );
  }
);

FileInput.displayName = 'FileInput';
