import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'sonner';
import { ErrorBoundary } from '@/components/ui/ErrorBoundary';
import { useThemeStore } from '@/stores/themeStore';
import 'react-loading-skeleton/dist/skeleton.css';
import './index.css';
import App from './App';

// Initialize theme on load
useThemeStore.getState();

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 30_000,
    },
  },
});

function ThemeAwareToaster() {
  const resolvedTheme = useThemeStore((s) => s.resolvedTheme);
  return <Toaster position="top-right" richColors closeButton theme={resolvedTheme} toastOptions={{ style: { fontSize: '13px' } }} />;
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ErrorBoundary>
      <BrowserRouter>
        <QueryClientProvider client={queryClient}>
          <App />
          <ThemeAwareToaster />
        </QueryClientProvider>
      </BrowserRouter>
    </ErrorBoundary>
  </StrictMode>,
);
