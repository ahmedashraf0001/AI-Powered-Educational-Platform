import { useState, useMemo } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { loadStripe } from '@stripe/stripe-js';
import { Elements, PaymentElement, useStripe, useElements } from '@stripe/react-stripe-js';
import { cartApi } from '@/api/cart.api';
import { checkoutApi } from '@/api/checkout.api';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { PageSpinner } from '@/components/ui/Spinner';
import { Button } from '@/components/ui/Button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { CheckCircle2, Clock, XCircle, CreditCard, ShoppingCart, Trash2, BookOpen } from 'lucide-react';
import { EmptyState } from '@/components/ui/Feedback';
import { OrderStatus } from '@/types';
import { toast } from 'sonner';

function PaymentForm({ onSuccess }: { onSuccess: () => void }) {
  const stripe = useStripe();
  const elements = useElements();
  const [isProcessing, setIsProcessing] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!stripe || !elements) return;

    setIsProcessing(true);
    try {
      const result = await stripe.confirmPayment({
        elements,
        confirmParams: {
          return_url: window.location.href,
        },
        redirect: 'if_required',
      });

      if (result.error || result.paymentIntent?.status !== 'succeeded') {
        toast.error(result.error?.message ?? 'Payment failed. Please try again.');
        setIsProcessing(false);
        return;
      }
      onSuccess();
    } catch (error: any) {
      toast.error(error?.userMessage ?? 'Payment failed. Please try again.');
      setIsProcessing(false);
    } 
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <PaymentElement />
      <Button
        type="submit"
        className="w-full text-lg h-12 mt-4"
        disabled={!stripe || !elements || isProcessing}
        loading={isProcessing}
      >
        <CreditCard className="h-5 w-5 mr-2" />
        Complete Purchase
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

  if (!order) {
    return (
      <div className="max-w-lg mx-auto px-4 py-16 text-center">
        <XCircle className="h-16 w-16 text-destructive mx-auto mb-4" />
        <h1 className="text-2xl font-bold mb-2">Order not found</h1>
        <p className="text-muted-foreground mb-6">We could not load this checkout record.</p>
        <Button onClick={() => navigate('/checkout')}>Back to Checkout</Button>
      </div>
    );
  }

  const isPaid = order?.status === OrderStatus.Paid;
  const isFailed = order?.status === OrderStatus.Failed;
  const hasEnrollments = (order?.enrolledCourses?.length ?? 0) > 0;
  const isPaidAndEnrolled = isPaid && hasEnrollments;
  const enrollmentFailed = isPaid && !hasEnrollments;

  const statusTitle = isPaidAndEnrolled
    ? 'Payment Successful!'
    : enrollmentFailed
      ? 'Enrollment Failed'
      : isFailed
        ? 'Payment Failed'
        : 'Processing Payment';

  const statusMessage = isPaidAndEnrolled
    ? 'Your order is complete and you are enrolled in the purchased course(s).'
    : enrollmentFailed
      ? 'Payment was processed but enrollment failed. Please contact support.'
      : isFailed
        ? 'Something went wrong with your payment. Please try again.'
        : paymentConfirmed
          ? 'Payment confirmed. Finalizing your enrollment...'
          : 'Please wait while we confirm your payment...';

  return (
    <div className="max-w-3xl mx-auto px-4 py-10 space-y-6">
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col sm:flex-row items-start sm:items-center gap-4">
            {isPaidAndEnrolled ? (
              <CheckCircle2 className="h-12 w-12 text-success shrink-0" />
            ) : enrollmentFailed || isFailed ? (
              <XCircle className="h-12 w-12 text-destructive shrink-0" />
            ) : (
              <Clock className="h-12 w-12 text-warning shrink-0 animate-pulse" />
            )}

            <div className="flex-1">
              <h1 className="text-2xl font-bold mb-1">{statusTitle}</h1>
              <p className="text-muted-foreground">{statusMessage}</p>
            </div>

            {isPaidAndEnrolled ? (
              <Button
                onClick={() => {
                  queryClient.invalidateQueries({ queryKey: ['cart'] });
                  navigate('/my-enrollments');
                }}
              >
                Go to My Enrollments
              </Button>
            ) : (
              <Button variant="outline" onClick={() => navigate('/checkout')}>
                Back to Checkout
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Checkout Summary</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 text-sm">
            <div className="rounded-md border bg-secondary/20 px-3 py-2">
              <p className="text-muted-foreground text-xs uppercase tracking-wide">Order ID</p>
              <p className="font-mono text-xs sm:text-sm break-all mt-1">{order.orderId}</p>
            </div>
            <div className="rounded-md border bg-secondary/20 px-3 py-2">
              <p className="text-muted-foreground text-xs uppercase tracking-wide">Status</p>
              <p className="font-semibold mt-1">{order.status}</p>
            </div>
            <div className="rounded-md border bg-secondary/20 px-3 py-2">
              <p className="text-muted-foreground text-xs uppercase tracking-wide">Total</p>
              <p className="font-semibold mt-1">${order.totalAmount.toFixed(2)} {order.currency.toUpperCase()}</p>
            </div>
            <div className="rounded-md border bg-secondary/20 px-3 py-2">
              <p className="text-muted-foreground text-xs uppercase tracking-wide">Paid At</p>
              <p className="font-semibold mt-1">{order.paidAt ? new Date(order.paidAt).toLocaleString() : 'Pending'}</p>
            </div>
          </div>

          <div>
            <h2 className="font-semibold mb-2">Courses</h2>
            {order.enrolledCourses.length > 0 ? (
              <div className="space-y-2">
                {order.enrolledCourses.map((course) => (
                  <div key={course.courseId} className="rounded-md border px-3 py-2 flex items-center justify-between gap-3">
                    <div className="min-w-0">
                      <p className="font-medium truncate">{course.courseTitle}</p>
                      <p className="text-xs text-muted-foreground">Course ID: {course.courseId}</p>
                    </div>
                    <p className="font-semibold shrink-0">${course.price.toFixed(2)}</p>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">
                Course details will appear here once enrollment is confirmed.
              </p>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

export default function CheckoutPage() {
  const { orderId } = useParams<{ orderId: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const queryClient = useQueryClient();

  const state = location.state as {
    clientSecret?: string;
    publishableKey?: string;
    paymentConfirmed?: boolean;
    orderId?: string;
    items?: any[];
    total?: number;
  } | null;

  const stripePromise = useMemo(
    () => (state?.publishableKey ? loadStripe(state.publishableKey) : null),
    [state?.publishableKey]
  );

  const { data: cart, isLoading: isCartLoading } = useQuery({
    queryKey: ['cart'],
    queryFn: () => cartApi.get(),
    select: (res) => res.data.data,
    enabled: !orderId && !state?.clientSecret,
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
        navigate(`/checkout/${data.orderId}`);
      } else {
        navigate(`/checkout`, {
          replace: true,
          state: {
            clientSecret: data.clientSecret,
            publishableKey: data.publishableKey,
            orderId: data.orderId,
            items: cart?.items,
            total: cart?.items.reduce((s: number, i: any) => s + (i.priceAtTimeOfAdding || 0), 0)
          },
        });
      }
    },
    onError: (error: any) => toast.error(error?.userMessage ?? 'Payment failed to initialize'),
  });

  // If navigated with orderId and payment is confirmed or no Stripe info -> poll status
  if (orderId && (!state?.clientSecret || state?.paymentConfirmed)) {
     return (
        <AnimatedPage>
          <OrderStatusView orderId={orderId!} paymentConfirmed={state?.paymentConfirmed} />
        </AnimatedPage>
     );
  }

  if (isCartLoading && !state?.clientSecret) return <PageSpinner />;

  // Unified State Handling (Payment mode vs Cart mode)
  const isPaymentMode = !!(state?.clientSecret && stripePromise && state?.orderId);
  const items = isPaymentMode ? (state?.items ?? []) : (cart?.items || []);
  const total = isPaymentMode ? (state?.total ?? 0) : items.reduce((sum: number, item: any) => sum + (item.priceAtTimeOfAdding || 0), 0);

  return (
    <AnimatedPage>
    <div className="max-w-6xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-8">Checkout</h1>

      {items.length === 0 ? (
        <EmptyState
          icon={<ShoppingCart className="h-12 w-12" />}
          title="Your checkout is empty"
          description="Browse courses to add them to your order"
          action={<Button onClick={() => navigate('/courses')}>Browse Courses</Button>}
        />
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          
          <div className="lg:col-span-2 space-y-4">
            <h2 className="text-xl font-semibold mb-4">{isPaymentMode ? 'Order Overview' : 'Review Your Items'}</h2>
            
            <div className="space-y-4">
              {items.map((item: any) => (
                <Card key={item.courseId} className="overflow-hidden border hover:border-primary/20 transition-all duration-300">
                  <div className="flex flex-col sm:flex-row items-stretch">
                    <div className="sm:w-48 h-36 flex-shrink-0 bg-muted flex items-center justify-center">
                      {item.courseThumbnailUrl ? (
                         <img src={item.courseThumbnailUrl} alt={item.courseTitle || item.course?.title} className="w-full h-full object-cover" />
                      ) : (
                         <div className="w-full h-full bg-secondary/10 flex items-center justify-center text-muted-foreground">
                           <BookOpen className="h-12 w-12 opacity-50" />
                         </div>
                      )}
                    </div>
                    
                    <CardContent className="p-4 flex-1 flex flex-col justify-between">
                      <div className="flex justify-between items-start gap-4">
                        <div>
                          <h3 className="font-semibold text-lg line-clamp-2">{item.courseTitle || item.course?.title}</h3>
                          <p className="text-sm text-muted-foreground mt-1">Instructor: {item.teacherName || 'AIEduPlatform Expert'}</p>
                          
                          <div className="flex flex-wrap gap-2 mt-4">
                            <span className="inline-flex items-center text-xs px-2 py-1 bg-primary/10 text-primary rounded-md">
                              <CheckCircle2 className="w-3 h-3 mr-1" /> Lifetime Access
                            </span>
                            <span className="inline-flex items-center text-xs px-2 py-1 bg-primary/10 text-primary rounded-md">
                              <CheckCircle2 className="w-3 h-3 mr-1" /> HD Video
                            </span>
                          </div>
                        </div>
                        <div className="text-right shrink-0">
                          <div className="text-xl font-bold">${(item.priceAtTimeOfAdding ?? item.price)?.toFixed(2)}</div>
                          {item.originalPrice > (item.priceAtTimeOfAdding ?? item.price) && (
                            <div className="text-sm text-muted-foreground line-through">${item.originalPrice?.toFixed(2)}</div>
                          )}
                        </div>
                      </div>
                      
                      {!isPaymentMode && (
                        <div className="flex justify-start mt-4">
                          <Button
                            variant="ghost"
                            size="sm"
                            className="text-destructive hover:bg-destructive/10 hover:text-destructive p-0 h-auto"
                            onClick={() => removeMutation.mutate(item.courseId)}
                          >
                            <Trash2 className="h-4 w-4 mr-2" /> Remove Item
                          </Button>
                        </div>
                      )}
                    </CardContent>
                  </div>
                </Card>
              ))}
            </div>
          </div>

          <div className="lg:col-span-1">
             {isPaymentMode ? (
                <Card className="sticky top-24 border-primary/40 shadow-xl shadow-primary/5">
                   <CardHeader>
                     <CardTitle className="flex justify-between items-center">
                        Secure Payment
                     </CardTitle>
                   </CardHeader>
                   <CardContent className="space-y-6">
                      <div className="flex items-center justify-between text-2xl font-bold bg-secondary/20 p-4 rounded-lg">
                        <span>Total:</span>
                        <span>${total.toFixed(2)}</span>
                      </div>
                      
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
                        <PaymentForm 
                          onSuccess={() => {
                            navigate(`/checkout/${state.orderId}`, {
                              replace: true,
                              state: { paymentConfirmed: true },
                            });
                          }} 
                        />
                      </Elements>
                      
                      <Button
                        variant="ghost"
                        className="w-full text-muted-foreground mt-4 text-sm"
                        onClick={() => navigate('/checkout', { replace: true, state: null })}
                      >
                        Cancel payment & go back
                      </Button>
                   </CardContent>
                </Card>
             ) : (
                <Card className="sticky top-24">
                   <CardHeader>
                     <CardTitle>Order Summary</CardTitle>
                   </CardHeader>
                   <CardContent className="space-y-6">
                      <div className="flex items-center justify-between text-2xl font-bold border-b pb-4">
                        <span>Total:</span>
                        <span>${total.toFixed(2)}</span>
                      </div>
                      
                      <div className="space-y-3">
                        <Button
                          variant="gradient"
                          className="w-full text-lg h-12 shadow-md shadow-primary/20"
                          onClick={() => checkoutMutation.mutate()}
                          loading={checkoutMutation.isPending}
                        >
                          <CreditCard className="w-5 h-5 mr-2" />
                          Proceed to Payment
                        </Button>
                        <Button
                          variant="ghost"
                          className="w-full text-muted-foreground"
                          onClick={() => clearMutation.mutate()}
                          loading={clearMutation.isPending}
                        >
                          Clear Cart
                        </Button>
                      </div>
                   </CardContent>
                </Card>
             )}
          </div>
          
        </div>
      )}
    </div>
    </AnimatedPage>
  );
}
