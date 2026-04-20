# Agent Task List — E-Learning Frontend: Bug Fixes & UI/UX Enhancements

## Context

- **Stack:** React, Vite, Zustand, react-router-dom, Axios, Recharts, react-leaflet
- **Language:** TypeScript (or JavaScript if no TS config present)
- **Constraint:** Do not modify the backend API contracts, routing library, or Zustand store structure. All changes must be backward-compatible. Run the app and confirm zero console errors after each task group.

Work through the tasks below **in order**. For each task:
1. Locate the relevant file(s).
2. Apply the exact change described.
3. Verify the acceptance criteria before moving to the next task.

---

## TASK GROUP 1 — Critical Bug Fixes

---

### TASK 1.1 — Fix broken image loading

**Files to check:** All components rendering `<img>` tags or CSS `background-image`. Likely suspects: `CourseCard.tsx`, `EnrollmentCard.tsx`, `UserAvatar.tsx`, `CourseBanner.tsx`.

**Steps:**
1. Search the codebase for all `<img src=` and `background-image: url(` usages.
2. For each one, check whether the `src` value is:
   - A relative path missing the Vite `import` or `VITE_API_BASE_URL` prefix.
   - A dynamic API URL that needs the base URL prepended.
   - A nullable value being rendered without a fallback.
3. Apply fixes:
   - Prepend `import.meta.env.VITE_API_BASE_URL` (or equivalent env var) to all API-sourced image URLs.
   - Add an `onError` fallback to every `<img>` tag pointing to a local placeholder asset.
   - Never render `<img src={undefined} />` — guard with `src={imageUrl ?? '/placeholder.png'}`.

**Acceptance criteria:**
- All course thumbnails, user avatars, and material previews render without broken image icons.
- No `404` image requests in the network tab.

---

### TASK 1.2 — Fix silent payment failures

**File:** The Stripe payment handler component (likely `PaymentForm.tsx`, `CheckoutPage.tsx`, or similar).

**Steps:**

1. After `stripe.confirmCardPayment(...)` resolves, check:
```ts
if (result.error || result.paymentIntent?.status !== 'succeeded') {
  showNotification({ type: 'error', message: result.error?.message ?? 'Payment failed. Please try again.' });
  return;
}
```
2. Only after confirming `status === 'succeeded'`, call the enrollment API endpoint.
3. Check the enrollment API response:
```ts
const enrollRes = await api.post(`/enrollments`, { courseId });
if (!enrollRes.data?.enrolled) {
  showNotification({
    type: 'error',
    message: 'Payment was processed but enrollment failed. Please contact support.',
  });
  return;
}
```
4. Only after both checks pass, show the success notification.

**Acceptance criteria:**
- "Payment Successful!" is never shown unless `paymentIntent.status === 'succeeded'` AND the enrollment API returns a successful enrollment.
- Every failure branch shows a specific, actionable error message.

---

### TASK 1.3 — Fix lecture count showing 0/0 on enrollment page

**File:** `EnrollmentPage.tsx` or `EnrolledCourseCard.tsx`.

**Steps:**
1. Locate where `lectureCount` and `completedLectures` are read from state or API response.
2. Log the raw API response for an enrolled course and identify the correct field names.
3. Map the response fields correctly:
```ts
const totalLectures = course.totalLectures ?? course.lectureCount ?? 0;
const completedLectures = course.completedLectures ?? course.progress?.completedLectures ?? 0;
```
4. Display as: `{completedLectures}/{totalLectures} lectures`

**Acceptance criteria:**
- Each enrolled course card shows the real lecture counts, not `0/0`.

---

### TASK 1.4 — Fix progress bar disappearing at 0%

**File:** The shared `ProgressBar.tsx` component (or wherever it is defined).

**Steps:**
1. Find the condition that hides or returns `null` when progress is `0`. Remove or fix it.
2. Ensure the bar always renders with a minimum visible track:
```tsx
<div className="progress-track" style={{ width: '100%', height: '8px', backgroundColor: '#e5e7eb', borderRadius: '4px' }}>
  <div
    className="progress-fill"
    style={{ width: `${Math.max(0, Math.min(100, progress))}%`, height: '100%', backgroundColor: '#3b82f6', borderRadius: '4px' }}
  />
</div>
```
3. Never conditionally hide the track — only the fill width changes.

