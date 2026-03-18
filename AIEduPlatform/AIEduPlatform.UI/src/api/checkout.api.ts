import client from './client';
import type { ApiResponse, CheckoutResponseDto, OrderStatusDto } from '@/types';

export const checkoutApi = {
  create: () =>
    client.post<ApiResponse<CheckoutResponseDto>>('/checkout', {}),

  getOrderStatus: (orderId: string) =>
    client.get<ApiResponse<OrderStatusDto>>(`/checkout/${orderId}`),
};
