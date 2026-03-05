# AI Agent Task Prompt: Enrollment System Revamp, Unenrollment Policy & Notification Updates

---

## CONTEXT

The current enrollment system allows users to enroll directly in a single course without a payment flow. This must be **completely replaced** with a proper **Cart → Checkout → Stripe Payment → Enrollment** pipeline.

Additionally, the **Unenrollment system** must enforce a time-limited refund policy via Stripe. After all enrollment changes are implemented, the **Notification system** must be updated and extended to cover every new event introduced by these changes.

Work through this prompt **in order, step by step**. Do not skip ahead.

---

## STEP 1 — CODEBASE DISCOVERY

Before writing a single line of code, map the project completely:

```
1. List all existing entities/models (especially: User, Course, Enrollment, any existing Cart or Payment model).
2. List all existing Controllers and their routes (focus on: EnrollmentController, any PaymentController, NotificationController).
3. List all existing Services and Repositories related to enrollment, payment, notifications.
4. Check if Stripe is already integrated — look for Stripe NuGet packages, StripeClient initialization, webhook endpoints.
5. Check the existing Notification system — how are notifications stored (DB table?), what events trigger them, what DTOs exist.
6. Read the NewFeature.md file if it exists in the project root or docs folder — note every feature listed there.
7. Note the current database schema for Enrollment (columns, foreign keys, timestamps).
8. Note the base API URL and any existing webhook routes.
```

Do not proceed to Step 2 until you have a clear picture of what already exists.

---

## STEP 2 — DATABASE SCHEMA CHANGES

Create or update the following tables. Write proper migrations (EF Core `Add-Migration` or equivalent):

### 2a. `Carts` Table
```
Cart
├── Id (Guid, PK)
├── UserId (Guid, FK → Users)
├── CreatedAt (DateTime)
├── UpdatedAt (DateTime)
└── Status (enum: Active | CheckedOut | Abandoned)
```

### 2b. `CartItems` Table
```
CartItem
├── Id (Guid, PK)
├── CartId (Guid, FK → Carts)
├── CourseId (Guid, FK → Courses)
├── PriceAtTimeOfAdding (decimal) ← snapshot the price when item is added
└── AddedAt (DateTime)
```
- Unique constraint on `(CartId, CourseId)` — a course cannot be added to the same cart twice.

### 2c. `Orders` Table
```
Order
├── Id (Guid, PK)
├── UserId (Guid, FK → Users)
├── CartId (Guid, FK → Carts)
├── TotalAmount (decimal)
├── Currency (string, default "usd")
├── Status (enum: Pending | Paid | Refunded | PartiallyRefunded | Failed)
├── StripePaymentIntentId (string)
├── StripePaymentIntentClientSecret (string)
├── CreatedAt (DateTime)
└── PaidAt (DateTime?)
```

### 2d. `OrderItems` Table
```
OrderItem
├── Id (Guid, PK)
├── OrderId (Guid, FK → Orders)
├── CourseId (Guid, FK → Courses)
└── Price (decimal)
```

### 2e. Update `Enrollments` Table
Add the following columns if they do not already exist:
```
├── OrderId (Guid?, FK → Orders) ← null for free course enrollments
├── AmountPaid (decimal, default 0)
├── RefundedAt (DateTime?)
├── RefundAmount (decimal?)
├── StripeRefundId (string?)
└── UnenrolledAt (DateTime?)
```

### 2f. Run and verify migrations
```
- Run: dotnet ef migrations add EnrollmentRevamp
- Run: dotnet ef database update
- Confirm all tables and columns exist in the database before proceeding.
```

---

## STEP 3 — CART SYSTEM

### 3a. Create `CartItem` and `Cart` Entities, Repositories, and Service

**CartService must expose:**
```csharp
Task<CartDto> GetOrCreateCartAsync(Guid userId);
Task<CartDto> AddCourseToCartAsync(Guid userId, Guid courseId);
Task<CartDto> RemoveCourseFromCartAsync(Guid userId, Guid courseId);
Task ClearCartAsync(Guid userId);
Task<CartDto> GetCartAsync(Guid userId);
```

**Business rules to enforce in CartService:**
- A user cannot add a course they are already enrolled in. Return a clear error.
- A user cannot add a course that is not published. Return a clear error.
- A user cannot add the same course twice to the cart (enforce via unique constraint + service check).
- Capture `PriceAtTimeOfAdding` from `Course.Price` at the moment of adding, not at checkout.
- A user can only have one Active cart at a time.

### 3b. Create `CartController`

Routes:
```
GET    /api/cart                        → GetCartAsync (returns current cart with items, subtotal)
POST   /api/cart/items                  → AddCourseToCartAsync (body: { CourseId })
DELETE /api/cart/items/{courseId}       → RemoveCourseFromCartAsync
DELETE /api/cart                        → ClearCartAsync
```

### 3c. Cart DTOs

