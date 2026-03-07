# AIEduPlatform — Feature Flows

> Step-by-step flows for every major feature in the platform. Each flow shows the exact sequence of actions, API calls, and system behaviors involved.

---

## Table of Contents

1. [Registration & Email Verification](#1-registration--email-verification)
2. [Authentication (Login / Logout / Refresh)](#2-authentication)
3. [Course Discovery & Browsing](#3-course-discovery--browsing)
4. [Course Enrollment (Cart → Checkout → Payment)](#4-course-enrollment)
5. [Course Learning (Lectures & Materials)](#5-course-learning)
6. [Material Progress Tracking](#6-material-progress-tracking)
7. [AI Study Session](#7-ai-study-session)
8. [Study Session — Chat (RAG + SSE)](#8-study-session--chat)
9. [Study Session — Flashcards](#9-study-session--flashcards)
10. [Study Session — Mind Maps](#10-study-session--mind-maps)
11. [Study Session — Practice Quiz](#11-study-session--practice-quiz)
12. [Study Session — Summary](#12-study-session--summary)
13. [Study Session — Dialogue Audio (TTS)](#13-study-session--dialogue-audio)
14. [Semantic Section Tools](#14-semantic-section-tools)
15. [Exam Taking & Submission (Student)](#15-exam-taking--submission)
16. [Viewing Grades & Submissions (Student)](#16-viewing-grades--submissions)
17. [Course Reviews](#17-course-reviews)
18. [Course Creation & Management (Teacher)](#18-course-creation--management)
19. [Lecture & Material Management (Teacher)](#19-lecture--material-management)
20. [Material Indexing (Background)](#20-material-indexing)
21. [Exam & Question Management (Teacher)](#21-exam--question-management)
22. [AI Question Generation](#22-ai-question-generation)
23. [Grading Workflow (Manual & AI)](#23-grading-workflow)
24. [Notifications](#24-notifications)
25. [Student Engagement Monitoring](#25-student-engagement-monitoring)
26. [Category Management](#26-category-management)
27. [AI Provider Switching](#27-ai-provider-switching)
28. [Voice Settings Configuration](#28-voice-settings-configuration)
29. [Unenrollment & Refund](#29-unenrollment--refund)
30. [Course Completion](#30-course-completion)
31. [User Profile & Dashboard](#31-user-profile--dashboard)
32. [Token Lifecycle](#32-token-lifecycle)

---

## 1. Registration & Email Verification

Two separate registration paths exist — one for students, one for teachers.

### Student Registration
```
1. User fills registration form
2. POST /api/auth/register/student
   Body: { email, userName, password, confirmPassword, fullName, gradeLevel?, interests? }
3. Server creates user with "Student" role
4. Server sends verification email with token + link
5. User clicks link in email
6. GET /api/auth/verify-email?Token={token}&Email={email}
7. Server marks email as verified
8. User can now log in
```

### Teacher Registration
```
1. User fills teacher registration form
2. POST /api/auth/register/teacher
   Body: { email, userName, password, confirmPassword, fullName, bio, qualifications, subjects }
3. Server creates user with "Teacher" role
4. Server sends verification email
5. User clicks link in email
6. GET /api/auth/verify-email?Token={token}&Email={email}
7. User can now log in
```

> **Note:** There is no "Become Teacher" endpoint. Users choose their role at registration. A user account can only have the role(s) assigned during registration.

---

## 2. Authentication

### Login
```
1. User enters email + password
2. POST /api/auth/login
3. Server validates credentials + email verification status
4. Server returns { accessToken, refreshToken, accessTokenExpiration, refreshTokenExpiration }
5. Frontend stores both tokens
6. Frontend decodes JWT to extract userId, roles, email, userName
7. Frontend sets up Authorization header interceptor
8. Redirect to dashboard based on role
```

### Logout
```
1. User clicks logout
2. POST /api/auth/logout  Body: { refreshToken }
3. Server revokes the refresh token in DB
4. Frontend clears all stored tokens
5. Frontend disconnects SignalR
6. Redirect to landing page
```

### Token Refresh
```
1. API call returns 401 Unauthorized
2. Frontend checks for stored refreshToken
3. POST /api/auth/refresh-token  Body: { accessToken, refreshToken }
4. Server validates refresh token, issues new pair
5. Frontend replaces both tokens
6. Frontend retries the failed request with new accessToken
7. If refresh fails → force logout + redirect to login
```

---

## 3. Course Discovery & Browsing

```
1. User navigates to course catalog (no auth required)
2. GET /api/courses?Page=1&PageSize=12  → paginated list of published courses
3. Optional: GET /api/categories  → load category filter options
4. Optional: GET /api/courses?Page=1&PageSize=12&CategoryId={id}  → filter by category
5. User types search query → GET /api/courses/search?Keyword={query}&Page=1&PageSize=12
6. User clicks a course card → GET /api/courses/{CourseId}  → course detail with lectures
7. GET /api/courses/{CourseId}/rating  → rating summary (avg, distribution)
8. GET /api/courses/{CourseId}/reviews?Page=1  → paginated reviews
```

---

## 4. Course Enrollment

Courses are enrolled via a **Cart → Checkout → Payment** flow (for paid courses) or directly for free courses.

### Adding to Cart
```
1. Student clicks "Add to Cart" on course detail page
2. POST /api/cart/items  Body: { courseId }
3. Server validates: course exists, not already enrolled, not duplicate in cart
4. Returns updated CartDto with items, total price
5. UI shows cart badge/count update
```

### Checkout (Paid Courses)
```
1. Student views cart → GET /api/cart
2. Cart displays items with course titles, prices, total
3. Student can remove items → DELETE /api/cart/items/{CourseId}
4. Student clicks "Checkout"
5. POST /api/checkout
6. Server creates an Order (status: Pending)
7. Server creates a Stripe PaymentIntent
8. Returns { clientSecret, orderId, totalAmount } to frontend
9. Frontend uses Stripe.js to complete payment with clientSecret
10. On payment success, Stripe sends webhook
11. POST /api/payments/webhook (Stripe → server)
12. Server confirms payment, updates Order status to "Paid"
13. Server auto-enrolls student in all cart courses
14. Server marks cart as CheckedOut
15. Student polls GET /api/checkout/{OrderId} to verify order status
16. UI shows "Enrolled!" for each course
```

### Checkout (Free Courses)
```
1. Student adds a free course ($0) to cart
2. POST /api/checkout
3. Server detects total = $0, skips Stripe
4. Server auto-enrolls student immediately
5. Returns success with order status "Paid"
6. No webhook needed
```

### Direct Enrollment (Free Course Alternative)
```
1. Student clicks "Enroll Now" on a free course
2. POST /api/courses/{CourseId}/enroll
3. Server creates enrollment with status "Active"
4. Teacher receives SignalR "NewEnrollment" notification
5. Student sees "Go to Course" button
```

---

## 5. Course Learning

```
1. Student navigates to enrolled course
2. GET /api/courses/{CourseId}/lectures?IncludeMaterials=true  → all lectures + materials
3. Student clicks a lecture → GET /api/lectures/{LectureId}  → detail with materials by type
4. Student clicks a material:
   - Video/Audio: GET /api/materials/{MaterialId}/stream  (supports HTTP Range for seeking)
   - PDF: GET /api/materials/{MaterialId}/stream  (inline display)
   - Image: GET /api/materials/{MaterialId}/stream
   - Download: GET /api/materials/{MaterialId}/download  (force download)
5. Frontend must fetch with Authorization header, convert to Blob URL for media elements
6. Student can view progress: GET /api/courses/{CourseId}/progress
7. Student can resume: GET /api/courses/continue-learning  → list of in-progress courses with resume points
```

---

## 6. Material Progress Tracking

```
1. Student opens a material (video, PDF, audio)
2. While consuming: frontend periodically tracks position (page number or seconds)
3. POST /api/materials/{MaterialId}/progress  Body: { position }
4. Server updates only if new position > stored position (no going backwards)
5. To check current progress: GET /api/materials/{MaterialId}/projection
6. Returns: material metadata + last saved position for resume
```

---

## 7. AI Study Session

### Starting
```
1. Student clicks "Start AI Study Session" from course page
2. POST /api/study-sessions  Body: { courseId }
3. Server creates session, returns sessionId
4. Frontend navigates to study session page with tabbed interface:
   Chat | Flashcards | Mind Map | Quiz | Summary | Dialogue Audio
```

### Resuming
```
1. GET /api/study-sessions?CourseId={id}&Page=1  → list of previous sessions
2. Student clicks a session → GET /api/study-sessions/{SessionId}  → full detail
3. Session loads with all previously generated content (flashcards, quizzes, etc.)
```

### Ending
```
1. Student clicks "End Session"
2. POST /api/study-sessions/{SessionId}/end
3. Session marked as ended; no further AI operations allowed
4. Redirect to course page
```

---

## 8. Study Session — Chat

RAG-powered chat with SSE streaming responses.

```
1. Load chat history: GET /api/study-sessions/{SessionId}/chat?Page=1&PageSize=50
2. Student types message
3. Optional: select specific lectureIds / materialIds for focused context
4. POST /api/study-sessions/{SessionId}/chat
   Body: { message, lectureIds?, materialIds? }
   Accept: text/event-stream
5. Server:
   a. Retrieves relevant material chunks via RAG (vector similarity search)
   b. Builds prompt with context + conversation history
   c. Streams LLM response token-by-token
6. Frontend reads SSE stream:
   - Each event: data: {"content": "token", "done": false}
   - Final event: data: {"content": "", "done": true, "sources": [...]}
7. Frontend appends each token to AI message bubble in real-time
8. On done=true, re-enable input and display source references
```

---

## 9. Study Session — Flashcards

```
1. Student fills form: topic (required), numberOfCards (default 10), lectureIds?, materialIds?
2. POST /api/study-sessions/{SessionId}/flashcards
3. Server retrieves relevant material via RAG, generates flashcards with LLM
4. Returns List<FlashcardDto> with front/back pairs
5. Frontend displays interactive flip cards with navigation
6. Previously generated: GET /api/study-sessions/{SessionId}/flashcards
```

---

## 10. Study Session — Mind Maps

```
1. Student fills form: centralTopic (required), maxDepth (default 3), lectureIds?, materialIds?
2. POST /api/study-sessions/{SessionId}/mindmaps
3. Server generates mind map structure with LLM
4. Returns MindMapDto with:
   - nodes: JSON string (recursive tree structure) — must JSON.parse()
   - connections: JSON string (edge array) — must JSON.parse()
5. Frontend renders with graph visualization library (ReactFlow, d3-tree)
6. Previously generated: GET /api/study-sessions/{SessionId}/mindmaps
```

---

## 11. Study Session — Practice Quiz

```
1. Student fills form: topic, numberOfQuestions (default 5), difficulty (default "medium"),
   questionTypes (default ["mcq"]), lectureIds?, materialIds?
2. POST /api/study-sessions/{SessionId}/quizzes
3. Server generates quiz with LLM, returns GeneratedQuizDto
   - questions field is a JSON string — must JSON.parse()
   - Each question: questionText, questionType, options, correctAnswer, explanation, difficulty
4. Student answers questions in UI
5. POST /api/study-sessions/{SessionId}/quizzes/{QuizId}/submit
   Body: { answers: { "0": "B", "1": "True", ... } }  (index → answer string)
6. Server grades: MCQ/TrueFalse auto-graded, ShortAnswer/Essay AI-graded
7. Returns QuizResultDto with per-question results + overall score
8. Previously generated: GET /api/study-sessions/{SessionId}/quizzes
```

---

## 12. Study Session — Summary

```
1. Student fills form: topic (required), summaryLength (default 500),
   includeKeyPoints (default true), lectureIds?, materialIds?
2. POST /api/study-sessions/{SessionId}/summary
3. Server retrieves relevant material via RAG, generates summary with LLM
4. Returns Summary object with markdown content, key points, key terms
5. Frontend renders with markdown renderer
```

---

## 13. Study Session — Dialogue Audio

AI-generated teacher-student dialogue synthesized into audio with TTS.

```
1. Optional: Configure voice preferences first (see Flow 28)
2. Student fills form:
   - topic (optional, auto-derived from course)
   - audienceLevel (default "intermediate")
   - numberOfExchanges (default 5)
   - dialogueLength (default "medium")
   - includeExamples, includeSummary, teachingStyle
   - focusConcepts?, lectureIds?, materialIds?
   - teacherVoiceId?, studentVoiceId?, teacherSpeed?, studentSpeed? (per-request overrides)
3. POST /api/study-sessions/{SessionId}/dialogue-audio
4. Server:
   a. Retrieves relevant material via RAG
   b. Generates dialogue script with LLM
   c. Synthesizes each turn with XTTS v2 TTS
   d. Merges audio with pauses, normalizes
5. Returns DialogueAudioResponseDto:
   - audioBase64: full audio as base64 string
   - turnTimestamps: array of { startTime, endTime, speaker, text }
   - exchanges: dialogue transcript
   - format, sampleRate, totalDurationMs
6. Frontend:
   a. Decode audioBase64 → Blob URL → <audio> player
   b. Sync transcript highlighting with playback using turnTimestamps
   c. Highlight current speaker/text as audio plays
```

---

## 14. Semantic Section Tools

Materials are automatically divided into semantic sections during indexing. Students can generate study tools from specific sections.

### View Sections
```
1. GET /api/materials/{MaterialId}/sections
2. Returns List<SemanticSectionDto> ordered by position
3. Each section has: id, title, content summary, position
```

### Section → Summary
```
1. POST /api/sessions/{SessionId}/sections/{SectionId}/summarize
   Body: { summaryLength?, includeKeyPoints? }
2. Returns summary focused on that specific section
```

### Section → Flashcards
```
1. POST /api/sessions/{SessionId}/sections/{SectionId}/flashcards
   Body: { numberOfCards? }
2. Returns flashcards generated from the section content
```

### Section → Quiz
```
1. POST /api/sessions/{SessionId}/sections/{SectionId}/quiz
   Body: { numberOfQuestions?, difficulty?, questionTypes? }
2. Returns quiz generated from the section content
```

---

## 15. Exam Taking & Submission

```
1. Student discovers available exams:
   - GET /api/exams/available  → all exams across enrolled courses
   - GET /api/exams/active/{CourseId}  → currently active for a course
   - GET /api/exams/upcoming/{CourseId}  → future exams
2. Student starts exam:
   - GET /api/exams/{ExamId}  → exam details + questions
   - GET /api/exams/{ExamId}/total-points  → total possible score
3. Exam-taking UI:
   - Countdown timer (durationMinutes)
   - Question navigator (answered vs unanswered)
   - Auto-save answers to localStorage
   - Hide correctAnswer field from display
4. Student clicks "Submit" or timer expires:
   - POST /api/exams/{ExamId}/submit
     Body: { answers: { "questionId1": "B", "questionId2": "True", ... } }
5. Server creates Submission
6. Teacher receives "ExamSubmitted" SignalR notification
7. Student sees confirmation: "Submitted! You'll be notified when graded."
```

---

## 16. Viewing Grades & Submissions

### Student
```
1. My Submissions: GET /api/exams/submissions/student  → all submissions
2. Submission detail: GET /api/exams/submissions/{SubmissionId}  → answers + grade (if graded)
3. My Grades: GET /api/exams/grades/student  → all grades
4. Grade detail: GET /api/exams/submissions/{SubmissionId}/grade
5. Grade stats: GET /api/grades/stats/student/{StudentId}?CourseId={optional}
```

### Teacher
```
1. Exam grades: GET /api/exams/{ExamId}/grades
2. Exam stats: GET /api/grades/stats/exam/{ExamId}  → average, median, pass rate
3. Distribution: GET /api/grades/distribution/{ExamId}  → A/B/C/D/F counts
4. Submission stats: GET /api/submissions/stats/{ExamId}
```

---

## 17. Course Reviews

```
1. Student views course → GET /api/courses/{CourseId}/reviews?Page=1
2. Student clicks "Write Review" (must be enrolled, one per course):
   POST /api/courses/{CourseId}/reviews
   Body: { rating: 1-5, comment?: string }
3. Teacher receives "NewReview" SignalR notification
4. Rating summary updates: GET /api/courses/{CourseId}/rating
5. Edit review: PUT /api/reviews/{ReviewId}  (author only)
6. Delete review: DELETE /api/reviews/{ReviewId}  (author or course instructor)
```

---

## 18. Course Creation & Management

```
1. Teacher creates course:
   POST /api/courses  (multipart/form-data)
   Fields: title, description, price, categoryId?, thumbnail?
   Returns: { courseId } — course is UNPUBLISHED

2. Teacher edits course:
   PUT /api/courses/{CourseId}  (multipart/form-data)

3. Teacher assigns categories:
   POST /api/courses/categories  Body: { courseId, categoryId }
   DELETE /api/courses/{CourseId}/categories/{CategoryId}

4. Teacher publishes course:
   POST /api/courses/{CourseId}/publish
   Students in course group receive "CoursePublished" notification

5. My courses: GET /api/courses/my-courses?IncludeUnpublished=true

6. Delete course: DELETE /api/courses/{CourseId}
```

---

## 19. Lecture & Material Management

### Lectures
```
1. Add lecture: POST /api/courses/{CourseId}/lectures
   Body: { title, description, orderIndex }
   Students receive "NewLectureAdded" SignalR notification

2. Get lectures: GET /api/courses/{CourseId}/lectures?IncludeMaterials=true

3. Update lecture: PUT /api/courses/lectures/{LectureId}
   Body: { title, description, orderIndex }

4. Delete lecture: DELETE /api/courses/lectures/{LectureId}
```

### Materials
```
1. Upload materials (bulk):
   POST /api/courses/lectures/{LectureId}/materials  (multipart/form-data)
   Fields: files (multiple), titles? (comma-separated)
   Students receive "NewMaterialUploaded" SignalR notification

2. After upload → background indexing begins (see Flow 20)

3. Get lecture materials: GET /api/courses/lectures/{LectureId}/materials

4. Delete material: DELETE /api/courses/materials/{MaterialId}
```

---

## 20. Material Indexing

Automatic background process after material upload.

```
1. Material is uploaded (files stored on disk)
2. Background service picks up the material for processing
3. Processing pipeline:
   a. Extract text content (PDF text extraction, audio transcription, etc.)
   b. Split into semantic sections (SemanticSection entities)
   c. Split into chunks (MaterialChunk entities)
   d. Generate embeddings for each chunk (via Ollama or Groq)
   e. Store embeddings in PostgreSQL pgvector
4. On completion:
   - Teacher receives "ReceiveIndexingNotification" via MaterialIndexingHub
   - Notification includes: success, chunksIndexed, indexTimeMs, embeddingTimeMs, chunksFailed
5. Material is now searchable for RAG queries in study sessions
```

---

## 21. Exam & Question Management

```
1. Create exam:
   POST /api/courses/{CourseId}/exams
   Body: { title, startTime, endTime, durationMinutes }
   Students receive "NewExamPosted" SignalR notification

2. Add questions (3 methods):
   a. Single: POST /api/exams/{ExamId}/questions
      Body: { type, text, options?, correctAnswer, points }
   b. Bulk: POST /api/exams/{ExamId}/questions/bulk
      Body: { questions: [...] }
   c. AI: POST /api/exams/{ExamId}/questions/generate-ai  (see Flow 22)

3. View questions: GET /api/exams/{ExamId}/questions
4. Edit question: PUT /api/exams/questions/{QuestionId}
5. Delete question: DELETE /api/exams/questions/{QuestionId}
6. Reorder: POST /api/exams/{ExamId}/questions/reorder
   Body: { questionOrders: { "questionId1": 1, "questionId2": 2 } }

7. Update exam: PUT /api/exams/{ExamId}
8. Delete exam: DELETE /api/exams/{ExamId}
   Students receive "ExamDeleted" SignalR notification
```

---

## 22. AI Question Generation

```
1. Teacher configures generation parameters:
   - numberOfQuestions (required)
   - difficulty? (easy/medium/hard)
   - questionTypes? (MultipleChoice, TrueFalse, ShortAnswer, Essay, FillInTheBlank)
   - focusTopics? (specific topics to focus on)
   - lectureIds? / materialIds? (specific source materials)

2. POST /api/exams/{ExamId}/questions/generate-ai

3. Server:
   a. Retrieves course materials (filtered by lectureIds/materialIds if provided)
   b. Builds prompt with topic, difficulty, and question type constraints
   c. Calls LLM to generate questions
   d. Parses LLM output into structured Question entities
   e. Saves questions to database

4. Returns GenerateAIQuestionsResult with generated questions
5. Teacher reviews, edits, or deletes individual questions as needed
```

---

## 23. Grading Workflow

### Manual Grading
```
1. Teacher views ungraded submissions:
   GET /api/exams/submissions/ungraded?ExamId={optional}

2. Teacher selects a submission:
   GET /api/exams/submissions/{SubmissionId}
   → Shows student answers alongside questions

3. Teacher evaluates and enters grade:
   POST /api/exams/submissions/{SubmissionId}/grade
   Body: { score, feedback }

4. Student receives "SubmissionGraded" SignalR notification
```

### AI Grading
```
1. Teacher clicks "AI Grade" on a submission:
   POST /api/exams/submissions/{SubmissionId}/grade-ai

2. Server:
   a. Loads submission answers + questions (with model answers and grading criteria)
   b. Sends to LLM for evaluation
   c. LLM returns per-question scores + feedback
   d. Creates Grade entity marked as AI-graded, NOT approved

3. Returns GradeSubmissionWithAIResult

4. Grade appears in "Pending Approval" list:
   GET /api/exams/grades/pending-approval?ExamId={optional}

5. Teacher reviews AI grade:
   a. Approve as-is: POST /api/exams/grades/{GradeId}/approve
   b. Modify then approve: PUT /api/exams/grades/{GradeId} → then approve
   c. Discard and grade manually instead

6. On approval: Student receives "GradeApproved" SignalR notification
7. On update: Student receives "GradeUpdated" SignalR notification
```

---

## 24. Notifications

### Real-Time (SignalR)
```
1. On login, frontend connects to:
   - StudentNotificationHub (/hubs/student-notifications) — all users
   - MaterialIndexingHub (/hubs/material-indexing) — teachers only

2. Student joins course groups:
   studentConnection.invoke("JoinCourseGroup", courseId)
   (repeat for each enrolled course)

3. Events received by students:
   Course-wide: NewExamPosted, NewMaterialUploaded, NewLectureAdded,
                CourseUpdated, CoursePublished, ExamUpdated, ExamDeleted
   Individual: SubmissionGraded, GradeApproved, GradeUpdated, EngagementAlert

4. Events received by teachers:
   ReceiveIndexingNotification, ExamSubmitted, NewEnrollment,
   NewReview, EnrollmentCompleted, StudentUnenrolled

5. On reconnect: re-join all course groups (groups lost on disconnect)
```

### Persistent Notifications
```
1. List notifications: GET /api/notifications?Page=1&PageSize=20&UnreadOnly=false
2. Unread count (for badge): GET /api/notifications/unread-count
3. Mark as read: PUT /api/notifications/{Id}/read
4. Mark all as read: PUT /api/notifications/read-all
5. Delete: DELETE /api/notifications/{Id}
```

---

## 25. Student Engagement Monitoring

```
1. Teacher views engagement report:
   GET /api/courses/{CourseId}/engagement
   Returns per-student metrics sorted by engagement (lowest first)

2. Engagement levels:
   - Critical (0-25%): Immediate attention
   - Low (26-50%): At risk
   - Moderate (51-75%): Adequate
   - High (76-100%): Actively engaged

3. Metrics include: sessions count, last active date, submission rate, average grade

4. Send individual alert:
   POST /api/courses/{CourseId}/engagement/alerts
   Body: { studentIds: [guid], customMessage?: "..." }

5. Send bulk alert (all at-risk):
   POST /api/courses/{CourseId}/engagement/alerts
   Body: { studentIds: null, customMessage?: "..." }
   → Server auto-targets Critical + Low students

6. Each targeted student receives "EngagementAlert" via SignalR
7. Returns send result with count + targeted student names
```

---

## 26. Category Management

```
1. List categories: GET /api/categories?SearchTerm={optional}
2. Get category: GET /api/categories/{CategoryId}
3. Create category: POST /api/categories  Body: { name, description? }  (Teacher)
4. Update category: PUT /api/categories/{CategoryId}  Body: { name, description? }  (Teacher)
5. Delete category: DELETE /api/categories/{CategoryId}  (Teacher)

6. Associate course with category:
   POST /api/courses/categories  Body: { courseId, categoryId }  (Teacher)
7. Remove association:
   DELETE /api/courses/{CourseId}/categories/{CategoryId}  (Teacher)
```

---

## 27. AI Provider Switching

```
1. Check current provider:
   GET /api/ai/provider
   Returns: { activeProvider, supportedProviders, isGroqConfigured }

2. Switch provider:
   POST /api/ai/provider/switch  Body: { provider: "ollama" | "groq" }

3. Validation:
   - Groq requires API key to be configured
   - Ollama requires local service running

4. Affects ALL AI features: chat, flashcards, quizzes, mind maps,
   summaries, dialogue audio, AI grading, AI question generation
```

---

## 28. Voice Settings Configuration

```
1. Get available voices: GET /api/dialogue/voices
2. Preview voices: GET /api/dialogue/voice-previews?VoiceId={id}&SampleText={text}
3. Get defaults: GET /api/dialogue/voice-config/default
4. Get supported formats: GET /api/dialogue/supported-formats
5. Get supported languages: GET /api/dialogue/supported-languages

6. Get current settings: GET /api/dialogue/voice-settings
   (returns defaults if none saved)

7. Save settings: PUT /api/dialogue/voice-settings
   Body: { teacherVoiceId?, studentVoiceId?, teacherSpeed?, studentSpeed?,
           outputFormat?, sampleRate?, includePauses?, pauseDurationMs?,
           pauseMultiplier?, normalizeAudio? }

8. Reset to defaults: DELETE /api/dialogue/voice-settings
```

---

## 29. Unenrollment & Refund

```
1. Student clicks "Unenroll" on enrolled course
2. DELETE /api/courses/{CourseId}/unenroll
3. Server checks:
   - Enrollment exists and is Active
   - 10-day refund window (from enrollment date)
   - Progress-based refund calculation for paid courses
4. Returns UnenrollmentResultDto:
   - refundAmount (if within window)
   - refundPercentage
   - isRefundEligible
5. If refund eligible: Stripe partial refund processed automatically
6. Enrollment status → "Dropped"
7. Teacher receives "StudentUnenrolled" SignalR notification
```

---

## 30. Course Completion

```
1. Student clicks "Complete Course"
2. POST /api/courses/{CourseId}/complete
3. Server marks enrollment status → "Completed"
4. Teacher receives "EnrollmentCompleted" SignalR notification
5. Course appears in student's completed courses
6. Student stats updated (coursesCompleted += 1)
```

---

## 31. User Profile & Dashboard

### Profile
```
1. View profile: GET /api/users/me
2. Update profile: PUT /api/users/me  (multipart/form-data for avatar upload)
   Fields: firstName?, lastName?, userName?, bio?, qualifications?, subjects?,
           gradeLevel?, interests?, avatar? (file), removeAvatar?, website?,
           linkedInUrl?, title?, location?, expertiseAreas?
3. View other user: GET /api/users/{UserId}
4. User stats: GET /api/users/stats?UserId={optional}
```

### Student Dashboard
```
1. GET /api/users/dashboard
   Returns: course progress, engagement, exam stats, grade trends, submission history
2. Continue learning: GET /api/courses/continue-learning
   Returns: in-progress courses with resume position
```

### Teacher Dashboard
```
1. GET /api/users/teacher/dashboard
   Returns: totalCourses, publishedCourses, totalStudentsEnrolled, totalExamsCreated,
            pendingGradeApprovals, ungradedSubmissions, recentEnrollments,
            coursePerformance, enrollmentTrend
```

---

## 32. Token Lifecycle

```
┌─ Login ──────────────────────────────────────────────────────┐
│ POST /api/auth/login                                         │
│ → { accessToken (short-lived), refreshToken (long-lived) }   │
└──────────────────────────────────────────────────────────────┘
         │
         ▼
┌─ Normal API Calls ───────────────────────────────────────────┐
│ Authorization: Bearer {accessToken}                          │
└──────────────────────────────────────────────────────────────┘
         │
    401 response
         │
         ▼
┌─ Refresh ────────────────────────────────────────────────────┐
│ POST /api/auth/refresh-token                                 │
│ Body: { accessToken, refreshToken }                          │
│ → new { accessToken, refreshToken }                          │
│ Retry original request                                       │
└──────────────────────────────────────────────────────────────┘
         │
    refresh fails
         │
         ▼
┌─ Force Logout ───────────────────────────────────────────────┐
│ Clear tokens → redirect to login                             │
└──────────────────────────────────────────────────────────────┘
```

---

> **Document Version:** 1.0
> **Last Updated:** March 2025
