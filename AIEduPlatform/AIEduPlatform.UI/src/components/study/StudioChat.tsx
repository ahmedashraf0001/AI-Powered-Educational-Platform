import { useRef, useEffect, useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { studySessionsApi } from '@/api/studySessions.api';
import { useSSEChat } from '@/hooks/useSSEChat';
import type { ChatMessageDto } from '@/types';
import { Button } from '@/components/ui/Button';
import { Spinner } from '@/components/ui/Spinner';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { Send, MessageSquare, Bot, User } from 'lucide-react';
import { renderTextWithRefs, type MaterialInfo } from './SourceReference';

interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

interface StudioChatProps {
  sessionId: string;
  lectureIds: string[];
  materialIds: string[];
  materials?: MaterialInfo[];
  onOpenMaterial?: (materialId: string, page?: number, timestamp?: number) => void;
}

export function StudioChat({ sessionId, lectureIds, materialIds, materials = [], onOpenMaterial }: StudioChatProps) {
  const [input, setInput] = useState('');
  const [page, setPage] = useState(1);
  const [localMessages, setLocalMessages] = useState<ChatMessage[]>([]);
  const bottomRef = useRef<HTMLDivElement>(null);

  const { data: historyData, isFetching } = useQuery({
    queryKey: ['chat-history', sessionId, page],
    queryFn: () => studySessionsApi.getChatHistory(sessionId, { page }),
    select: (res) => res.data.data,
  });

  const { isStreaming, streamContent, sendMessage } = useSSEChat(sessionId);

  const historyMessages: ChatMessage[] = historyData?.items
    ? [...historyData.items].reverse().map((m: ChatMessageDto) => ({
        role: (m.role === 'Student' || m.role === 'user' ? 'user' : 'assistant') as 'user' | 'assistant',
        content: m.content || '',
      }))
    : [];

  const allMessages = [...historyMessages, ...localMessages];

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [allMessages.length, streamContent]);

  const handleSend = async () => {
    if (!input.trim() || isStreaming) return;
    const msg = input.trim();
    setInput('');
    setLocalMessages((prev) => [...prev, { role: 'user', content: msg }]);

    try {
      await sendMessage(msg, lectureIds, materialIds);
    } catch {
      // error handled in hook
    }
  };

  // When streaming finishes and we have streamContent, push it as assistant message
  useEffect(() => {
    if (!isStreaming && streamContent) {
      setLocalMessages((prev) => [...prev, { role: 'assistant', content: streamContent }]);
    }
  }, [isStreaming, streamContent]);

  // Custom markdown renderers that make [Source: ...] references clickable
  const markdownComponents = useMemo(() => {
    if (!onOpenMaterial || materials.length === 0) return undefined;
    return {
      p: ({ children, ...props }: any) => {
        const processChild = (child: any): any => {
          if (typeof child === 'string') {
            return renderTextWithRefs(child, materials, onOpenMaterial);
          }
          return child;
        };
        const processed = Array.isArray(children)
          ? children.map(processChild)
          : processChild(children);
        return <p {...props}>{processed}</p>;
      },
      li: ({ children, ...props }: any) => {
        const processChild = (child: any): any => {
          if (typeof child === 'string') {
            return renderTextWithRefs(child, materials, onOpenMaterial);
          }
          return child;
        };
        const processed = Array.isArray(children)
          ? children.map(processChild)
          : processChild(children);
        return <li {...props}>{processed}</li>;
      },
    };
  }, [materials, onOpenMaterial]);

  return (
    <div className="flex flex-col h-full">
      <div className="flex-1 overflow-y-auto p-4 space-y-4">
        {/* Empty state */}
        {allMessages.length === 0 && !isStreaming && (
          <div className="flex flex-col items-center justify-center h-full gap-3 text-muted-foreground">
            <div className="p-4 rounded-full bg-primary/10">
              <MessageSquare className="h-8 w-8 text-primary" />
            </div>
            <p className="text-sm font-medium">Start a conversation</p>
            <p className="text-xs text-center max-w-xs">Ask questions about your course materials. Select references from the materials panel to give the AI context.</p>
          </div>
        )}

        {historyData?.hasNext && (
          <div className="text-center">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setPage((p) => p + 1)}
              loading={isFetching}
            >
              Load older messages
            </Button>
          </div>
        )}

        {allMessages.map((msg, idx) => (
          <div
            key={idx}
            className={`flex gap-3 ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}
          >
            {msg.role === 'assistant' && (
              <div className="flex-shrink-0 w-7 h-7 rounded-full bg-primary/10 flex items-center justify-center mt-1">
                <Bot className="h-4 w-4 text-primary" />
              </div>
            )}
            <div
              className={`max-w-[75%] rounded-2xl px-4 py-3 ${
                msg.role === 'user'
                  ? 'bg-primary text-primary-foreground rounded-br-md'
                  : 'bg-secondary rounded-bl-md'
              }`}
            >
              {msg.role === 'user' ? (
                <p className="whitespace-pre-wrap text-sm">{msg.content}</p>
              ) : (
                <div className="prose prose-sm dark:prose-invert max-w-none">
                  <ReactMarkdown remarkPlugins={[remarkGfm]} components={markdownComponents}>
                    {msg.content}
                  </ReactMarkdown>
                </div>
              )}
            </div>
            {msg.role === 'user' && (
              <div className="flex-shrink-0 w-7 h-7 rounded-full bg-primary flex items-center justify-center mt-1">
                <User className="h-4 w-4 text-primary-foreground" />
              </div>
            )}
          </div>
        ))}

        {isStreaming && streamContent && (
          <div className="flex gap-3 justify-start">
            <div className="flex-shrink-0 w-7 h-7 rounded-full bg-primary/10 flex items-center justify-center mt-1">
              <Bot className="h-4 w-4 text-primary" />
            </div>
            <div className="max-w-[75%] bg-secondary rounded-2xl rounded-bl-md px-4 py-3">
              <div className="prose prose-sm dark:prose-invert max-w-none">
                <ReactMarkdown remarkPlugins={[remarkGfm]} components={markdownComponents}>
                  {streamContent}
                </ReactMarkdown>
              </div>
            </div>
          </div>
        )}

        {isStreaming && !streamContent && (
          <div className="flex gap-3 justify-start">
            <div className="flex-shrink-0 w-7 h-7 rounded-full bg-primary/10 flex items-center justify-center mt-1">
              <Bot className="h-4 w-4 text-primary" />
            </div>
            <div className="bg-secondary rounded-2xl rounded-bl-md px-4 py-3">
              <div className="flex items-center gap-2">
                <Spinner className="h-4 w-4" />
                <span className="text-xs text-muted-foreground">Thinking...</span>
              </div>
            </div>
          </div>
        )}
        <div ref={bottomRef} />
      </div>

      <div className="border-t p-3 bg-card/50">
        <div className="flex gap-2 items-end">
          <textarea
            className="flex-1 p-3 border rounded-xl bg-background resize-none text-sm focus:outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary transition-all"
            rows={2}
            placeholder="Ask about your course materials..."
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                handleSend();
              }
            }}
            disabled={isStreaming}
          />
          <Button
            onClick={handleSend}
            disabled={isStreaming || !input.trim()}
            size="icon"
            variant="gradient"
            className="rounded-xl h-11 w-11"
          >
            <Send className="h-4 w-4" />
          </Button>
        </div>
      </div>
    </div>
  );
}
