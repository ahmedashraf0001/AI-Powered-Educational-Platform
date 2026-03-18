import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { dialogueApi } from '@/api/dialogue.api';
import type { VoiceDto, UserVoiceSettingsDto } from '@/types';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Select } from '@/components/ui/Select';
import { Input } from '@/components/ui/Input';
import { PageSpinner } from '@/components/ui/Spinner';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { useForm, Controller } from 'react-hook-form';
import { toast } from 'sonner';
import { useEffect, useRef } from 'react';
import { Mic, Play, RotateCcw } from 'lucide-react';

export default function VoiceSettingsPage() {
  const queryClient = useQueryClient();
  const audioRef = useRef<HTMLAudioElement>(null);

  const { data: voices, isLoading: loadingVoices } = useQuery({
    queryKey: ['voices'],
    queryFn: () => dialogueApi.getVoices(),
    select: (res) => res.data.data as VoiceDto[],
  });

  const { data: settings, isLoading: loadingSettings } = useQuery({
    queryKey: ['voice-settings'],
    queryFn: () => dialogueApi.getVoiceSettings(),
    select: (res) => res.data.data as UserVoiceSettingsDto,
  });

  const { data: formats } = useQuery({
    queryKey: ['voice-formats'],
    queryFn: () => dialogueApi.getFormats(),
    select: (res) => res.data.data,
  });

  const { data: _languages } = useQuery({
    queryKey: ['voice-languages'],
    queryFn: () => dialogueApi.getLanguages(),
    select: (res) => res.data.data,
  });

  const form = useForm<Partial<UserVoiceSettingsDto>>();

  useEffect(() => {
    if (settings) form.reset(settings);
  }, [settings]);

  const saveMutation = useMutation({
    mutationFn: (data: Partial<UserVoiceSettingsDto>) => dialogueApi.saveVoiceSettings(data),
    onSuccess: () => {
      toast.success('Voice settings saved');
      queryClient.invalidateQueries({ queryKey: ['voice-settings'] });
    },
    onError: () => toast.error('Failed to save settings'),
  });

  const resetMutation = useMutation({
    mutationFn: () => dialogueApi.deleteVoiceSettings(),
    onSuccess: () => {
      toast.success('Voice settings reset to defaults');
      queryClient.invalidateQueries({ queryKey: ['voice-settings'] });
    },
  });

  const handlePreview = async (voiceId: string) => {
    try {
      const res = await dialogueApi.getPreviews({ VoiceId: voiceId, SampleText: 'Hello! This is a voice preview.' });
      const audioData = res.data.data;
      if (audioData && audioRef.current) {
        const blob = new Blob([Uint8Array.from(atob(audioData), c => c.charCodeAt(0))], { type: 'audio/mp3' });
        audioRef.current.src = URL.createObjectURL(blob);
        audioRef.current.play();
      }
    } catch {
      toast.error('Failed to load preview');
    }
  };

  if (loadingVoices || loadingSettings) return <PageSpinner />;

  const voiceOptions = (voices || []).map((v) => ({ label: v.name, value: v.voiceId }));

  return (
    <AnimatedPage>
    <div className="max-w-2xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold flex items-center gap-2 mb-2">
        <Mic className="h-8 w-8" /> Voice Settings
      </h1>
      <p className="text-muted-foreground mb-8">
        Configure voices for AI dialogue generation.
      </p>

      <audio ref={audioRef} className="hidden" />

      <form onSubmit={form.handleSubmit((d) => saveMutation.mutate(d))} className="space-y-6">
        <Card>
          <CardHeader><CardTitle>Voice Selection</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-end gap-2">
              <div className="flex-1">
                <Controller
                  name="teacherVoiceId"
                  control={form.control}
                  render={({ field }) => (
                    <Select label="Teacher Voice" options={voiceOptions} {...field} />
                  )}
                />
              </div>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => handlePreview(form.getValues('teacherVoiceId') ?? '')}
              >
                <Play className="h-4 w-4" />
              </Button>
            </div>
            <div className="flex items-end gap-2">
              <div className="flex-1">
                <Controller
                  name="studentVoiceId"
                  control={form.control}
                  render={({ field }) => (
                    <Select label="Student Voice" options={voiceOptions} {...field} />
                  )}
                />
              </div>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => handlePreview(form.getValues('studentVoiceId') ?? '')}
              >
                <Play className="h-4 w-4" />
              </Button>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle>Speed & Timing</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Teacher Speed"
                type="number"
                step="0.1"
                min="0.5"
                max="2.0"
                {...form.register('teacherSpeed', { valueAsNumber: true })}
              />
              <Input
                label="Student Speed"
                type="number"
                step="0.1"
                min="0.5"
                max="2.0"
                {...form.register('studentSpeed', { valueAsNumber: true })}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Pause Duration (ms)"
                type="number"
                step="100"
                min="0"
                {...form.register('pauseDurationMs', { valueAsNumber: true })}
              />
              <Input
                label="Pause Multiplier"
                type="number"
                step="0.1"
                min="0.1"
                {...form.register('pauseMultiplier', { valueAsNumber: true })}
              />
            </div>
            <label className="flex items-center gap-2">
              <input type="checkbox" {...form.register('includePauses')} className="rounded" />
              <span className="text-sm">Include pauses between speakers</span>
            </label>
            <label className="flex items-center gap-2">
              <input type="checkbox" {...form.register('normalizeAudio')} className="rounded" />
              <span className="text-sm">Normalize audio levels</span>
            </label>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle>Output Format</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <Controller
              name="outputFormat"
              control={form.control}
              render={({ field }) => (
                <Select
                  label="Format"
                  options={(formats as string[] || ['mp3', 'wav', 'ogg']).map((f: string) => ({ label: f.toUpperCase(), value: f }))}
                  {...field}
                />
              )}
            />
            <Input
              label="Sample Rate (Hz)"
              type="number"
              step="1000"
              {...form.register('sampleRate', { valueAsNumber: true })}
            />
          </CardContent>
        </Card>

        <div className="flex gap-2">
          <Button type="submit" loading={saveMutation.isPending}>Save Settings</Button>
          <Button
            type="button"
            variant="outline"
            onClick={() => resetMutation.mutate()}
            loading={resetMutation.isPending}
          >
            <RotateCcw className="h-4 w-4 mr-2" /> Reset to Defaults
          </Button>
        </div>
      </form>
    </div>
    </AnimatedPage>
  );
}
