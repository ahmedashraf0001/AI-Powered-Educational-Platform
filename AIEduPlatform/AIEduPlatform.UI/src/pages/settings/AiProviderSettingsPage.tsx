import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { aiProviderApi } from '@/api/aiProvider.api';
import { Card, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { PageSpinner } from '@/components/ui/Spinner';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { toast } from 'sonner';
import { Cpu, CheckCircle, Circle } from 'lucide-react';

export default function AiProviderSettingsPage() {
  const queryClient = useQueryClient();

  const { data: status, isLoading } = useQuery({
    queryKey: ['ai-provider'],
    queryFn: () => aiProviderApi.getStatus(),
    select: (res) => res.data.data,
  });

  const switchMutation = useMutation({
    mutationFn: (provider: string) => aiProviderApi.switch(provider),
    onSuccess: (res) => {
      toast.success(res.data.data?.message ?? 'Provider switched');
      queryClient.invalidateQueries({ queryKey: ['ai-provider'] });
    },
    onError: () => toast.error('Failed to switch provider'),
  });

  if (isLoading) return <PageSpinner />;

  return (
    <AnimatedPage>
    <div className="max-w-2xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold flex items-center gap-2 mb-2">
        <Cpu className="h-8 w-8" /> AI Provider Settings
      </h1>
      <p className="text-muted-foreground mb-8">
        Choose which AI provider powers your study sessions and AI features.
      </p>

      <div className="space-y-4">
        {(status?.supportedProviders || []).map((provider: string) => {
          const isActive = status?.activeProvider === provider;
          const isGroq = provider.toLowerCase() === 'groq';

          return (
            <Card key={provider} className={isActive ? 'border-primary border-2' : ''}>
              <CardContent className="p-6 flex items-center justify-between">
                <div className="flex items-center gap-4">
                  {isActive ? (
                    <CheckCircle className="h-6 w-6 text-primary" />
                  ) : (
                    <Circle className="h-6 w-6 text-muted-foreground" />
                  )}
                  <div>
                    <h3 className="text-lg font-semibold capitalize">{provider}</h3>
                    <p className="text-sm text-muted-foreground">
                      {isGroq
                        ? 'Cloud-based AI provider (Groq)'
                        : 'Local AI provider (Ollama)'}
                    </p>
                    {isGroq && !status?.isGroqConfigured && (
                      <p className="text-xs text-destructive mt-1">API Key not configured</p>
                    )}
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  {isActive && <Badge variant="success">Active</Badge>}
                  {!isActive && (
                    <Button
                      onClick={() => switchMutation.mutate(provider)}
                      loading={switchMutation.isPending}
                      disabled={isGroq && !status?.isGroqConfigured}
                    >
                      Switch
                    </Button>
                  )}
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>
    </div>
    </AnimatedPage>
  );
}
