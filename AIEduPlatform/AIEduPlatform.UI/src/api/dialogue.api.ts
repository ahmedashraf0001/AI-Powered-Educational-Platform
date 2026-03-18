import client from './client';
import type { ApiResponse, VoiceDto, UserVoiceSettingsDto } from '@/types';

export const dialogueApi = {
  getVoices: () =>
    client.get<ApiResponse<VoiceDto[]>>('/dialogue/voices'),

  getPreviews: (params?: { VoiceId?: string; SampleText?: string; Format?: string }) =>
    client.get('/dialogue/voice-previews', { params }),

  getDefaultConfig: () =>
    client.get('/dialogue/voice-config/default'),

  getFormats: () =>
    client.get('/dialogue/supported-formats'),

  getLanguages: () =>
    client.get('/dialogue/supported-languages'),

  getVoiceSettings: () =>
    client.get<ApiResponse<UserVoiceSettingsDto>>('/dialogue/voice-settings'),

  saveVoiceSettings: (data: Partial<UserVoiceSettingsDto>) =>
    client.put('/dialogue/voice-settings', data),

  deleteVoiceSettings: () =>
    client.delete('/dialogue/voice-settings'),
};
