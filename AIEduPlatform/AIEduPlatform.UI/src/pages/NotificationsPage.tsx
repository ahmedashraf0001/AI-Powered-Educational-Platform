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
import { useNavigate } from 'react-router-dom';
import { Bell, CheckCheck, Trash2, MessageSquare, AlertCircle, CheckCircle, Info, Calendar } from 'lucide-react';
import { formatRelative } from '@/utils/formatters';
import { getNotificationNavigationPath } from '@/utils/notificationNavigation';

const getNotificationIcon = (type: string, sizeClasses = "h-5 w-5") => {
  switch (type?.toLowerCase()) {
    case 'message':
    case 'reply':
    case 'discussion':
      return <MessageSquare className={`${sizeClasses} text-blue-500`} />;
    case 'alert':
    case 'warning':
    case 'important':
      return <AlertCircle className={`${sizeClasses} text-destructive`} />;
    case 'success':
    case 'grade':
    case 'completed':
    case 'submissiongraded':
    case 'gradeapproved':
    case 'gradeupdated':
    case 'paymentsuccess':
    case 'checkoutsuccess':
      return <CheckCircle className={`${sizeClasses} text-green-500`} />;
    case 'event':
    case 'exam':
    case 'deadline':
    case 'newexamposted':
    case 'newmaterialuploaded':
    case 'newlectureadded':
      return <Calendar className={`${sizeClasses} text-amber-500`} />;
    case 'info':
    case 'system':
      return <Info className={`${sizeClasses} text-blue-400`} />;
    default:
      return <Bell className={`${sizeClasses} text-primary`} />;
  }
};

export default function NotificationsPage() {
  const queryClient = useQueryClient();
  const navigate = useNavigate();
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

  const deleteAllMutation = useMutation({
    mutationFn: () => notificationsApi.deleteAll(),
    onSuccess: () => {
      useNotificationStore.getState().setUnreadCount(0);
      invalidateAndSyncCount();
      toast.success('All notifications cleared');
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

  const unreadCount = notifications.filter((n: NotificationDto) => !n.isRead).length;

  const handleNotificationClick = (notification: NotificationDto) => {
    if (!notification.isRead) {
      markReadMutation.mutate(notification.id);
    }

    navigate(getNotificationNavigationPath(notification));
  };

  return (
    <AnimatedPage>
    <div className="max-w-4xl mx-auto px-4 py-10 md:py-16">
      {/* Header section */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-10">
        <div>
          <h1 className="text-3xl md:text-4xl font-extrabold tracking-tight flex items-center gap-3">
            Notifications
            {unreadCount > 0 && (
              <Badge variant="default" className="text-sm px-3 py-0.5 animate-in fade-in zoom-in rounded-full">
                {unreadCount} New
              </Badge>
            )}
          </h1>
          <p className="text-muted-foreground mt-2 text-lg">Stay updated on your learning journey</p>
        </div>
        
        {notifications.length > 0 && (
          <div className="flex gap-2 w-full md:w-auto">
            {unreadCount > 0 && (
              <Button 
                variant="outline" 
                onClick={() => markAllReadMutation.mutate()}
                className="w-full md:w-auto shadow-sm hover:bg-secondary/80 transition-all font-medium"
              >
                <CheckCheck className="h-4 w-4 mr-2 text-primary" /> Mark all as read
              </Button>
            )}
            <Button 
              variant="destructive" 
              onClick={() => deleteAllMutation.mutate()}
              className="w-full md:w-auto shadow-sm transition-all font-medium"
            >
              <Trash2 className="h-4 w-4 mr-2" /> Clear all
            </Button>
          </div>
        )}
      </div>

      {notifications.length === 0 ? (
        <EmptyState 
          icon={<Bell className="h-10 w-10 text-muted-foreground opacity-50" />}
          title="You're all caught up!" 
          description="We'll notify you when there's something new or important."
        />
      ) : (
        <div className="space-y-4">
          {notifications.map((n: NotificationDto) => (
            <Card 
              key={n.id} 
              className={`overflow-hidden transition-all duration-200 border-none shadow-sm hover:shadow-md cursor-pointer ${
                !n.isRead ? 'bg-primary/[0.03] ring-1 ring-primary/20' : 'bg-card ring-1 ring-border border'
              }`}
              onClick={() => handleNotificationClick(n)}
            >
              <CardContent className="p-0">
                <div className="flex items-stretch">
                  {/* Status Indicator Bar */}
                  <div className={`w-1.5 shrink-0 transition-colors ${!n.isRead ? 'bg-primary' : 'bg-transparent'}`} />
                  
                  <div className="flex-1 p-5 sm:p-6 flex flex-col sm:flex-row gap-4 sm:gap-6 items-start">
                    
                    {/* Icon */}
                    <div className={`shrink-0 h-12 w-12 rounded-full flex items-center justify-center shadow-sm ${
                      !n.isRead ? 'bg-primary/10 ring-4 ring-primary/5' : 'bg-secondary/80'
                    }`}>
                      {getNotificationIcon(n.type, "h-6 w-6")}
                    </div>

                    {/* Content */}
                    <div 
                      className={`flex-1 min-w-0 ${!n.isRead ? 'font-medium' : 'text-muted-foreground'}`}
                    >
                      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-1 mb-1">
                        <h3 className={`text-base tracking-tight truncate ${!n.isRead ? 'font-bold text-foreground' : 'font-semibold text-foreground/80'}`}>
                          {n.title || n.message}
                        </h3>
                        <span className={`text-xs whitespace-nowrap shrink-0 ${!n.isRead ? 'text-primary font-semibold' : 'text-muted-foreground'}`}>
                          {formatRelative(n.createdAt)}
                        </span>
                      </div>
                      
                      {n.title && n.title !== n.message && (
                        <p className={`text-sm mt-1.5 line-clamp-2 ${!n.isRead ? 'text-foreground/90' : 'text-muted-foreground'}`}>
                          {n.message}
                        </p>
                      )}

                      {!n.isRead && (
                        <div className="inline-flex items-center gap-1.5 mt-3 text-xs font-semibold text-primary/80 bg-primary/10 px-2 py-1 rounded-md">
                          <span className="h-1.5 w-1.5 rounded-full bg-primary animate-pulse" />
                          Unread
                        </div>
                      )}
                    </div>

                    {/* Actions */}
                    <div className="shrink-0 flex items-center self-start sm:self-center ml-14 sm:ml-0">
                      <Button 
                        variant="ghost" 
                        size="icon" 
                        className="text-muted-foreground hover:text-destructive hover:bg-destructive/10 -mr-2"
                        onClick={(e) => {
                          e.stopPropagation();
                          deleteMutation.mutate(n);
                        }}
                      >
                        <Trash2 className="h-4 w-4" />
                        <span className="sr-only">Delete notification</span>
                      </Button>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {data && data.totalPages > 1 && (
        <div className="mt-10 flex justify-center">
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