```json
// CartDto (GET /api/cart response)
{
  "CartId": "...",
  "Items": [
    {
      "CartItemId": "...",
      "CourseId": "...",
      "CourseTitle": "...",
      "CourseThumbnailUrl": "...",
      "TeacherName": "...",
      "OriginalPrice": 49.99,
      "PriceAtTimeOfAdding": 49.99
    }
  ],
  "ItemCount": 1,
  "Subtotal": 49.99,
  "Currency": "usd"
}
```

---

## STEP 4 — CHECKOUT & STRIPE PAYMENT INTENT

### 4a. Stripe Setup (if not already done)

- Install: `Stripe.net` NuGet package.
- Add to `appsettings.json`:
```json
"Stripe": {
  "SecretKey": "sk_test_...",
  "WebhookSecret": "whsec_...",
  "PublishableKey": "pk_test_..."
}
```
- Register `StripeClient` in `Program.cs` / `Startup.cs`:
```csharp
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
```

### 4b. Create `CheckoutService`

**Method: `CreateCheckoutSessionAsync(Guid userId)`**

Logic:
```
1. Fetch the user's Active cart. Error if cart is empty.
2. Validate all courses in cart are still published and exist.
3. Calculate total = sum of PriceAtTimeOfAdding for all items.
4. If total == 0 (all courses are free):
   - Skip Stripe entirely.
   - Create an Order with Status = Paid, TotalAmount = 0.
   - Immediately enroll user in all cart courses.
   - Clear the cart.
   - Return { OrderId, RequiresPayment: false }
5. If total > 0:
   - Create a Stripe PaymentIntent:
       Amount = total * 100 (convert to cents)
       Currency = "usd"
       Metadata = { UserId, CartId, OrderId }
   - Create an Order record with Status = Pending, StripePaymentIntentId = paymentIntentId.
   - Create OrderItems for each cart item.
   - Set Cart.Status = CheckedOut.
   - Return { OrderId, ClientSecret: paymentIntent.ClientSecret, RequiresPayment: true }
```

### 4c. Create `CheckoutController`

Routes:
```
POST /api/checkout          → CreateCheckoutSessionAsync
                              Returns: { OrderId, ClientSecret, RequiresPayment, PublishableKey }

GET  /api/checkout/{orderId} → GetOrderStatusAsync
                              Returns: { OrderId, Status, PaidAt, EnrolledCourses }
```

### 4d. Checkout DTOs

```json
// POST /api/checkout response
{
  "OrderId": "...",
  "ClientSecret": "pi_xxx_secret_xxx",
  "PublishableKey": "pk_test_...",
  "RequiresPayment": true,
  "TotalAmount": 49.99,
  "Currency": "usd",
  "Items": [
    { "CourseId": "...", "CourseTitle": "...", "Price": 49.99 }
  ]
}
```

---

## STEP 5 — STRIPE WEBHOOK: ENROLLMENT ON PAYMENT SUCCESS

### 5a. Create Webhook Endpoint

Route: `POST /api/webhooks/stripe`

This endpoint must:
1. Read the raw request body (do NOT let ASP.NET parse it — Stripe signature validation requires the raw bytes).
2. Get the `Stripe-Signature` header.
3. Validate the event using `EventUtility.ConstructEvent(payload, signature, webhookSecret)`.
4. Handle the following event types:

**`payment_intent.succeeded`:**
```
1. Extract UserId, CartId, OrderId from event.Data.Object.Metadata.
2. Find the Order by OrderId. If already Paid, skip (idempotency guard).
3. Set Order.Status = Paid, Order.PaidAt = now.
4. Fetch all OrderItems for this Order.
5. For each OrderItem:
   a. Check if user is already enrolled in this course (idempotency guard).
   b. If not enrolled: create Enrollment record with OrderId, AmountPaid = OrderItem.Price.
   c. Trigger: Send "EnrollmentConfirmed" notification to user.
   d. Trigger: Send "NewStudentEnrolled" notification to the course teacher.
6. Clear / mark cart as CheckedOut.
7. Return HTTP 200 immediately (Stripe requires fast response).
```

**`payment_intent.payment_failed`:**
```
1. Extract OrderId from metadata.
2. Set Order.Status = Failed.
3. Trigger: Send "PaymentFailed" notification to user.
4. Return HTTP 200.
```

**Important:** Wrap the entire webhook handler in try-catch. Always return `200 OK` to Stripe even on internal errors — log the error and handle it separately. Returning non-200 causes Stripe to retry.

### 5b. Free Course Enrollment (No Payment)

For courses with `Price == 0`, enrollment should bypass the cart entirely if desired, OR go through a zero-total checkout. Choose one approach and be consistent:

**Recommended:** Allow `POST /api/courses/{id}/enroll` only for free courses (Price == 0). Paid courses must go through cart → checkout.

Update the existing EnrollmentController:
```
- If course.Price > 0 → return 400 with message: "Paid courses must be enrolled via the checkout process."
- If course.Price == 0 → create enrollment directly (no order needed), set AmountPaid = 0, OrderId = null.
- Trigger: Send "EnrollmentConfirmed" notification.
- Trigger: Send "NewStudentEnrolled" notification to teacher.
```

