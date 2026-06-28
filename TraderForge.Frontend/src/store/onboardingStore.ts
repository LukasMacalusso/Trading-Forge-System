import { create } from 'zustand';
import { OnboardingRepository } from '@utils/OnboardingRepository';

interface OnboardingState {
  isActive: boolean;
  stepIndex: number;
  /** Starts the tour from the first step. */
  start: () => void;
  setStepIndex: (index: number) => void;
  /** Ends the tour; pass `true` when the user finished it to stop auto-launch. */
  stop: (completed: boolean) => void;
}

export const useOnboardingStore = create<OnboardingState>((set) => ({
  isActive: false,
  stepIndex: 0,

  start: () => set({ isActive: true, stepIndex: 0 }),

  setStepIndex: (stepIndex) => set({ stepIndex }),

  stop: (completed) => {
    if (completed) OnboardingRepository.markCompleted();
    set({ isActive: false, stepIndex: 0 });
  },
}));
