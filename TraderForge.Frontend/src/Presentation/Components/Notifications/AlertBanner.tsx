import { useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { CheckCircle, XCircle, AlertTriangle, Info, X } from 'lucide-react';
import { useNotificationStore, type NotificationType } from '../../../Application/Store/notificationStore';

const icons: Record<NotificationType, React.ReactNode> = {
  success: <CheckCircle size={16} className="text-emerald-400" />,
  error:   <XCircle size={16} className="text-red-400" />,
  warning: <AlertTriangle size={16} className="text-amber-400" />,
  info:    <Info size={16} className="text-blue-400" />,
};

const bgClasses: Record<NotificationType, string> = {
  success: 'border-emerald-500/30 bg-emerald-500/10',
  error:   'border-red-500/30 bg-red-500/10',
  warning: 'border-amber-500/30 bg-amber-500/10',
  info:    'border-blue-500/30 bg-blue-500/10',
};

export function AlertBanner() {
  const { notifications, removeNotification } = useNotificationStore();

  useEffect(() => {
    if (notifications.length === 0) return;
    const latest = notifications[notifications.length - 1];
    const timer = setTimeout(() => removeNotification(latest.id), 4000);
    return () => clearTimeout(timer);
  }, [notifications]);

  return (
    <div className="fixed bottom-4 right-4 z-50 flex flex-col gap-2 w-80">
      <AnimatePresence>
        {notifications.map((n) => (
          <motion.div
            key={n.id}
            initial={{ opacity: 0, x: 40 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: 40 }}
            className={`flex items-start gap-3 p-3 rounded-lg border ${bgClasses[n.type]}`}
          >
            <span className="mt-0.5 shrink-0">{icons[n.type]}</span>
            <p className="text-sm text-neutral-200 flex-1">{n.message}</p>
            <button onClick={() => removeNotification(n.id)} className="text-neutral-500 hover:text-neutral-300 shrink-0">
              <X size={14} />
            </button>
          </motion.div>
        ))}
      </AnimatePresence>
    </div>
  );
}
