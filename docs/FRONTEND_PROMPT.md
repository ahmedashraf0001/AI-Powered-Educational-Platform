# React Frontend Development Prompt

> Use this document as a comprehensive prompt/specification to build the entire React frontend for AIEduPlatform. It describes every screen, user flow, and interaction pattern based on the API and user journey.

---

## Project Overview

Build a **React Single-Page Application (SPA)** for **AIEduPlatform** — an AI-powered educational platform. Two user roles exist (**Student** and **Teacher**, which can coexist on the same account) plus an **Admin** role. The app integrates with a .NET REST API backend that uses JWT authentication.

**Key Features:**
- Course browsing, enrollment, and learning
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
│   ├── auth.api.ts               # Auth endpoints
│   ├── courses.api.ts            # Course CRUD endpoints
│   ├── enrollments.api.ts        # Enrollment endpoints
│   ├── lectures.api.ts           # Lecture endpoints
│   ├── materials.api.ts          # Material upload/stream/download
│   ├── exams.api.ts              # Exam endpoints
│   ├── questions.api.ts          # Question endpoints
│   ├── submissions.api.ts        # Submission endpoints
│   ├── grades.api.ts             # Grading endpoints
│   ├── reviews.api.ts            # Review endpoints
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
│   ├── study/
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
│   ├── student/                  # Dashboard, Enrollments, Learning, StudySession, Exams, Grades
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
3. **User fills out registration form** (email, username, password, confirm password) → On submit, the app calls `POST /api/auth/register`. On success, the user sees a success toast and is redirected to the login page.
4. **User fills out login form** (email, password) → On submit, the app calls `POST /api/auth/login`. The response includes `accessToken` and `refreshToken`. Both tokens are stored in persistent client state. The JWT is decoded to extract `sub` (user ID), `email`, `name`, and `role` (string or array). The user is redirected to the dashboard.

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
   - **Logged in, not enrolled** → "Enroll Now" → calls `POST /api/enrollments/enroll` with `courseId`, then shows success toast
   - **Logged in, enrolled** → "Go to Course" → navigates to the learning page
   - **Course owner (teacher)** → "Manage Course" → navigates to course management
5. **Write a review**: Enrolled students see a review form (star rating + comment). Submit calls `POST /api/reviews` with `courseId`, `rating`, and `comment`. Only one review per student per course is allowed (409 if duplicate).

### Flow 5: Course Learning

1. **The learning page** has a **sidebar** listing all lectures (loaded from `GET /api/courses/{courseId}/lectures?IncludeMaterials=true`) and a **main content area**.
2. **User clicks a lecture** in the sidebar → The main area shows the lecture title, description, and a list of materials grouped by type (PDFs, videos, audio, images).
3. **Viewing materials**:
   - Since media tags (`<video>`, `<audio>`, `<img>`) cannot set auth headers, the app fetches each material via `GET /api/materials/{materialId}/stream` with the auth header, converts the response to a Blob URL, and uses that as the media source.
   - **PDF**: Rendered using a PDF viewer component.
   - **Video/Audio**: Rendered using native HTML5 players with Blob URLs.
   - **Images**: Displayed with Blob URLs.
   - **Download**: A download button calls `GET /api/materials/{materialId}/download`.
4. **Quick action buttons** at the top:
   - "Start AI Study Session" → navigates to study session creation
   - "Take Exam" → navigates to available exams for this course
   - "Complete Course" → calls `POST /api/courses/{courseId}/complete`, shows confirmation dialog first

### Flow 6: AI Study Session

1. **Starting a session**: The user clicks "Start AI Study Session" from a course. The app calls `POST /api/study-sessions` with `courseId`. The response includes the session ID. The user is navigated to the study session page.
2. **The study session page** has a **tabbed interface** with these tabs: **Chat**, **Flashcards**, **Mind Map**, **Quiz**, **Summary**, **Dialogue Audio**.
3. **Scope filter** (shared across all tabs): A multi-select dropdown lets the user pick specific `lectureIds` and/or `materialIds` to focus the AI on. If nothing is selected, the AI uses all course materials.

#### 6a: Chat Tab (SSE Streaming)

