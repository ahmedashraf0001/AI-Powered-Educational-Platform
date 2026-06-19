import { useMutation, useQueryClient } from '@tanstack/react-query';
import { studySessionsApi } from '@/api/studySessions.api';
import { Button } from '@/components/ui/Button';
import { Spinner } from '@/components/ui/Spinner';
import { toast } from 'sonner';
import { useState, useRef, useEffect } from 'react';
import { cn } from '@/utils/cn';
import { Play, Pause } from 'lucide-react';

interface DialogueAudioViewProps {
  sessionId: string;
  lectureIds: string[];
  materialIds: string[];
}

interface TurnTimestamp {
  startTime: number;
  endTime: number;
  speaker: string;
  text: string;
}

export function DialogueAudioView({
  sessionId,
  lectureIds,
  materialIds,
}: DialogueAudioViewProps) {
  const [audioUrl, setAudioUrl] = useState<string | null>(null);
  const [turns, setTurns] = useState<TurnTimestamp[]>([]);
  const [exchanges, setExchanges] = useState<Array<{ speaker: string; text: string }>>([]);
  const [currentTurn, setCurrentTurn] = useState(-1);
  const [isPlaying, setIsPlaying] = useState(false);
  const audioRef = useRef<HTMLAudioElement>(null);
  const queryClient = useQueryClient();

  const generateMutation = useMutation({
    mutationFn: () => {
      const promise = studySessionsApi.generateDialogueAudio(sessionId, { lectureIds, materialIds })
        .then((res) => {
          queryClient.invalidateQueries({ queryKey: ['dialogues-history', sessionId] });
          return res;
        });

      toast.promise(promise, {
        loading: 'Generating dialogue audio...',
        success: 'Dialogue audio generated successfully!',
        error: (err: any) => err?.userMessage || 'Failed to generate dialogue audio'
      });

      return promise;
    },
    onSuccess: (res) => {
      const data = res.data.data;
      if (!data) return;
      // Decode base64 audio
      if (data.audioBase64) {
        const binary = atob(data.audioBase64);
        const bytes = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) {
          bytes[i] = binary.charCodeAt(i);
        }
        const blob = new Blob([bytes], { type: 'audio/mp3' });
        setAudioUrl(URL.createObjectURL(blob));
      }
      setTurns(data.turnTimestamps || []);
      setExchanges(data.exchanges || []);
    },
  });

  // Sync transcript with audio playback
  useEffect(() => {
    const audio = audioRef.current;
    if (!audio || turns.length === 0) return;

    const onTimeUpdate = () => {
      const t = audio.currentTime;
      const idx = turns.findIndex(
        (turn) => t >= turn.startTime && t < turn.endTime
      );
      setCurrentTurn(idx);
    };

    audio.addEventListener('timeupdate', onTimeUpdate);
    return () => audio.removeEventListener('timeupdate', onTimeUpdate);
  }, [turns]);

  useEffect(() => {
    return () => {
      if (audioUrl) URL.revokeObjectURL(audioUrl);
    };
  }, [audioUrl]);

  const togglePlay = () => {
    const audio = audioRef.current;
    if (!audio) return;
    if (audio.paused) {
      audio.play();
      setIsPlaying(true);
    } else {
      audio.pause();
      setIsPlaying(false);
    }
  };

  return (
    <div className="p-4 space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="font-bold text-lg">Dialogue Audio</h3>
        <Button
          onClick={() => generateMutation.mutate()}
          loading={generateMutation.isPending}
        >
          Generate Dialogue
        </Button>
      </div>

      {generateMutation.isPending && (
        <div className="flex items-center justify-center py-12">
          <Spinner />
          <span className="ml-2 text-muted-foreground">
            Generating dialogue audio (this may take 30–60 seconds)...
          </span>
        </div>
      )}

      {audioUrl && (
        <div className="space-y-4">
          <div className="flex items-center gap-3">
            <Button variant="outline" size="icon" onClick={togglePlay}>
              {isPlaying ? <Pause className="h-4 w-4" /> : <Play className="h-4 w-4" />}
            </Button>
            <audio
              ref={audioRef}
              src={audioUrl}
              onEnded={() => setIsPlaying(false)}
              className="flex-1"
              controls
            />
          </div>

          {/* Transcript */}
          <div className="border rounded-lg max-h-96 overflow-y-auto">
            {(turns.length > 0 ? turns : exchanges).map((turn, idx) => (
              <div
                key={idx}
                className={cn(
                  'p-3 border-b last:border-b-0 transition-colors cursor-pointer',
                  idx === currentTurn && 'bg-primary/10'
                )}
                onClick={() => {
                  const audio = audioRef.current;
                  if (audio && turns[idx]) {
                    audio.currentTime = turns[idx].startTime;
                    audio.play();
                    setIsPlaying(true);
                  }
                }}
              >
                <span className="text-xs font-semibold text-primary">
                  {turn.speaker}
                </span>
                <p className="text-sm mt-1">{turn.text}</p>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
