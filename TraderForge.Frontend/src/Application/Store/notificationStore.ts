import { create } from 'zustand';
import type { PendingOperation } from '../../Domain/Entities/BotFlow';

export type NotificationType = 'success' | 'error' | 'warning' | 'info';

export interface AppNotification {
  id: string;
  type: NotificationType;
  message: string;
  createdAt: number;
}

interface NotificationState {
  notifications: AppNotification[];
  pendingOperations: PendingOperation[];
  addNotification: (type: NotificationType, message: string) => void;
  removeNotification: (id: string) => void;
  setPendingOperations: (ops: PendingOperation[]) => void;
  removePendingOperation: (id: string) => void;
}

export const useNotificationStore = create<NotificationState>((set) => ({
  notifications: [],
  pendingOperations: [],

  addNotification: (type, message) =>
    set((state) => ({
      notifications: [
        ...state.notifications,
        { id: crypto.randomUUID(), type, message, createdAt: Date.now() },
      ],
    })),

  removeNotification: (id) =>
    set((state) => ({
      notifications: state.notifications.filter((n) => n.id !== id),
    })),

  setPendingOperations: (pendingOperations) => set({ pendingOperations }),

  removePendingOperation: (id) =>
    set((state) => ({
      pendingOperations: state.pendingOperations.filter((op) => op.id !== id),
    })),
}));
