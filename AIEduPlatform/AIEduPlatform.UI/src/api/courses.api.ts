import client from './client';
import { CourseRemovalReason } from '@/types';
import type {
  ApiResponse,
  PagedResult,
  CourseListDto,
  CourseDetailDto,
  ContinueLearningDto,
  RecommendationSectionsDto,
  CourseProgressDto,
  CourseEngagementReport,
  PaginationParams,
  DeleteCourseResult,
} from '@/types';

export const coursesApi = {
  getAll: (params?: PaginationParams & { CategoryId?: string }) =>
    client.get<ApiResponse<PagedResult<CourseListDto>>>('/courses', { params }),

  search: (keyword: string, params?: PaginationParams & { CategoryId?: string }) =>
    client.get<ApiResponse<PagedResult<CourseListDto>>>('/courses/search', {
      params: { Keyword: keyword, ...params },
    }),

  getById: (courseId: string) =>
    client.get<ApiResponse<CourseDetailDto>>(`/courses/${courseId}`),

  create: (data: FormData) =>
    client.post<ApiResponse<{ courseId: string }>>('/courses', data, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }),

  update: (courseId: string, data: FormData) =>
    client.put(`/courses/${courseId}`, data, {
      headers: { 'Content-Type': 'multipart/form-data' },
    }),

  delete: (
    courseId: string,
    reason: CourseRemovalReason = CourseRemovalReason.InstructorRequest
  ) =>
    client.delete<ApiResponse<DeleteCourseResult>>(`/courses/${courseId}`, {
      params: { reason },
    }),

  publish: (courseId: string) =>
    client.post(`/courses/${courseId}/publish`, {}),

  getMyCourses: (params?: PaginationParams & { IncludeUnpublished?: boolean }) =>
    client.get<ApiResponse<PagedResult<CourseListDto>>>('/courses/my-courses', { params }),

  getByInstructor: (instructorId: string, params?: PaginationParams) =>
    client.get<ApiResponse<PagedResult<CourseListDto>>>(
      `/courses/instructor/${instructorId}`,
      { params }
    ),

  continueLearning: () =>
    client.get<ApiResponse<ContinueLearningDto[]>>('/courses/continue-learning'),

  getRecommended: (top = 10) =>
    client.get<ApiResponse<CourseListDto[]>>('/courses/recommended', {
      params: { Top: top },
    }),

  getRecommendationSections: (top = 10) =>
    client.get<ApiResponse<RecommendationSectionsDto>>('/courses/recommended/sections', {
      params: { Top: top },
    }),

  getProgress: (courseId: string) =>
    client.get<ApiResponse<CourseProgressDto>>(`/courses/${courseId}/progress`),

  getEngagement: (courseId: string) =>
    client.get<ApiResponse<CourseEngagementReport>>(`/courses/${courseId}/engagement`),

  sendEngagementAlerts: (
    courseId: string,
    data: { studentIds?: string[]; customMessage?: string }
  ) => client.post(`/courses/${courseId}/engagement/alerts`, data),

  // Enrollment
  enroll: (courseId: string) =>
    client.post<ApiResponse<{ enrollmentId: string }>>(`/courses/${courseId}/enroll`, {}),

  unenroll: (courseId: string) => client.delete(`/courses/${courseId}/unenroll`),

  complete: (courseId: string) => client.post(`/courses/${courseId}/complete`, {}),

  getEnrolled: (params?: PaginationParams) =>
    client.get<ApiResponse<PagedResult<import('@/types').EnrollmentDto>>>('/courses/enrolled', {
      params,
    }),

  getCourseEnrollments: (courseId: string, params?: PaginationParams) =>
    client.get(`/courses/${courseId}/enrollments`, { params }),
};
