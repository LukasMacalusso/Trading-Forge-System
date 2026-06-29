const ONBOARDING_KEY = 'tf_onboarding_done';

/**
 * Tracks whether the user has completed the beginner onboarding. The backend
 * has no field for this, so it is persisted client-side in localStorage.
 */
export const OnboardingRepository = {
  isCompleted: (): boolean => localStorage.getItem(ONBOARDING_KEY) === 'true',
  markCompleted: (): void => localStorage.setItem(ONBOARDING_KEY, 'true'),
  reset: (): void => localStorage.removeItem(ONBOARDING_KEY),
};
