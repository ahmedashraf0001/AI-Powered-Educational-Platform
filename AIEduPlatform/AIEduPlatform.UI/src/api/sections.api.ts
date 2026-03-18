import client from './client';
import type { ApiResponse, SemanticSectionDto, SummaryDto, FlashcardDto, GeneratedQuizDto } from '@/types';

export const sectionsApi = {
  getByMaterial: (materialId: string) =>
    client.get<ApiResponse<SemanticSectionDto[]>>(`/materials/${materialId}/sections`),

  summarize: (sessionId: string, sectionId: string, data?: {
    summaryLength?: number;
    includeKeyPoints?: boolean;
  }) => client.post<ApiResponse<SummaryDto>>(
    `/study-sessions/${sessionId}/sections/${sectionId}/summarize`,
    data
  ),

  generateFlashcards: (sessionId: string, sectionId: string, data?: {
    numberOfCards?: number;
  }) => client.post<ApiResponse<FlashcardDto[]>>(
    `/study-sessions/${sessionId}/sections/${sectionId}/flashcards`,
    data
  ),

  generateQuiz: (sessionId: string, sectionId: string, data?: {
    numberOfQuestions?: number;
    difficulty?: string;
    questionTypes?: string[];
  }) => client.post<ApiResponse<GeneratedQuizDto>>(
    `/study-sessions/${sessionId}/sections/${sectionId}/quiz`,
    data
  ),
};
