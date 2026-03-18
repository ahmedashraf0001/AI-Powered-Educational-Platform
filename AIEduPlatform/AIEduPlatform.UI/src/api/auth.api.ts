import client from './client';
import type {
  ApiResponse,
  AuthTokens,
  LoginRequest,
  RegisterStudentRequest,
  RegisterTeacherRequest,
} from '@/types';

export const authApi = {
  registerStudent: (data: RegisterStudentRequest) =>
    client.post<ApiResponse<null>>('/auth/register/student', data),

  registerTeacher: (data: RegisterTeacherRequest) =>
    client.post<ApiResponse<null>>('/auth/register/teacher', data),

  verifyEmail: (token: string, email: string) =>
    client.get<ApiResponse<null>>('/auth/verify-email', {
      params: { Token: token, Email: email },
    }),

  login: (data: LoginRequest) =>
    client.post<ApiResponse<AuthTokens>>('/auth/login', data),

  refreshToken: (accessToken: string, refreshToken: string) =>
    client.post<ApiResponse<AuthTokens>>('/auth/refresh-token', {
      accessToken,
      refreshToken,
    }),

  logout: (refreshToken: string) =>
    client.post<ApiResponse<null>>('/auth/logout', { refreshToken }),
};
