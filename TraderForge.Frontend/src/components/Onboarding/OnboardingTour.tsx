import { useCallback, useEffect, useLayoutEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { useLocation, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { X, ArrowLeft, ArrowRight, Check } from 'lucide-react';
import { useOnboardingStore } from '@store/onboardingStore';
import type { TourPlacement } from '@models/Onboarding';
import { TOUR_STEPS } from './tourSteps';

const TOOLTIP_WIDTH = 320;
const GAP = 14; // distance between target and tooltip
const MARGIN = 12; // viewport clamp margin
const SPOTLIGHT_PADDING = 8;

interface Rect {
  top: number;
  left: number;
  width: number;
  height: number;
}

function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max);
}

/** Computes the tooltip position from the target rect, falling back to center. */
function getTooltipPosition(
  rect: Rect | null,
  placement: TourPlacement,
  tooltipHeight: number,
): { top: number; left: number } {
  const vw = window.innerWidth;
  const vh = window.innerHeight;

  if (!rect || placement === 'center') {
    return {
      top: Math.max(MARGIN, vh / 2 - tooltipHeight / 2),
      left: vw / 2 - TOOLTIP_WIDTH / 2,
    };
  }

  let top: number;
  let left: number;

  switch (placement) {
    case 'top':
      top = rect.top - tooltipHeight - GAP;
      left = rect.left + rect.width / 2 - TOOLTIP_WIDTH / 2;
      break;
    case 'left':
      top = rect.top + rect.height / 2 - tooltipHeight / 2;
      left = rect.left - TOOLTIP_WIDTH - GAP;
      break;
    case 'right':
      top = rect.top + rect.height / 2 - tooltipHeight / 2;
      left = rect.left + rect.width + GAP;
      break;
    case 'bottom':
    default:
      top = rect.top + rect.height + GAP;
      left = rect.left + rect.width / 2 - TOOLTIP_WIDTH / 2;
      break;
  }

  return {
    top: clamp(top, MARGIN, vh - tooltipHeight - MARGIN),
    left: clamp(left, MARGIN, vw - TOOLTIP_WIDTH - MARGIN),
  };
}

