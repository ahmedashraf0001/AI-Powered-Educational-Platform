import client from './client';
import type { ApiResponse, NotificationListDto } from '@/types';

export const notificationsApi = {
  getAll: (params?: { Page?: number; PageSize?: number; UnreadOnly?: boolean }) =>
    client.get<ApiResponse<NotificationListDto>>('/notifications', { params }),

  getUnreadCount: () =>
    client.get<ApiResponse<{ count: number }>>('/notifications/unread-count'),

  markAsRead: (id: string) =>
    client.put(`/notifications/${id}/read`),

  markAllAsRead: () =>
    client.put('/notifications/read-all'),

  delete: (id: string) =>
    client.delete(`/notifications/${id}`),
};
