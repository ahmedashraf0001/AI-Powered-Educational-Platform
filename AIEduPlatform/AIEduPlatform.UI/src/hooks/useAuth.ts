import { useAuthStore } from '@/stores/authStore';
import { authApi } from '@/api/auth.api';
import { usersApi } from '@/api/users.api';
import type { LoginRequest, RegisterStudentRequest, RegisterTeacherRequest } from '@/types';
import { useMutation, useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';

export function useAuth() {
  const { isAuthenticated, roles, setTokens, setUser, logout: clearAuth, accessToken, user } = useAuthStore();
  const navigate = useNavigate();

  const loginMutation = useMutation({
    mutationFn: (data: LoginRequest) => authApi.login(data),
    onSuccess: async (response) => {
      const tokens = response.data.data!;
      setTokens(tokens);
      try {
        const profileRes = await usersApi.getMe();
        setUser(profileRes.data.data!);
      } catch { /* profile will be loaded later */ }
      toast.success('Login successful');
      const updatedRoles = useAuthStore.getState().roles;
      navigate(updatedRoles.includes('Teacher') ? '/teacher/dashboard' : '/dashboard');
    },
    onError: (error: unknown) => {
      const data = (error as { response?: { data?: { message?: string; Message?: string } } })?.response?.data;
      const msg = data?.message || data?.Message || 'Login failed';
      toast.error(msg);
    },
  });

  const registerStudentMutation = useMutation({
    mutationFn: (data: RegisterStudentRequest) => authApi.registerStudent(data),
    onSuccess: () => {
      toast.success('Registration successful! Please check your email to verify your account.');
      navigate('/login');
    },
    onError: (error: unknown) => {
      const msg = (error as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Registration failed';
      toast.error(msg);
    },
  });

  const registerTeacherMutation = useMutation({
    mutationFn: (data: RegisterTeacherRequest) => authApi.registerTeacher(data),
    onSuccess: () => {
      toast.success('Registration successful! Please check your email to verify your account.');
      navigate('/login');
    },
    onError: (error: unknown) => {
      const msg = (error as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Registration failed';
      toast.error(msg);
    },
  });

  const logoutMutation = useMutation({
    mutationFn: async () => {
      const refreshToken = useAuthStore.getState().refreshToken;
      if (refreshToken) {
        await authApi.logout(refreshToken);
      }
    },
    onSettled: () => {
      clearAuth();
      navigate('/');
    },
  });

  const profileQuery = useQuery({
    queryKey: ['profile'],
    queryFn: async () => {
      const res = await usersApi.getMe();
      const profile = res.data.data!;
      setUser(profile);
      return profile;
    },
    enabled: isAuthenticated,
  });

  return {
    isAuthenticated,
    user,
    roles,
    accessToken,
    isTeacher: roles.includes('Teacher'),
    isStudent: roles.includes('Student'),
    login: loginMutation.mutate,
    loginLoading: loginMutation.isPending,
    registerStudent: registerStudentMutation.mutate,
    registerStudentLoading: registerStudentMutation.isPending,
    registerTeacher: registerTeacherMutation.mutate,
    registerTeacherLoading: registerTeacherMutation.isPending,
    logout: logoutMutation.mutate,
    profile: profileQuery.data,
    profileLoading: profileQuery.isLoading,
  };
}