---

## STEP 6 — UNENROLLMENT POLICY

### 6a. Unenrollment Rules

Before payment (cart stage): User simply removes the item from the cart. No DB enrollment record exists yet. No Stripe interaction needed. This is handled by `DELETE /api/cart/items/{courseId}` already built in Step 3.

After payment (enrolled stage): Enforce the following policy:

```
Rule 1 — Time Window:
  If now - Enrollment.CreatedAt > 10 days → DENY unenrollment.
  Return 400: "Unenrollment is only allowed within 10 days of enrollment."

Rule 2 — Progress-Based Refund:
  Calculate student's progress: CompletedLectures / TotalLectures * 100

  If progress <= 50%:
    → Full refund of AmountPaid via Stripe.
    → RefundAmount = AmountPaid.

  If progress > 50%:
    → Partial refund: 50% of AmountPaid via Stripe.
    → RefundAmount = AmountPaid * 0.5.

Rule 3 — Free Courses:
  If AmountPaid == 0 → allow unenrollment freely, no Stripe refund needed.

Rule 4 — Already Refunded:
  If Enrollment.RefundedAt is not null → DENY. Already unenrolled.
```

### 6b. UnenrollmentService

Create `UnenrollmentService` with method `UnenrollAsync(Guid userId, Guid courseId)`:

```
1. Fetch enrollment. If not found → 404.
2. Check unenrollment is not already done (idempotency).
3. Apply Rule 1 (time window check).
4. Calculate progress percentage (completed lectures / total lectures).
5. Determine refund amount based on Rule 2.
6. If refund amount > 0 and enrollment has an OrderId:
   a. Create a Stripe Refund:
      - Find the PaymentIntent from Order.StripePaymentIntentId.
      - Call Stripe Refunds API: RefundAmount in cents, reason = "requested_by_customer".
      - Store Enrollment.StripeRefundId = refund.Id.
7. Set Enrollment.UnenrolledAt = now.
8. Set Enrollment.RefundedAt = now (if refund was issued).
9. Set Enrollment.RefundAmount = calculated amount.
10. Delete or soft-delete the Enrollment record (prefer soft delete — keep record for audit).
11. Trigger: Send "UnenrollmentConfirmed" notification to user (include refund amount and ETA).
12. Trigger: Send "StudentUnenrolled" notification to teacher.
```

### 6c. Update Unenrollment Controller

Route: `DELETE /api/enrollments/{courseId}`

Response on success:
```json
{
  "Success": true,
  "Message": "You have been unenrolled from the course.",
  "RefundAmount": 24.99,
  "RefundCurrency": "usd",
  "RefundEta": "5-10 business days",
  "StripeRefundId": "re_..."
}
```

Response on denial (past 10 days):
```json
{
  "Success": false,
  "Message": "Unenrollment is no longer available. The 10-day window has passed.",
  "EnrolledAt": "2026-02-15T10:00:00Z",
  "DeadlineWas": "2026-02-25T10:00:00Z"
}
```

---

## STEP 7 — NOTIFICATION SYSTEM UPDATES

### 7a. Discovery

Before adding anything, audit the current notification system:
```
1. What is the current Notification entity structure?
2. How are notifications created? (Service method? Direct DB insert?)
3. Are notifications real-time (SignalR) or polled (REST)?
4. What notification types/events already exist?
5. What is the NotificationDto shape returned to clients?
```

### 7b. Update Notification Entity

Ensure the `Notification` entity has at minimum:
```
Notification
├── Id (Guid, PK)
├── UserId (Guid, FK → Users) ← the recipient
├── Type (string or enum)
├── Title (string)
├── Message (string)
├── IsRead (bool, default false)
├── RelatedEntityId (Guid?) ← CourseId, OrderId, EnrollmentId, etc.
├── RelatedEntityType (string?) ← "Course", "Order", "Enrollment"
├── Metadata (string/JSON?) ← optional extra data (refund amount, etc.)
├── CreatedAt (DateTime)
└── ReadAt (DateTime?)
```

### 7c. Add All New Notification Types

Implement a `NotificationService.CreateAsync(...)` method (or extend the existing one) that handles all the following event types. For each, specify the recipient, title, and message template:

---

#### CART EVENTS

| Type | Recipient | Title | Message Template |
|---|---|---|---|
| `CourseAddedToCart` | Student | "Course Added to Cart" | "**{CourseTitle}** has been added to your cart." |
| `CourseRemovedFromCart` | Student | "Course Removed from Cart" | "**{CourseTitle}** has been removed from your cart." |

---

#### CHECKOUT & PAYMENT EVENTS

