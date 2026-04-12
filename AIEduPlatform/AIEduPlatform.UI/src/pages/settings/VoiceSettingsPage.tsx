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
import { useEffect, useMemo, useRef } from 'react';
import { Mic, Play, RotateCcw } from 'lucide-react';

export default function VoiceSettingsPage() {
  const queryClient = useQueryClient();
  const audioRef = useRef<HTMLAudioElement>(null);
  const previewObjectUrlRef = useRef<string | null>(null);

  const { data: voices, isLoading: loadingVoices } = useQuery({
    queryKey: ['voices'],
    queryFn: () => dialogueApi.getVoices(),
    select: (res) => {
      const raw = res.data.data;
      if (!Array.isArray(raw)) return [] as VoiceDto[];

      return raw
        .map((voice: any) => ({
          voiceId: (voice?.voiceId ?? voice?.voice_id ?? '').toString(),
          name: (voice?.name ?? voice?.voiceName ?? voice?.voice_id ?? '').toString(),
          description: (voice?.description ?? '').toString(),
          previewUrl: voice?.previewUrl ?? voice?.preview_url ?? null,
        }))
        .filter((voice: VoiceDto) => voice.voiceId.length > 0 && voice.name.length > 0);
    },
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
    if (!voiceId) {
      toast.error('Please select a voice first');
      return;
    }

    try {
      const res = await dialogueApi.getPreviews({ VoiceId: voiceId, SampleText: 'Hello! This is a voice preview.' });
      const payload = res.data?.data as any;
      const firstPreview = Array.isArray(payload) ? payload[0] : payload;
      const audioBase64 =
        (typeof payload === 'string' ? payload : null) ??
        firstPreview?.audioBase64 ??
        firstPreview?.audio_base64 ??
        null;
      const format = (firstPreview?.format ?? 'mp3').toString().toLowerCase();

      if (!audioBase64 || !audioRef.current) {
        toast.error('No preview audio returned for this voice');
        return;
      }

      const cleanedBase64 = audioBase64.replace(/\s+/g, '');
      const bytes = Uint8Array.from(atob(cleanedBase64), (c) => c.charCodeAt(0));

      const mimeType =
        format === 'wav' ? 'audio/wav' :
        format === 'ogg' ? 'audio/ogg' :
        'audio/mpeg';

      if (previewObjectUrlRef.current) {
        URL.revokeObjectURL(previewObjectUrlRef.current);
      }

      const blob = new Blob([bytes], { type: mimeType });
      const objectUrl = URL.createObjectURL(blob);
      previewObjectUrlRef.current = objectUrl;

      audioRef.current.src = objectUrl;
      await audioRef.current.play();
    } catch {
      toast.error('Failed to load preview');
    }
  };

  useEffect(() => {
    return () => {
      if (previewObjectUrlRef.current) {
        URL.revokeObjectURL(previewObjectUrlRef.current);
      }
    };
  }, []);

  const availableFormats = useMemo(() => {
    const fallback = ['mp3', 'wav', 'ogg'];

    if (Array.isArray(formats)) {
      return formats;
    }

    if (formats && typeof formats === 'object') {
      const formatObject = formats as {
        supportedFormats?: string[];
        supported_formats?: string[];
        formats?: string[];
      };

      if (Array.isArray(formatObject.supportedFormats)) {
        return formatObject.supportedFormats;
      }

      if (Array.isArray(formatObject.supported_formats)) {
        return formatObject.supported_formats;
      }

      if (Array.isArray(formatObject.formats)) {
        return formatObject.formats;
      }
    }

    return fallback;
  }, [formats]);

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
                  options={availableFormats.map((f: string) => ({ label: f.toUpperCase(), value: f }))}
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