1. The chat area displays previous messages loaded from `GET /api/study-sessions/{id}/chat?Page=1` (paginated, scroll-up to load older messages).
2. **User types a message and clicks send** → The app sends a `POST /api/study-sessions/{id}/chat` request with `{ message, lectureIds, materialIds }` using `fetch()` (not Axios) to read the SSE stream.
3. **The response streams in token-by-token** via Server-Sent Events. Each SSE `data:` chunk contains a JSON object with a `content` field. The app appends each content chunk to the AI message in real-time, creating a typing effect. The stream ends with `data: [DONE]`.
4. AI responses may include **source citations** from course materials. These are displayed as clickable references below the message.
5. While streaming, the send button is disabled and a loading indicator is shown.

#### 6b: Flashcards Tab

1. The user clicks "Generate Flashcards" → calls `POST /api/study-sessions/{id}/flashcards` with optional `lectureIds`/`materialIds`.
2. A loading spinner shows while the AI generates (may take 10-30 seconds).
3. The response contains an array of flashcards (front/back). They are displayed as interactive flip cards — click to reveal the answer.
4. Previous flashcard sets are loaded from `GET /api/study-sessions/{id}/flashcards?Page=1`.

#### 6c: Mind Map Tab

1. The user clicks "Generate Mind Map" → calls `POST /api/study-sessions/{id}/mindmaps` with optional scope.
2. The response contains `nodes` (a JSON string with a recursive tree structure) and `connections` (a JSON array of edges). Both must be `JSON.parse()`'d before rendering.
3. The parsed data is rendered using a graph visualization library (ReactFlow or react-d3-tree) with zoom/pan controls.
4. Previous mind maps are loaded from `GET /api/study-sessions/{id}/mindmaps?Page=1`.

#### 6d: Quiz Tab

1. The user clicks "Generate Quiz" → calls `POST /api/study-sessions/{id}/quizzes` with optional scope.
2. The response contains a `questions` field that is a JSON string. Parse it to get an array of questions, each with: `questionText`, `questionType`, `options`, `correctAnswer`, `explanation`, `difficulty`.
3. The user answers each question. On submit, the app calls `POST /api/study-sessions/{id}/quizzes/{quizId}/submit` with the answers.
4. The response shows the score and per-question feedback (correct/incorrect, explanation).
5. Previous quizzes are loaded from `GET /api/study-sessions/{id}/quizzes?Page=1`.

#### 6e: Summary Tab

1. The user clicks "Generate Summary" → calls `POST /api/study-sessions/{id}/summary` with optional scope.
2. The response is a markdown summary. Render it with a markdown renderer.

#### 6f: Dialogue Audio Tab

1. The user clicks "Generate Dialogue Audio" → calls `POST /api/study-sessions/{id}/dialogue-audio` with optional scope.
2. This may take 30-60 seconds. Show a progress indicator.
3. The response contains:
   - `audioBase64`: The full audio file as a base64 string. Decode it to a Blob URL and use in an `<audio>` player.
   - `turnTimestamps`: An array of timing data for each dialogue turn (`startTime`, `endTime`, speaker, text).
   - `exchanges`: The dialogue transcript (speaker + text pairs).
4. Display the audio player with synchronized transcript — as the audio plays, highlight the current turn based on `currentTime` matching the `turnTimestamps`.

#### 6g: End Session

1. An "End Session" button is always visible. Clicking it shows a confirmation dialog.
2. On confirm, the app calls `POST /api/study-sessions/{id}/end`.
3. The user is redirected back to the course learning page.

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

1. **User becomes a teacher** by clicking a "Become Teacher" button on their profile → calls `POST /api/users/become-teacher`. The response includes new tokens (with the Teacher role added). Both tokens must be replaced immediately.
2. **Teacher Dashboard**: Loaded via `GET /api/users/teacher/dashboard`. Shows summary cards (total courses, published courses, total students, total exams) and action alerts (pending AI approvals, ungraded submissions).
3. **Creating a course**: Teacher fills out a form (title, description, category, level, language, price, thumbnail) → `POST /api/courses` with multipart form data.
4. **Managing a course**: The teacher selects a course → the management page shows:
   - Course info edit form → `PUT /api/courses/{courseId}`
   - Publish/unpublish toggle → `PUT /api/courses/{courseId}` with `isPublished`
   - Delete course → `DELETE /api/courses/{courseId}`
