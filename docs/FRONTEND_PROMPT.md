# React Frontend Development Prompt

> Use this document as a comprehensive prompt/specification to build the entire React frontend for AIEduPlatform. It describes every screen, user flow, and interaction pattern based on the API and user journey.

---

## Project Overview

Build a **React Single-Page Application (SPA)** for **AIEduPlatform** — an AI-powered educational platform. Two user roles exist (**Student** and **Teacher**, which can coexist on the same account) plus an **Admin** role. The app integrates with a .NET REST API backend that uses JWT authentication.

**Key Features:**
- Course browsing, enrollment, and learning
- Unified Studio + Material Viewer page (Google NotebookLM-style)
- Sectioned material viewer (PDF, video, audio) with per-section quiz / summarize / flashcard actions
- Progress tracking (scroll-based for PDF, time-based for video/audio)
- AI-powered study sessions (chat with SSE streaming, flashcards, mind maps, quizzes, summaries, dialogue audio)
- Exam taking and submission
- AI-assisted grading and grade management
- Real-time notifications via SignalR
- Student engagement monitoring for teachers
- Switchable AI providers (Ollama/Groq)
- Material streaming (video, audio, PDF, images)

---

## Tech Stack & Libraries

### Core

| Library | Version | Purpose |
|---------|---------|---------|
| `react` | ^18 or ^19 | UI framework |
| `react-dom` | ^18 or ^19 | DOM rendering |
| `react-router-dom` | ^6 or ^7 | Client-side routing |
| `typescript` | ^5 | Type safety |
| `vite` | ^5 or ^6 | Build tool & dev server |

### State Management & Data Fetching

| Library | Purpose |
|---------|---------|
| `@tanstack/react-query` | Server state management, caching, pagination |
| `zustand` or `@reduxjs/toolkit` | Client state (auth, UI state, notifications) |
| `axios` | HTTP client with interceptors for auth |

### UI & Styling

| Library | Purpose |
|---------|---------|
| `tailwindcss` | Utility-first CSS |
| `@shadcn/ui` (or `@radix-ui/react-*`) | Accessible, composable UI components |
| `lucide-react` | Icon library |
| `clsx` + `tailwind-merge` | Conditional class names |
| `react-hot-toast` or `sonner` | Toast notifications |
| `framer-motion` | Animations & transitions |

### Specialized Features

| Library | Purpose |
|---------|---------|
| `@microsoft/signalr` | Real-time SignalR hub connections |
| `react-pdf` or `@react-pdf-viewer/core` | PDF document viewer |
| `reactflow` or `react-d3-tree` | Mind map visualization |
| `react-markdown` + `remark-gfm` | Render markdown AI responses |
| `highlight.js` or `prism-react-renderer` | Code syntax highlighting in AI chat |
| `recharts` or `chart.js` + `react-chartjs-2` | Charts for analytics & grade distribution |
| `date-fns` or `dayjs` | Date formatting & manipulation |
| `jwt-decode` | Decode JWT tokens for role/claims extraction |
| `zod` | Schema validation for forms |
| `react-hook-form` | Form handling with validation |

### Development

| Library | Purpose |
|---------|---------|
| `eslint` + `@typescript-eslint/*` | Linting |
| `prettier` | Code formatting |
| `@testing-library/react` | Component testing |
| `vitest` | Test runner |
| `msw` | API mocking for tests |

---

## Project Structure

