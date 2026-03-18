import client from './client';
import type { ApiResponse, LectureDto, LectureDetailDto } from '@/types';

export const lecturesApi = {
  getCourseLectures: (courseId: string, includeMaterials = true) =>
    client.get<ApiResponse<LectureDto[]>>(`/courses/${courseId}/lectures`, {
      params: { IncludeMaterials: includeMaterials },
    }),

  getById: (lectureId: string) =>
    client.get<ApiResponse<LectureDetailDto>>(`/lectures/${lectureId}`),

  create: (courseId: string, data: { title: string; description: string; orderIndex: number }) =>
    client.post<ApiResponse<{ lectureId: string }>>(`/courses/${courseId}/lectures`, data),

  update: (lectureId: string, data: { title: string; description: string; orderIndex: number }) =>
    client.put(`/courses/lectures/${lectureId}`, data),

  delete: (lectureId: string) => client.delete(`/courses/lectures/${lectureId}`),
};