**Acceptance criteria:**
- A course at 0% progress shows a full-width empty track bar.
- A course at 100% shows a fully filled bar.

---

## TASK GROUP 2 — Notification System Overhaul

---

### TASK 2.1 — Convert notification button to a popup

**Files:** `NotificationBell.tsx` (or equivalent), `NotificationsPage.tsx`, the router config.

**Steps:**
1. In `NotificationBell.tsx`, add local state: `const [open, setOpen] = useState(false)`.
2. On bell icon click: `setOpen(prev => !prev)`.
3. When `open` becomes `true`, call `api.post('/notifications/mark-all-read')` immediately.
4. Render a dropdown panel (position: absolute, z-index high) beneath the bell:
   - List the 5 most recent notifications.
   - Each item shows: icon, message text, timestamp (relative, e.g., "2 min ago").
   - At the bottom of the panel, render a button: `"View All"` — clicking it navigates to the full `/notifications` page.
5. Attach a `useEffect` that adds a `mousedown` event listener to `document` and closes the popup when the click target is outside the panel ref:
```ts
useEffect(() => {
  const handler = (e: MouseEvent) => {
    if (panelRef.current && !panelRef.current.contains(e.target as Node)) setOpen(false);
  };
  document.addEventListener('mousedown', handler);
  return () => document.removeEventListener('mousedown', handler);
}, []);
```
6. Do **not** remove the `NotificationsPage` route — the "View All" button still links to it.

**Acceptance criteria:**
- Clicking the bell opens a dropdown panel in place, no page navigation.
- Clicking outside closes the panel.
- Opening the panel marks all notifications as read via the API.
- "View All" navigates to the full notifications page.

---

### TASK 2.2 — Parse and surface specific API error messages

**File:** Your Axios instance configuration file (e.g., `api.ts`, `axiosInstance.ts`).

**Steps:**
1. Add a response error interceptor:
```ts
axiosInstance.interceptors.response.use(
  (res) => res,
  (error) => {
    const data = error.response?.data;

    // Extract a human-readable message from common error shapes
    const message =
      data?.errors?.[0]?.message ||   // FluentValidation array shape
      data?.message ||                 // Generic { message: string }
      data?.detail ||                  // RFC 7807 shape
      data?.title ||                   // ASP.NET ProblemDetails
      'An unexpected error occurred.';

    error.userMessage = message;
    return Promise.reject(error);
  }
);
```
2. In every `catch` block across the codebase that currently calls `showNotification({ message: 'Failed to ...' })`, replace the hardcoded string with `error.userMessage`:
```ts
catch (error: any) {
  showNotification({ type: 'error', message: error.userMessage ?? 'Failed to complete the action.' });
}
```

**Acceptance criteria:**
- Creating an exam with `startTime` in the past shows: *"Start time must be in the future."*
- No UI interaction that fails at the API level shows only a generic hardcoded string.

---

### TASK 2.3 — Add frontend validation matching API rules

**File:** `ExamForm.tsx` (and then apply the same pattern to all other forms).

**Steps:**
1. Install `react-hook-form` and `zod` if not already present: `npm install react-hook-form zod @hookform/resolvers`.
2. Define a Zod schema for exam creation:
```ts
const examSchema = z.object({
  title: z.string().min(1, 'Title is required'),
  startTime: z.date().refine((d) => d > new Date(), { message: 'Start time must be in the future' }),
  endTime: z.date(),
}).refine((data) => data.endTime > data.startTime, {
  message: 'End time must be after start time',
  path: ['endTime'],
});
```
3. Wire the schema to `useForm` via `zodResolver`.
4. Display field-level errors inline beneath each input using `formState.errors`.
5. The form must not call the API at all if `isValid` is false.
6. Repeat the same pattern for: course creation form, material upload form.

**Acceptance criteria:**
- Submitting an exam form with `startTime` in the past shows an inline field error and does not fire any API request.
- Submitting with `endTime` before `startTime` shows an inline field error on the `endTime` field.

---

## TASK GROUP 3 — Background Service Failure Feedback

---

### TASK 3.1 — Notify teacher on tag extraction failure

**File:** Wherever the course/material creation API response is handled after upload (e.g., `CourseCreatePage.tsx`, `MaterialUploadForm.tsx`).