| Type | Recipient | Title | Message Template |
|---|---|---|---|
| `CheckoutInitiated` | Student | "Checkout Started" | "Your order for {ItemCount} course(s) totalling {TotalAmount} {Currency} is being processed." |
| `PaymentSucceeded` | Student | "Payment Successful 🎉" | "Your payment of {Amount} {Currency} was successful. You are now enrolled in {CourseCount} course(s)." |
| `PaymentFailed` | Student | "Payment Failed ❌" | "Your payment of {Amount} {Currency} could not be processed. Please try again or use a different payment method." |

---

#### ENROLLMENT EVENTS

| Type | Recipient | Title | Message Template |
|---|---|---|---|
| `EnrollmentConfirmed` | Student | "Enrollment Confirmed ✅" | "You are now enrolled in **{CourseTitle}**. Start learning today!" |
| `NewStudentEnrolled` | Teacher | "New Student Enrolled 🎓" | "**{StudentName}** has just enrolled in your course **{CourseTitle}**." |
| `FreeEnrollmentConfirmed` | Student | "Enrolled for Free ✅" | "You have successfully enrolled in **{CourseTitle}** for free. Enjoy the course!" |

---

#### UNENROLLMENT EVENTS

| Type | Recipient | Title | Message Template |
|---|---|---|---|
| `UnenrollmentConfirmed` | Student | "Unenrollment Confirmed" | "You have been unenrolled from **{CourseTitle}**. A refund of {RefundAmount} {Currency} has been initiated and should arrive in 5–10 business days." |
| `UnenrollmentConfirmedNoRefund` | Student | "Unenrollment Confirmed" | "You have been unenrolled from **{CourseTitle}**. No refund was issued (course was free or refund was not applicable)." |
| `UnenrollmentDenied` | Student | "Unenrollment Not Allowed" | "Your request to unenroll from **{CourseTitle}** was denied. The 10-day unenrollment window has passed." |
| `StudentUnenrolled` | Teacher | "Student Unenrolled" | "**{StudentName}** has unenrolled from your course **{CourseTitle}**. A refund of {RefundAmount} was processed." |
| `PartialRefundIssued` | Student | "Partial Refund Issued" | "A partial refund of {RefundAmount} {Currency} has been issued for **{CourseTitle}** (you completed more than 50% of the course)." |

---

#### NEWFEATURE.md EVENTS

Read the `NewFeature.md` file in the project and for **every feature listed there** that involves a user action or state change:
```
1. Identify what event is triggered.
2. Identify who the recipient(s) are.
3. Create a notification type entry for it following the same format above.
4. Implement it in NotificationService.
```

Do not skip this. Every feature in NewFeature.md that changes data must have at least one corresponding notification.

---

### 7d. Notification API Endpoints

Verify these endpoints exist and work correctly. Create them if missing:

```
GET    /api/notifications               → Get all notifications for current user (paginated)
                                          Query params: ?page=1&pageSize=20&unreadOnly=false
GET    /api/notifications/unread-count  → Returns { Count: 5 }
PUT    /api/notifications/{id}/read     → Mark single notification as read
PUT    /api/notifications/read-all      → Mark all as read
DELETE /api/notifications/{id}          → Delete a notification
```

### 7e. Notification DTOs

```json
// GET /api/notifications response
{
  "Items": [
    {
      "Id": "...",
      "Type": "EnrollmentConfirmed",
      "Title": "Enrollment Confirmed ✅",
      "Message": "You are now enrolled in Intro To Embedded. Start learning today!",
      "IsRead": false,
      "RelatedEntityId": "...",
      "RelatedEntityType": "Course",
      "CreatedAt": "2026-03-01T22:17:15Z",
      "ReadAt": null
    }
  ],
  "UnreadCount": 3,
  "Page": 1,
  "PageSize": 20,
  "TotalCount": 12
}
```

---

## STEP 8 — FRONTEND UPDATES

### 8a. Cart UI

Create or update a Cart page/component:
- Display all cart items with course thumbnail, title, teacher name, and price.
- Show subtotal at the bottom.
- "Remove" button per item.
- "Clear Cart" button.
- "Proceed to Checkout" button that calls `POST /api/checkout`.

A cart icon in the header/navbar must show the current item count (badge).

### 8b. Checkout Page

After calling `POST /api/checkout`:
- If `RequiresPayment = false` (all free): show success message and redirect to enrolled courses.
- If `RequiresPayment = true`:
  - Load Stripe.js using the returned `PublishableKey`.
  - Initialize Stripe Elements (Card Element or Payment Element).
  - On form submit: call `stripe.confirmCardPayment(clientSecret, { payment_method: { card: cardElement } })`.
  - On success: poll `GET /api/checkout/{orderId}` until `Status = Paid`, then redirect to enrolled courses.
  - On failure: display Stripe's error message to the user.

### 8c. Unenrollment UI

On the student's enrolled course list or course detail page:
- Show an "Unenroll" button.
- On click: show a confirmation modal with the refund policy clearly stated:
  - "Unenrollment is available within 10 days of enrollment."
  - "If you have completed more than 50% of the course, only a 50% refund will be issued."
  - "If you have completed 50% or less, a full refund will be issued."
  - Show the current progress and the estimated refund amount before confirming.
