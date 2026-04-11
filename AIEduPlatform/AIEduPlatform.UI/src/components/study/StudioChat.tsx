import { useRef, useEffect, useState, useMemo, useImperativeHandle, forwardRef } from 'react';
import { useQuery } from '@tanstack/react-query';
import { studySessionsApi } from '@/api/studySessions.api';
import { useSSEChat } from '@/hooks/useSSEChat';
import type { ChatMessageDto } from '@/types';
import { Button } from '@/components/ui/Button';
import { Spinner } from '@/components/ui/Spinner';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import remarkMath from 'remark-math';
import rehypeKatex from 'rehype-katex';
import 'katex/dist/katex.min.css';
import { Send, MessageSquare, Bot, User, BookOpen, Wand2 } from 'lucide-react';
import { renderTextWithRefs, type MaterialInfo } from './SourceReference';

/**
 * Preprocesses text to convert LaTeX inside parentheses to proper math delimiters.
 * Converts patterns like (c = m^{e} \bmod n) to $c = m^{e} \bmod n$
 */
function preprocessMath(text: string): string {
  // LaTeX indicators that suggest math content
  const latexIndicators = [
    /\\[a-zA-Z]+/, // LaTeX commands like \bmod, \phi, \times, \frac
    /\^{/, // Superscript with braces
    /_{/, // Subscript with braces
    /\\frac/, // Fractions
    /\\sqrt/, // Square root
    /\\sum/, // Sum
    /\\int/, // Integral
    /\\prod/, // Product
    /\\lim/, // Limit
  ];

  // Match content in parentheses that's likely math
  // Look for (content) where content has LaTeX indicators
  return text.replace(/\(([^()]+)\)/g, (match, inner) => {
    // Check if inner content has LaTeX indicators
    const hasLatex = latexIndicators.some(pattern => pattern.test(inner));

    // Also check for common math patterns: equations with =, ^, etc.
    const hasMathPattern = /[=<>]/.test(inner) && (/[\^_]/.test(inner) || /\\/.test(inner));

    if (hasLatex || hasMathPattern) {
      // Convert to inline math
      return `$${inner}$`;
    }

    return match;
  });
}

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

export interface StudioChatRef {
  sendMessage: (message: string) => Promise<void>;
}

export const StudioChat = forwardRef<StudioChatRef, StudioChatProps>(function StudioChat(
  { sessionId, lectureIds, materialIds, materials = [], onOpenMaterial },
  ref
) {
  const [input, setInput] = useState('');
  const [page, setPage] = useState(1);
  const [localMessages, setLocalMessages] = useState<ChatMessage[]>([]);
  const bottomRef = useRef<HTMLDivElement>(null);

  const { data: historyData, isFetching } = useQuery({
    queryKey: ['chat-history', sessionId, page],
    queryFn: () => studySessionsApi.getChatHistory(sessionId, { page }),
    select: (res) => res.data.data,
  });

  const { isStreaming, streamContent, sendMessage: sendSSEMessage } = useSSEChat(sessionId);

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

  const handleSend = async (messageOverride?: string) => {
    const msg = (messageOverride ?? input).trim();
    if (!msg || isStreaming) return;

    if (!messageOverride) {
      setInput('');
    }
    setLocalMessages((prev) => [...prev, { role: 'user', content: msg }]);

    try {
      await sendSSEMessage(msg, lectureIds, materialIds);
    } catch {
      // error handled in hook
    }
  };

  // Expose sendMessage to parent via ref
  useImperativeHandle(ref, () => ({
    sendMessage: async (message: string) => {
      await handleSend(message);
    },
  }), [isStreaming, lectureIds, materialIds]);

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
                <div className="prose-ai">
                  <ReactMarkdown
                    remarkPlugins={[remarkGfm, remarkMath]}
                    rehypePlugins={[rehypeKatex]}
                    components={markdownComponents}
                  >
                    {preprocessMath(msg.content)}
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
              <div className="prose-ai">
                <ReactMarkdown
                  remarkPlugins={[remarkGfm, remarkMath]}
                  rehypePlugins={[rehypeKatex]}
                  components={markdownComponents}
                >
                  {preprocessMath(streamContent)}
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

      <div className="border-t p-3 bg-card/80 backdrop-blur-md">
        <div className="flex items-center gap-2 mb-2 pb-1 overflow-x-auto no-scrollbar mask-edges">
          <Button
            variant="outline"
            size="sm"
            className="text-xs h-7 rounded-full bg-background shrink-0"
            onClick={() => handleSend('Summarize the selected materials.')}
            disabled={isStreaming || (lectureIds.length === 0 && materialIds.length === 0)}
          >
            <BookOpen className="h-3 w-3 mr-1" />
            Summarize Selected
          </Button>
          <Button
            variant="outline"
            size="sm"
            className="text-xs h-7 rounded-full bg-background shrink-0"
            onClick={() => handleSend('Explain the key concepts.')}
            disabled={isStreaming || (lectureIds.length === 0 && materialIds.length === 0)}
          >
            <Wand2 className="h-3 w-3 mr-1" />
            Key Concepts
          </Button>
        </div>
        <div className="flex gap-2 items-end">
          <textarea
            className="flex-1 p-3 border border-border/50 rounded-xl bg-background resize-none text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all shadow-sm"
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
            onClick={() => handleSend()}
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
});
