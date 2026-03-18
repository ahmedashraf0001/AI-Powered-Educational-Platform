import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { cartApi } from '@/api/cart.api';
import { checkoutApi } from '@/api/checkout.api';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import { PageSpinner } from '@/components/ui/Spinner';
import { EmptyState } from '@/components/ui/Feedback';
import { toast } from 'sonner';
import { useNavigate } from 'react-router-dom';
import { ShoppingCart, Trash2 } from 'lucide-react';

export default function CartPage() {
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const { data: cart, isLoading } = useQuery({
    queryKey: ['cart'],
    queryFn: () => cartApi.get(),
    select: (res) => res.data.data,
  });

  const removeMutation = useMutation({
    mutationFn: (courseId: string) => cartApi.removeItem(courseId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      toast.success('Item removed');
    },
  });

  const clearMutation = useMutation({
    mutationFn: cartApi.clear,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      toast.success('Cart cleared');
    },
  });

  const checkoutMutation = useMutation({
    mutationFn: () => checkoutApi.create(),
    onSuccess: (res) => {
      const data = res.data.data;
      if (!data) return;

      queryClient.invalidateQueries({ queryKey: ['cart'] });

      if (!data.requiresPayment) {
        // Free checkout — already enrolled
        navigate(`/checkout/${data.orderId}`);
      } else {
        // Paid checkout — pass Stripe data to payment page
        navigate(`/checkout/${data.orderId}`, {
          state: {
            clientSecret: data.clientSecret,
            publishableKey: data.publishableKey,
          },
        });
      }
    },
    onError: () => toast.error('Checkout failed'),
  });

  if (isLoading) return <PageSpinner />;

  const items = cart?.items || [];
  const total = items.reduce((sum: number, item: any) => sum + (item.priceAtTimeOfAdding || 0), 0);

  return (
    <AnimatedPage>
    <div className="max-w-3xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-8">Shopping Cart</h1>

      {items.length === 0 ? (
        <EmptyState
          icon={<ShoppingCart className="h-12 w-12" />}
          title="Your cart is empty"
          description="Browse courses to add them to your cart"
          action={<Button onClick={() => navigate('/courses')}>Browse Courses</Button>}
        />
      ) : (
        <>
          <div className="space-y-3 mb-6">
            {items.map((item: any) => (
              <Card key={item.courseId}>
                <CardContent className="p-4 flex items-center justify-between">
                  <div>
                    <h3 className="font-semibold">{item.courseTitle}</h3>
                    <p className="text-sm text-muted-foreground">${item.priceAtTimeOfAdding?.toFixed(2)}</p>
                  </div>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => removeMutation.mutate(item.courseId)}
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                </CardContent>
              </Card>
            ))}
          </div>

          <div className="border rounded-lg p-6 space-y-4">
            <div className="flex items-center justify-between text-lg font-bold">
              <span>Total</span>
              <span>${total.toFixed(2)}</span>
            </div>
            <div className="flex gap-2">
              <Button
                variant="outline"
                onClick={() => clearMutation.mutate()}
                loading={clearMutation.isPending}
              >
                Clear Cart
              </Button>
              <Button
                className="flex-1"
                onClick={() => checkoutMutation.mutate()}
                loading={checkoutMutation.isPending}
              >
                Checkout
              </Button>
            </div>
          </div>
        </>
      )}
    </div>
    </AnimatedPage>
  );
}
