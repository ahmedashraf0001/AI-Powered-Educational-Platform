import client from './client';
import type {
  ApiResponse,
  UserProfile,
  UserStats,
  TeacherDashboard,
  StudentDashboard,
} from '@/types';

export const usersApi = {
  getMe: () => client.get<ApiResponse<UserProfile>>('/users/me'),

  updateMe: (data: FormData) =>
    client.put<ApiResponse<UserProfile>>('/users/me', data, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }),

  getById: (userId: string) =>
    client.get<ApiResponse<UserProfile>>(`/users/${userId}`),

  getStats: (userId?: string) =>
    client.get<ApiResponse<UserStats>>('/users/stats', {
      params: userId ? { UserId: userId } : undefined,
    }),

  getStudentDashboard: () =>
    client.get<ApiResponse<StudentDashboard>>('/users/dashboard'),

  getTeacherDashboard: () =>
    client.get<ApiResponse<TeacherDashboard>>('/users/teacher/dashboard'),
};
