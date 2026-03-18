import client from './client';
import type {
  ApiResponse,
  PagedResult,
  ExamDto,
  ExamDetailDto,
  PaginationParams,
  ExamAttemptDto,
} from '@/types';

export const examsApi = {
  create: (courseId: string, data: {
    title: string;
    startTime: string;
    endTime: string;
    durationMinutes: number;
  }) => client.post(`/courses/${courseId}/exams`, data),

  getById: (examId: string) =>
    client.get<ApiResponse<ExamDetailDto>>(`/exams/${examId}`),

  getByCourse: (courseId: string, params?: PaginationParams) =>
    client.get<ApiResponse<PagedResult<ExamDto>>>(`/exams/course/${courseId}`, { params }),

  getActive: (courseId: string, params?: PaginationParams) =>
    client.get<ApiResponse<PagedResult<ExamDto>>>(`/exams/active/${courseId}`, { params }),

  getUpcoming: (courseId: string, params?: PaginationParams) =>
    client.get<ApiResponse<PagedResult<ExamDto>>>(`/exams/upcoming/${courseId}`, { params }),

  getPast: (courseId: string, params?: PaginationParams) =>
    client.get<ApiResponse<PagedResult<ExamDto>>>(`/exams/past/${courseId}`, { params }),

  getAvailable: (params?: PaginationParams) =>
    client.get<ApiResponse<PagedResult<ExamDto>>>('/exams/available', { params }),

  getTotalPoints: (examId: string) =>
    client.get<ApiResponse<number>>(`/exams/${examId}/total-points`),

  update: (examId: string, data: {
    title: string;
    startTime: string;
    endTime: string;
    durationMinutes: number;
  }) => client.put(`/exams/${examId}`, data),

  delete: (examId: string) => client.delete(`/exams/${examId}`),

  submit: (examId: string, answers: Record<string, string>) =>
    client.post(`/exams/${examId}/submit`, { answers }),

  // Exam attempt endpoints for timer persistence
  startAttempt: (examId: string) =>
    client.post<ApiResponse<ExamAttemptDto>>(`/exams/${examId}/attempt`, {}),

  saveAnswers: (examId: string, answers: Record<string, string>) =>
    client.put<ApiResponse<boolean>>(`/exams/${examId}/attempt/answers`, { answers }),
};