**Steps:**
1. After a successful course or material creation, check the response for a tag extraction status field (e.g., `response.data.tagExtractionStatus === 'failed'` — confirm field name from the actual API response).
2. If the status indicates failure, call:
```ts
showNotification({
  type: 'warning',
  message: `Tag extraction failed for "${courseName}". This may reduce course discoverability and engagement.`,
  persistent: true, // do not auto-dismiss
});
```
3. Ensure your notification system supports a `persistent: true` flag — if it doesn't, add that option to the notification component.

**Acceptance criteria:**
- When tag extraction fails, a persistent warning notification appears. It does not auto-dismiss.
- The notification message references the specific course/material name.

---

### TASK 3.2 — Notify teacher on AI indexing failure and mark materials

**Files:** Course/material creation handlers, `MaterialList.tsx` or `LectureItem.tsx`, AI study session entry component.

**Steps:**

**A. Course-level indexing failure notification:**
```ts
if (response.data.indexingStatus === 'failed') {
  showNotification({
    type: 'warning',
    message: `Indexing failed for "${courseName}". Students cannot use this content in AI study sessions.`,
    persistent: true,
  });
}
```

**B. Material-level indexing failure:**
1. When a material's indexing fails, update its record in local state to set `isAiIndexed: false`.
2. In `MaterialList.tsx` or wherever materials are listed in the course management view, render a warning badge on any material where `isAiIndexed === false`:
```tsx
{!material.isAiIndexed && (
  <span className="badge badge-warning" title="AI indexing failed for this material">
    ⚠ Not available for AI study
  </span>
)}
```

**C. Block AI study session access for unindexed materials:**
1. In the AI study session component, before loading a material, check `material.isAiIndexed`.
2. If `false`, render:
```tsx
<div className="ai-unavailable-notice">
  This material is not available for AI study sessions due to an indexing failure.
</div>
```
And do not proceed to load the AI session.

**Acceptance criteria:**
- Materials with `isAiIndexed: false` show a visible warning badge in course management.
- Attempting to start an AI study session on an unindexed material shows the notice and blocks the session.

---

## TASK GROUP 4 — Layout & Navigation Restructure

---

### TASK 4.1 — Convert dashboard to full-width page and replace sidebar with navbar

**Files:** `App.tsx` or root layout component, `Sidebar.tsx`, `Navbar.tsx` (create if missing), `Dashboard.tsx`.

**Steps:**
1. In the root layout, remove the sidebar column from the flex/grid layout.
2. Create (or update) `Navbar.tsx` as a full-width top bar containing:
   - App logo/name (left).
   - Navigation links — exactly the same links currently in the sidebar (center or left group).
   - User avatar + notification bell (right).
3. Wrap all page content in a `<main>` that is `width: 100%` with no left offset or margin previously reserved for the sidebar.
4. `Dashboard.tsx` must render as `width: 100%` with no max-width constraint that was sized for a partial panel.

**Acceptance criteria:**
- No sidebar is visible anywhere in the app.
- The navbar appears at the top of every page.
- Dashboard occupies the full page width.

---

## TASK GROUP 5 — Enrollment Page Visual Overhaul

---

### TASK 5.1 — Redesign enrolled course cards