- On confirm: call `DELETE /api/enrollments/{courseId}`.
- Display the result (success with refund amount, or denial reason).

### 8d. Notification Bell

Create or update the notification bell/dropdown in the header:
- Show unread count badge from `GET /api/notifications/unread-count`.
- On click: show dropdown with last 10 notifications.
- Each notification shows: icon (based on type), title, truncated message, timestamp (relative: "2 hours ago").
- "Mark all as read" button.
- "View all notifications" link to a full notifications page.
- New notifications from enrollment/payment events should appear here.

---

## STEP 9 — INTEGRATION VERIFICATION CHECKLIST

After implementing everything, verify the full chain for each feature:

| Feature | DB Schema | Entity | Repository | Service | Controller | DTO | Frontend | Notification Triggered |
|---|---|---|---|---|---|---|---|---|
| Cart: Add Course | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ |
| Cart: Remove Course | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ |
| Checkout: Create PaymentIntent | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ |
| Stripe Webhook: Payment Success | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ |
| Stripe Webhook: Payment Failed | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ |
| Free Course Enrollment | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ |
| Unenrollment: In Window + Full Refund | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ |
| Unenrollment: In Window + Partial Refund | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ |
| Unenrollment: Denied (past 10 days) | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ |
| Unenrollment: Free Course | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ |
| Notification: All Cart Events | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | N/A |
| Notification: All Payment Events | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | N/A |
| Notification: All Enrollment Events | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | N/A |
| Notification: All Unenrollment Events | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | ✓/✗ | N/A |

Fill in every cell. Fix any `✗` before running tests.

---

## STEP 10 — END-TO-END WORKFLOW TESTING

Run both servers. Execute every step below. Record result after each one.

### TEACHER SETUP

```
TS-01: Login as teacher (or register if needed).
TS-02: Create 2 courses — one paid (e.g. $49.99) and one free ($0).
TS-03: Add at least 3 lectures to each course.
TS-04: Publish both courses.
TS-05: Confirm both appear in GET /api/courses with correct Price field.
```

### STUDENT — FREE ENROLLMENT FLOW

```
SF-01: Login as student (or register).
SF-02: GET /api/courses — confirm free course shows Price = 0.
SF-03: POST /api/courses/{freeCourseId}/enroll — direct enroll.
       EXPECT: 200 OK, enrollment created, AmountPaid = 0, OrderId = null.
SF-04: GET /api/notifications — confirm "FreeEnrollmentConfirmed" notification exists.
SF-05: GET /api/notifications (teacher account) — confirm "NewStudentEnrolled" notification exists.
SF-06: GET /api/users/me/enrollments — confirm free course appears with ProgressPercentage = 0.
```

### STUDENT — PAID ENROLLMENT FLOW (Cart → Checkout → Stripe)

```
SP-01: POST /api/cart/items body: { CourseId: paidCourseId }
       EXPECT: 200 OK, cart returned with 1 item and correct price.
SP-02: GET /api/notifications — confirm "CourseAddedToCart" notification exists.
SP-03: GET /api/cart — confirm item is present with PriceAtTimeOfAdding populated.
SP-04: POST /api/cart/items (same courseId again)
       EXPECT: 400 error — duplicate item rejected.
SP-05: POST /api/checkout
       EXPECT: 200 OK, ClientSecret returned, RequiresPayment = true, TotalAmount = 49.99.
SP-06: GET /api/notifications — confirm "CheckoutInitiated" notification exists.
SP-07: Simulate Stripe payment using test card 4242 4242 4242 4242.
       OR: Manually trigger the webhook using Stripe CLI:
           stripe trigger payment_intent.succeeded
       EXPECT: Webhook returns 200, Order.Status = Paid, Enrollment created.
SP-08: GET /api/notifications (student) — confirm "PaymentSucceeded" and "EnrollmentConfirmed" notifications.
SP-09: GET /api/notifications (teacher) — confirm "NewStudentEnrolled" notification.
SP-10: GET /api/users/me/enrollments — confirm paid course appears with AmountPaid = 49.99, OrderId populated.
SP-11: GET /api/cart — EXPECT: cart is empty or in CheckedOut state.
```

### STUDENT — PAID ENROLLMENT VIA CART EDGE CASE

```
SE-01: Try POST /api/courses/{paidCourseId}/enroll (direct enroll on paid course).
       EXPECT: 400 error — "Paid courses must be enrolled via the checkout process."
SE-02: Try adding an already-enrolled course to the cart.
       EXPECT: 400 error — "You are already enrolled in this course."
SE-03: Try adding an unpublished course to the cart.
       EXPECT: 400 error — "This course is not available for enrollment."
```

### STUDENT — UNENROLLMENT FLOW

