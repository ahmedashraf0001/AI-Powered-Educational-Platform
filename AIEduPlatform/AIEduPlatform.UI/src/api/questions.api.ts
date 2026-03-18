import client from './client';
import type { ApiResponse, QuestionDto, QuestionType } from '@/types';

export const questionsApi = {
  add: (examId: string, data: {
    type: QuestionType;
    text: string;
    options?: string[];
    correctAnswer: string;
    points: number;
  }) => client.post(`/exams/${examId}/questions`, data),

  addBulk: (examId: string, questions: Array<{
    type: QuestionType;
    text: string;
    options?: string[];
    correctAnswer: string;
    points: number;
  }>) => client.post(`/exams/${examId}/questions/bulk`, { questions }),

  generateAI: (examId: string, data: {
    numberOfQuestions: number;
    difficulty?: string;
    questionTypes?: QuestionType[];
    focusTopics?: string[];
    lectureIds?: string[];
    materialIds?: string[];
  }) => client.post(`/exams/${examId}/questions/generate-ai`, data),

  getByExam: (examId: string) =>
    client.get<ApiResponse<QuestionDto[]>>(`/exams/${examId}/questions`),

  update: (questionId: string, data: {
    type: QuestionType;
    text: string;
    options?: string[];
    correctAnswer: string;
    points: number;
  }) => client.put(`/exams/questions/${questionId}`, data),

  reorder: (examId: string, questionOrders: Record<string, number>) =>
    client.post(`/exams/${examId}/questions/reorder`, { questionOrders }),

  delete: (questionId: string) =>
    client.delete(`/exams/questions/${questionId}`),
};