**File:** `EnrollmentPage.tsx`, `EnrolledCourseCard.tsx` (create the card as a separate component if it isn't already).

**Steps:**
1. Each card must display:
   - Course thumbnail `<img>` (with fallback).
   - Course title (bold, prominent).
   - Instructor name (muted subtext).
   - Progress bar (from Task 1.4, showing real % from Task 1.3).
   - `{completedLectures}/{totalLectures} lectures` label.
2. Remove the standalone "Go to Course" button. Instead, wrap the entire card in:
```tsx
<div onClick={() => navigate(`/courses/${course.id}`)} style={{ cursor: 'pointer' }}>
  {/* card content */}
</div>
```
3. Add a hover state: subtle box-shadow lift on hover using CSS transition.
4. Use a responsive grid layout: `grid-template-columns: repeat(auto-fill, minmax(280px, 1fr))`.

**Acceptance criteria:**
- Clicking anywhere on an enrolled course card navigates to the course detail page.
- Course thumbnails are visible on all cards.
- No "Go to Course" button exists as a separate element.

---

## TASK GROUP 6 — Course Detail & Lecture UX

---

### TASK 6.1 — Redesign lecture list inside course detail

**File:** `CourseDetailPage.tsx`, `LectureList.tsx` or `LectureItem.tsx`.

**Steps:**
1. Replace plain `<li>` or `<p>` text lecture rows with styled cards:
   - Left: icon indicating material type — use `react-icons` (`npm install react-icons`) or inline SVG. Map types: `video → 📹`, `pdf → 📄`, `quiz → 📝`.
   - Center: lecture title and subtitle (e.g., number of materials).
   - Right: checkmark icon if `lecture.isCompleted === true`, else an empty circle.
2. Add a hover background highlight on each row.
3. Add a smooth transition on hover: `transition: background 0.15s ease`.

**Acceptance criteria:**
- Each lecture row shows a type icon and a completion indicator.
- No lecture row is unstyled plain text.

---

### TASK 6.2 — Auto-select first material when a lecture is clicked

**File:** `CourseDetailPage.tsx` or the lecture viewer component.

**Steps:**
1. Locate the `onClick` handler for lecture selection.
2. When a lecture is selected, immediately also set the active material to the first item:
```ts
const handleLectureClick = (lecture: Lecture) => {
  setActiveLecture(lecture);
  setActiveMaterial(lecture.materials?.[0] ?? null);
};
```
3. Ensure the material viewer component loads the material immediately when `activeMaterial` is set — there must be no intermediate empty/blank state.

**Acceptance criteria:**
- Clicking a lecture immediately loads its first material in the viewer.
- No blank content area is ever shown after clicking a lecture.

---

## TASK GROUP 7 — General UI Polish

---

### TASK 7.1 — Add loading skeletons for async content

**Files:** Any page that fetches data and currently renders nothing or a spinner while loading.

**Steps:**
1. Install `react-loading-skeleton` if not present: `npm install react-loading-skeleton`.
2. For any list or card grid that waits on an API call, render skeleton placeholders during the loading state:
```tsx
{isLoading
  ? Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} height={200} borderRadius={8} />)
  : courses.map(course => <CourseCard key={course.id} course={course} />)
}
```
3. Apply to: enrollment page, course detail page, lecture list.

---

### TASK 7.2 — Add empty state components

**Files:** `EnrollmentPage.tsx`, any list page that can have zero results.

**Steps:**
1. When a fetched list is empty (and not loading), render a centered empty state block:
```tsx
{!isLoading && courses.length === 0 && (
  <div style={{ textAlign: 'center', padding: '64px 0', color: '#9ca3af' }}>
    <img src="/empty-state.svg" alt="No courses" style={{ width: 120, marginBottom: 16 }} />
    <p style={{ fontSize: '1.1rem' }}>You haven't enrolled in any courses yet.</p>
  </div>
)}
```
2. Provide a relevant placeholder SVG or use a free illustration from `undraw.co`.

---

### TASK 7.3 — Typography and spacing consistency

**File:** Global CSS / Tailwind config / CSS module base styles.

**Steps:**
1. Define and enforce a type scale. If using plain CSS:
```css
h1 { font-size: 2rem;   font-weight: 700; }
h2 { font-size: 1.5rem; font-weight: 600; }
h3 { font-size: 1.2rem; font-weight: 600; }
p, li { font-size: 1rem; line-height: 1.6; color: #374151; }
.muted { color: #9ca3af; font-size: 0.875rem; }
```
2. Remove any inline `style={{ fontSize: '...' }}` overrides that deviate from the scale.
3. Ensure all pages use consistent horizontal padding (`padding: 0 24px` on mobile, `0 48px` on desktop via media query).

**Acceptance criteria:**
- All pages use the same font sizes for equivalent content roles (page title, card title, body text, metadata).
- No page has content flush against the viewport edge on mobile.

---

## Hard Constraints (apply to every task)

```
1. Do NOT change the backend API endpoint paths or request/response shapes.
2. Do NOT change the routing library or route structure (only add routes if strictly necessary).
3. Do NOT replace or restructure the Zustand store — only add new slices/actions if needed.
4. After completing each Task Group, run the dev server and confirm:
   - Zero React console errors.
   - Zero broken network requests.
   - All pre-existing features still function.
5. Do not introduce a UI library that conflicts with the existing styling system.
   Approved additions: react-hook-form, zod, react-loading-skeleton, react-icons, framer-motion.
```