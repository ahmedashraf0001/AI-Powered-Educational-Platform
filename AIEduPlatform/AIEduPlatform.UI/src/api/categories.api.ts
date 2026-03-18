import client from './client';
import type { ApiResponse, CategoryDto } from '@/types';

export const categoriesApi = {
  getAll: (searchTerm?: string) =>
    client.get<ApiResponse<CategoryDto[]>>('/categories', {
      params: searchTerm ? { SearchTerm: searchTerm } : undefined,
    }),

  getById: (categoryId: string) =>
    client.get<ApiResponse<CategoryDto>>(`/categories/${categoryId}`),

  create: (data: { name: string; description?: string }) =>
    client.post<ApiResponse<string>>('/categories', data),

  update: (categoryId: string, data: { name: string; description?: string }) =>
    client.put(`/categories/${categoryId}`, data),

  delete: (categoryId: string) => client.delete(`/categories/${categoryId}`),

  addCourseToCategory: (courseId: string, categoryId: string) =>
    client.post('/courses/categories', { courseId, categoryId }),

  removeCourseFromCategory: (courseId: string, categoryId: string) =>
    client.delete(`/courses/${courseId}/categories/${categoryId}`),
};
