const API_BASE = (import.meta.env.VITE_API_URL || '/api').replace(/\/api$/, '');

/**
 * Resolve a backend URL (e.g., /uploads/...) to a full URL.
 * If already absolute, returns as-is.
 */
export function resolveUrl(path: string | null | undefined): string | null {
  if (!path) return null;
  if (path.startsWith('http://') || path.startsWith('https://')) return path;
  return `${API_BASE}${path}`;
}
