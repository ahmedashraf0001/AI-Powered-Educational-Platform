import client from './client';
import type {
  ApiResponse,
  PagedResult,
  ReviewDto,
  CourseRatingSummaryDto,
  PaginationParams,
} from '@/types';

export const reviewsApi = {
  add: (courseId: string, data: { rating: number; comment?: string }) =>
    client.post(`/courses/${courseId}/reviews`, data),

  getByCourse: (courseId: string, params?: PaginationParams) =>
    client.get<ApiResponse<PagedResult<ReviewDto>>>(`/courses/${courseId}/reviews`, { params }),

  getRating: (courseId: string) =>
    client.get<ApiResponse<CourseRatingSummaryDto>>(`/courses/${courseId}/rating`),

  update: (reviewId: string, data: { rating: number; comment?: string }) =>
    client.put(`/reviews/${reviewId}`, data),

  delete: (reviewId: string) => client.delete(`/reviews/${reviewId}`),
};