```
SU-01: Complete 1 of 3 lectures in the paid course (progress = 33%).
SU-02: DELETE /api/enrollments/{paidCourseId}
       EXPECT: 200 OK, RefundAmount = 49.99 (full refund, progress <= 50%).
       Confirm StripeRefundId is populated in response.
SU-03: GET /api/notifications (student) — confirm "UnenrollmentConfirmed" notification with refund amount.
SU-04: GET /api/notifications (teacher) — confirm "StudentUnenrolled" notification.
SU-05: Re-enroll in the paid course (repeat SP-01 to SP-10).
SU-06: Complete 2 of 3 lectures (progress = 66%).
SU-07: DELETE /api/enrollments/{paidCourseId}
       EXPECT: 200 OK, RefundAmount = 24.99 (50% refund, progress > 50%).
       Confirm "PartialRefundIssued" notification sent.
```

### STUDENT — UNENROLLMENT DENIAL TEST

```
SD-01: Manually update enrollment CreatedAt to 11 days ago in the database.
SD-02: DELETE /api/enrollments/{courseId}
       EXPECT: 400 error with message "The 10-day unenrollment window has passed."
       Confirm "UnenrollmentDenied" notification is sent.
```

### FAILED PAYMENT TEST

```
FP-01: POST /api/checkout (add a paid course to cart first).
FP-02: Simulate a failed payment using Stripe test card 4000 0000 0000 0002.
       OR: stripe trigger payment_intent.payment_failed
FP-03: GET /api/notifications (student) — confirm "PaymentFailed" notification.
FP-04: GET /api/users/me/enrollments — confirm NO enrollment was created.
FP-05: GET /api/orders/{orderId} — confirm Order.Status = Failed.
```

### NOTIFICATION SYSTEM TESTS

```
NT-01: GET /api/notifications — confirm all previously triggered notifications are present.
NT-02: GET /api/notifications/unread-count — confirm count matches unread notifications.
NT-03: PUT /api/notifications/{id}/read — mark one as read.
NT-04: GET /api/notifications/unread-count — confirm count decreased by 1.
NT-05: PUT /api/notifications/read-all — mark all as read.
NT-06: GET /api/notifications/unread-count — confirm count = 0.
```

---

## STEP 11 — REPORT

After all tests are complete, produce a report in this exact format:

```markdown
## ENROLLMENT REVAMP TEST REPORT

### ✅ PASSED
For each passing test, list:
- Test ID (e.g., SP-07)
- Endpoint called
- HTTP status returned
- What was verified (one sentence)

### ❌ FAILED
For each failing test, list:
- Test ID
- Endpoint called
- Expected result
- Actual result (exact error, wrong field, missing data)
- Root cause (file and method where bug originates)
- Fix applied (if auto-fixable) OR Action required (if needs developer)

### ⚠️ REQUIRES DEVELOPER ATTENTION
For each issue you could NOT fix:
- Clear problem description
- File and line reference
- What the developer must do
- Complexity: Low / Medium / High

### 🔧 FIXES APPLIED DURING TESTING
For each fix you applied:
- File modified (with path)
- What changed
- Confirmed working after fix: Yes / No

### 🔔 NOTIFICATION COVERAGE REPORT
List every notification type defined in Step 7c.
For each one:
- Was it triggered during testing? Yes / No
- Did it appear in GET /api/notifications? Yes / No
- Was the message content correct? Yes / No / Not Tested

### 📋 KNOWN GAPS & REMAINING WORK
List anything that is architecturally incomplete, even if not failing:
- Example: "Stripe webhook retry handling is not idempotent if DB write fails mid-transaction."
- Example: "Cart does not expire — abandoned carts will accumulate indefinitely."
- Example: "Refund amount is not validated against actual Stripe charge amount."
```

---

## STEP 12 — ENDPOINT & DTO AUDIT: CLEANUP AFTER MODIFICATIONS

This step runs **after** all implementation and testing in Steps 2–11 are complete. Its purpose is to ensure the API surface is clean, consistent, and free of dead or outdated code. Work through every section below methodically.

---

### 12a. ENUMERATE ALL ENDPOINTS

List every single route in the project across all controllers. For each route, record:

```
Method | Route | Controller | Action Method | DTO In (Request) | DTO Out (Response)
```

Example output:
```
POST   | /api/cart/items              | CartController      | AddCourseToCart   | AddToCartRequest    | CartDto
DELETE | /api/enrollments/{courseId}  | EnrollmentController| Unenroll          | —                   | UnenrollmentResultDto
POST   | /api/courses/{id}/enroll     | EnrollmentController| DirectEnroll      | —                   | EnrollmentDto
```

Do this for the entire API. Do not skip any controller.

---

### 12b. FLAG ENDPOINTS MADE OBSOLETE BY THE REVAMP

After the Cart → Checkout → Stripe flow is implemented, certain old endpoints may now be:

- **Fully replaced** by new ones (e.g., direct paid enrollment replaced by cart + checkout).
- **Narrowed in scope** (e.g., direct enroll now only valid for free courses).
- **Duplicated** by a new endpoint that does the same thing better.
- **Orphaned** — they exist but nothing calls them anymore (frontend or other services).

