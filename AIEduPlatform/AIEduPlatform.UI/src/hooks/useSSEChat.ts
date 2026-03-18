import { useState, useCallback } from 'react';
import { studySessionsApi } from '@/api/studySessions.api';

export function useSSEChat(sessionId: string) {
  const [isStreaming, setIsStreaming] = useState(false);
  const [streamContent, setStreamContent] = useState('');
  const [sources, setSources] = useState<string[]>([]);

  const sendMessage = useCallback(
    async (
      message: string,
      lectureIds?: string[],
      materialIds?: string[]
    ) => {
      setIsStreaming(true);
      setStreamContent('');
      setSources([]);

      try {
        await studySessionsApi.sendChat(
          sessionId,
          message,
          lectureIds,
          materialIds,
          (content, done, srcs) => {
            if (done) {
              setIsStreaming(false);
              if (srcs) setSources(srcs);
            } else {
              setStreamContent((prev) => prev + content);
            }
          }
        );
      } catch (error) {
        setIsStreaming(false);
        throw error;
      }
    },
    [sessionId]
  );

  return { sendMessage, isStreaming, streamContent, sources };
}