export function OnboardingTour() {
  const { isActive, stepIndex, setStepIndex, stop } = useOnboardingStore();
  const navigate = useNavigate();
  const location = useLocation();

  const [rect, setRect] = useState<Rect | null>(null);
  const [tooltipHeight, setTooltipHeight] = useState(180);
  const tooltipRef = useRef<HTMLDivElement>(null);

  const step = TOUR_STEPS[stepIndex];
  const isFirst = stepIndex === 0;
  const isLast = stepIndex === TOUR_STEPS.length - 1;
  const placement = step?.placement ?? 'bottom';

  const measure = useCallback(() => {
    if (!step?.target) {
      setRect(null);
      return;
    }
    const el = document.querySelector<HTMLElement>(step.target);
    if (!el) {
      setRect(null);
      return;
    }
    const r = el.getBoundingClientRect();
    setRect({ top: r.top, left: r.left, width: r.width, height: r.height });
  }, [step]);

  // Navigate to the step's route, scroll its target into view, then measure it.
  useEffect(() => {
    if (!isActive || !step) return;

    if (step.route && location.pathname !== step.route) {
      navigate(step.route);
      return; // re-runs once the route updates
    }

    const raf = requestAnimationFrame(() => {
      const el = step.target ? document.querySelector<HTMLElement>(step.target) : null;
      el?.scrollIntoView({ behavior: 'smooth', block: 'center' });
      window.setTimeout(measure, 220);
    });

    window.addEventListener('resize', measure);
    window.addEventListener('scroll', measure, true);
    return () => {
      cancelAnimationFrame(raf);
      window.removeEventListener('resize', measure);
      window.removeEventListener('scroll', measure, true);
    };
  }, [isActive, stepIndex, step, location.pathname, navigate, measure]);

  // Keep the measured tooltip height in sync for accurate positioning.
  useLayoutEffect(() => {
    if (tooltipRef.current) setTooltipHeight(tooltipRef.current.offsetHeight);
  }, [stepIndex, rect, isActive]);

  const handleNext = useCallback(() => {
    if (isLast) stop(true);
    else setStepIndex(stepIndex + 1);
  }, [isLast, stop, setStepIndex, stepIndex]);

  const handlePrev = useCallback(() => {
    if (!isFirst) setStepIndex(stepIndex - 1);
  }, [isFirst, setStepIndex, stepIndex]);

  // Keyboard navigation while the tour is active.
  useEffect(() => {
    if (!isActive) return;
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') stop(false);
      else if (e.key === 'ArrowRight' || e.key === 'Enter') handleNext();
      else if (e.key === 'ArrowLeft') handlePrev();
    }
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [isActive, handleNext, handlePrev, stop]);

  if (!isActive || !step) return null;

  const position = getTooltipPosition(rect, placement, tooltipHeight);

  return createPortal(
    // pointer-events-none so the tour never blocks the app — the highlighted
    // element stays usable; only the tooltip (and the centered backdrop) catch
    // clicks.
    <div
      className="fixed inset-0 z-[1000] pointer-events-none"
      role="dialog"
      aria-modal="true"
      aria-label="Tutorial de bienvenida"
    >
      {/* Dimmed backdrop for centered steps (no element to interact with) */}
      {!rect && (
        <div 
          className="absolute inset-0 bg-black/70 pointer-events-auto" 
          onClick={() => stop(false)}
          onKeyDown={(e) => {
            if (e.key === 'Enter' || e.key === ' ') stop(false);
          }}
          role="button"
          tabIndex={0}
          aria-label="Cerrar tutorial"
        />
      )}

      {/* Spotlight around the highlighted element */}
      {rect && (
        <motion.div
          aria-hidden
          className="absolute rounded-xl ring-2 ring-amber-400/60 pointer-events-none"
          style={{ boxShadow: '0 0 0 9999px rgba(0,0,0,0.7)' }}
          initial={false}
          animate={{
            top: rect.top - SPOTLIGHT_PADDING,
            left: rect.left - SPOTLIGHT_PADDING,
            width: rect.width + SPOTLIGHT_PADDING * 2,
            height: rect.height + SPOTLIGHT_PADDING * 2,
          }}
          transition={{ type: 'spring', stiffness: 320, damping: 32 }}
        />
      )}

      {/* Tooltip card */}
      <AnimatePresence mode="wait">
        <motion.div
          key={step.id}
          ref={tooltipRef}
          initial={{ opacity: 0, y: 8 }}
          animate={{ opacity: 1, y: 0 }}
          exit={{ opacity: 0, y: -8 }}
          transition={{ duration: 0.18 }}
          className="absolute rounded-2xl border border-neutral-700 bg-neutral-900 shadow-2xl p-5 pointer-events-auto"
          style={{ top: position.top, left: position.left, width: TOOLTIP_WIDTH }}
        >
          <div className="flex items-start justify-between gap-3 mb-2">
            <span className="text-[11px] font-semibold text-amber-400 uppercase tracking-widest">
              Paso {stepIndex + 1} de {TOUR_STEPS.length}
            </span>
            <button
              onClick={() => stop(false)}
              aria-label="Saltar tutorial"
              className="p-1 -m-1 text-neutral-500 hover:text-neutral-300 transition-colors"
            >
              <X size={15} />
            </button>
          </div>

          <h3 className="text-base font-semibold text-neutral-100 mb-1.5">{step.title}</h3>
          <p className="text-sm text-neutral-400 leading-relaxed">{step.body}</p>

          {/* Progress dots */}
          <div className="flex items-center gap-1.5 mt-4 mb-4" aria-hidden>
            {TOUR_STEPS.map((s, i) => (
              <span
                key={s.id}
                className={`h-1.5 rounded-full transition-all ${
                  i === stepIndex ? 'w-5 bg-amber-400' : 'w-1.5 bg-neutral-700'
                }`}
              />
            ))}
          </div>

          <div className="flex items-center justify-between gap-2">
            <button
              onClick={() => stop(false)}
              className="text-xs text-neutral-500 hover:text-neutral-300 transition-colors"
            >
              Saltar
            </button>
            <div className="flex items-center gap-2">
              {!isFirst && (
                <button
                  onClick={handlePrev}
                  aria-label="Paso anterior"
                  className="flex items-center gap-1 px-3 py-1.5 text-sm text-neutral-300 border border-neutral-700 rounded-lg hover:border-neutral-600 transition-colors"
                >
                  <ArrowLeft size={14} />
                  Atrás
                </button>
              )}
              <button
                onClick={handleNext}
                className="flex items-center gap-1.5 px-3.5 py-1.5 text-sm bg-amber-500 text-neutral-950 rounded-lg font-semibold hover:bg-amber-400 transition-colors"
              >
                {isLast ? (
                  <>
                    <Check size={14} />
                    Empezar
                  </>
                ) : (
                  <>
                    Siguiente
                    <ArrowRight size={14} />
                  </>
                )}
              </button>
            </div>
          </div>
        </motion.div>
      </AnimatePresence>
    </div>,
    document.body,
  );
}
