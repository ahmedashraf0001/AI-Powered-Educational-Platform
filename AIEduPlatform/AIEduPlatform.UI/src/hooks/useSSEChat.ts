import { useState, useCallback, useRef } from 'react';
import { studySessionsApi } from '@/api/studySessions.api';

export function useSSEChat(sessionId: string, onFinish?: (content: string) => void) {
  const [isStreaming, setIsStreaming] = useState(false);
  const [streamContent, setStreamContent] = useState('');
  const [sources, setSources] = useState<string[]>([]);
  const contentRef = useRef('');

  const sendMessage = useCallback(
    async (
      message: string,
      lectureIds?: string[],
      materialIds?: string[],
      sectionId?: string
    ) => {
      setIsStreaming(true);
      setStreamContent('');
      contentRef.current = '';
      setSources([]);

      try {
        await studySessionsApi.sendChat(
          sessionId,
          message,
          lectureIds,
          materialIds,
          sectionId,
          (content, done, srcs) => {
            if (done) {
              setIsStreaming(false);
              if (srcs) setSources(srcs);
              if (onFinish) {
                onFinish(contentRef.current);
              }
            } else {
              contentRef.current += content;
              setStreamContent(contentRef.current);
            }
          }
        );
      } catch (error) {
        setIsStreaming(false);
        throw error;
      }
    },
    [sessionId, onFinish]
  );

  return { sendMessage, isStreaming, streamContent, sources };
}