```
src/
├── api/                          # API client layer
│   ├── client.ts                 # Axios instance with interceptors
│   ├── auth.api.ts               # Auth endpoints (register/student, register/teacher, verify-email, login, logout, refresh)
│   ├── courses.api.ts            # Course CRUD endpoints
│   ├── categories.api.ts         # Category CRUD + course-category associations
│   ├── cart.api.ts               # Shopping cart endpoints
│   ├── checkout.api.ts           # Checkout & order status
│   ├── enrollments.api.ts        # Enrollment endpoints
│   ├── lectures.api.ts           # Lecture endpoints
│   ├── materials.api.ts          # Material upload/stream/download
│   ├── exams.api.ts              # Exam endpoints
│   ├── questions.api.ts          # Question endpoints
│   ├── submissions.api.ts        # Submission endpoints
│   ├── grades.api.ts             # Grading endpoints
│   ├── reviews.api.ts            # Review endpoints
│   ├── notifications.api.ts      # Notification list, mark-read, delete
│   ├── sections.api.ts           # Semantic sections endpoints
│   ├── studySessions.api.ts      # Study session + AI tools
│   ├── aiProvider.api.ts         # AI provider switch
│   └── dialogue.api.ts           # Dialogue/audio config
│
├── components/                   # Reusable UI components
│   ├── ui/                       # Base components (Button, Input, Card, Modal, etc.)
│   ├── layout/                   # Layout components
│   │   ├── AppLayout.tsx         # Main app layout with sidebar/nav
│   │   ├── Navbar.tsx
│   │   ├── Sidebar.tsx
│   │   └── Footer.tsx
│   ├── auth/                     # Auth-related components
│   │   ├── LoginForm.tsx
│   │   ├── RegisterForm.tsx
│   │   └── ProtectedRoute.tsx
│   ├── courses/
│   ├── study/                        # Studio chat, flashcards, mindmap, quiz, summary, dialogue
│   ├── viewer/                       # Material Viewer (PDF, Video, Audio viewers, section actions)
│   ├── exams/
│   ├── grades/
│   ├── materials/
│   ├── engagement/
│   └── notifications/
│
├── hooks/                        # Custom React hooks
│   ├── useAuth.ts
│   ├── useSignalR.ts
│   ├── useSSEChat.ts
│   ├── usePagination.ts
│   ├── useDebounce.ts
│   └── useMediaStream.ts
│
├── pages/                        # Route-level page components
│   ├── public/                   # LandingPage, LoginPage, RegisterPage, CourseCatalogPage
│   ├── student/                  # Dashboard, Enrollments, Learning, StudioPage (unified viewer+session), Exams, Grades
│   └── teacher/                  # Dashboard, CourseManagement, Lectures, Exams, Grading, Engagement, AIProvider
│
├── stores/                       # Zustand/Redux stores
│   ├── authStore.ts              # Auth tokens, user info, roles
│   ├── notificationStore.ts      # SignalR notification state
│   └── uiStore.ts                # Sidebar, theme, modals
│
├── types/                        # TypeScript type definitions
├── utils/                        # Utility functions (jwt, formatters, validators, constants)
├── App.tsx                       # Root component with router
├── main.tsx                      # Entry point
└── index.css                     # Tailwind imports
```

---

## Application Flow

This section describes the complete app flow from the user's perspective — what happens on each screen, what actions the user can take, and how the system responds.

### Flow 1: Landing & Onboarding

1. **User visits the app** → The landing page displays a hero section explaining the platform, feature highlights (AI study tools, smart grading, engagement tracking), and prominent "Get Started" / "Login" buttons.
2. **User clicks "Get Started"** → Navigates to the registration page.
3. **User fills out registration form** (email, username, password, confirm password, full name — teachers also provide a bio) → On submit, the app calls `POST /api/auth/register/student` or `POST /api/auth/register/teacher`. On success, the user sees a message to check their email for a verification link.
4. **User clicks verification link in email** → The link hits `GET /api/auth/verify-email?Token=...&Email=...`. After verification, the user is redirected to the login page.
5. **User fills out login form** (email, password) → On submit, the app calls `POST /api/auth/login`. The response includes `accessToken` and `refreshToken`. Both tokens are stored in persistent client state. The JWT is decoded to extract `sub` (user ID), `email`, `name`, and `role` (string or array). The user is redirected to the dashboard.

### Flow 2: Authentication & Session Management

1. **On every API request**, the app automatically attaches the `Authorization: Bearer {accessToken}` header via an Axios request interceptor.
2. **If any API returns 401**, the Axios response interceptor automatically calls `POST /api/auth/refresh-token` with the current `accessToken` and `refreshToken`. If successful, both tokens are replaced in state and the original request is retried. If the refresh also fails, the user is logged out and redirected to the login page.
3. **User logs out** → The app calls `POST /api/auth/logout` with the refresh token, clears all auth state, disconnects SignalR, and redirects to the landing page.
4. **Role checks**: Throughout the app, the decoded JWT roles determine which navigation items, pages, and features are visible. A user can be both a Student and Teacher simultaneously.

### Flow 3: Browsing & Discovering Courses

1. **User navigates to the course catalog** (available to everyone, including unauthenticated users).
2. **The catalog page** loads courses via `GET /api/courses?Page=1&PageSize=12` and displays them in a responsive grid of cards. Each card shows: title, teacher name, rating stars, lecture count, enrollment count.
3. **Search**: A search bar at the top calls `GET /api/courses/search?Keyword={keyword}&Page=1&PageSize=12` with debounced input (300ms delay). Results update the grid in real-time.
4. **Pagination**: Previous/Next buttons or page numbers navigate through results. The current page is reflected in the URL query string.
5. **Enrolled indicator**: If the user is logged in, courses they are enrolled in show an "Enrolled ✓" badge.
6. **User clicks a course card** → Navigates to the Course Detail page.

