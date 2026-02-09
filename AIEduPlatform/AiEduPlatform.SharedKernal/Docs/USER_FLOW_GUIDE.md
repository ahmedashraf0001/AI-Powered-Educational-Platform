# AI Educational Platform — User Flow Guide

> Comprehensive documentation of all implemented features, user flows, and system behavior.

---

## Table of Contents

1. [System Overview](#system-overview)
2. [User Roles & Identity](#user-roles--identity)
3. [Authentication](#authentication)
4. [Course Management (Teacher)](#course-management-teacher)
5. [Student Enrollment](#student-enrollment)
6. [Study Sessions (AI-Powered)](#study-sessions-ai-powered)
7. [Exam System](#exam-system)
8. [Grading System](#grading-system)
9. [API Endpoint Reference](#api-endpoint-reference)

---

## System Overview

The AI Educational Platform is a Udemy-style learning platform enhanced with AI-powered study tools. Users can create courses (as teachers), enroll in courses (as students), and leverage AI features like chat, flashcard generation, mind maps, quizzes, and summaries — all grounded in the actual course materials via **Retrieval-Augmented Generation (RAG)**.

### Architecture

- **Backend**: .NET 10, ASP.NET Core with FastEndpoints
- **CQRS**: MediatR with FluentValidation
- **Database**: PostgreSQL with pgvector (for embedding-based RAG)
- **AI**: Local Ollama LLM with RAG pipeline (embeddings, chunking, reranking)
- **Auth**: ASP.NET Core Identity with JWT + Refresh Tokens

---

## User Roles & Identity

The platform follows the **Udemy model** — there is a single user account, and any user can be both a student and a teacher simultaneously.

| Aspect | Details |
|---|---|
| **Registration** | Users register with email, username, password, and optional first/last name. There is **no role selection** at registration. |
| **Becoming a Teacher** | Any user becomes a teacher simply by creating their first course. No approval process or role switch is needed. |
| **Being a Student** | Any user can browse and enroll in courses created by others. |
| **Dual Role** | A user can publish their own courses (teacher) while also being enrolled in and studying other users' courses (student) at the same time. |

### What This Means in Practice

- The `Course` entity has a `TeacherId` — whoever creates the course is the teacher for that course.
- Enrollment checks are per-course. A teacher of course A can enroll as a student in course B.
- All AI study features are available to enrolled students of a course.

---

## Authentication

All endpoints (except register, login, and refresh token) require a valid JWT Bearer token.

### Flows

#### Register
- **POST** `/api/auth/register`
- Body: `{ email, userName, password, confirmPassword, firstName?, lastName? }`
- Returns a success message. User should verify their email.

#### Login
- **POST** `/api/auth/login`
- Body: `{ email, password }`
- Returns: `{ accessToken, refreshToken, expiresAt }`
- The access token is a JWT used in the `Authorization: Bearer <token>` header.

#### Refresh Token
- **POST** `/api/auth/refresh`
- Body: `{ refreshToken }`
- Returns a new access/refresh token pair when the access token expires.

#### Logout
- **POST** `/api/auth/logout`
- Invalidates the current refresh token on the server.

---

## Course Management (Teacher)

Any authenticated user can create a course, which makes them the teacher for that course.

### Course Hierarchy

```
Course
├── Lectures (ordered content units)
│   └── Materials (PDFs, videos, documents, etc.)
└── Exams (assessments with questions)
```

### Course Flows

#### Create a Course
- **POST** `/api/courses`
- Body: `{ title, description }`
- The authenticated user becomes the `TeacherId` of the course.
- The course starts in an unpublished state (`IsPublished = false`).

#### View Courses
- **GET** `/api/courses` — Browse all published courses.
- **GET** `/api/courses/{courseId}` — Get course details by ID.
- **GET** `/api/courses/my` — Get courses created by the current user (teacher view).

#### Delete a Course
- **DELETE** `/api/courses/{courseId}`
- Only the course teacher can delete. Cascades to lectures, materials, exams, enrollments, study sessions, and all related AI-generated content.

### Lecture Flows

#### Add a Lecture
- **POST** `/api/courses/{courseId}/lectures`
- Only the course teacher can add lectures.
- Body: `{ title, description, orderIndex }`

#### View Lectures
- **GET** `/api/courses/{courseId}/lectures`
- Returns all lectures for a course, ordered by `OrderIndex`.

#### Delete a Lecture
- **DELETE** `/api/courses/lectures/{lectureId}`
- Only the course teacher. Cascades to all materials under the lecture.

### Material Flows

Materials are the actual learning content (PDFs, videos, documents) that the AI system processes for RAG.

#### Upload a Material
- **POST** `/api/courses/lectures/{lectureId}/materials`
- Multipart form: `{ title, type (PDF/Video/Document/etc.), file?, fileUrl? }`
- When a file (especially PDF) is uploaded, the system:
  1. Stores the file
  2. Extracts text content
  3. Chunks the content into smaller pieces
  4. Generates vector embeddings for each chunk
  5. Stores chunks and embeddings in PostgreSQL (pgvector) for RAG retrieval
- This is what makes the AI features work — the AI can search through actual course content.

#### View Materials
- **GET** `/api/courses/lectures/{lectureId}/materials`
- Returns all materials for a lecture.

#### Delete a Material
- **DELETE** `/api/courses/materials/{materialId}`
- Only the course teacher. Also removes associated content chunks and embeddings.

---

## Student Enrollment

Students must enroll in a course before they can access study sessions or take exams.

#### Enroll in a Course
- **POST** `/api/courses/{courseId}/enroll`
- Any authenticated user can enroll (except the course teacher, optionally).
- Returns the enrollment ID.

#### View Enrollments
- **GET** `/api/enrollments/my` — Get courses the current user is enrolled in.
- **GET** `/api/courses/{courseId}/enrollments` — (Teacher) View all students enrolled in a course.

---

## Study Sessions (AI-Powered)

Study sessions are the core AI-powered learning feature. A study session is **scoped to a course** — it gives the student access to AI tools that are grounded in that course's materials via RAG.

### How Sessions Work

1. A student starts a study session for a specific course.
2. The session is long-lived — if an active session already exists for that course, it is reused.
3. Sessions automatically track `LastActivity` — every AI interaction updates the timestamp.
4. There is **no explicit "end session"** action — sessions are simply reused or become stale naturally.
5. All AI features within a session are powered by RAG against the course's uploaded materials.

### RAG Scoping (Lecture/Material Filtering)

All AI features accept optional `lectureId` and `materialIds` parameters. When provided, the RAG retrieval is scoped to only the specified lecture or materials instead of the entire course. This is useful when the student is viewing a specific lecture and wants AI answers grounded only in that lecture's materials.

- **`lectureId`** (optional GUID) — Filters RAG to materials within this specific lecture.
- **`materialIds`** (optional list of GUIDs) — Filters RAG to these specific materials.
- If neither is provided, RAG searches all materials in the course.

### Starting a Session

- **POST** `/api/study-sessions`
- Body: `{ courseId }`
- Requires enrollment in the course.
- If an active session already exists for this student + course, the existing session ID is returned (not a new one).
- Returns the session ID (GUID).

### Viewing Sessions

- **GET** `/api/study-sessions` — List all sessions for the current student. Optional `?courseId=` filter.
- **GET** `/api/study-sessions/{sessionId}` — Get session details including counts of messages, flashcards, quizzes, and mind maps.
- **GET** `/api/study-sessions/stats` — Get aggregated statistics: total sessions, messages, flashcards, quizzes, mind maps, total study time, and last session date. Optional `?courseId=` filter.

---

### AI Chat (Streaming via SSE)

The chat feature allows students to ask questions about the course material. The AI response is **streamed in real-time via Server-Sent Events (SSE)**, so the student sees tokens appear progressively (like ChatGPT).

#### Send a Chat Message
- **POST** `/api/study-sessions/{sessionId}/chat`
- Body: `{ "message": "Explain the concept of polymorphism", "lectureId?": "guid", "materialIds?": ["guid"] }`
- **Response**: `Content-Type: text/event-stream`

#### How Streaming Works

1. The student sends a message.
2. The system performs **RAG retrieval** — searches course material chunks for relevant context.
3. The last 20 chat messages are loaded as conversation history (for context continuity).
4. The student's message is persisted immediately.
5. The response is streamed to the client as SSE events:

```
data: {"content":"Polymorphism ","done":false}

data: {"content":"is a concept ","done":false}

data: {"content":"in object-oriented programming...","done":false}

data: {"content":"","done":true,"sources":["Lecture 3 - OOP Concepts.pdf"]}
```

6. Each chunk contains:
   - `content` — the text fragment generated by the AI
   - `done` — `false` for content chunks, `true` for the final event
   - `sources` — (only on the final event) list of source material titles used for context
7. After streaming completes, the full AI response is persisted as a chat message.
8. If an error occurs mid-stream, an error event is sent: `{"content":"","done":true,"error":"..."}`

#### Frontend Consumption (POST + Streaming)

Since this is a POST endpoint (not GET), the browser's `EventSource` API cannot be used. The frontend should use `fetch` with streaming:

```javascript
const response = await fetch(`/api/study-sessions/${sessionId}/chat`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({ message: userMessage })
});

const reader = response.body.getReader();
const decoder = new TextDecoder();

while (true) {
  const { done, value } = await reader.read();
  if (done) break;

  const text = decoder.decode(value);
  // Parse SSE lines: each event is "data: {...}\n\n"
  const lines = text.split('\n');
  for (const line of lines) {
    if (line.startsWith('data: ')) {
      const data = JSON.parse(line.slice(6));
      if (data.error) { /* handle error */ }
      else if (data.done) { /* final event — extract sources */ }
      else { /* append data.content to the chat bubble */ }
    }
  }
}
```

#### View Chat History
- **GET** `/api/study-sessions/{sessionId}/chat`
- Returns all persisted chat messages (both student and AI) for the session.
- Each message includes: `id`, `role` (Student/System), `content`, `sources`, `createdAt`.

---

### AI Flashcard Generation

Generate flashcards from course materials on a given topic. Flashcards are persisted for later review.

#### Generate Flashcards
- **POST** `/api/study-sessions/{sessionId}/flashcards`
- Body: `{ "topic": "Data Structures", "numberOfCards": 10, "lectureId?": "guid", "materialIds?": ["guid"] }`
- The system:
  1. Performs RAG retrieval for the topic.
  2. Sends context + topic to the AI to generate flashcards.
  3. Persists each flashcard (front text, back text, topic, difficulty).
- Returns: list of `FlashcardDto` (id, front, back, topic, difficulty).
- **Note**: The AI generates the entire response as a single JSON block — this is **not streamed**.

#### View Session Flashcards
- **GET** `/api/study-sessions/{sessionId}/flashcards`
- Returns all flashcards generated in this session.

---

### AI Quiz Generation & Submission

Generate quizzes with multiple question types, then submit answers for automatic grading.

#### Generate a Quiz
- **POST** `/api/study-sessions/{sessionId}/quizzes`
- Body: `{ "topic": "Algorithms", "numberOfQuestions": 5, "difficulty": "Medium", "lectureId?": "guid", "materialIds?": ["guid"] }`
- Difficulty options: `Easy`, `Medium`, `Hard`.
- Question types generated: `MultipleChoice`, `TrueFalse`, `ShortAnswer`, `Essay`, `FillInTheBlank`.
- The system:
  1. Performs RAG retrieval for the topic (scoped to lectureId/materialIds if provided).
  2. AI generates quiz questions with correct answers and explanations.
  3. Quiz is persisted with questions stored as JSON.
- Returns: `GeneratedQuizDto` (id, topic, difficulty, questions).
- **Note**: Generated as a single JSON response — **not streamed**.

#### Submit Quiz Answers
- **POST** `/api/study-sessions/{sessionId}/quizzes/{quizId}/submit`
- Body: `{ "answers": { "0": "answer for Q1", "1": "B", "2": "true" } }`
- Keys are question indices (0-based), values are the student's answers.
- **Grading behavior depends on question type:**
  - **MCQ / TrueFalse / FillInTheBlank** → Exact string comparison (case-insensitive). Score is 100% or 0%.
  - **ShortAnswer / Essay** → **AI-reviewed grading** via `GradeEssayAsync`. The AI evaluates the student's answer against the model answer and course context, returning a percentage score and detailed feedback.
- Returns: `QuizResultDto` with overall score percentage and per-question results:
  - `isCorrect` — For MCQ: exact match. For written: AI score ≥ 50%.
  - `aiScore` — (written questions only) AI-assigned percentage score.
  - `aiFeedback` — (written questions only) Detailed AI feedback on the answer.

#### View Session Quizzes
- **GET** `/api/study-sessions/{sessionId}/quizzes`
- Returns all quizzes generated in this session, including scores if submitted.

---

### AI Mind Map Generation

Generate visual mind maps from course materials on a topic.

#### Generate a Mind Map
- **POST** `/api/study-sessions/{sessionId}/mindmaps`
- Body: `{ "topic": "Machine Learning Basics", "lectureId?": "guid", "materialIds?": ["guid"] }`
- The system:
  1. Performs RAG retrieval for the topic.
  2. AI generates a hierarchical mind map structure (nodes with labels, descriptions, and children).
  3. Nodes and connections are persisted as JSON.
- Returns: `MindMapDto` (id, topic, nodesJson, connectionsJson).
- **Note**: Generated as a single JSON response — **not streamed**.

#### View Session Mind Maps
- **GET** `/api/study-sessions/{sessionId}/mindmaps`
- Returns all mind maps generated in this session.

---

### AI Summary Generation

Generate summaries of course material on a specific topic.

#### Generate a Summary
- **POST** `/api/study-sessions/{sessionId}/summary`
- Body: `{ "topic": "Neural Networks", "summaryLength": 500, "includeKeyPoints": true, "lectureId?": "guid", "materialIds?": ["guid"] }`
- The system:
  1. Performs RAG retrieval for the topic (scoped to lectureId/materialIds if provided).
  2. AI generates a summary with content, key points, and key terms.
- Returns: `Summary` (content, keyPoints, keyTerms, sourceTitle, originalLength, summaryLength).
- **Note**: Generated as a single JSON response — **not streamed**. The summary is **not persisted** to the database (it's returned directly).

---

## Exam System

Teachers can create formal exams for their courses. Students take exams within a time window.

### Teacher Exam Flows

#### Create an Exam
- **POST** `/api/exams`
- Body: `{ courseId, title, startTime, endTime, durationMinutes }`
- Only the course teacher can create exams.
- `startTime` / `endTime` define when the exam is available. `durationMinutes` is the per-student time limit.

#### Add Questions to an Exam
- **POST** `/api/exams/{examId}/questions`
- Body: `{ questionText, questionType, options?, correctAnswer, explanation?, points }`
- Question types: `MultipleChoice`, `TrueFalse`, `ShortAnswer`, `Essay`, `FillInTheBlank`.

#### Add Questions in Bulk
- **POST** `/api/exams/{examId}/questions/bulk`
- Body: array of question objects.

#### AI-Generated Questions
- **POST** `/api/exams/{examId}/questions/generate`
- The AI generates exam questions from the course materials (RAG-powered).
- Teacher reviews and can add them to the exam.

#### View Exam Questions
- **GET** `/api/exams/{examId}/questions`
- Teacher view — includes correct answers and explanations.

#### Delete a Question
- **DELETE** `/api/exams/questions/{questionId}`

#### Delete an Exam
- **DELETE** `/api/exams/{examId}`
- Cascades to all questions and submissions.

### Student Exam Flows

#### View Available Exams
- **GET** `/api/exams/available?courseId={courseId}`
- Returns exams the student can take (within the time window, not yet submitted).

#### View Exam Details
- **GET** `/api/exams/{examId}`
- Returns exam info. Questions are returned **without correct answers** for students.

#### Submit an Exam
- **POST** `/api/exams/{examId}/submit`
- Body: `{ "answers": { "questionId1": "answer1", "questionId2": "B" } }`
- Keys are question GUIDs, values are the student's answers.
- Answers are persisted as a submission. Auto-grading runs for objective question types (MCQ, TrueFalse). Essay questions are flagged for manual review.

#### View Submissions
- **GET** `/api/submissions/my` — Get the current student's exam submissions.
- **GET** `/api/submissions/{submissionId}` — Get a specific submission.
- **GET** `/api/exams/{examId}/submissions` — (Teacher) View all submissions for an exam.
- **GET** `/api/submissions/ungraded` — (Teacher) View submissions pending manual grading.

---

## Grading System

Exam submissions are graded through a combination of automatic and manual processes.

### Auto-Grading
- Objective question types (MultipleChoice, TrueFalse, FillInTheBlank) are graded automatically when the exam is submitted.
- Essay and ShortAnswer questions may be AI-assisted graded, but require teacher approval.

### Teacher Grading Flows

#### View Grades
- **GET** `/api/exams/{examId}/grades` — View all grades for an exam.
- **GET** `/api/grades/pending` — View grades pending teacher approval.

#### Approve a Grade
- **POST** `/api/exams/grades/{gradeId}/approve`
- Teacher reviews AI-suggested grades and approves or adjusts.

#### Student Grade View
- **GET** `/api/grades/my` — View the current student's grades across all exams.
- **GET** `/api/grades/submission/{submissionId}` — View grade for a specific submission.

---

## API Endpoint Reference

### Auth (`/api/auth`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login and get tokens |
| POST | `/api/auth/refresh` | Refresh access token |
| POST | `/api/auth/logout` | Invalidate refresh token |

### Courses (`/api/courses`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/courses` | Create a course |
| GET | `/api/courses` | List all courses |
| GET | `/api/courses/{courseId}` | Get course by ID |
| GET | `/api/courses/my` | Get my courses (teacher) |
| GET | `/api/courses/instructor/{instructorId}` | Get courses by instructor |
| DELETE | `/api/courses/{courseId}` | Delete a course |

### Enrollments
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/courses/{courseId}/enroll` | Enroll in a course |
| GET | `/api/enrollments/my` | Get my enrolled courses |
| GET | `/api/courses/{courseId}/enrollments` | Get course enrollments (teacher) |

### Lectures
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/courses/{courseId}/lectures` | Add a lecture |
| GET | `/api/courses/{courseId}/lectures` | Get course lectures |
| DELETE | `/api/courses/lectures/{lectureId}` | Delete a lecture |

### Materials
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/courses/lectures/{lectureId}/materials` | Upload a material |
| GET | `/api/courses/lectures/{lectureId}/materials` | Get lecture materials |
| DELETE | `/api/courses/materials/{materialId}` | Delete a material |

### Study Sessions (`/api/study-sessions`)
| Method | Endpoint | Description | Response |
|--------|----------|-------------|----------|
| POST | `/api/study-sessions` | Start/resume a session | JSON (session ID) |
| GET | `/api/study-sessions` | List my sessions | JSON |
| GET | `/api/study-sessions/{sessionId}` | Get session details | JSON |
| GET | `/api/study-sessions/stats` | Get study stats | JSON |
| POST | `/api/study-sessions/{sessionId}/chat` | Send chat message | **SSE stream** |
| GET | `/api/study-sessions/{sessionId}/chat` | Get chat history | JSON |
| POST | `/api/study-sessions/{sessionId}/flashcards` | Generate flashcards | JSON |
| GET | `/api/study-sessions/{sessionId}/flashcards` | Get session flashcards | JSON |
| POST | `/api/study-sessions/{sessionId}/quizzes` | Generate a quiz | JSON |
| POST | `/api/study-sessions/{sessionId}/quizzes/{quizId}/submit` | Submit quiz answers | JSON |
| GET | `/api/study-sessions/{sessionId}/quizzes` | Get session quizzes | JSON |
| POST | `/api/study-sessions/{sessionId}/mindmaps` | Generate a mind map | JSON |
| GET | `/api/study-sessions/{sessionId}/mindmaps` | Get session mind maps | JSON |
| POST | `/api/study-sessions/{sessionId}/summary` | Generate a summary | JSON |

### Exams (`/api/exams`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/exams` | Create an exam |
| GET | `/api/exams/{examId}` | Get exam by ID |
| GET | `/api/exams/course/{courseId}` | Get exams for a course |
| GET | `/api/exams/available` | Get available exams (student) |
| GET | `/api/exams/active` | Get active exams |
| DELETE | `/api/exams/{examId}` | Delete an exam |

### Questions (`/api/exams`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/exams/{examId}/questions` | Add a question |
| POST | `/api/exams/{examId}/questions/bulk` | Add questions in bulk |
| POST | `/api/exams/{examId}/questions/generate` | AI-generate questions |
| GET | `/api/exams/{examId}/questions` | Get exam questions |
| DELETE | `/api/exams/questions/{questionId}` | Delete a question |

### Submissions (`/api/exams`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/exams/{examId}/submit` | Submit exam answers |
| GET | `/api/submissions/{submissionId}` | Get submission by ID |
| GET | `/api/submissions/my` | Get my submissions |
| GET | `/api/exams/{examId}/submissions` | Get exam submissions (teacher) |
| GET | `/api/submissions/ungraded` | Get ungraded submissions |

### Grades
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/exams/{examId}/grades` | Get exam grades (teacher) |
| GET | `/api/grades/my` | Get my grades (student) |
| GET | `/api/grades/submission/{submissionId}` | Get grade for submission |
| GET | `/api/grades/pending` | Get pending approval grades |
| POST | `/api/exams/grades/{gradeId}/approve` | Approve a grade (teacher) |

---

## Technical Notes

### RAG (Retrieval-Augmented Generation)

All AI features are grounded in actual course materials. When a student asks a question or requests a summary:

1. The query is converted to a vector embedding.
2. PostgreSQL pgvector performs a similarity search across all content chunks for the course.
3. Optional reranking further filters the most relevant chunks.
4. The relevant chunks are sent to the Ollama LLM as context along with the user's prompt.
5. The AI generates a response that references the actual course content.

### Session Lifecycle

- Sessions are **course-scoped**, not lecture-scoped.
- Sessions are **reused** — starting a session for a course you already have an active session for returns the existing session.
- Sessions track `LastActivity` which is updated automatically on every interaction.
- There is **no explicit end session** — sessions naturally become inactive when the student stops interacting.

### AI Response Delivery

| Feature | Response Delivery |
|---------|-------------------|
| **Chat** | **Server-Sent Events (SSE)** — streamed token-by-token in real-time |
| **Flashcards** | Single JSON response (not streamed) |
| **Quizzes** | Single JSON response (not streamed) |
| **Mind Maps** | Single JSON response (not streamed) |
| **Summaries** | Single JSON response (not streamed) |

### Error Handling

All errors are handled by a global exception handler middleware that returns consistent JSON error responses:

- `400 Bad Request` — Validation errors
- `401 Unauthorized` — Missing or invalid JWT
- `403 Forbidden` — Insufficient permissions (e.g., not the course teacher)
- `404 Not Found` — Resource not found
- `500 Internal Server Error` — Unexpected errors
