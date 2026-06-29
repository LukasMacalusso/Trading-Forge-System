import { useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { useNavigate } from 'react-router-dom';
import { AnimatePresence, motion } from 'framer-motion';
import { Bell, CheckCircle2, X } from 'lucide-react';
import { usePendingOperations } from '@hooks/usePendingOperations';
import { PendingOperationCard } from './PendingOperationCard';

/**
 * Bell + dropdown listing bot actions awaiting manual authorization (FR-14).
 * Lives in the sidebar; the panel is portalled so it isn't clipped by the
 * layout's `overflow-hidden`.
 */
export function NotificationCenter() {
  const { pendingOperations, approve, reject, expire, pendingId } = usePendingOperations();
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const [coords, setCoords] = useState({ top: 0, left: 0 });
  const buttonRef = useRef<HTMLButtonElement>(null);
  const panelRef = useRef<HTMLDivElement>(null);

  const count = pendingOperations.length;

  function toggle() {
    if (!open && buttonRef.current) {
      const r = buttonRef.current.getBoundingClientRect();
      setCoords({ top: r.bottom + 8, left: r.left });
    }
    setOpen((v) => !v);
  }

  // Close on Escape or click outside the panel/button.
  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => e.key === 'Escape' && setOpen(false);
    const onClick = (e: MouseEvent) => {
      const t = e.target as Node;
      if (!panelRef.current?.contains(t) && !buttonRef.current?.contains(t)) setOpen(false);
    };
    window.addEventListener('keydown', onKey);
    window.addEventListener('mousedown', onClick);
    return () => {
      window.removeEventListener('keydown', onKey);
      window.removeEventListener('mousedown', onClick);
    };
  }, [open]);

  return (
    <>
      <button
        ref={buttonRef}
        onClick={toggle}
        aria-label={`Operaciones pendientes${count ? ` (${count})` : ''}`}
        aria-expanded={open}
        className="relative p-1.5 rounded-lg text-neutral-400 hover:text-neutral-100 hover:bg-neutral-800 transition-colors"
      >
        <Bell size={18} />
        {count > 0 && (
          <span className="absolute -top-0.5 -right-0.5 min-w-4 h-4 px-1 flex items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white">
            {count > 9 ? '9+' : count}
          </span>
        )}
      </button>

      {createPortal(
        <AnimatePresence>
          {open && (
            <motion.div
              ref={panelRef}
              initial={{ opacity: 0, y: -8, scale: 0.98 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              exit={{ opacity: 0, y: -8, scale: 0.98 }}
              transition={{ duration: 0.15, ease: 'easeOut' }}
              style={{ top: coords.top, left: coords.left }}
              className="fixed z-50 w-80 max-h-[70vh] flex flex-col bg-neutral-900 border border-neutral-800 rounded-xl shadow-2xl shadow-black/40 overflow-hidden"
              role="dialog"
              aria-label="Operaciones pendientes"
            >
              <div className="flex items-center justify-between px-4 py-3 border-b border-neutral-800 shrink-0">
                <div className="flex items-center gap-2">
                  <h3 className="text-sm font-semibold text-neutral-100">Operaciones pendientes</h3>
                  {count > 0 && (
                    <span className="text-xs font-semibold text-amber-400 bg-amber-500/10 rounded-full px-1.5 py-0.5">
                      {count}
                    </span>
                  )}
                </div>
                <button
                  onClick={() => setOpen(false)}
                  aria-label="Cerrar"
                  className="text-neutral-500 hover:text-neutral-200"
                >
                  <X size={15} />
                </button>
              </div>

              <div className="flex-1 overflow-y-auto scrollbar-thin p-3 flex flex-col gap-3">
                {count === 0 ? (
                  <div className="flex flex-col items-center justify-center text-center py-10 gap-2">
                    <CheckCircle2 size={28} className="text-neutral-700" />
                    <p className="text-sm text-neutral-500">No hay operaciones pendientes</p>
                    <p className="text-xs text-neutral-600">
                      Tus bots te avisarán aquí cuando requieran tu autorización.
                    </p>
                  </div>
                ) : (
                  pendingOperations.map((op) => (
                    <PendingOperationCard
                      key={op.id}
                      operation={op}
                      isBusy={pendingId === op.id}
                      onApprove={approve}
                      onReject={reject}
                      onExpire={expire}
                    />
                  ))
                )}
              </div>

              {count > 0 && (
                <button
                  onClick={() => {
                    setOpen(false);
                    navigate('/pending');
                  }}
                  className="shrink-0 px-4 py-2.5 text-xs font-medium text-amber-400 hover:bg-neutral-800 border-t border-neutral-800 transition-colors"
                >
                  Ver todas
                </button>
              )}
            </motion.div>
          )}
        </AnimatePresence>,
        document.body,
      )}
    </>
  );
}
