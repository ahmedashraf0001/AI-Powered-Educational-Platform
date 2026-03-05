using FastEndpoints.Swagger;
using NSwag;

namespace AIEduPlatform.Api.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.SwaggerDocument(o =>
            {
                o.DocumentSettings = s =>
                {
                    s.Title = "AIEduPlatform API";
                    s.Version = "v1";
                    s.Description =
                        "AI-Powered Educational Platform API.\n\n" +
                        "## Authentication\n" +
                        "Most endpoints require a JWT Bearer token. Register, verify your email, then login to get a token.\n\n" +
                        "## Enrollment Flow (Revamped)\n" +
                        "Enrollment now uses a **Cart → Checkout → Payment → Enrollment** pipeline:\n\n" +
                        "1. **Cart**: Add courses to your cart (`POST /api/cart/items`), view cart (`GET /api/cart`), remove items (`DELETE /api/cart/items/{courseId}`), or clear (`DELETE /api/cart`).\n" +
                        "2. **Checkout**: Create a checkout session (`POST /api/checkout`). Free carts enroll immediately; paid carts return a Stripe `clientSecret`.\n" +
                        "3. **Payment**: Use the `clientSecret` with Stripe.js `confirmCardPayment()` on the frontend. Stripe webhook auto-enrolls on success.\n" +
                        "4. **Order Status**: Poll `GET /api/checkout/{orderId}` to check order status and enrolled courses.\n" +
                        "5. **Free Courses**: Can still be enrolled directly via `POST /api/enrollments` (free courses only).\n\n" +
                        "## Unenrollment & Refund Policy\n" +
                        "- Within **10 days** of enrollment and **≤50% progress**: full refund.\n" +
                        "- Within **10 days** and **>50% progress**: 50% refund.\n" +
                        "- After 10 days or free courses: no refund.\n" +
                        "- Use `DELETE /api/enrollments/{courseId}` to unenroll.\n\n" +
                        "## Notifications\n" +
                        "Real-time notifications via SignalR (`/hubs/student-notifications`) and REST endpoints under `/api/notifications`.\n\n" +
                        "## Stripe Payments\n" +
                        "Paid courses require checkout with Stripe PaymentIntent. " +
                        "Use the **Checkout** endpoints to create a checkout session. " +
                        "In test mode, use the **Simulate payment confirmation** endpoint to bypass Stripe.\n\n" +
                        "### Test Card Numbers\n" +
                        "| Card | Scenario |\n" +
                        "|------|----------|\n" +
                        "| `4242 4242 4242 4242` | Succeeds |\n" +
                        "| `4000 0000 0000 0002` | Declined |\n" +
                        "| `4000 0025 0000 3155` | Requires 3D Secure |";
                };
                o.EnableJWTBearerAuth = true;
                o.AutoTagPathSegmentIndex = 100;
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwaggerGen();
            return app;
        }
    }
}
