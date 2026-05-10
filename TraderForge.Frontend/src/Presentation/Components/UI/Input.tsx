import type { InputHTMLAttributes } from 'react';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
}

/**
 * @param label - Field label displayed above the input
 * @param error - Validation error message shown below
 */
export function Input({ label, error, className = '', id, ...rest }: InputProps) {
  return (
    <div className="flex flex-col gap-1">
      {label && (
        <label htmlFor={id} className="text-xs font-medium text-neutral-400 uppercase tracking-wider">
          {label}
        </label>
      )}
      <input
        id={id}
        className={`w-full bg-neutral-900 border ${error ? 'border-red-500' : 'border-neutral-700'} text-neutral-100 rounded-lg px-3 py-2.5 text-sm placeholder-neutral-600 focus:outline-none focus:border-emerald-500 transition-colors ${className}`}
        {...rest}
      />
      {error && <span className="text-xs text-red-400">{error}</span>}
    </div>
  );
}
