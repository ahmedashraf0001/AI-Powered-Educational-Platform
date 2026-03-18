import client from './client';
import type { ApiResponse, AiProviderStatus } from '@/types';

export const aiProviderApi = {
  getStatus: () =>
    client.get<ApiResponse<AiProviderStatus>>('/ai/provider'),

  switch: (provider: string) =>
    client.post<ApiResponse<{ previousProvider: string; activeProvider: string; message: string }>>(
      '/ai/provider/switch',
      { provider }
    ),
};
