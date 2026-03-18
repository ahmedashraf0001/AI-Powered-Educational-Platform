import client from './client';
import type { ApiResponse, PagedResult, SubmissionDto, SubmissionDetailDto, PaginationParams } from '@/types';

export const submissionsApi = {
  getById: (submissionId: string) =>
    client.get<ApiResponse<SubmissionDetailDto>>(`/exams/submissions/${submissionId}`),

  getByExam: (examId: string, params?: PaginationParams) =>
    client.get<ApiResponse<PagedResult<SubmissionDto>>>(`/exams/${examId}/submissions`, { params }),

  getMine: (params?: PaginationParams) =>
    client.get<ApiResponse<PagedResult<SubmissionDto>>>('/exams/submissions/student', { params }),

  getUngraded: (examId?: string) =>
    client.get<ApiResponse<PagedResult<SubmissionDto>>>('/exams/submissions/ungraded', {
      params: examId ? { ExamId: examId } : undefined,
    }),

  getStats: (examId: string) =>
    client.get(`/submissions/stats/${examId}`),
};
