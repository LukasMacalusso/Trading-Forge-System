import { create } from 'zustand';
import { OnboardingRepository } from '@utils/OnboardingRepository';

interface OnboardingState {
  isActive: boolean;
  stepIndex: number;
  /** Starts the tour from the first step. */
  start: () => void;
  setStepIndex: (index: number) => void;
  /** Ends the tour. `completed` is kept for callers but the tour never
   * auto-launches again once the user has seen it (finished or skipped). */
  stop: (completed: boolean) => void;
}

export const useOnboardingStore = create<OnboardingState>((set) => ({
  isActive: false,
  stepIndex: 0,

  start: () => set({ isActive: true, stepIndex: 0 }),

  setStepIndex: (stepIndex) => set({ stepIndex }),

  stop: () => {
    // Once seen, don't auto-launch again on refresh — whether the user
    // finished the tour or skipped it. They can replay it from the Help button.
    OnboardingRepository.markCompleted();
    set({ isActive: false, stepIndex: 0 });
  },
}));