For each endpoint in the list from 12a, answer:

```
Is this endpoint still needed?
  YES → keep it, verify it works and its DTO is up to date.
  NO  → mark for removal or deprecation.
  CHANGED SCOPE → update its behavior, DTO, and documentation.
```

Specific endpoints to scrutinize hard:

| Endpoint | Question to Answer |
|---|---|
| `POST /api/courses/{id}/enroll` | Does it now reject paid courses? Is the DTO updated to reflect that? |
| `DELETE /api/enrollments/{courseId}` (old) | Was there a previous unenroll endpoint? Is it fully replaced by the new one with refund logic? |
| Any direct payment endpoint outside `/api/checkout` | Is it still needed or replaced by the new checkout flow? |
| Any admin enrollment override endpoint | Does it bypass the cart? Is this intentional? Does it need an OrderId or not? |
| Any enrollment count or enrollment list endpoint | Does its DTO now include `AmountPaid`, `ProgressPercentage`, `RefundedAt`, etc.? |

---

### 12c. AUDIT ALL REQUEST DTOs (INPUT)

For every request DTO (used as a body or query parameter in an endpoint), check:

**Remove fields that are now obsolete:**
- Any field that was used to pass payment info before Stripe was integrated (e.g., a manual `Amount` or `PaymentMethod` field on an enrollment request).
- Any field that duplicates data now calculated server-side (e.g., if the client used to send `TotalAmount` and now the server calculates it from cart items).
- Any `EnrollmentType` or `IsPaid` flag that is now derived from `Course.Price`.

**Add fields that are now required but missing:**
- `CategoryId` on `CreateCourseRequest` and `UpdateCourseRequest` if not already present.
- `Price` on `CreateCourseRequest` and `UpdateCourseRequest` if not already present.
- Any field in the new profile update DTO that is missing from the request body.

**Validate constraints are declared:**
- Required fields have `[Required]` or equivalent validation attribute.
- `Price` has `[Range(0, double.MaxValue)]` — cannot be negative.
- `CategoryId` references a valid category (validated in service layer if not via attribute).
- String fields have `[MaxLength]` where appropriate.

---

### 12d. AUDIT ALL RESPONSE DTOs (OUTPUT)

For every response DTO, check the following:

**Remove fields that no longer make sense:**
- Any field that was part of the old direct-enrollment flow that the new system makes irrelevant.
- Any `PaymentStatus` field that is now correctly part of `OrderDto` and should not be duplicated on `EnrollmentDto`.
- Redundant fields that appear in both a parent and nested child DTO (e.g., `CourseId` repeated where `CourseDetails` is already embedded).

**Add fields that should now be present:**
- `CategoryId` and `CategoryName` on every DTO that returns course data:
  - `CourseListItemDto`
  - `CourseDetailsDto`
  - `EnrolledCourseDto`
  - `TeacherCourseDto`
  - `CartItemDto`
  - `OrderItemDto`
- `Price` and `IsFree` on every DTO that returns course data (same list as above).
- `AmountPaid`, `RefundedAt`, `RefundAmount`, `UnenrolledAt` on `EnrollmentDto`.
- `ProgressPercentage`, `CompletedLectures`, `TotalLectures`, `IsCompleted`, `LastAccessedAt` on `EnrollmentDto` and `EnrolledCourseDto`.
- `OrderId` on `EnrollmentDto` (nullable — null for free courses).
- `StripePaymentIntentId` on `OrderDto` (for frontend to complete payment).
- All teacher dashboard metrics defined in the previous prompt's Step 2d.
- All student stats metrics defined in the previous prompt's Step 2e.

**Check for data leaks — remove fields that should never be public:**
- Never expose `StripeSecretKey`, `WebhookSecret`, or any internal Stripe configuration.
- Never expose raw database IDs for internal join tables (e.g., `CartItem.CartId` does not need to appear in the API response if `CartDto` already wraps the items).
- Never expose `PasswordHash`, `PasswordSalt`, or any credential field on any user-related DTO.
- Never expose `RefundedAt` or `StripeRefundId` to the teacher — only to the student who made the purchase.

---

### 12e. CHECK DTO NAMING CONSISTENCY

Go through all DTO names and enforce a consistent naming convention across the project:

```
Request DTOs  → end with "Request"   (e.g., CreateCourseRequest, AddToCartRequest)
Response DTOs → end with "Dto"       (e.g., CourseDto, CartDto, OrderDto)
List wrappers → end with "ListDto" or use the existing pagination wrapper consistently
```

Rename any DTO that violates this convention. Update all references (controllers, services, AutoMapper profiles, frontend API service files) after renaming.

---

### 12f. CHECK AUTOMAPPER PROFILES (OR MANUAL MAPPINGS)

For every new DTO field added in this prompt and the previous one:

