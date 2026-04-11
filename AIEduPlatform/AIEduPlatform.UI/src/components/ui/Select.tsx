import { forwardRef, type SelectHTMLAttributes, useState, useRef, useEffect } from 'react';
import { cn } from '@/utils/cn';
import { ChevronDown, Check } from 'lucide-react';
import { AnimatePresence, motion } from 'framer-motion';

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
  hint?: string;
  options: Array<{ value: string; label: string }>;
  placeholder?: string;
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ className, label, error, hint, id, options, placeholder, value, defaultValue, onChange, ...props }, ref) => {
    const [isOpen, setIsOpen] = useState(false);
    
    // Manage internal state to display selected value
    const isControlled = value !== undefined;
    const [internalValue, setInternalValue] = useState(value ?? defaultValue ?? '');
    
    const currentValue = isControlled ? value : internalValue;
    const containerRef = useRef<HTMLDivElement>(null);
    const selectRef = useRef<HTMLSelectElement>(null);

    useEffect(() => {
      if (isControlled) {
        setInternalValue(value);
      }
    }, [value, isControlled]);

    useEffect(() => {
      const handleOutsideClick = (e: MouseEvent) => {
        if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
          setIsOpen(false);
        }
      };
      document.addEventListener('mousedown', handleOutsideClick);
      return () => document.removeEventListener('mousedown', handleOutsideClick);
    }, []);

    const setRefs = (node: HTMLSelectElement) => {
       (selectRef as any).current = node;
       if (typeof ref === 'function') ref(node);
       else if (ref) (ref as any).current = node;
    };

    const handleSelectOption = (v: string) => {
      setInternalValue(v);
      setIsOpen(false);

      if (selectRef.current) {
        // Essential step for deeply nested React forms combining refs:
        const nativeInputValueSetter = Object.getOwnPropertyDescriptor(
          window.HTMLSelectElement.prototype,
          'value'
        )?.set;
        nativeInputValueSetter?.call(selectRef.current, v);

        // Native change event to bubble up properly to react-hook-form
        selectRef.current.dispatchEvent(new Event('change', { bubbles: true }));
      }
    };

    return (
      <div className="space-y-1.5" ref={containerRef}>
        {label && (
          <label htmlFor={id} className="block text-sm font-medium text-foreground">
            {label}
          </label>
        )}
        <div className="relative">
          {/* Hidden select for accessibility and react-hook-form integration */}
          <select
            ref={setRefs}
            id={id}
            value={currentValue}
            onChange={onChange}
            className="sr-only"
            {...props}
          >
            {placeholder !== undefined && (
              <option value="" disabled>
                {placeholder}
              </option>
            )}
            {options.map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </select>

          {/* Custom Trigger UI */}
          <button
            type="button"
            className={cn(
              'flex h-11 w-full items-center justify-between rounded-lg border border-input bg-card px-3.5 py-2.5 text-sm shadow-sm transition-all duration-200 hover:border-primary/50 focus:outline-none focus:ring-2 focus:ring-ring/30 focus:border-primary disabled:cursor-not-allowed disabled:opacity-50 text-left',
              error && 'border-destructive hover:border-destructive focus:ring-destructive/30',
              !currentValue && 'text-muted-foreground',
              isOpen && 'border-primary ring-2 ring-ring/30',
              className
            )}
            onClick={() => !props.disabled && setIsOpen(!isOpen)}
            disabled={props.disabled}
          >
            <span className="truncate">
              {currentValue
                ? options.find((o) => o.value === String(currentValue))?.label || currentValue
                : placeholder || 'Select...'}
            </span>
            <ChevronDown
              className={cn(
                'h-4 w-4 text-muted-foreground transition-transform duration-200',
                isOpen && 'rotate-180'
              )}
            />
          </button>

          {/* Custom Dropdown List */}
          <AnimatePresence>
            {isOpen && (
              <motion.div
                initial={{ opacity: 0, y: -10 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: -10 }}
                transition={{ duration: 0.15 }}
                className="absolute z-50 mt-1 max-h-60 w-full overflow-auto rounded-lg border border-border bg-popover p-1 shadow-md"
              >
                {options.map((opt) => (
                  <button
                    key={opt.value}
                    type="button"
                    className={cn(
                      'relative flex w-full cursor-pointer select-none items-center rounded-sm py-2 pl-3 pr-9 text-sm outline-none transition-colors hover:bg-secondary/80 focus:bg-secondary/80',
                      String(currentValue) === opt.value ? 'bg-primary/10 text-primary font-medium hover:bg-primary/20' : 'text-popover-foreground'
                    )}
                    onClick={() => handleSelectOption(opt.value)}
                  >
                    <span className="block truncate">{opt.label}</span>
                    {String(currentValue) === opt.value && (
                      <span className="absolute inset-y-0 right-0 flex items-center pr-3 text-primary">
                        <Check className="h-4 w-4" />
                      </span>
                    )}
                  </button>
                ))}
              </motion.div>
            )}
          </AnimatePresence>
        </div>
        {hint && !error && <p className="text-xs text-muted-foreground">{hint}</p>}
        {error && <p className="text-xs text-destructive mt-1">{error}</p>}
      </div>
    );
  }
);

Select.displayName = 'Select';
