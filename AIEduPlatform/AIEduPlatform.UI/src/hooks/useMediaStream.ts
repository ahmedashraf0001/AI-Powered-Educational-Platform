import { useState, useEffect, useCallback, useRef } from 'react';
import { materialsApi } from '@/api/materials.api';

export function useMediaStream(materialId: string | null) {
  const [blobUrl, setBlobUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const blobUrlRef = useRef<string | null>(null);

  const loadStream = useCallback(async (id: string) => {
    setLoading(true);
    setError(null);
    try {
      const url = await materialsApi.getStreamUrl(id);
      blobUrlRef.current = url;
      setBlobUrl(url);
    } catch (err) {
      setError('Failed to load media');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    // Reset state when materialId changes
    setBlobUrl(null);
    setError(null);

    if (materialId) {
      loadStream(materialId);
    }
    return () => {
      if (blobUrlRef.current) {
        URL.revokeObjectURL(blobUrlRef.current);
        blobUrlRef.current = null;
      }
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [materialId]);

  return { blobUrl, loading, error };
}
