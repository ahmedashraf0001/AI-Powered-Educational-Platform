import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { notificationsApi } from '@/api/notifications.api';
import { useNotificationStore } from '@/stores/notificationStore';
import type { NotificationDto } from '@/types';
import { Card, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Pagination } from '@/components/ui/Pagination';
import { PageSpinner } from '@/components/ui/Spinner';
import { EmptyState } from '@/components/ui/Feedback';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { useState } from 'react';
import { toast } from 'sonner';
import { Bell, CheckCheck, Trash2 } from 'lucide-react';
import { formatDate } from '@/utils/formatters';

export default function NotificationsPage() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ['notifications', page],
    queryFn: () => notificationsApi.getAll({ Page: page, PageSize: 20 }),
    select: (res) => res.data.data,
  });

  const invalidateAndSyncCount = () => {
    queryClient.invalidateQueries({ queryKey: ['notifications'] });
    queryClient.invalidateQueries({ queryKey: ['unread-notification-count'] });
  };

  const markReadMutation = useMutation({
    mutationFn: (id: string) => notificationsApi.markAsRead(id),
    onSuccess: (_data, id) => {
      useNotificationStore.getState().markAsRead(id);
      invalidateAndSyncCount();
    },
  });

  const markAllReadMutation = useMutation({
    mutationFn: () => notificationsApi.markAllAsRead(),
    onSuccess: () => {
      useNotificationStore.getState().markAllAsRead();
      invalidateAndSyncCount();
      toast.success('All notifications marked as read');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (notification: NotificationDto) => notificationsApi.delete(notification.id),
    onSuccess: (_data, notification) => {
      if (!notification.isRead) {
        useNotificationStore.getState().markAsRead(notification.id);
      }
      useNotificationStore.getState().removeNotification(notification.id);
      invalidateAndSyncCount();
      toast.success('Notification deleted');
    },
  });

  if (isLoading) return <PageSpinner />;

  const notifications = data?.items || [];

  return (
    <AnimatedPage>
    <div className="max-w-3xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-3xl font-bold flex items-center gap-2">
          <Bell className="h-8 w-8" /> Notifications
        </h1>
        <Button variant="outline" onClick={() => markAllReadMutation.mutate()}>
          <CheckCheck className="h-4 w-4 mr-2" /> Mark All Read
        </Button>
      </div>

      {notifications.length === 0 ? (
        <EmptyState title="No notifications yet" />
      ) : (
        <div className="space-y-3">
          {notifications.map((n: NotificationDto) => (
            <Card key={n.id} className={!n.isRead ? 'border-l-4 border-l-primary' : ''}>
              <CardContent className="p-4 flex items-start justify-between gap-4">
                <div
                  className={`flex-1 cursor-pointer ${!n.isRead ? 'font-medium' : 'text-muted-foreground'}`}
                  onClick={() => !n.isRead && markReadMutation.mutate(n.id)}
                >
                  <div className="flex items-center gap-2">
                    <h3 className="text-sm font-semibold">{n.title}</h3>
                    {!n.isRead && <Badge variant="default" className="text-xs">New</Badge>}
                  </div>
                  <p className="text-sm mt-1">{n.message}</p>
                  <p className="text-xs text-muted-foreground mt-1">{formatDate(n.createdAt)}</p>
                </div>
                <Button variant="ghost" size="sm" onClick={() => deleteMutation.mutate(n)}>
                  <Trash2 className="h-4 w-4" />
                </Button>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {data && data.totalPages > 1 && (
        <div className="mt-6">
          <Pagination
            page={page}
            totalPages={data.totalPages}
            onPageChange={setPage}
            hasPrevious={data.hasPrevious}
            hasNext={data.hasNext}
          />
        </div>
      )}
    </div>
    </AnimatedPage>
  );
}