```
1. Find the AutoMapper profile (or manual mapping method) that maps the entity to this DTO.
2. Confirm the new field is explicitly mapped.
3. If the field requires a JOIN or computed value (e.g., CategoryName from a related table, ProgressPercentage computed from lecture completions), confirm the query in the repository/service fetches that data before the mapping runs.
4. If any old mapping maps a field that no longer exists on the entity or DTO, remove that mapping line.
```

An unmapped field silently returns `null` or `0` — this is the most common source of "field exists in DTO but is always empty" bugs. Verify every field end-to-end.

---

### 12g. VERIFY RESPONSE SHAPE CONSISTENCY

Every API endpoint in this project must return responses in a consistent envelope shape. Verify that all endpoints — old and new — wrap their data consistently:

```json
{
  "Success": true,
  "Data": { ... },
  "Message": null
}
```

Or for errors:
```json
{
  "Success": false,
  "Data": null,
  "Message": "Descriptive error message here."
}
```

Check every controller action:
- Are there any that return raw objects without the envelope?
- Are there any that return `200 OK` with `Success: false`? (Should be a 4xx status code.)
- Are there any that return `201 Created` without a `Location` header when creating a resource?
- Are error messages user-friendly (not stack traces, not raw EF exceptions)?

Fix all inconsistencies found.

---

### 12h. DEAD CODE REMOVAL

After the revamp:

```
1. Check for any Service methods that are no longer called by any controller or other service.
   → Delete them.

2. Check for any Repository methods that are no longer called by any service.
   → Delete them.

3. Check for any DTO classes that are no longer used anywhere.
   → Delete them.

4. Check for any old enrollment-related migration logic or seed data that contradicts the new schema.
   → Remove or update it.

5. Check the frontend API service files (e.g., enrollmentApi.ts, courseApi.ts) for any functions
   that called the old direct-enrollment endpoint and are now replaced by cart/checkout calls.
   → Remove or redirect them.
```

Before deleting anything, do a full-text search for the class/method name to confirm it has zero remaining usages. Do not delete anything with active references.

---

### 12i. PRODUCE THE FINAL AUDIT REPORT

After completing 12a–12h, append the following section to the test report from Step 11:

```markdown
## ENDPOINT & DTO AUDIT REPORT

### 📋 ENDPOINT INVENTORY
Total endpoints found: [N]
Endpoints kept unchanged: [N]
Endpoints updated (scope or DTO change): [N] — list each with what changed
Endpoints removed (obsolete): [N] — list each with reason for removal
New endpoints added: [N] — list each

### 🗑️ REMOVED / DEPRECATED
For each removed endpoint or DTO:
- What it was
- Why it was removed
- What replaced it (if anything)
- Confirmed no remaining references: Yes / No

### 🔧 DTO CHANGES SUMMARY
For each DTO that was modified:
- DTO name
- Fields added (with type and reason)
- Fields removed (with reason)
- Fields renamed (old name → new name)
- Mapping updated: Yes / No
- Confirmed returning correct data in API response: Yes / No

### ⚠️ DATA LEAK RISKS FOUND
List any fields that were found exposed but should not be, even if already fixed:
- DTO name, field name, risk description, fixed: Yes / No

### 🧹 DEAD CODE REMOVED
- List every service method, repository method, or DTO class deleted
- Confirmed zero references before deletion: Yes / No

### ✅ CONSISTENCY CHECKS
- All endpoints use consistent response envelope: Yes / No / Partially (list exceptions)
- All DTOs follow naming convention: Yes / No / Partially (list exceptions)
- All new DTO fields are mapped in AutoMapper/manual mapping: Yes / No / Partially (list gaps)
```

---

## EXECUTION RULES FOR THE AGENT

0. **Step 12 is mandatory.** The endpoint and DTO audit must run after all implementation is complete. Do not skip it because the system "seems to be working." Leftover dead code, stale DTO fields, and exposed internal fields are bugs and security risks. The audit report from Step 12 is a required deliverable.
1. **Read before writing.** Always inspect existing code before adding or changing anything. Never duplicate logic that already exists.
2. **Migrations before code.** Schema changes go first. Service code that depends on new columns must come after the migration is confirmed to have run.
3. **Stripe is external.** Never assume Stripe calls succeed. Always handle exceptions from Stripe API calls and return meaningful errors.
4. **Webhooks are idempotent by requirement.** The payment webhook will be called more than once by Stripe. Guard every state transition: check current status before writing, never process the same event twice.
5. **Notifications are fire-and-forget.** Never let a notification failure block a business operation. Wrap notification calls in try-catch and log failures separately.
6. **Test with Stripe test cards only.** Never use real card numbers. Use `4242 4242 4242 4242` for success and `4000 0000 0000 0002` for decline.
7. **Report honestly.** If a test fails and you cannot fix it, say so clearly. Do not mark something as passed if it returned an unexpected result.
8. **Do not break existing features.** After each change, verify that previously working endpoints still return correct responses.
9. **Read NewFeature.md completely** before writing any notification code. Every feature in that file that triggers a state change needs a notification.