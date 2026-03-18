import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { AuthTokens, UserProfile } from '@/types';
import { decodeToken, getUserRoles, isTokenExpired } from '@/utils/jwt';

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  user: UserProfile | null;
  isAuthenticated: boolean;
  roles: string[];

  setTokens: (tokens: AuthTokens) => void;
  setUser: (user: UserProfile) => void;
  logout: () => void;
  isTeacher: () => boolean;
  isStudent: () => boolean;
  userId: () => string | null;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      accessToken: null,
      refreshToken: null,
      user: null,
      isAuthenticated: false,
      roles: [],

      setTokens: (tokens: AuthTokens) => {
        const roles = getUserRoles(tokens.accessToken);
        set({
          accessToken: tokens.accessToken,
          refreshToken: tokens.refreshToken,
          isAuthenticated: true,
          roles,
        });
      },

      setUser: (user: UserProfile) => {
        set({ user });
      },

      logout: () => {
        set({
          accessToken: null,
          refreshToken: null,
          user: null,
          isAuthenticated: false,
          roles: [],
        });
      },

      isTeacher: () => get().roles.includes('Teacher'),
      isStudent: () => get().roles.includes('Student'),

      userId: () => {
        const token = get().accessToken;
        if (!token) return null;
        const decoded = decodeToken(token);
        return decoded?.sub ?? null;
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        roles: state.roles,
        isAuthenticated: state.accessToken ? !isTokenExpired(state.accessToken) : false,
      }),
    }
  )
);
