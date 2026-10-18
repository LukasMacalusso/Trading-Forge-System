type BadgeVariant = 'up' | 'down' | 'neutral' | 'warning' | 'info';

interface BadgeProps {
  variant?: BadgeVariant;
  children: React.ReactNode;
  className?: string;
}

const variantClasses: Record<BadgeVariant, string> = {
  up:      'bg-emerald-500/15 text-emerald-400 border border-emerald-500/20',
  down:    'bg-red-500/15 text-red-400 border border-red-500/20',
  neutral: 'bg-neutral-800 text-neutral-400 border border-neutral-700',
  warning: 'bg-amber-500/15 text-amber-400 border border-amber-500/20',
  info:    'bg-blue-500/15 text-blue-400 border border-blue-500/20',
};

export function Badge({ variant = 'neutral', children, className = '' }: BadgeProps) {
  return (
    <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${variantClasses[variant]} ${className}`}>
      {children}
    </span>
  );
}
