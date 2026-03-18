import client from './client';
import type { ApiResponse, CartDto } from '@/types';

export const cartApi = {
  get: () => client.get<ApiResponse<CartDto>>('/cart'),

  addItem: (courseId: string) =>
    client.post<ApiResponse<CartDto>>('/cart/items', { courseId }),

  removeItem: (courseId: string) =>
    client.delete<ApiResponse<CartDto>>(`/cart/items/${courseId}`),

  clear: () => client.delete('/cart'),
};
