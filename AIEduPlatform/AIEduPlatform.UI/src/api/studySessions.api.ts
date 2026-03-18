import client from './client';
import type {
  ApiResponse,
  PagedResult,
  SessionSummaryDto,
  SessionDetailDto,
  ChatMessageDto,
  FlashcardDto,
  GeneratedQuizDto,
  QuizResultDto,
  MindMapDto,
  SummaryDto,
  DialogueAudioResponseDto,
  PaginationParams,
} from '@/types';
import { useAuthStore } from '@/stores/authStore';

const API_URL = import.meta.env.VITE_API_URL || '/api';

export const studySessionsApi = {
  start: (courseId: string) =>
    client.post<ApiResponse<{ sessionId: string }>>('/study-sessions', { courseId }),

  end: (sessionId: string) =>
    client.post(`/study-sessions/${sessionId}/end`, {}),

  getById: (sessionId: string) =>
    client.get<ApiResponse<SessionDetailDto>>(`/study-sessions/${sessionId}`),

  getMine: (params?: PaginationParams & { CourseId?: string }) =>
    client.get<ApiResponse<PagedResult<SessionSummaryDto>>>('/study-sessions', { params }),

  // SSE Chat — uses fetch, not axios
  sendChat: async (
    sessionId: string,
    message: string,
    lectureIds?: string[],
    materialIds?: string[],
    onChunk: (content: string, done: boolean, sources?: string[]) => void = () => {}
  ) => {
    const token = useAuthStore.getState().accessToken;
    const response = await fetch(`${API_URL}/study-sessions/${sessionId}/chat`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Accept: 'text/event-stream',
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
      body: JSON.stringify({ message, lectureIds, materialIds }),
    });

    if (!response.ok) throw new Error(`Chat request failed: ${response.status}`);

    const reader = response.body?.getReader();
    if (!reader) throw new Error('No response body');

    const decoder = new TextDecoder();
    let buffer = '';

    while (true) {
      const { done, value } = await reader.read();
      if (done) break;

      buffer += decoder.decode(value, { stream: true });
      const lines = buffer.split('\n');
      buffer = lines.pop() || '';

      for (const line of lines) {
        if (line.startsWith('data: ')) {
          const data = line.slice(6).trim();
          if (data === '[DONE]') {
            onChunk('', true);
            return;
          }
          try {
            const parsed = JSON.parse(data);
            onChunk(parsed.content || '', parsed.done || false, parsed.sources);
            if (parsed.done) return;
          } catch {
            // skip unparseable lines
          }
        }
      }
    }
  },

  getChatHistory: (sessionId: string, params?: PaginationParams) =>
    client.get<ApiResponse<PagedResult<ChatMessageDto>>>(
      `/study-sessions/${sessionId}/chat`,
      { params }
    ),

  generateSummary: (sessionId: string, data: {
    topic: string;
    summaryLength?: number;
    includeKeyPoints?: boolean;
    lectureIds?: string[];
    materialIds?: string[];
  }) => client.post<ApiResponse<SummaryDto>>(`/study-sessions/${sessionId}/summary`, data),

  generateFlashcards: (sessionId: string, data: {
    topic: string;
    numberOfCards?: number;
    lectureIds?: string[];
    materialIds?: string[];
  }) => client.post<ApiResponse<FlashcardDto[]>>(`/study-sessions/${sessionId}/flashcards`, data),

  getFlashcards: (sessionId: string) =>
    client.get<ApiResponse<PagedResult<FlashcardDto>>>(`/study-sessions/${sessionId}/flashcards`),

  generateQuiz: (sessionId: string, data: {
    topic: string;
    numberOfQuestions?: number;
    difficulty?: string;
    questionTypes?: string[];
    lectureIds?: string[];
    materialIds?: string[];
  }) => client.post<ApiResponse<GeneratedQuizDto>>(`/study-sessions/${sessionId}/quizzes`, data),

  submitQuiz: (sessionId: string, quizId: string, answers: Record<string, string>) =>
    client.post<ApiResponse<QuizResultDto>>(
      `/study-sessions/${sessionId}/quizzes/${quizId}/submit`,
      { answers }
    ),

  getQuizzes: (sessionId: string) =>
    client.get<ApiResponse<PagedResult<GeneratedQuizDto>>>(`/study-sessions/${sessionId}/quizzes`),

  generateMindMap: (sessionId: string, data: {
    centralTopic: string;
    maxDepth?: number;
    lectureIds?: string[];
    materialIds?: string[];
  }) => client.post<ApiResponse<MindMapDto>>(`/study-sessions/${sessionId}/mindmaps`, data),

  getMindMaps: (sessionId: string) =>
    client.get<ApiResponse<PagedResult<MindMapDto>>>(`/study-sessions/${sessionId}/mindmaps`),

  generateDialogueAudio: (sessionId: string, data: {
    topic?: string;
    numberOfExchanges?: number;
    focusConcepts?: string[];
    lectureIds?: string[];
    materialIds?: string[];
    teacherVoiceId?: string;
    studentVoiceId?: string;
  }) => client.post<ApiResponse<DialogueAudioResponseDto>>(
    `/study-sessions/${sessionId}/dialogue-audio`,
    data
  ),
};
