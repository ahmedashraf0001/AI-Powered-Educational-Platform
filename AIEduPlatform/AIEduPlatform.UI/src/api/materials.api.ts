import client from './client';
import type { ApiResponse, MaterialDto, MaterialProjectionDto } from '@/types';
import { useAuthStore } from '@/stores/authStore';

const API_URL = import.meta.env.VITE_API_URL || '/api';

export const materialsApi = {
  upload: (lectureId: string, data: FormData, titles?: string) =>
    client.post<ApiResponse<{ materialIds: string[] }>>(
      `/courses/lectures/${lectureId}/materials${titles ? `?Titles=${encodeURIComponent(titles)}` : ''}`,
      data,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    ),

  getLectureMaterials: (lectureId: string) =>
    client.get<ApiResponse<MaterialDto[]>>(`/courses/lectures/${lectureId}/materials`),

  getProjection: (materialId: string) =>
    client.get<ApiResponse<MaterialProjectionDto>>(`/materials/${materialId}/projection`),

  updateProgress: (materialId: string, position: number) =>
    client.post(`/materials/${materialId}/progress`, { position }),

  delete: (materialId: string) =>
    client.delete(`/courses/materials/${materialId}`),

  // Streaming — returns a Blob URL for use in <video>, <audio>, <img>, <iframe>
  getStreamUrl: async (materialId: string): Promise<string> => {
    const token = useAuthStore.getState().accessToken;
    const response = await fetch(`${API_URL}/materials/${materialId}/stream`, {
      headers: token ? { Authorization: `Bearer ${token}` } : {},
    });
    // Preserve the content-type from the response so blob URLs render correctly
    const contentType = response.headers.get('content-type') || 'application/octet-stream';
    const buffer = await response.arrayBuffer();
    const blob = new Blob([buffer], { type: contentType });
    return URL.createObjectURL(blob);
  },

  getDownloadUrl: (materialId: string) =>
    `${API_URL}/materials/${materialId}/download`,
};
