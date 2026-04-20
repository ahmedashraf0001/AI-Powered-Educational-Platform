import { Outlet, useLocation } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useEffect } from 'react';
import { Navbar } from './Navbar';
import { useAuthStore } from '@/stores/authStore';
import { useNotificationStore } from '@/stores/notificationStore';
import { useSignalR } from '@/hooks/useSignalR';
import { coursesApi } from '@/api/courses.api';
import { notificationsApi } from '@/api/notifications.api';

export function AppLayout() {
  const { isAuthenticated } = useAuthStore();
  const location = useLocation();

  // Fetch enrolled course IDs for SignalR course groups
  const { data: enrolledCourseIds } = useQuery({
    queryKey: ['enrolled-course-ids'],
    queryFn: () => coursesApi.getEnrolled({ page: 1, pageSize: 100 }),
    select: (res) => (res.data.data?.items ?? []).map((e: any) => e.courseId),
    enabled: isAuthenticated,
    staleTime: 60_000,
  });

  // Fetch unread notification count from server and sync to Zustand store
  const { data: unreadData } = useQuery({
    queryKey: ['unread-notification-count'],
    queryFn: () => notificationsApi.getUnreadCount(),
    enabled: isAuthenticated,
    staleTime: 30_000,
    refetchInterval: 60_000,
  });

  useEffect(() => {
    if (unreadData?.data?.data != null) {
      const raw = unreadData.data.data;
      const count = typeof raw === 'number' ? raw : (raw as any).count ?? 0;
      useNotificationStore.getState().setUnreadCount(count);
    }
  }, [unreadData]);

  // Connect to SignalR hubs
  useSignalR(enrolledCourseIds ?? []);

  const publicPaths = ['/', '/login', '/register', '/verify-email'];
  const isPublic = publicPaths.includes(location.pathname);
  const showAppShell = isAuthenticated || !isPublic;

  return (
    <div className="min-h-screen bg-background dark:bg-[radial-gradient(ellipse_at_top,rgba(59,130,246,0.03),transparent_50%)]">
      <Navbar />
      <main className={showAppShell ? 'w-full transition-all duration-300' : 'transition-all duration-300'}>
        <Outlet />
      </main>
    </div>
  );
}