### Flow 4: Course Detail & Enrollment

1. **The course detail page** loads the course via `GET /api/courses/{courseId}`, which returns the full course info plus the list of lectures.
2. **Rating section** loads via `GET /api/courses/{courseId}/rating` showing the average rating and distribution bar chart (5-star breakdown).
3. **Reviews section** loads via `GET /api/courses/{courseId}/reviews?Page=1`, showing individual reviews with pagination.
4. **Enrollment button** has different states:
   - **Not logged in** → "Login to Enroll" → redirects to login
   - **Logged in, not enrolled, free course** → "Enroll Now" → calls `POST /api/courses/{courseId}/enroll`, then shows success toast
   - **Logged in, not enrolled, paid course** → "Add to Cart" → calls `POST /api/cart/items` with `courseId`, updates cart badge
   - **Logged in, enrolled** → "Go to Course" → navigates to the learning page
   - **Course owner (teacher)** → "Manage Course" → navigates to course management
5. **Write a review**: Enrolled students see a review form (star rating + comment). Submit calls `POST /api/courses/{courseId}/reviews` with `rating` and `comment`. Only one review per student per course is allowed (409 if duplicate).

### Flow 5: Course Learning

1. **The learning page** provides an entry point into course content.
2. **User clicks a lecture/material** or clicks the **"AI Study" button** from a course → Navigated to the **Unified Studio + Material Viewer page** (see Flow 6).
3. **Quick action buttons** also available from here:
   - "Take Exam" → navigates to available exams for this course
   - "Complete Course" → calls `POST /api/courses/{courseId}/complete`, shows confirmation dialog first

### Flow 6: Unified Studio Session + Material Viewer Page

This page combines the **Material Viewer** and the **Studio Session** into a single unified layout, modeled after **Google NotebookLM**. It is the primary learning interface.

#### 6.0 Page Entry & Layout

1. **User clicks the "AI Study" button** from within a course → The app calls `POST /api/study-sessions` with `courseId`. The response returns a `sessionId`. The user is navigated to `/courses/:courseId/studio/:sessionId`.
2. **The page layout**:
   - **Left panel (Material Viewer)** — Initially hidden; appears when a material is selected. Takes up ~60% of the width.
   - **Right panel (Studio Session)** — Shown by default; takes up the remaining space. Contains the chat interface and AI tool buttons.
   - **References panel** — Embedded within the Studio side. Lists all lectures and their materials for the course (loaded via `GET /api/courses/{courseId}/lectures?IncludeMaterials=true`). The user selects specific materials to both define the AI scope and launch the viewer.
3. **Session ID persistence**: The `sessionId` must be maintained and passed to every AI feature call for the entire duration of this page visit.

---

#### 6.1 Material Viewer

The viewer supports three material types, each rendered with a dedicated viewer component.

##### Initialization sequence (triggered when user selects a material from the References panel):

1. **Load Material Projection** — `GET /api/materials/{materialId}/projection` — fetches the user's current progress and last known position so playback/reading resumes where they left off.
2. **Load Sections** — `GET /api/materials/{materialId}/sections` — fetches the material's section data.
3. **Load Material Stream** — fetch `GET /api/materials/{materialId}/stream` with the `Authorization` header, convert to a Blob URL (since `<video>`, `<audio>`, `<img>` tags cannot send auth headers).

Using sections + stream together, the material is rendered in a **sectioned view**.

##### Per-material-type rendering:

- **PDF**: Rendered with a PDF viewer component, scroll-based, sections overlaid.
- **Video/Audio**: Native HTML5 player with Blob URL, sections listed as a timeline/sidebar.
- **Images**: Displayed inline with Blob URL.
- **Download**: A button calls `GET /api/materials/{materialId}/download`.

##### Section actions:

Each section exposes three action buttons:
- **Make Quiz** → calls `POST /api/materials/{materialId}/sections/{sectionId}/quiz` (or the Study Session section quiz endpoint). Results rendered inline or in the Studio panel.
- **Summarize** → calls the section summary endpoint. Result shown inline.
- **Build Flashcards** → calls the section flashcards endpoint. Result shown inline or sent to the Studio panel.

> All three section actions must include the active `sessionId`.

##### Progress Tracking:

- **PDF**: Progress is **scroll-triggered** — when the user navigates to the next page, if `newPage > lastRecordedPage`, call `PUT /api/materials/{materialId}/progress` with the new position.
- **Video / Audio**: Progress is **time-triggered** — every 30 seconds of playback, if `currentTimestamp > lastRecordedTimestamp`, call the update progress endpoint.

