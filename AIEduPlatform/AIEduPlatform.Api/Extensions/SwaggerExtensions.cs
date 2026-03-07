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
                        "AI-Powered Educational Platform REST API.\n\n" +
                        "## Getting Started\n" +
                        "1. Register an account (`POST /api/auth/register/student` or `/teacher`).\n" +
                        "2. Check your email and verify via the link (`GET /api/auth/verify-email`).\n" +
                        "3. Login (`POST /api/auth/login`) to receive `accessToken` and `refreshToken`.\n" +
                        "4. Click the **Authorize** button above and enter `Bearer {accessToken}`.\n\n" +
                        "## Authentication\n" +
                        "Most endpoints require a JWT Bearer token. Tokens expire periodically — use `POST /api/auth/refresh-token` to get a new pair without re-logging in. " +
                        "If the refresh token is also expired, the user must login again.\n\n" +
                        "## Roles\n" +
                        "| Role | Description |\n" +
                        "|------|-------------|\n" +
                        "| **Student** | Can browse courses, enroll, study, take exams |\n" +
                        "| **Teacher** | Can create/manage courses, lectures, materials, exams, and grade submissions |\n\n" +
                        "A single account can hold both roles simultaneously.\n\n" +
                        "## Enrollment Flow\n" +
                        "Enrollment uses a **Cart → Checkout → Payment → Enrollment** pipeline:\n\n" +
                        "1. **Cart**: Add courses to your cart (`POST /api/cart/items`), view cart (`GET /api/cart`), remove items (`DELETE /api/cart/items/{courseId}`), or clear (`DELETE /api/cart`).\n" +
                        "2. **Checkout**: Create a checkout session (`POST /api/checkout`). Free carts enroll immediately; paid carts return a Stripe `clientSecret`.\n" +
                        "3. **Payment**: Use the `clientSecret` with Stripe.js `confirmCardPayment()` on the frontend. Stripe webhook auto-enrolls on success.\n" +
                        "4. **Order Status**: Poll `GET /api/checkout/{orderId}` to check order status and enrolled courses.\n" +
                        "5. **Free Courses**: Can also be enrolled directly via `POST /api/courses/{courseId}/enroll`.\n\n" +
                        "## Unenrollment & Refund Policy\n" +
                        "- Within **10 days** of enrollment and **≤50% progress**: full refund.\n" +
                        "- Within **10 days** and **>50% progress**: 50% refund.\n" +
                        "- After 10 days or free courses: no refund.\n" +
                        "- Use `DELETE /api/enrollments/{courseId}` to unenroll.\n\n" +
                        "## Study Sessions\n" +
                        "Start a study session (`POST /api/study-sessions`) to access AI-powered features: chat (SSE streaming), flashcards, mind maps, quizzes, summaries, and dialogue audio generation. " +
                        "All study features require an active `sessionId`.\n\n" +
                        "## Real-time Notifications\n" +
                        "Connect to SignalR hub at `/hubs/student-notifications` for real-time push notifications. " +
                        "REST endpoints under `/api/notifications` provide notification history and management.\n\n" +
                        "## Stripe Test Cards\n" +
                        "| Card | Scenario |\n" +
                        "|------|----------|\n" +
                        "| `4242 4242 4242 4242` | Succeeds |\n" +
                        "| `4000 0000 0000 0002` | Declined |\n" +
                        "| `4000 0025 0000 3155` | Requires 3D Secure |\n\n" +
                        "## Error Responses\n" +
                        "All errors follow the standard `ApiResponse<T>` wrapper with `IsSuccess`, `Message`, and `Errors` fields.\n\n" +
                        "| Status | Meaning |\n" +
                        "|--------|---------|\n" +
                        "| `400` | Validation error — check `Errors` array |\n" +
                        "| `401` | Not authenticated — login or refresh token |\n" +
                        "| `403` | Forbidden — insufficient role or permissions |\n" +
                        "| `404` | Resource not found |\n" +
                        "| `409` | Conflict (e.g., duplicate review) |\n" +
                        "| `429` | Rate limited — wait and retry |";
                };
                o.EnableJWTBearerAuth = true;
                o.AutoTagPathSegmentIndex = 100;
                o.TagDescriptions = t =>
                {
                    t["Auth"] = "Registration, login, email verification, token refresh, logout, and password management";
                    t["Users"] = "User profile retrieval and updates (bio, avatar, social links)";
                    t["Courses"] = "Course CRUD, catalog browsing, search, completion, and engagement tracking";
                    t["Categories"] = "Course category management for organizing the catalog";
                    t["Lectures"] = "Lecture CRUD within courses — ordering, content structure";
                    t["Materials"] = "Upload, stream, download, and manage lecture materials (PDF, video, audio, images)";
                    t["Semantic Sections"] = "AI-extracted semantic sections from materials — summaries, flashcards, and quizzes per section";
                    t["Enrollments"] = "Enroll, unenroll, track progress, and manage course enrollments";
                    t["Cart"] = "Shopping cart management — add, remove, view, and clear course items";
                    t["Checkout"] = "Checkout sessions, order creation, and order status tracking";
                    t["Payments"] = "Stripe payment processing, webhook handling, and payment simulation (test mode)";
                    t["StudySessions"] = "AI-powered study sessions — start/end sessions, chat (SSE), flashcards, mind maps, quizzes, summaries, dialogue audio";
                    t["Exams"] = "Exam CRUD, availability checks, and AI-assisted exam generation";
                    t["Questions"] = "Exam question management — create, update, delete, and reorder questions";
                    t["Submissions"] = "Exam submission and answer management for students";
                    t["Grades"] = "Grade viewing, AI-assisted grading, and grade management for teachers";
                    t["Reviews"] = "Course reviews and ratings by enrolled students";
                    t["Notifications"] = "Notification retrieval, read/unread management, and notification preferences";
                    t["AI"] = "AI provider status and runtime switching between LLM providers (Ollama/Groq)";
                    t["Dialogue"] = "Dialogue audio configuration — voice selection, preview, settings, and format options";
                };
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
