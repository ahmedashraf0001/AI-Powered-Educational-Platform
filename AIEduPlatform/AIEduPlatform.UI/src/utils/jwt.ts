import { jwtDecode } from 'jwt-decode';
import type { JwtPayload } from '@/types';

export function decodeToken(token: string): JwtPayload | null {
  try {
    return jwtDecode<JwtPayload>(token);
  } catch {
    return null;
  }
}

export function isTokenExpired(token: string): boolean {
  const decoded = decodeToken(token);
  if (!decoded) return true;
  return decoded.exp * 1000 < Date.now();
}

export function getUserRoles(token: string): string[] {
  const decoded = decodeToken(token);
  if (!decoded) return [];
  const role = decoded.role ?? decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
  if (Array.isArray(role)) return role;
  if (typeof role === 'string') return [role];
  return [];
}

export function hasRole(token: string, role: string): boolean {
  return getUserRoles(token).includes(role);
}
