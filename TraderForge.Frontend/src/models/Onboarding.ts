export type TourPlacement = 'top' | 'bottom' | 'left' | 'right' | 'center';

/**
 * A single step of the beginner onboarding tour. When `target` resolves to a
 * mounted element the step spotlights it; otherwise it falls back to a centered
 * card, so a step never blocks the tour if its anchor is missing.
 */
export interface TourStep {
  id: string;
  title: string;
  body: string;
  /** CSS selector for the highlighted element, e.g. `[data-tour="chart"]`. */
  target?: string;
  /** Preferred tooltip side relative to the target. Defaults to `bottom`. */
  placement?: TourPlacement;
  /** Route to navigate to before showing the step. */
  route?: string;
}
