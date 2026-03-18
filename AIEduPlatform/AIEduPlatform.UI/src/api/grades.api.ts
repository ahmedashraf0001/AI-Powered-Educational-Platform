import client from './client';
import type { ApiResponse, GradeDto, ExamGradeStats } from '@/types';

export const gradesApi = {
  gradeManual: (submissionId: string, data: { feedback: string; questionGrades: Record<string, number> }) =>
    client.post(`/exams/submissions/${submissionId}/grade`, data),

  gradeAI: (submissionId: string) =>
    client.post(`/exams/submissions/${submissionId}/grade-ai`, {}),

  approve: (gradeId: string) =>
    client.post(`/exams/grades/${gradeId}/approve`, {}),

  update: (gradeId: string, data: { score: number; feedback: string }) =>
    client.put(`/exams/grades/${gradeId}`, data),

  getBySubmission: (submissionId: string) =>
    client.get<ApiResponse<GradeDto>>(`/exams/submissions/${submissionId}/grade`),

  getByExam: (examId: string) =>
    client.get(`/exams/${examId}/grades`),

  getPendingApproval: (examId?: string) =>
    client.get('/exams/grades/pending-approval', {
      params: examId ? { ExamId: examId } : undefined,
    }),

  getMyGrades: () =>
    client.get('/exams/grades/student'),

  getExamStats: (examId: string) =>
    client.get<ApiResponse<ExamGradeStats>>(`/grades/stats/exam/${examId}`),

  getStudentStats: (studentId: string, courseId?: string) =>
    client.get(`/grades/stats/student/${studentId}`, {
      params: courseId ? { CourseId: courseId } : undefined,
    }),

  getDistribution: (examId: string) =>
    client.get<ApiResponse<Record<string, number>>>(`/grades/distribution/${examId}`),
};