---

#### 6.2 Studio Session (NotebookLM-style)

The Studio panel mimics Google NotebookLM's design. It contains:

- A **chat interface** (see 6a).
- **AI feature buttons** — each has a settings icon (opens a config panel for that feature's parameters) and a **one-click default action** (triggers the feature with default settings immediately). Features: Flashcards, Mind Map, Quiz, Summary, Dialogue Audio.
- A **References panel** listing all lectures and materials; selecting a material launches the viewer. Multi-select is supported — selected materials also act as the **scope filter** for all AI features.

##### 6a: Chat (SSE Streaming)

1. Previous messages load from `GET /api/study-sessions/{id}/chat?Page=1` (scroll up to load older).
2. User sends a message → `POST /api/study-sessions/{id}/chat` with `{ message, lectureIds, materialIds }` using `fetch()` (not Axios) to read SSE.
3. Response streams token-by-token. Each `data:` chunk has a `content` field. Append to the message in real-time. Stream ends with `data: [DONE]`.
4. AI responses may include source citations — display as clickable references.
5. Send button is disabled while streaming; loading indicator visible.

##### 6b: Flashcards

1. Click "Generate Flashcards" (or one-click default) → `POST /api/study-sessions/{id}/flashcards` with optional `lectureIds`/`materialIds`.
2. Show loading spinner. Response has an array of flashcard objects (front/back). Render as interactive flip cards.
3. Previous sets load from `GET /api/study-sessions/{id}/flashcards?Page=1`.
4. **Settings panel params**: none beyond scope.

##### 6c: Mind Map

1. Click "Generate Mind Map" → `POST /api/study-sessions/{id}/mindmaps` with optional scope.
2. Response has `nodes` (JSON string, recursive tree) and `connections` (JSON array of edges). Both require `JSON.parse()` before rendering.
3. Render with ReactFlow or react-d3-tree with zoom/pan.
4. Previous maps load from `GET /api/study-sessions/{id}/mindmaps?Page=1`.

##### 6d: Quiz

1. Click "Generate Quiz" → `POST /api/study-sessions/{id}/quizzes` with optional scope.
2. Response `questions` field is a JSON string — parse to get array of `{ questionText, questionType, options, correctAnswer, explanation, difficulty }`.
3. User answers, clicks submit → `POST /api/study-sessions/{id}/quizzes/{quizId}/submit`. Shows score and per-question feedback.
4. Previous quizzes load from `GET /api/study-sessions/{id}/quizzes?Page=1`.
5. **Settings panel params**: topic, difficulty, question count.

##### 6e: Summary

1. Click "Generate Summary" → `POST /api/study-sessions/{id}/summary` with optional scope.
2. Response is markdown — render with a markdown renderer.

##### 6f: Dialogue Audio

1. Click "Generate Dialogue Audio" → `POST /api/study-sessions/{id}/dialogue-audio` with optional scope.
2. May take 30–60 seconds — show progress indicator.
3. Response:
   - `audioBase64`: decode to Blob URL, use in `<audio>` player.
   - `turnTimestamps`: array of `{ startTime, endTime, speaker, text }` for synchronization.
   - `exchanges`: full dialogue transcript (speaker + text pairs).
4. Display audio player with synchronized transcript — highlight the current turn as audio plays based on `currentTime` vs `turnTimestamps`.
5. **Settings panel params**: `FocusConcepts`, `NumberOfExchanges`, voice selection.

##### 6g: End Session

1. "End Session" button always visible. Confirmation dialog on click.
2. On confirm → `POST /api/study-sessions/{id}/end`.
3. User redirected back to the course learning page.

### Flow 7: Exams (Student)

1. **Available exams**: From the course learning page, the student sees available exams via `GET /api/exams/available?courseId={courseId}`. Only active exams (current time is within the exam time window) appear.
2. **Student starts an exam** → Navigates to the exam-taking page. The app loads exam details and questions via `GET /api/exams/{examId}` (includes questions). Also loads total points via `GET /api/exams/{examId}/total-points`.
3. **Exam-taking UI**:
   - A **countdown timer** based on `durationMinutes` is shown prominently.
   - A **question navigator** panel shows question numbers, color-coded: unanswered (gray), answered (green).
   - The student selects/types answers for each question.
   - Answers are **auto-saved to localStorage** so they survive page refreshes.
   - `correctAnswer` field is **hidden** during the exam — never display it.
4. **Submitting**: The student clicks "Submit Exam" → A confirmation dialog shows the count of answered vs. total questions. On confirm, the app calls `POST /api/exams/{examId}/submit` with all answers.
5. **Auto-submit**: If the timer reaches zero, the exam automatically submits with whatever answers are provided.
6. **After submission**: The student is redirected to a confirmation page showing that the exam was submitted successfully and they will be notified when it's graded.

### Flow 8: Viewing Grades & Submissions (Student)

1. **My Submissions page**: Lists all the student's submissions via `GET /api/exams/submissions/my-submissions?Page=1`. Each entry shows the exam title, course, submission date, and grading status.
2. **Submission detail**: Clicking a submission loads `GET /api/exams/submissions/{submissionId}` showing the full submission with answers and (if graded) the teacher's feedback.
3. **My Grades page**: Lists all grades via `GET /api/exams/grades/student?Page=1`. Each shows the exam title, score, total, percentage, and whether it was manually graded or AI-graded.

### Flow 9: Teacher — Course & Content Management

1. **Teacher Dashboard**: Loaded via `GET /api/users/teacher/dashboard`. Shows summary cards (total courses, published courses, total students, total exams) and action alerts (pending AI approvals, ungraded submissions).
2. **Creating a course**: Teacher fills out a form (title, description, price, categoryId?, thumbnail?) → `POST /api/courses` with multipart form data.
3. **Managing a course**: The teacher selects a course → the management page shows:
   - Course info edit form → `PUT /api/courses/{courseId}`
   - Publish course → `POST /api/courses/{courseId}/publish`
   - Delete course → `DELETE /api/courses/{courseId}`
4. **Managing lectures**: Within a course:
   - Add lecture (title, description, order) → `POST /api/courses/{courseId}/lectures`
   - Edit lecture → `PUT /api/courses/lectures/{lectureId}`
   - Delete lecture → `DELETE /api/courses/lectures/{lectureId}`
   - Reorder lectures by changing the `orderIndex` field
5. **Uploading materials**: Within a lecture:
   - Upload form with file picker and title → `POST /api/courses/lectures/{lectureId}/materials` (multipart)
   - Types supported: PDF, video, audio, images, text
   - After upload, the material enters a background indexing queue for AI processing. The teacher receives a real-time SignalR notification when indexing is complete.
   - Delete material → `DELETE /api/courses/materials/{materialId}`

### Flow 10: Teacher — Exam & Question Management

1. **Creating an exam**: Teacher fills out (title, description, duration, start time, end time, course) → `POST /api/exams`.
2. **Adding questions**:
   - Manual: Add one question at a time (text, type, options, correct answer, points) → `POST /api/exams/{examId}/questions`
   - Bulk: Add multiple questions at once → `POST /api/exams/{examId}/questions/bulk`
   - AI-generated: Click "Generate with AI" → `POST /api/exams/{examId}/questions/generate` (sends topic/difficulty/count parameters). Wait for AI response (may take 15-30 seconds). Review the generated questions and confirm.
3. **Editing/deleting questions**: `PUT /api/questions/{questionId}`, `DELETE /api/questions/{questionId}`
4. **Reordering questions**: Drag-and-drop or up/down arrows → `PUT /api/questions/reorder`
5. **Managing exam lifecycle**: The exam is a draft until questions are added. Once the time window opens, it becomes active. View course exams via `GET /api/exams/course/{courseId}`.

### Flow 11: Teacher — Grading

1. **Grading page** has two sections:
   - **Ungraded submissions**: Loaded from `GET /api/exams/submissions/ungraded`. Shown as a list.
   - **Pending AI approvals**: Loaded from `GET /api/exams/grades/pending-approval`. These are AI-assigned grades waiting for teacher review.
2. **Manual grading**: Teacher selects a submission → sees the student's answers alongside the questions. Teacher enters points and feedback for each answer → `POST /api/exams/submissions/{submissionId}/grade` with `score`, `feedback`, `isPassed`.
3. **AI grading**: Teacher clicks "AI Grade" on a submission → `POST /api/exams/submissions/{submissionId}/grade-ai`. The AI returns a detailed breakdown with per-question scores, strengths, weaknesses, and an overall score. This grade goes into "pending approval" status.
4. **Approving AI grades**: Teacher reviews the AI grade (sees breakdown per essay question) → either:
   - Approve as-is → `POST /api/exams/grades/{gradeId}/approve`
   - Modify the score/feedback then approve → `PUT /api/exams/grades/{gradeId}` then approve
5. **Grade statistics**: `GET /api/exams/grades/exam/{examId}/stats` shows average, min, max, pass rate. `GET /api/exams/grades/exam/{examId}/distribution` shows grade distribution for a bar chart.

### Flow 12: Real-Time Notifications (SignalR)

1. **On login**, the app establishes two SignalR connections (simultaneous):
   - **StudentNotificationHub** (`/hubs/student-notifications`) — for all authenticated users
   - **MaterialIndexingHub** (`/hubs/material-indexing`) — for teachers only
2. **Both connections** use the JWT token for authentication (`accessTokenFactory` returns the current access token).
3. **StudentNotificationHub** — after connecting, the app joins course groups for each enrolled course by calling `JoinCourseGroup(courseId)` on the hub. Events received:
   - `NewExamPosted` → toast: "New exam: {examTitle}" 
   - `NewMaterialUploaded` → toast: "New material in {courseName}"
   - `NewLectureAdded` → toast: "New lecture: {lectureTitle}"
   - `CourseUpdated` → toast: "Course {courseName} updated"
   - `ExamUpdated`, `ExamDeleted` → exam-related toasts
   - `SubmissionGraded` → toast: "Your exam has been graded"
   - `GradeApproved`, `GradeUpdated` → grade-related toasts
   - `EngagementAlert` → highlighted alert toast with teacher's message
4. **MaterialIndexingHub** — teachers receive:
   - `ReceiveIndexingNotification` → success or failure toast for material indexing
   - `ExamSubmitted` → toast: "{studentName} submitted {examTitle}"
   - `NewEnrollment` → toast: "{studentName} enrolled in {courseName}"
   - `NewReview` → toast: "New {rating}★ review for {courseName}"
   - `EnrollmentCompleted` → toast: "{studentName} completed {courseName}"
   - `StudentUnenrolled` → toast: "{studentName} left {courseName}"
5. **On reconnect** (automatic retry with backoff: 0s, 2s, 5s, 10s, 30s), all course groups must be re-joined because group membership is lost on disconnect.
6. **Notification UI**: A bell icon in the navbar shows an unread count badge. Clicking it opens a notification list/dropdown showing recent notifications.

### Flow 13: Student Engagement Monitoring (Teacher)

1. **Teacher opens the engagement page** for a course → `GET /api/courses/{courseId}/engagement`.
2. **The page shows**:
   - Summary bar: total enrolled, active students, at-risk students, average engagement score
   - A table of all students sorted by engagement score (lowest first), with columns: name, engagement score, last active date, submission rate, average grade
   - Color-coded engagement levels: **Critical** (red, ≤25%), **Low** (orange, 26-50%), **Moderate** (yellow, 51-75%), **High** (green, 76-100%)
3. **Sending alerts**:
   - Individual: Teacher clicks the alert button next to a student, types a custom message → `POST /api/courses/{courseId}/engagement/alerts` with `{ studentIds: [id], message }`
   - Bulk: Teacher clicks "Alert All At-Risk" → sends the same endpoint with all at-risk student IDs
4. The student receives the alert as a real-time `EngagementAlert` notification via SignalR.

### Flow 14: AI Provider Management

1. **User navigates to AI Provider settings** (available to any authenticated user).
2. **Current status**: Loads via `GET /api/ai/provider` — shows which provider is active (Ollama or Groq) and configuration status.
3. **Switching providers**: The user selects a provider from radio buttons and clicks "Switch" → `POST /api/ai/provider/switch` with `{ provider: "Ollama" | "Groq" }`.
4. **Error handling**: If Groq is not configured (missing API key), the switch fails and an error message is shown. Ollama requires the local Ollama service to be running.
5. The selected AI provider affects all AI operations: chat, flashcards, quizzes, mind maps, summaries, dialogue audio, AI grading, and AI question generation.

### Flow 15: Dialogue Audio Configuration

1. **Before generating dialogue audio**, the user can configure voice settings (optional flow — defaults are used if not configured).
2. **Load available voices**: `GET /api/dialogue/voices` — returns a list of available TTS voices.
3. **Set voice config**: `POST /api/dialogue/voice-config` — set preferred voice.
4. **Load formats/languages**: `GET /api/dialogue/formats`, `GET /api/dialogue/languages`.
5. **Preview voices**: `GET /api/dialogue/previews` — play sample clips before choosing.

### Flow 16: User Profile & Settings

1. **Profile page** shows user info loaded from `GET /api/users/me`.
2. **Update profile** → `PUT /api/users/update` with updated fields.
3. **View stats** → `GET /api/users/me/stats` shows study statistics, courses enrolled, exams taken, etc.
4. **Become Teacher**: If the user is only a Student, a "Become Teacher" button calls `POST /api/users/become-teacher`. The response includes new tokens — replace both and re-decode the JWT so Teacher nav items appear immediately.

---

## Routing Structure

| Route | Page | Access |
|-------|------|--------|
| `/` | Landing Page | Public |
| `/login` | Login Page | Public |
| `/register` | Registration Page (Student) | Public |
| `/register/teacher` | Teacher Registration Page | Public |
| `/verify-email` | Email Verification Page | Public |
| `/courses` | Course Catalog | Public |
| `/courses/:courseId` | Course Detail | Public |
| `/dashboard` | Student Dashboard | Authenticated |
| `/my-enrollments` | My Enrollments | Authenticated |
| `/cart` | Shopping Cart | Authenticated |
| `/checkout/:orderId` | Checkout / Order Status | Authenticated |
| `/courses/:courseId/learn` | Course Learning (entry point) | Authenticated + Enrolled |
| `/courses/:courseId/studio/:sessionId` | Unified Studio + Material Viewer | Authenticated + Enrolled |
| `/study-sessions/:sessionId` | Study Session (redirect to unified page) | Authenticated |
| `/exams/:examId/take` | Exam Taking | Authenticated + Enrolled |
| `/my-submissions` | My Submissions | Authenticated |
| `/my-grades` | My Grades | Authenticated |
| `/notifications` | Notifications List | Authenticated |
| `/profile` | User Profile | Authenticated |
| `/settings/ai-provider` | AI Provider Settings | Authenticated |
| `/settings/voice` | Voice Settings | Authenticated |
| `/teacher/dashboard` | Teacher Dashboard | Teacher role |
| `/teacher/courses/:courseId` | Course Management | Teacher role + Owner |
| `/teacher/courses/:courseId/lectures/:lectureId` | Lecture Management | Teacher role + Owner |
| `/teacher/exams/:examId` | Exam Management | Teacher role + Owner |
| `/teacher/exams/:examId/questions` | Question Editor | Teacher role + Owner |
| `/teacher/grading` | Grading Page | Teacher role |
| `/teacher/courses/:courseId/engagement` | Engagement Page | Teacher role + Owner |
| `/teacher/categories` | Category Management | Teacher role |

---

## Key Implementation Rules

### 1. All API responses use the envelope format
Every response is wrapped in `{ success: boolean, data: T | null, message?: string }`. Always unwrap `response.data.data` for the actual payload. Check `response.data.success` for errors.

### 2. Pagination is consistent
All paginated endpoints accept `?Page=1&PageSize=10`. Default is page 1, size 10. The response includes: `items`, `page`, `pageSize`, `totalCount`, `totalPages`, `hasPrevious`, `hasNext`.

### 3. Auth token management is critical
- Store tokens in persistent storage (zustand persist or localStorage)
- Intercept 401s and refresh automatically via `POST /api/auth/refresh-token`
- Replace BOTH tokens after: login, refresh
- Decode JWT to extract roles for UI rendering

### 4. SSE streaming for chat only
Only the chat endpoint (`POST /api/study-sessions/{id}/chat`) uses SSE streaming. All other endpoints return standard JSON. Use `fetch()` with `ReadableStream` for the chat; use Axios for everything else.

### 5. SignalR connections based on role
- **All authenticated users** → connect to StudentNotificationHub (`/hubs/student-notifications`)
- **Teachers only** → also connect to MaterialIndexingHub (`/hubs/material-indexing`)
- Join course groups for each enrolled course
- Re-join groups on reconnect

### 6. Material streaming requires auth
Since HTML `<video>`, `<audio>`, and `<img>` tags cannot set Authorization headers, fetch the media with the auth header, convert to Blob URL, and use that as the source.

### 7. AI operations are slow
All AI operations (chat, flashcards, quizzes, mind maps, summaries, dialogue audio, AI grading, AI question generation) may take 5-60 seconds. Always show appropriate loading UI with skeleton screens or progress indicators.

### 8. Study session scope uses arrays
All study session tools accept `lectureIds` and `materialIds` as arrays, allowing students to focus on multiple lectures/materials at once. Use multi-select UI components for the scope filter.

### 9. Mind map data is JSON strings
The `nodes` and `connections` fields in mind map responses are JSON strings that need `JSON.parse()` before rendering with a graph visualization library.

### 10. Quiz questions are JSON strings
The `questions` field in generated quizzes is a JSON string. Parse it to render the quiz UI. Each question has: `questionText`, `questionType`, `options`, `correctAnswer`, `explanation`, `difficulty`.

### 11. Dialogue audio uses base64
The `audioBase64` field contains the full audio file as base64. Decode it, create a Blob URL, and use it in an `<audio>` element. Use `turnTimestamps` to sync transcript highlighting with playback.

### 12. Role-based UI rendering
Decode the JWT and check roles to conditionally render:
- **Teacher features**: course management, grading, dashboard, engagement, AI question gen
- **Student features**: enrollment, study sessions, exam taking, reviews
- **Both**: AI provider switching, notifications, profile

### 13. Error handling
- `400` → Show validation errors inline on forms
- `401` → Attempt refresh, redirect to login if fails
- `403` → Show "Permission Denied" page
- `404` → Show "Not Found" page
- `409` → Show specific conflict message (e.g., already reviewed)
- `429` → Show "Rate limited, try again later"
- `500` → Show generic error with retry option

### 14. Material Viewer initialization order
When a user selects a material, always load in this exact order: (1) **projection** (resume position), (2) **sections** (layout), (3) **stream** (content). Render only after all three resolve. Resume playback/scroll position from the projection data.

### 15. Section actions require the active session ID
The "Make Quiz", "Summarize", and "Build Flashcards" buttons on each material section must include the current `sessionId` in their requests. Do not allow these actions if no session is active.

### 16. Material progress tracking is conditional
- **PDF**: call the update progress endpoint only when scrolling to a **new page > lastRecordedPage**.
- **Video/Audio**: call the update progress endpoint every 30 seconds of playback, but only when `currentTimestamp > lastRecordedTimestamp`. Do not spam the endpoint on every tick.

---

## Quick Start Commands

```bash
# Create project
npm create vite@latest aieduplatform-frontend -- --template react-ts
cd aieduplatform-frontend

# Install core dependencies
npm install react-router-dom @tanstack/react-query axios zustand

# Install UI
npm install -D tailwindcss @tailwindcss/vite
npm install lucide-react clsx tailwind-merge sonner framer-motion

# Install specialized
npm install @microsoft/signalr jwt-decode zod react-hook-form @hookform/resolvers
npm install react-markdown remark-gfm
npm install recharts
npm install date-fns
npm install reactflow             # for mind maps
npm install react-pdf             # for PDF viewing

# Install dev dependencies
npm install -D @types/node @testing-library/react vitest
```

---

## Environment Variables

```env
VITE_API_URL=http://localhost:5069/api
VITE_SIGNALR_URL=http://localhost:5069
```

---

## API Endpoint Quick Reference

| Category | Count | Key Endpoints |
|----------|-------|---------------|
| Auth | 6 | register/student, register/teacher, verify-email, login, refresh-token, logout |
| Users | 5 | me, update, {id}, stats, dashboard, teacher/dashboard |
| Courses | 13 | CRUD, search, publish, my-courses, instructor, continue-learning, progress, engagement, alerts |
| Cart | 4 | get, add item, remove item, clear |
| Checkout | 2 | create session, order status |
| Payments | 1 | Stripe webhook |
| Categories | 7 | CRUD, course-category associations |
| Enrollments | 5 | enroll, unenroll, complete, enrolled, course enrollments |
| Lectures | 5 | CRUD (add, get course, get detail, update, delete) |
| Materials | 7 | upload, get lecture, stream, download, progress, projection, delete |
| Exams | 10 | CRUD, course/active/upcoming/past, available, total-points |
| Questions | 7 | add, bulk, AI generate, get, update, delete, reorder |
| Submissions | 6 | submit, exam subs, detail, student subs, ungraded, stats |
| Grades | 11 | manual, AI, approve, update, exam/pending/student/submission, stats, distribution |
| Reviews | 5 | add, get, rating, update, delete |
| Notifications | 5 | list, unread-count, mark-read, mark-all-read, delete |
| Study Sessions | 16 | start, end, sessions, detail, chat (SSE), flashcards, mindmaps, quizzes, quiz-submit, summary, dialogue-audio |
| Sections | 4 | get sections, section summary, section flashcards, section quiz |
| AI Provider | 2 | get status, switch provider |
| Dialogue | 8 | voices, previews, default-config, formats, languages, voice-settings CRUD |
| **Total** | **~130** | |

> For complete endpoint documentation with request/response schemas, see [API_REFERENCE.md](API_REFERENCE.md).
> For SignalR hub details including Flutter integration, see [SIGNALR_IMPLEMENTATION.md](SIGNALR_IMPLEMENTATION.md).
> For user journey flows and UI wireframes, see [USER_JOURNEY.md](USER_JOURNEY.md).