5. **Managing lectures**: Within a course:
   - Add lecture (title, description, order) → `POST /api/lectures`
   - Edit lecture → `PUT /api/lectures/{lectureId}`
   - Delete lecture → `DELETE /api/lectures/{lectureId}`
   - Reorder lectures by changing the `order` field
6. **Uploading materials**: Within a lecture:
   - Upload form with file picker and title → `POST /api/materials/upload` (multipart)
   - Types supported: PDF, video, audio, images, text
   - After upload, the material enters a background indexing queue for AI processing. The teacher receives a real-time SignalR notification when indexing is complete (see Flow 12).
   - Delete material → `DELETE /api/materials/{materialId}`

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
| `/register` | Registration Page | Public |
| `/courses` | Course Catalog | Public |
| `/courses/:courseId` | Course Detail | Public |
| `/dashboard` | Student Dashboard | Authenticated |
| `/my-enrollments` | My Enrollments | Authenticated |
| `/courses/:courseId/learn` | Course Learning | Authenticated + Enrolled |
| `/study-sessions/:sessionId` | Study Session | Authenticated |
| `/exams/:examId/take` | Exam Taking | Authenticated + Enrolled |
| `/my-submissions` | My Submissions | Authenticated |
| `/my-grades` | My Grades | Authenticated |
| `/profile` | User Profile | Authenticated |
| `/settings/ai-provider` | AI Provider Settings | Authenticated |
| `/teacher/dashboard` | Teacher Dashboard | Teacher role |
| `/teacher/courses/:courseId` | Course Management | Teacher role + Owner |
| `/teacher/courses/:courseId/lectures/:lectureId` | Lecture Management | Teacher role + Owner |
| `/teacher/exams/:examId` | Exam Management | Teacher role + Owner |
| `/teacher/exams/:examId/questions` | Question Editor | Teacher role + Owner |
| `/teacher/grading` | Grading Page | Teacher role |
| `/teacher/courses/:courseId/engagement` | Engagement Page | Teacher role + Owner |

---

## Key Implementation Rules

### 1. All API responses use the envelope format
Every response is wrapped in `{ success: boolean, data: T | null, message?: string }`. Always unwrap `response.data.data` for the actual payload. Check `response.data.success` for errors.

### 2. Pagination is consistent
All paginated endpoints accept `?Page=1&PageSize=10`. Default is page 1, size 10. The response includes: `items`, `page`, `pageSize`, `totalCount`, `totalPages`, `hasPrevious`, `hasNext`.

### 3. Auth token management is critical
- Store tokens in persistent storage (zustand persist or localStorage)
- Intercept 401s and refresh automatically via `POST /api/auth/refresh-token`
- Replace BOTH tokens after: login, refresh, become-teacher
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
| Auth | 4 | register, login, refresh-token, logout |
| Users | 6 | me, update, {id}, stats, become-teacher, teacher/dashboard |
| Courses | 11 | CRUD, search, publish, my-courses, instructor, engagement, alerts |
| Enrollments | 5 | enroll, unenroll, complete, enrolled, course enrollments |
| Lectures | 5 | CRUD (add, get course, get detail, update, delete) |
| Materials | 5 | upload, get lecture, stream, download, delete |
| Exams | 10 | CRUD, course/active/upcoming/past, available, total-points |
| Questions | 7 | add, bulk, AI generate, get, update, delete, reorder |
| Submissions | 6 | submit, exam subs, detail, student subs, ungraded, stats |
| Grades | 11 | manual, AI, approve, update, exam/pending/student/submission, stats, distribution |
| Reviews | 5 | add, get, rating, update, delete |
| Study Sessions | 16 | start, end, sessions, detail, stats, chat, flashcards, mindmaps, quizzes, quiz-submit, summary, dialogue-audio |
| AI Provider | 2 | get status, switch provider |
| Dialogue | 5 | voices, voice-config, formats, languages, previews |
| **Total** | **98** | |

> For complete endpoint documentation with request/response schemas, see [API_REFERENCE.md](API_REFERENCE.md).
> For SignalR hub details including Flutter integration, see [SIGNALR_IMPLEMENTATION.md](SIGNALR_IMPLEMENTATION.md).
> For user journey flows and UI wireframes, see [USER_JOURNEY.md](USER_JOURNEY.md).
