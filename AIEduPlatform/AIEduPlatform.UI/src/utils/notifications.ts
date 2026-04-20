import { toast } from 'sonner';

export type NotificationType = 'success' | 'error' | 'warning' | 'info';

export interface ShowNotificationOptions {
  type: NotificationType;
  message: string;
  persistent?: boolean;
}

export function showNotification({ type, message, persistent = false }: ShowNotificationOptions) {
  const options = persistent ? { duration: Infinity } : undefined;

  if (type === 'success') {
    toast.success(message, options);
    return;
  }

  if (type === 'warning') {
    toast.warning(message, options);
    return;
  }

  if (type === 'info') {
    toast.info(message, options);
    return;
  }

  toast.error(message, options);
}
