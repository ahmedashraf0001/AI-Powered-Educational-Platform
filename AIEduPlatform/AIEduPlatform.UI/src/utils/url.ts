const LOCALHOST_PATTERN = /^(https?:\/\/)?(localhost|127\.0\.0\.1)(:\d+)?(\/|$)/i;
const configuredApiUrl = (import.meta.env.VITE_API_URL ?? '').trim().replace(/\/+$/, '');
const safeApiUrl =
  import.meta.env.PROD && LOCALHOST_PATTERN.test(configuredApiUrl)
    ? '/api'
    : configuredApiUrl || '/api';
const API_BASE = safeApiUrl.replace(/\/api\/?$/, '');

/**
 * Resolve a backend URL (e.g., /uploads/...) to a full URL.
 * If already absolute, returns as-is.
 */
export function resolveUrl(path: string | null | undefined): string | null {
  if (!path) return null;

  const normalizedPath = path.replace(/\\/g, '/').trim();
  if (normalizedPath.startsWith('http://') || normalizedPath.startsWith('https://')) return normalizedPath;

  const pathWithLeadingSlash = normalizedPath.startsWith('/') ? normalizedPath : `/${normalizedPath}`;
  return `${API_BASE}${pathWithLeadingSlash}`;
}
