import { useState, useMemo } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { loadStripe } from '@stripe/stripe-js';
import { Elements, PaymentElement, useStripe, useElements } from '@stripe/react-stripe-js';
import { checkoutApi } from '@/api/checkout.api';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { PageSpinner } from '@/components/ui/Spinner';
import { Button } from '@/components/ui/Button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { CheckCircle2, Clock, XCircle, CreditCard } from 'lucide-react';
import { OrderStatus } from '@/types';
import { toast } from 'sonner';

function PaymentForm({ orderId }: { orderId: string }) {
  const stripe = useStripe();
  const elements = useElements();
  const [isProcessing, setIsProcessing] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!stripe || !elements) return;

    setIsProcessing(true);

    const { error, paymentIntent } = await stripe.confirmPayment({
      elements,
      confirmParams: {
        return_url: window.location.href,
      },
      redirect: 'if_required',
    });

    if (error) {
      toast.error(error.message || 'Payment failed');
      setIsProcessing(false);
    } else {
      // Pass payment result in state so we can show appropriate UI while webhook processes
      navigate(`/checkout/${orderId}`, {
        replace: true,
        state: { paymentConfirmed: paymentIntent?.status === 'succeeded' },
      });
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <PaymentElement />
      <Button
        type="submit"
        className="w-full"
        disabled={!stripe || !elements || isProcessing}
        loading={isProcessing}
      >
        <CreditCard className="h-4 w-4 mr-2" />
        Pay Now
      </Button>
    </form>
  );
}

function OrderStatusView({ orderId, paymentConfirmed }: { orderId: string; paymentConfirmed?: boolean }) {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const { data: order, isLoading } = useQuery({
    queryKey: ['order', orderId],
    queryFn: () => checkoutApi.getOrderStatus(orderId),
    enabled: !!orderId,
    refetchInterval: (query) => {
      const status = query.state.data?.data?.data?.status as OrderStatus | undefined;
      if (status === OrderStatus.Paid || status === OrderStatus.Failed || status === OrderStatus.Refunded) {
        return false;
      }
      return 3000;
    },
    select: (res) => res.data.data,
  });

  if (isLoading) return <PageSpinner />;

  // If Stripe confirmed payment client-side but webhook hasn't updated the order yet, show success
  const isPaid = order?.status === OrderStatus.Paid || paymentConfirmed;
  const isFailed = order?.status === OrderStatus.Failed;

  return (
    <div className="max-w-lg mx-auto px-4 py-16 text-center">
      {isPaid ? (
        <>
          <CheckCircle2 className="h-16 w-16 text-success mx-auto mb-4" />
          <h1 className="text-2xl font-bold mb-2">Payment Successful!</h1>
          <p className="text-muted-foreground mb-6">
            You have been enrolled in the course(s).
          </p>
          <Button onClick={() => {
            queryClient.invalidateQueries({ queryKey: ['cart'] });
            navigate('/my-enrollments');
          }}>Go to My Enrollments</Button>
        </>
      ) : isFailed ? (
        <>
          <XCircle className="h-16 w-16 text-destructive mx-auto mb-4" />
          <h1 className="text-2xl font-bold mb-2">Payment Failed</h1>
          <p className="text-muted-foreground mb-6">
            Something went wrong with your payment. Please try again.
          </p>
          <Button onClick={() => navigate('/cart')}>Back to Cart</Button>
        </>
      ) : (
        <>
          <Clock className="h-16 w-16 text-warning mx-auto mb-4 animate-pulse" />
          <h1 className="text-2xl font-bold mb-2">Processing Payment</h1>
          <p className="text-muted-foreground mb-6">
            Please wait while we confirm your payment...
          </p>
        </>
      )}
    </div>
  );
}

export default function CheckoutPage() {
  const { orderId } = useParams<{ orderId: string }>();
  const location = useLocation();
  const state = location.state as {
    clientSecret?: string;
    publishableKey?: string;
    paymentConfirmed?: boolean;
  } | null;

  const stripePromise = useMemo(
    () => (state?.publishableKey ? loadStripe(state.publishableKey) : null),
    [state?.publishableKey]
  );

  // If we have Stripe state from CartPage, show the payment form
  if (state?.clientSecret && stripePromise) {
    return (
      <AnimatedPage>
        <div className="max-w-lg mx-auto px-4 py-8">
          <Card>
            <CardHeader>
              <CardTitle>Complete Payment</CardTitle>
            </CardHeader>
            <CardContent>
              <Elements
                stripe={stripePromise}
                options={{
                  clientSecret: state.clientSecret,
                  appearance: {
                    theme: 'night',
                    variables: {
                      colorPrimary: '#6366f1',
                      borderRadius: '8px',
                    },
                  },
                }}
              >
                <PaymentForm orderId={orderId!} />
              </Elements>
            </CardContent>
          </Card>
        </div>
      </AnimatedPage>
    );
  }

  // Otherwise, show order status (polling for webhook confirmation or free checkout)
  return (
    <AnimatedPage>
      <OrderStatusView orderId={orderId!} paymentConfirmed={state?.paymentConfirmed} />
    </AnimatedPage>
  );
}
