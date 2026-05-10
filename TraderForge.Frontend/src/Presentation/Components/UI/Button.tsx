import { motion } from 'framer-motion';
import type { ButtonHTMLAttributes } from 'react';

type Variant = 'primary' | 'secondary' | 'danger' | 'ghost';
type Size = 'sm' | 'md' | 'lg';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  size?: Size;
  isLoading?: boolean;
}

const variantClasses: Record<Variant, string> = {
  primary:   'bg-emerald-500 hover:bg-emerald-400 text-neutral-950 font-semibold',
  secondary: 'bg-neutral-800 hover:bg-neutral-700 text-neutral-100 border border-neutral-700',
  danger:    'bg-red-500/20 hover:bg-red-500/30 text-red-400 border border-red-500/30',
  ghost:     'hover:bg-neutral-800 text-neutral-400 hover:text-neutral-100',
};

const sizeClasses: Record<Size, string> = {
  sm: 'px-3 py-1.5 text-xs rounded-md',
  md: 'px-4 py-2 text-sm rounded-lg',
  lg: 'px-6 py-3 text-base rounded-lg',
};

/**
 * @param variant - Visual style of the button
 * @param size - Padding/font size preset
 * @param isLoading - Shows a spinner and disables interaction
 */
export function Button({ variant = 'primary', size = 'md', isLoading, children, className = '', disabled, ...rest }: ButtonProps) {
  return (
    <motion.button
      whileTap={{ scale: 0.97 }}
      disabled={disabled || isLoading}
      className={`inline-flex items-center justify-center gap-2 transition-colors duration-150 disabled:opacity-50 disabled:cursor-not-allowed ${variantClasses[variant]} ${sizeClasses[size]} ${className}`}
      {...(rest as object)}
    >
      {isLoading && (
        <span className="w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin" />
      )}
      {children}
    </motion.button>
  );
}
