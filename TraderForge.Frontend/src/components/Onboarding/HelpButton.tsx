import { HelpCircle } from 'lucide-react';
import { useOnboardingStore } from '@store/onboardingStore';

/** Re-launches the onboarding tour from anywhere it is rendered. */
export function HelpButton() {
  const start = useOnboardingStore((s) => s.start);

  return (
    <button
      onClick={start}
      aria-label="Ver tutorial de bienvenida"
      title="Ver tutorial"
      className="flex items-center gap-3 w-full px-3 py-2 rounded-lg text-sm text-neutral-500 hover:text-neutral-200 hover:bg-neutral-800 transition-colors"
    >
      <HelpCircle size={16} />
      Tutorial
    </button>
  );
}
