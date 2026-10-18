import { useEffect, useId, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { AnimatePresence, motion } from 'framer-motion';
import type { ReactNode } from 'react';
import { Button } from './Button';

interface ConfirmDialogProps {
  isOpen: boolean;
  title: string;
  description: ReactNode;
  /** Visual style of the confirm button. */
  variant?: 'primary' | 'danger';
  confirmLabel?: string;
  cancelLabel?: string;
  isLoading?: boolean;
  /** Optional icon shown above the title (e.g. a lucide icon node). */
  icon?: ReactNode;
  /**
   * When set, the confirm button stays disabled until the user types this exact
   * word — a guard for irreversible actions (e.g. deleting an account).
   */
  requireText?: string;
  onConfirm: () => void;
  onClose: () => void;
}

/**
 * Accessible, animated confirmation modal. Reused for any destructive or
 * irreversible action across the app (reset simulation, delete account, …).
 */
export function ConfirmDialog({
  isOpen,
  title,
  description,
  variant = 'primary',
  confirmLabel = 'Confirmar',
  cancelLabel = 'Cancelar',
  isLoading = false,
  icon,
  requireText,
  onConfirm,
  onClose,
}: ConfirmDialogProps) {
  const titleId = useId();
  const descId = useId();
  const panelRef = useRef<HTMLDivElement>(null);
  const [typed, setTyped] = useState('');

  const matchesRequired = !requireText || typed.trim() === requireText;
  const canConfirm = matchesRequired && !isLoading;

  // Reset the typed guard whenever the dialog opens/closes.
  useEffect(() => {
    if (!isOpen) setTyped('');
  }, [isOpen]);

  // Close on Escape and lock body scroll while open.
  useEffect(() => {
    if (!isOpen) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && !isLoading) onClose();
    };
    window.addEventListener('keydown', onKey);
    const prevOverflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    return () => {
      window.removeEventListener('keydown', onKey);
      document.body.style.overflow = prevOverflow;
    };
  }, [isOpen, isLoading, onClose]);

  // Move focus into the dialog when it opens (skipped when a type-to-confirm
  // input is present, since that field autofocuses itself).
  useEffect(() => {
    if (isOpen && !requireText) panelRef.current?.focus();
  }, [isOpen, requireText]);

  return createPortal(
    <AnimatePresence>
      {isOpen && (
        <motion.div
          className="fixed inset-0 z-50 flex items-center justify-center p-4"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.15 }}
        >
          {/* Backdrop */}
          <div
            className="absolute inset-0 bg-neutral-950/70 backdrop-blur-sm"
            onClick={() => !isLoading && onClose()}
            aria-hidden="true"
          />

          {/* Panel */}
          <motion.div
            ref={panelRef}
            role="dialog"
            aria-modal="true"
            aria-labelledby={titleId}
            aria-describedby={descId}
            tabIndex={-1}
            className="relative w-full max-w-md bg-neutral-900 border border-neutral-800 rounded-2xl shadow-2xl shadow-black/40 p-6 flex flex-col gap-4 focus:outline-none"
            initial={{ opacity: 0, scale: 0.96, y: 8 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.96, y: 8 }}
            transition={{ duration: 0.18, ease: 'easeOut' }}
          >
            {icon && (
              <div
                className={`w-11 h-11 rounded-full flex items-center justify-center ${
                  variant === 'danger'
                    ? 'bg-red-500/10 text-red-400'
                    : 'bg-amber-500/10 text-amber-400'
                }`}
              >
                {icon}
              </div>
            )}

            <div className="flex flex-col gap-1.5">
              <h2 id={titleId} className="text-lg font-semibold text-neutral-100">
                {title}
              </h2>
              <div id={descId} className="text-sm text-neutral-400 leading-relaxed">
                {description}
              </div>
            </div>

            {requireText && (
              <div className="flex flex-col gap-1.5">
                <label className="text-xs text-neutral-500">
                  Escribe{' '}
                  <span className="font-mono font-semibold text-neutral-300">{requireText}</span>{' '}
                  para confirmar
                </label>
                <input
                  autoFocus
                  value={typed}
                  onChange={(e) => setTyped(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' && canConfirm) onConfirm();
                  }}
                  placeholder={requireText}
                  className="w-full bg-neutral-950 border border-neutral-700 text-neutral-100 rounded-lg px-3 py-2.5 text-sm placeholder-neutral-600 focus:outline-none focus:border-red-500 transition-colors"
                />
              </div>
            )}

            <div className="flex items-center justify-end gap-2 mt-1">
              <Button variant="ghost" onClick={onClose} disabled={isLoading}>
                {cancelLabel}
              </Button>
              <Button
                variant={variant === 'danger' ? 'danger' : 'primary'}
                onClick={onConfirm}
                isLoading={isLoading}
                disabled={!canConfirm}
              >
                {confirmLabel}
              </Button>
            </div>
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>,
    document.body,
  );
}
