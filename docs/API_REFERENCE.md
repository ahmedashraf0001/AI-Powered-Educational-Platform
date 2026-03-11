# AIEduPlatform — API Reference

> **Base URL:** `http://localhost:5000/api` (development) | `https://your-domain.com/api` (production)
>
> **Authentication:** JWT Bearer Token — include `Authorization: Bearer <access_token>` header on all authenticated endpoints.
>
> **Content Type:** `application/json` unless otherwise noted (file uploads use `multipart/form-data`).

---

## Table of Contents

1. [Response Envelope](#1-response-envelope)
2. [Authentication](#2-authentication)
3. [Users](#3-users)
4. [Courses](#4-courses)
5. [Categories](#5-categories)
6. [Enrollments](#6-enrollments)
7. [Lectures](#7-lectures)
8. [Materials](#8-materials)
9. [Cart](#9-cart)
10. [Checkout & Payments](#10-checkout--payments)
11. [Exams](#11-exams)
12. [Questions](#12-questions)
13. [Submissions](#13-submissions)
14. [Grades](#14-grades)
15. [Reviews](#15-reviews)
16. [Notifications](#16-notifications)
17. [Study Sessions](#17-study-sessions)
18. [Semantic Sections](#18-semantic-sections)
19. [AI Provider](#19-ai-provider)
20. [Dialogue & Audio](#20-dialogue--audio)
21. [Enums Reference](#21-enums-reference)
22. [Error Handling](#22-error-handling)

---

## 1. Response Envelope

All endpoints (except file streaming) return responses wrapped in a standard envelope:

```json
{
  "success": true,
  "data": { ... },
  "message": "Optional message"
}
```

| Field     | Type      | Description                                      |
| --------- | --------- | ------------------------------------------------ |
| `success` | `boolean` | `true` if the request succeeded, `false` on error |
| `data`    | `T?`      | The response payload (null on failure)             |
| `message` | `string?` | Human-readable message (especially on errors)      |

### Paginated Responses

Endpoints returning lists use `PagedResult<T>`:

```json
{
  "success": true,
  "data": {
    "items": [ ... ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 47,
    "totalPages": 5,
    "hasPrevious": false,
    "hasNext": true
  }
}
```

| Field         | Type      | Description                           |
| ------------- | --------- | ------------------------------------- |
| `items`       | `T[]`     | Array of results for the current page |
| `page`        | `int`     | Current page number (1-based)         |
| `pageSize`    | `int`     | Items per page                        |
| `totalCount`  | `int`     | Total items across all pages          |
| `totalPages`  | `int`     | Total number of pages                 |
| `hasPrevious` | `boolean` | Whether a previous page exists        |
| `hasNext`     | `boolean` | Whether a next page exists            |

**Default pagination:** `page=1`, `pageSize=20` when not specified (some endpoints default to 10).

---

## 2. Authentication

Authentication uses JWT access tokens + refresh tokens. Access tokens are short-lived; refresh tokens are long-lived and stored in the database. Email verification is required before login.

### JWT Token Claims

| Claim  | Value                                    |
| ------ | ---------------------------------------- |
| `sub`  | User ID (GUID)                           |
| `email`| User email                               |
| `name` | Username                                 |
| `jti`  | Unique token ID                          |
| `role` | One entry per role (`Student`, `Teacher`) |

### Roles

| Role      | Description                          |
| --------- | ------------------------------------ |
| `Student` | Assigned on student registration     |
| `Teacher` | Assigned on teacher registration; users can hold both roles |
| `Admin`   | System administrator role            |

---

### 2.1 Register Student

Creates a new student account. A verification email is sent upon registration.

```
POST /api/auth/register/student
```

**Auth:** None (public)

**Request Body:**

| Field             | Type      | Required | Notes                        |
| ----------------- | --------- | -------- | ---------------------------- |
| `email`           | `string`  | Yes      | Valid email format           |
| `userName`        | `string`  | Yes      | Unique                       |
| `password`        | `string`  | Yes      | Meets identity password rules|
| `confirmPassword` | `string`  | Yes      | Must match `password`        |
| `fullName`        | `string`  | Yes      |                              |

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": null,
  "message": "Registration successful. Please check your email to verify your account."
}
```

---

### 2.2 Register Teacher

Creates a new teacher account. A verification email is sent upon registration.

```
POST /api/auth/register/teacher
```

**Auth:** None (public)

**Request Body:**

| Field             | Type     | Required | Notes                         |
| ----------------- | -------- | -------- | ----------------------------- |
| `email`           | `string` | Yes      | Valid email format            |
| `userName`        | `string` | Yes      | Unique                        |
| `password`        | `string` | Yes      | Meets identity password rules |
| `confirmPassword` | `string` | Yes      | Must match `password`         |
| `fullName`        | `string` | Yes      |                               |
| `bio`             | `string` | Yes      | Teacher biography             |

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": null,
  "message": "Registration successful. Please check your email to verify your account."
}
```

---

### 2.3 Verify Email

Validates the token sent via email and marks the user as verified. Required before login.

```
GET /api/auth/verify-email?Token={token}&Email={email}
```

**Auth:** None (public)

| Parameter | Type     | In    | Required |
| --------- | -------- | ----- | -------- |
| `Token`   | `string` | Query | Yes      |
| `Email`   | `string` | Query | Yes      |

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": null,
  "message": "Email verified successfully. You can now log in."
}
```

---

### 2.4 Login

Authenticates a user and returns JWT tokens.

```
POST /api/auth/login
```

**Auth:** None (public) | **Rate Limited:** Yes (LoginPolicy)

**Request Body:**

| Field      | Type     | Required |
| ---------- | -------- | -------- |
| `email`    | `string` | Yes      |
| `password` | `string` | Yes      |

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "accessTokenExpiration": "2026-03-07T15:30:00Z",
    "refreshTokenExpiration": "2026-04-06T13:30:00Z"
  },
  "message": "Login successful."
}
```

> **Frontend Note:** Store both tokens securely. Use `accessToken` in the `Authorization: Bearer <token>` header. When it expires, use the refresh token endpoint.

---

### 2.5 Refresh Token

Exchanges an expired access token and a valid refresh token for a new token pair.

```
POST /api/auth/refresh-token
```

**Auth:** None (public)

**Request Body:**

| Field          | Type     | Required |
| -------------- | -------- | -------- |
| `accessToken`  | `string` | Yes      |
| `refreshToken` | `string` | Yes      |

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "new-refresh-token-guid"
  }
}
```

---

### 2.6 Logout

Revokes the user's refresh token.

```
POST /api/auth/logout
```

**Auth:** Required

**Request Body:**

| Field          | Type     | Required |
| -------------- | -------- | -------- |
| `refreshToken` | `string` | Yes      |

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": null,
  "message": "Logout successful."
}
```

---

## 3. Users

### 3.1 Get My Profile

```
GET /api/users/me
```

**Auth:** Required

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "email": "user@example.com",
    "userName": "john_doe",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["Student"],
    "bio": null,
    "qualifications": null,
    "subjects": null,
    "gradeLevel": null,
    "interests": null,
    "avatarUrl": null,
    "website": null,
    "linkedInUrl": null,
    "title": null,
    "location": null,
    "expertiseAreas": null,
    "createdAt": "2026-01-10T08:00:00Z",
    "updatedAt": "2026-02-01T12:00:00Z"
  }
}
```

---

### 3.2 Update My Profile

Updates the authenticated user's profile. Supports avatar file upload via `multipart/form-data`.

```
PUT /api/users/me
```

**Auth:** Required | **Content-Type:** `multipart/form-data` (when uploading avatar) or `application/json`

**Request Body:**

| Field            | Type         | Required | Notes                              |
| ---------------- | ------------ | -------- | ---------------------------------- |
| `firstName`      | `string?`    | No       |                                    |
| `lastName`       | `string?`    | No       |                                    |
| `userName`       | `string?`    | No       | Must be unique                     |
| `bio`            | `string?`    | No       | Teacher bio                        |
| `avatarUrl`      | `string?`    | No       | Direct URL (alternative to upload) |
| `avatar`         | `IFormFile?` | No       | Avatar image file upload           |
| `removeAvatar`   | `boolean`    | No       | Set `true` to remove current avatar|
| `website`        | `string?`    | No       |                                    |
| `linkedInUrl`    | `string?`    | No       |                                    |
| `location`       | `string?`    | No       |                                    |

**Success Response:** `200 OK`

---

### 3.3 Get User by ID

Returns any user's public profile.

```
GET /api/users/{UserId}
```

**Auth:** Required

---

### 3.4 Get User Stats

Returns learning/teaching statistics. Defaults to authenticated user if no `UserId` provided.

```
GET /api/users/stats
```

**Auth:** Required

| Parameter | Type    | In    | Required | Default            |
| --------- | ------- | ----- | -------- | ------------------ |
| `UserId`  | `Guid?` | Query | No       | Authenticated user |

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "coursesEnrolled": 5,
    "coursesCompleted": 2,
    "coursesTaught": 0,
    "totalStudySessions": 15,
    "examsTaken": 8,
    "averageExamScore": 82.5,
    "flashcardsCreated": 45,
    "quizzesTaken": 12,
    "totalStudyTime": "05:30:00",
    "lastActiveDate": "2026-03-07T18:00:00Z"
  }
}
```

---

### 3.5 Student Dashboard

Returns comprehensive academic performance data for the authenticated student.

```
GET /api/users/dashboard
```

**Auth:** Required | **Role:** `Student`

**Response:** `200 OK` — Returns `StudentDashboardDto` with course progress, engagement analytics, exam statistics, grade trends, and submission history.

---

### 3.6 Teacher Dashboard

Returns aggregated statistics for the teacher.

```
GET /api/users/teacher/dashboard
```

**Auth:** Required | **Role:** `Teacher`

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "totalCourses": 3,
    "publishedCourses": 2,
    "totalStudentsEnrolled": 87,
    "totalExamsCreated": 12,
    "pendingGradeApprovals": 5,
    "ungradedSubmissions": 14,
    "recentEnrollments": [...],
    "coursePerformance": [...],
    "enrollmentTrend": [...]
  }
}
```

---

## 4. Courses

### 4.1 Get All Courses

Returns paginated **published** courses with optional category filter.

```
GET /api/courses
```

**Auth:** None (public)

| Parameter    | Type    | In    | Default |
| ------------ | ------- | ----- | ------- |
| `Page`       | `int?`  | Query | 1       |
| `PageSize`   | `int?`  | Query | 20      |
| `CategoryId` | `Guid?` | Query | —       |

**Response:** `200 OK` — `PagedResult<CourseListDto>`

---

### 4.2 Search Courses

Searches published courses by keyword with optional category filter.

```
GET /api/courses/search
```

**Auth:** None (public)

| Parameter    | Type     | In    | Required |
| ------------ | -------- | ----- | -------- |
| `Keyword`    | `string` | Query | Yes      |
| `Page`       | `int?`   | Query | No       |
| `PageSize`   | `int?`   | Query | No       |
| `CategoryId` | `Guid?`  | Query | No       |

**Response:** `200 OK` — `PagedResult<CourseListDto>`

---

### 4.3 Get Course Details

```
GET /api/courses/{CourseId}
```

**Auth:** None (public)

**Response:** `200 OK` — `CourseDetailDto` with metadata, lectures, and categories.

---

### 4.4 Create Course

Creates a new course with optional thumbnail. Requires `multipart/form-data`.

```
POST /api/courses
```

**Auth:** Required | **Role:** `Teacher` | **Content-Type:** `multipart/form-data`

| Field         | Type         | Required | Notes                    |
| ------------- | ------------ | -------- | ------------------------ |
| `title`       | `string`     | Yes      |                          |
| `description` | `string`     | Yes      |                          |
| `price`       | `decimal`    | Yes      | Use 0 for free courses   |
| `categoryId`  | `Guid?`      | No       | Primary category         |
| `thumbnail`   | `IFormFile?` | No       | Course thumbnail image   |

**Success Response:** `201 Created`
```json
{
  "success": true,
  "data": { "courseId": "guid" },
  "message": "Course created successfully."
}
```

---

### 4.5 Update Course

```
PUT /api/courses/{CourseId}
```

**Auth:** Required | **Role:** `Teacher` (course owner) | **Content-Type:** `multipart/form-data`

| Field             | Type         | Required | Notes                               |
| ----------------- | ------------ | -------- | ----------------------------------- |
| `title`           | `string`     | Yes      |                                     |
| `description`     | `string`     | Yes      |                                     |
| `price`           | `decimal?`   | No       |                                     |
| `categoryId`      | `Guid?`      | No       |                                     |
| `thumbnail`       | `IFormFile?` | No       | New thumbnail image                 |
| `removeThumbnail` | `boolean`    | No       | `true` to remove existing thumbnail |

**Success Response:** `204 No Content`

---

### 4.6 Delete Course

```
DELETE /api/courses/{CourseId}
```

**Auth:** Required | **Role:** `Teacher` (course owner)

**Success Response:** `204 No Content`

---

### 4.7 Publish Course

```
POST /api/courses/{CourseId}/publish
```

**Auth:** Required | **Role:** `Teacher` (course owner)

**Success Response:** `200 OK`

---

### 4.8 Get My Courses (Teacher)

Returns all courses created by the authenticated teacher, including unpublished drafts.

```
GET /api/courses/my-courses
```

**Auth:** Required | **Role:** `Teacher`

| Parameter            | Type   | In    | Default |
| -------------------- | ------ | ----- | ------- |
| `IncludeUnpublished` | `bool` | Query | `true`  |
| `Page`               | `int?` | Query | 1       |
| `PageSize`           | `int?` | Query | 20      |

**Response:** `200 OK` — `PagedResult<CourseListDto>`

---

### 4.9 Get Courses by Instructor

```
GET /api/courses/instructor/{InstructorId}
```

**Auth:** Required

| Parameter            | Type   | In    | Default |
| -------------------- | ------ | ----- | ------- |
| `IncludeUnpublished` | `bool` | Query | `false` |
| `Page`               | `int?` | Query | 1       |
| `PageSize`           | `int?` | Query | 20      |

**Response:** `200 OK` — `PagedResult<CourseListDto>`

---

### 4.10 Continue Learning

Returns in-progress courses with resume position for the authenticated student.

```
GET /api/courses/continue-learning
```

**Auth:** Required | **Role:** `Student`

**Response:** `200 OK` — `List<ContinueLearningDto>`

---

### 4.11 Get Course Progress

Returns the student's detailed progress for a specific course.

```
GET /api/courses/{CourseId}/progress
```

**Auth:** Required | **Role:** `Student`

**Response:** `200 OK` — `CourseProgressDto`

---

### 4.12 Get Course Engagement Report

Returns per-student engagement metrics. Students are sorted by engagement (lowest first for at-risk identification).

```
GET /api/courses/{CourseId}/engagement
```

**Auth:** Required | **Role:** `Teacher` (course owner)

**Response:** `200 OK` — `CourseEngagementReport`

---

### 4.13 Send Engagement Alerts

Sends real-time notifications to at-risk students.

```
POST /api/courses/{CourseId}/engagement/alerts
```

**Auth:** Required | **Role:** `Teacher` (course owner)

**Request Body:**

| Field           | Type          | Required | Notes                                          |
| --------------- | ------------- | -------- | ---------------------------------------------- |
| `studentIds`    | `List<Guid>?` | No       | Specific students (null = all low engagement)  |
| `customMessage` | `string?`     | No       | Custom alert message                           |

**Success Response:** `200 OK` — `SendEngagementAlertsResult`

---

## 5. Categories

### 5.1 Get All Categories

```
GET /api/categories
```

**Auth:** None (public)

| Parameter    | Type      | In    | Required |
| ------------ | --------- | ----- | -------- |
| `SearchTerm` | `string?` | Query | No       |

**Response:** `200 OK` — `List<CategoryDto>`

---

### 5.2 Get Category by ID

```
GET /api/categories/{CategoryId}
```

**Auth:** None (public)

---

### 5.3 Create Category

```
POST /api/categories
```

**Auth:** Required | **Role:** `Teacher`

| Field         | Type      | Required |
| ------------- | --------- | -------- |
| `name`        | `string`  | Yes      |
| `description` | `string?` | No       |

**Success Response:** `200 OK` — Returns `Guid` (category ID)

---

### 5.4 Update Category

```
PUT /api/categories/{CategoryId}
```

**Auth:** Required | **Role:** `Teacher`

| Field         | Type      | Required |
| ------------- | --------- | -------- |
| `name`        | `string`  | Yes      |
| `description` | `string?` | No       |

---

### 5.5 Delete Category

```
DELETE /api/categories/{CategoryId}
```

**Auth:** Required | **Role:** `Teacher`

---

### 5.6 Add Course to Category

```
POST /api/courses/categories
```

**Auth:** Required | **Role:** `Teacher`

| Field        | Type   | Required |
| ------------ | ------ | -------- |
| `courseId`   | `Guid` | Yes      |
| `categoryId` | `Guid` | Yes      |

---

### 5.7 Remove Course from Category

```
DELETE /api/courses/{CourseId}/categories/{CategoryId}
```

**Auth:** Required | **Role:** `Teacher`

---

## 6. Enrollments

### 6.1 Enroll in Course

```
POST /api/courses/{CourseId}/enroll
```

**Auth:** Required

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": { "enrollmentId": "guid" },
  "message": "Enrolled successfully."
}
```

---

### 6.2 Unenroll from Course

Enforces a 10-day refund policy with progress-based refund calculation for paid courses.

```
DELETE /api/courses/{CourseId}/unenroll
```

**Auth:** Required

**Response:** `200 OK` — `UnenrollmentResultDto` with refund details.

---

### 6.3 Complete Course

```
POST /api/courses/{CourseId}/complete
```

**Auth:** Required

---

### 6.4 Get My Enrolled Courses

```
GET /api/courses/enrolled
```

**Auth:** Required

| Parameter  | Type   | In    | Default |
| ---------- | ------ | ----- | ------- |
| `Page`     | `int?` | Query | 1       |
| `PageSize` | `int?` | Query | 20      |

**Response:** `200 OK` — `PagedResult<EnrollmentDto>`

---

### 6.5 Get Course Enrollments (Teacher)

Returns all enrolled students for a specific course.

```
GET /api/courses/{CourseId}/enrollments
```

**Auth:** Required | **Role:** `Teacher` (course owner)

| Parameter  | Type   | In    | Default |
| ---------- | ------ | ----- | ------- |
| `Page`     | `int?` | Query | 1       |
| `PageSize` | `int?` | Query | 20      |

**Response:** `200 OK` — `PagedResult<EnrollmentDto>`

---

## 7. Lectures

### 7.1 Add Lecture

```
POST /api/courses/{CourseId}/lectures
```

**Auth:** Required | **Role:** `Teacher` (course owner)

| Field         | Type     | Required |
| ------------- | -------- | -------- |
| `title`       | `string` | Yes      |
| `description` | `string` | Yes      |
| `orderIndex`  | `int`    | Yes      |

**Success Response:** `201 Created`
```json
{
  "success": true,
  "data": { "lectureId": "guid" },
  "message": "Lecture created successfully."
}
```

---

### 7.2 Get Course Lectures

```
GET /api/courses/{CourseId}/lectures
```

**Auth:** Required (enrolled or instructor)

| Parameter          | Type   | In    | Default |
| ------------------ | ------ | ----- | ------- |
| `IncludeMaterials` | `bool` | Query | `true`  |

**Response:** `200 OK` — `List<LectureDto>`

---

### 7.3 Get Lecture by ID

Returns lecture details with materials categorized by type (Video, Document, Audio, Image).

```
GET /api/lectures/{LectureId}
```

**Auth:** Required (enrolled or instructor)

**Response:** `200 OK` — `LectureDetailDto`

---

### 7.4 Update Lecture

```
PUT /api/courses/lectures/{LectureId}
```

**Auth:** Required | **Role:** `Teacher` (course owner)

| Field         | Type     | Required |
| ------------- | -------- | -------- |
| `title`       | `string` | Yes      |
| `description` | `string` | Yes      |
| `orderIndex`  | `int`    | Yes      |

**Success Response:** `204 No Content`

---

### 7.5 Delete Lecture

```
DELETE /api/courses/lectures/{LectureId}
```

**Auth:** Required | **Role:** `Teacher` (course owner)

**Success Response:** `204 No Content`

---

## 8. Materials

### 8.1 Upload Materials (Bulk)

Uploads one or more files as course materials. Material type is inferred from file extension.

```
POST /api/courses/lectures/{LectureId}/materials
```

**Auth:** Required | **Role:** `Teacher` (course owner) | **Content-Type:** `multipart/form-data` | **Rate Limited:** Yes (FileUploadPolicy)

| Field    | Type              | Required | Notes                                      |
| -------- | ----------------- | -------- | ------------------------------------------ |
| `files`  | `List<IFormFile>`  | Yes      | Max 100 MB per file                        |
| `titles` | `string`          | No       | Comma-separated titles matching file order |

**Supported formats:** `.pdf`, `.mp4`, `.mp3`, `.wav`, `.ogg`, `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.docx`, `.pptx`, `.txt`, `.md`, `.webm`

**Success Response:** `201 Created`
```json
{
  "success": true,
  "data": { "materialIds": ["guid1", "guid2"] },
  "message": "Materials uploaded successfully."
}
```

---

### 8.2 Get Lecture Materials

```
GET /api/courses/lectures/{LectureId}/materials
```

**Auth:** Required (enrolled or instructor)

**Response:** `200 OK` — `List<MaterialDto>`

---

### 8.3 Stream Material

Streams a material file with HTTP Range support for video/audio seeking. PDFs and images are served inline.

```
GET /api/materials/{MaterialId}/stream
```

**Auth:** Required (enrolled or instructor)

**Responses:**
- `200` — Full file content
- `206` — Partial content (range request)

---

### 8.4 Download Material

Forces a file download (Content-Disposition: attachment).

```
GET /api/materials/{MaterialId}/download
```

**Auth:** Required (enrolled or instructor)

---

### 8.5 Get Material Projection

Returns material metadata with progress and resume position (read-only, no side effects).

```
GET /api/materials/{MaterialId}/projection
```

**Auth:** Required | **Role:** `Student`

**Response:** `200 OK` — `MaterialProjectionDto`

---

### 8.6 Update Material Progress

Updates the student's progress position for a material. Only overwrites if the new position is strictly greater.

```
POST /api/materials/{MaterialId}/progress
```

**Auth:** Required | **Role:** `Student`

| Field      | Type  | Required | Notes                              |
| ---------- | ----- | -------- | ---------------------------------- |
| `position` | `int` | Yes      | Current position (page or seconds) |

---

### 8.7 Delete Material

```
DELETE /api/courses/materials/{MaterialId}
```

**Auth:** Required | **Role:** `Teacher` (course owner)

**Success Response:** `204 No Content`

---

## 9. Cart

### 9.1 Get Cart

Returns the current user's active shopping cart.

```
GET /api/cart
```

**Auth:** Required

**Response:** `200 OK` — `CartDto`

---

### 9.2 Add Course to Cart

```
POST /api/cart/items
```

**Auth:** Required

| Field      | Type   | Required |
| ---------- | ------ | -------- |
| `courseId`  | `Guid` | Yes      |

**Response:** `200 OK` — `CartDto` (updated cart)

**Errors:** `400` (already enrolled, duplicate, or course not available), `404` (course not found)

---

### 9.3 Remove Course from Cart

```
DELETE /api/cart/items/{CourseId}
```

**Auth:** Required

**Response:** `200 OK` — `CartDto` (updated cart)

---

### 9.4 Clear Cart

```
DELETE /api/cart
```

**Auth:** Required

---

## 10. Checkout & Payments

### 10.1 Create Checkout Session

Creates a checkout session from the user's cart. Returns a Stripe client secret for payment, or completes the order immediately if all courses are free.

```
POST /api/checkout
```

**Auth:** Required

**Response:** `200 OK` — `CheckoutResponseDto`

---

### 10.2 Get Order Status

```
GET /api/checkout/{OrderId}
```

**Auth:** Required

**Response:** `200 OK` — `OrderStatusDto`

---

### 10.3 Stripe Webhook

Handles Stripe webhook events for payment confirmation. **Do not call directly.**

```
POST /api/payments/webhook
```

**Auth:** None (Stripe signature verification)

---

## 11. Exams

### 11.1 Create Exam

```
POST /api/courses/{CourseId}/exams
```

**Auth:** Required | **Role:** `Teacher` (course owner)

| Field             | Type       | Required |
| ----------------- | ---------- | -------- |
| `title`           | `string`   | Yes      |
| `startTime`       | `DateTime` | Yes      |
| `endTime`         | `DateTime` | Yes      |
| `durationMinutes` | `int`      | Yes      |

**Success Response:** `201 Created`

---

### 11.2 Get Exam by ID

```
GET /api/exams/{ExamId}
```

**Auth:** Required

**Response:** `200 OK` — `ExamDetailDto` with questions and course info.

---

### 11.3 Get Exams by Course

```
GET /api/exams/course/{CourseId}
```

**Auth:** Required

| Parameter  | Type   | In    | Default |
| ---------- | ------ | ----- | ------- |
| `Page`     | `int?` | Query | 1       |
| `PageSize` | `int?` | Query | 20      |

**Response:** `200 OK` — `PagedResult<ExamDto>`

---

### 11.4 Get Active Exams

Returns exams currently in progress for a course.

```
GET /api/exams/active/{CourseId}
```

**Auth:** Required

| Parameter  | Type   | In    | Default |
| ---------- | ------ | ----- | ------- |
| `Page`     | `int?` | Query | 1       |
| `PageSize` | `int?` | Query | 20      |

**Response:** `200 OK` — `PagedResult<ExamDto>`

---

### 11.5 Get Upcoming Exams

```
GET /api/exams/upcoming/{CourseId}
```

**Auth:** Required

| Parameter  | Type   | In    | Default |
| ---------- | ------ | ----- | ------- |
| `Page`     | `int?` | Query | 1       |
| `PageSize` | `int?` | Query | 20      |

**Response:** `200 OK` — `PagedResult<ExamDto>`

---

### 11.6 Get Past Exams

```
GET /api/exams/past/{CourseId}
```

**Auth:** Required

| Parameter  | Type   | In    | Default |
| ---------- | ------ | ----- | ------- |
| `Page`     | `int?` | Query | 1       |
| `PageSize` | `int?` | Query | 20      |

**Response:** `200 OK` — `PagedResult<ExamDto>`

---

### 11.7 Get Available Exams (Student)

Returns exams available to the student based on enrolled courses.

```
GET /api/exams/available
```

**Auth:** Required

| Parameter  | Type   | In    | Default |
| ---------- | ------ | ----- | ------- |
| `Page`     | `int?` | Query | 1       |
| `PageSize` | `int?` | Query | 20      |

**Response:** `200 OK` — `PagedResult<ExamDto>`

---

### 11.8 Get Exam Total Points

```
GET /api/exams/{ExamId}/total-points
```

**Auth:** Required

**Response:** `200 OK` — `int`

---

### 11.9 Update Exam

```
PUT /api/exams/{ExamId}
```

**Auth:** Required | **Role:** `Teacher` (course owner)

| Field             | Type       | Required |
| ----------------- | ---------- | -------- |
| `title`           | `string`   | Yes      |
| `startTime`       | `DateTime` | Yes      |
| `endTime`         | `DateTime` | Yes      |
| `durationMinutes` | `int`      | Yes      |

**Success Response:** `204 No Content`

---

### 11.10 Delete Exam

```
DELETE /api/exams/{ExamId}
```

**Auth:** Required | **Role:** `Teacher` (course owner)

**Success Response:** `204 No Content`

---

## 12. Questions

### 12.1 Add Question

```
POST /api/exams/{ExamId}/questions
```

**Auth:** Required | **Role:** `Teacher` (course owner)

| Field           | Type             | Required | Notes                              |
| --------------- | ---------------- | -------- | ---------------------------------- |
| `type`          | `QuestionType`   | Yes      | MCQ, TrueFalse, ShortAnswer, Essay |
| `text`          | `string`         | Yes      | Question text                      |
| `options`       | `List<string>?`  | No       | Required for MCQ                   |
| `correctAnswer` | `string`         | Yes      |                                    |
| `points`        | `int`            | Yes      |                                    |

**Success Response:** `201 Created`

---

### 12.2 Add Bulk Questions

```
POST /api/exams/{ExamId}/questions/bulk
```

**Auth:** Required | **Role:** `Teacher` (course owner)

| Field       | Type                          | Required |
| ----------- | ----------------------------- | -------- |
| `questions` | `List<BulkQuestionItemRequest>` | Yes    |

Each item has the same fields as Add Question.

---

### 12.3 Generate AI Questions

Uses AI to auto-generate exam questions from course materials.

```
POST /api/exams/{ExamId}/questions/generate-ai
```

**Auth:** Required | **Role:** `Teacher` (course owner)

| Field               | Type                | Required |
| ------------------- | ------------------- | -------- |
| `numberOfQuestions`  | `int`              | Yes      |
| `difficulty`        | `string?`           | No       |
| `questionTypes`     | `List<QuestionType>?` | No     |
| `focusTopics`       | `List<string>?`     | No       |
| `lectureIds`        | `List<Guid>?`       | No       |
| `materialIds`       | `List<Guid>?`       | No       |

**Response:** `200 OK` — `GenerateAIQuestionsResult`

---

### 12.4 Get Exam Questions

```
GET /api/exams/{ExamId}/questions
```

**Auth:** Required

**Response:** `200 OK` — `List<QuestionDto>`

---

### 12.5 Update Question

```
PUT /api/exams/questions/{QuestionId}
```

**Auth:** Required | **Role:** `Teacher` (course owner)

**Success Response:** `204 No Content`

---

### 12.6 Reorder Questions

```
POST /api/exams/{ExamId}/questions/reorder
```

**Auth:** Required | **Role:** `Teacher` (course owner)

| Field            | Type                    | Required |
| ---------------- | ----------------------- | -------- |
| `questionOrders` | `Dictionary<Guid, int>` | Yes      |

**Success Response:** `204 No Content`

---

### 12.7 Delete Question

```
DELETE /api/exams/questions/{QuestionId}
```

**Auth:** Required | **Role:** `Teacher` (course owner)

**Success Response:** `204 No Content`

---

## 13. Submissions

### 13.1 Submit Exam

```
POST /api/exams/{ExamId}/submit
```

**Auth:** Required

| Field     | Type                      | Required | Notes                         |
| --------- | ------------------------- | -------- | ----------------------------- |
| `answers` | `Dictionary<Guid, string>` | Yes     | questionId → answer text      |

**Success Response:** `201 Created`

---

### 13.2 Get Submission by ID

```
GET /api/exams/submissions/{SubmissionId}
```

**Auth:** Required

**Response:** `200 OK` — `SubmissionDetailDto`

---

### 13.3 Get Exam Submissions (Teacher)

```
GET /api/exams/{ExamId}/submissions
```

**Auth:** Required | **Role:** `Teacher` (course owner)

---

### 13.4 Get My Submissions (Student)

```
GET /api/exams/submissions/student
```

**Auth:** Required

---

### 13.5 Get Ungraded Submissions

```
GET /api/exams/submissions/ungraded
```

**Auth:** Required | **Role:** `Teacher`

| Parameter | Type    | In    | Required |
| --------- | ------- | ----- | -------- |
| `ExamId`  | `Guid?` | Query | No       |

---

### 13.6 Get Exam Submission Stats

```
GET /api/submissions/stats/{ExamId}
```

**Auth:** Required | **Role:** `Teacher` (course owner)

**Response:** `200 OK` — `SubmissionStats`

---

## 14. Grades

### 14.1 Grade Submission (Manual)

```
POST /api/exams/submissions/{SubmissionId}/grade
```

**Auth:** Required | **Role:** `Teacher` (course owner)

| Field      | Type     | Required |
| ---------- | -------- | -------- |
| `score`    | `float`  | Yes      |
| `feedback` | `string` | Yes      |

**Success Response:** `201 Created`

---

### 14.2 Grade Submission with AI

Uses AI to automatically grade a submission. Marked as AI-graded, requires teacher approval.

```
POST /api/exams/submissions/{SubmissionId}/grade-ai
```

**Auth:** Required | **Role:** `Teacher` (course owner) | **Rate Limited:** Yes (AiEndpointsPolicy)

**Response:** `200 OK` — `GradeSubmissionWithAIResult`

---

### 14.3 Approve AI Grade

```
POST /api/exams/grades/{GradeId}/approve
```

**Auth:** Required | **Role:** `Teacher` (course owner)

**Success Response:** `204 No Content`

---

### 14.4 Update Grade

```
PUT /api/exams/grades/{GradeId}
```

**Auth:** Required | **Role:** `Teacher` (course owner)

| Field      | Type     | Required |
| ---------- | -------- | -------- |
| `score`    | `float`  | Yes      |
| `feedback` | `string` | Yes      |

**Success Response:** `204 No Content`

---

### 14.5 Get Grade by Submission

```
GET /api/exams/submissions/{SubmissionId}/grade
```

**Auth:** Required

---

### 14.6 Get Exam Grades (Teacher)

```
GET /api/exams/{ExamId}/grades
```

**Auth:** Required | **Role:** `Teacher` (course owner)

---

### 14.7 Get Pending Approval Grades

```
GET /api/exams/grades/pending-approval
```

**Auth:** Required | **Role:** `Teacher`

| Parameter | Type    | In    | Required |
| --------- | ------- | ----- | -------- |
| `ExamId`  | `Guid?` | Query | No       |

---

### 14.8 Get My Grades (Student)

```
GET /api/exams/grades/student
```

**Auth:** Required

---

### 14.9 Get Exam Grade Stats

```
GET /api/grades/stats/exam/{ExamId}
```

**Auth:** Required | **Role:** `Teacher` (course owner)

**Response:** `200 OK` — `ExamGradeStats` (average, median, pass rate, etc.)

---

### 14.10 Get Student Grade Stats

```
GET /api/grades/stats/student/{StudentId}
```

**Auth:** Required

| Parameter  | Type    | In    | Required |
| ---------- | ------- | ----- | -------- |
| `CourseId` | `Guid?` | Query | No       |

**Response:** `200 OK` — `StudentGradeStats`

---

### 14.11 Get Grade Distribution

Returns grade distribution (A, B, C, D, F) for a specific exam.

```
GET /api/grades/distribution/{ExamId}
```

**Auth:** Required | **Role:** `Teacher` (course owner)

**Response:** `200 OK` — `Dictionary<string, int>`

---

## 15. Reviews

### 15.1 Add Review

One review per student per course. Must be enrolled.

```
POST /api/courses/{CourseId}/reviews
```

**Auth:** Required | **Role:** `Student`

| Field     | Type      | Required |
| --------- | --------- | -------- |
| `rating`  | `int`     | Yes      |
| `comment` | `string?` | No       |

**Success Response:** `201 Created`

---

### 15.2 Get Course Reviews

```
GET /api/courses/{CourseId}/reviews
```

**Auth:** None (public)

| Parameter  | Type   | In    | Default |
| ---------- | ------ | ----- | ------- |
| `Page`     | `int?` | Query | 1       |
| `PageSize` | `int?` | Query | 10      |

**Response:** `200 OK` — `PagedResult<ReviewDto>`

---

### 15.3 Get Course Rating Summary

```
GET /api/courses/{CourseId}/rating
```

**Auth:** None (public)

**Response:** `200 OK` — `CourseRatingSummaryDto` (average rating, total reviews, rating distribution)

---

### 15.4 Update Review

```
PUT /api/reviews/{ReviewId}
```

**Auth:** Required | **Role:** `Student` (review author)

---

### 15.5 Delete Review

Deletable by the review author or the course instructor.

```
DELETE /api/reviews/{ReviewId}
```

**Auth:** Required

---

## 16. Notifications

### 16.1 Get Notifications

```
GET /api/notifications
```

**Auth:** Required

| Parameter    | Type   | In    | Default |
| ------------ | ------ | ----- | ------- |
| `Page`       | `int`  | Query | 1       |
| `PageSize`   | `int`  | Query | 20      |
| `UnreadOnly` | `bool` | Query | `false` |

**Response:** `200 OK` — `NotificationListDto`

---

### 16.2 Get Unread Notification Count

```
GET /api/notifications/unread-count
```

**Auth:** Required

**Response:** `200 OK`
```json
{
  "success": true,
  "data": { "count": 5 }
}
```

---

### 16.3 Mark Notification as Read

```
PUT /api/notifications/{Id}/read
```

**Auth:** Required

---

### 16.4 Mark All Notifications as Read

```
PUT /api/notifications/read-all
```

**Auth:** Required

---

### 16.5 Delete Notification

```
DELETE /api/notifications/{Id}
```

**Auth:** Required

---

## 17. Study Sessions

### 17.1 Start Session

Creates a new AI-powered study session for a course.

```
POST /api/study-sessions
```

**Auth:** Required (must be enrolled)

| Field      | Type   | Required |
| ---------- | ------ | -------- |
| `courseId`  | `Guid` | Yes      |

**Success Response:** `201 Created`

---

### 17.2 End Session

```
POST /api/study-sessions/{SessionId}/end
```

**Auth:** Required (session owner)

---

### 17.3 Get Session by ID

Returns full session details including chat messages, flashcards, quizzes, and mind maps.

```
GET /api/study-sessions/{SessionId}
```

**Auth:** Required (session owner)

**Response:** `200 OK` — `SessionDetailDto`

---

### 17.4 Get My Study Sessions

```
GET /api/study-sessions
```

**Auth:** Required

| Parameter  | Type    | In    | Required |
| ---------- | ------- | ----- | -------- |
| `CourseId` | `Guid?` | Query | No       |
| `Page`     | `int?`  | Query | No       |
| `PageSize` | `int?`  | Query | No       |

**Response:** `200 OK` — `PagedResult<SessionSummaryDto>`

---

### 17.5 Send Chat Message (SSE Streaming)

Sends a message to the AI tutor and streams the response via Server-Sent Events.

```
POST /api/study-sessions/{SessionId}/chat
```

**Auth:** Required (session owner)

| Field         | Type          | Required | Notes                          |
| ------------- | ------------- | -------- | ------------------------------ |
| `message`     | `string`      | Yes      | Student's message              |
| `lectureIds`  | `List<Guid>?` | No       | Scope RAG to specific lectures |
| `materialIds` | `List<Guid>?` | No       | Scope RAG to specific materials|

**Response:** `200 OK` — `text/event-stream`

SSE events format:
```
data: {"content": "chunk of text", "done": false}
data: {"content": "", "done": true, "sources": ["source1", "source2"]}
```

---

### 17.6 Get Chat History

```
GET /api/study-sessions/{SessionId}/chat
```

**Auth:** Required (session owner)

| Parameter  | Type   | In    | Default |
| ---------- | ------ | ----- | ------- |
| `Page`     | `int?` | Query | 1       |
| `PageSize` | `int?` | Query | 50      |

**Response:** `200 OK` — `PagedResult<ChatMessageDto>`

---

### 17.7 Generate Summary

Uses AI to generate a topic summary grounded in course materials.

```
POST /api/study-sessions/{SessionId}/summary
```

**Auth:** Required (session owner)

| Field             | Type          | Required | Default |
| ----------------- | ------------- | -------- | ------- |
| `topic`           | `string`      | Yes      |         |
| `summaryLength`   | `int`         | No       | 500     |
| `includeKeyPoints`| `bool`        | No       | `true`  |
| `lectureIds`      | `List<Guid>?` | No       |         |
| `materialIds`     | `List<Guid>?` | No       |         |

**Response:** `200 OK` — `Summary`

---

### 17.8 Generate Flashcards

```
POST /api/study-sessions/{SessionId}/flashcards
```

**Auth:** Required (session owner)

| Field           | Type          | Required | Default |
| --------------- | ------------- | -------- | ------- |
| `topic`         | `string`      | Yes      |         |
| `numberOfCards` | `int`         | No       | 10      |
| `lectureIds`    | `List<Guid>?` | No       |         |
| `materialIds`   | `List<Guid>?` | No       |         |

**Success Response:** `201 Created` — `List<FlashcardDto>`

---

### 17.9 Get Session Flashcards

```
GET /api/study-sessions/{SessionId}/flashcards
```

**Auth:** Required (session owner)

---

### 17.10 Generate Quiz

```
POST /api/study-sessions/{SessionId}/quizzes
```

**Auth:** Required (session owner)

| Field               | Type            | Required | Default    |
| ------------------- | --------------- | -------- | ---------- |
| `topic`             | `string`        | Yes      |            |
| `numberOfQuestions`  | `int`          | No       | 5          |
| `difficulty`        | `string`        | No       | `"medium"` |
| `questionTypes`     | `List<string>`  | No       | `["mcq"]`  |
| `lectureIds`        | `List<Guid>?`   | No       |            |
| `materialIds`       | `List<Guid>?`   | No       |            |

**Success Response:** `201 Created` — `GeneratedQuizDto`

---

### 17.11 Submit Quiz Answers

MCQ/True-False are auto-graded; Short Answer/Essay are AI-graded.

```
POST /api/study-sessions/{SessionId}/quizzes/{QuizId}/submit
```

**Auth:** Required (session owner)

| Field     | Type                       | Required |
| --------- | -------------------------- | -------- |
| `answers` | `Dictionary<int, string>`  | Yes      |

**Response:** `200 OK` — `QuizResultDto`

---

### 17.12 Get Session Quizzes

```
GET /api/study-sessions/{SessionId}/quizzes
```

**Auth:** Required (session owner)

---

### 17.13 Generate Mind Map

```
POST /api/study-sessions/{SessionId}/mindmaps
```

**Auth:** Required (session owner)

| Field           | Type          | Required | Default |
| --------------- | ------------- | -------- | ------- |
| `centralTopic`  | `string`      | Yes      |         |
| `maxDepth`      | `int`         | No       | 3       |
| `lectureIds`    | `List<Guid>?` | No       |         |
| `materialIds`   | `List<Guid>?` | No       |         |

**Success Response:** `201 Created` — `MindMapDto`

---

### 17.14 Get Session Mind Maps

```
GET /api/study-sessions/{SessionId}/mindmaps
```

**Auth:** Required (session owner)

---

### 17.15 Generate Dialogue Audio

Generates a teacher-student dialogue using AI and synthesizes it into audio with timestamps.

```
POST /api/study-sessions/{SessionId}/dialogue-audio
```

**Auth:** Required (session owner)

| Field              | Type            | Required | Default          |
| ------------------ | --------------- | -------- | ---------------- |
| `topic`            | `string?`       | No       | Auto from course |
| `audienceLevel`    | `string`        | No       | `"intermediate"` |
| `numberOfExchanges`| `int`           | No       | 5                |
| `dialogueLength`   | `string`        | No       | `"medium"`       |
| `includeExamples`  | `bool`          | No       | `true`           |
| `includeSummary`   | `bool`          | No       | `true`           |
| `teachingStyle`    | `string`        | No       | `"interactive"`  |
| `focusConcepts`    | `List<string>?` | No       |                  |
| `lectureIds`       | `List<Guid>?`   | No       |                  |
| `materialIds`      | `List<Guid>?`   | No       |                  |
| `teacherVoiceId`   | `string?`       | No       | Per-request override |
| `studentVoiceId`   | `string?`       | No       | Per-request override |
| `teacherSpeed`     | `double?`       | No       | Per-request override |
| `studentSpeed`     | `double?`       | No       | Per-request override |

**Success Response:** `201 Created` — `DialogueAudioResponseDto`

---

## 18. Semantic Sections

### 18.1 Get Sections by Material

Returns semantic sections extracted from a material, ordered by position.

```
GET /api/materials/{MaterialId}/sections
```

**Auth:** Required | **Roles:** `Student`, `Teacher`

**Response:** `200 OK` — `List<SemanticSectionDto>`

---

### 18.2 Summarize Section

Generates an AI summary of a specific semantic section.

```
POST /api/sessions/{SessionId}/sections/{SectionId}/summarize
```

**Auth:** Required | **Role:** `Student` (session owner)

| Field              | Type   | Required | Default |
| ------------------ | ------ | -------- | ------- |
| `summaryLength`    | `int`  | No       | 500     |
| `includeKeyPoints` | `bool` | No       | `true`  |

---

### 18.3 Generate Section Flashcards

```
POST /api/sessions/{SessionId}/sections/{SectionId}/flashcards
```

**Auth:** Required | **Role:** `Student` (session owner)

| Field           | Type  | Required | Default |
| --------------- | ----- | -------- | ------- |
| `numberOfCards` | `int` | No       | 10      |

---

### 18.4 Generate Section Quiz

```
POST /api/sessions/{SessionId}/sections/{SectionId}/quiz
```

**Auth:** Required | **Role:** `Student` (session owner)

| Field               | Type           | Required | Default    |
| ------------------- | -------------- | -------- | ---------- |
| `numberOfQuestions`  | `int`         | No       | 5          |
| `difficulty`        | `string`       | No       | `"medium"` |
| `questionTypes`     | `List<string>` | No       | `["mcq"]`  |

---

## 19. AI Provider

### 19.1 Get Provider Status

```
GET /api/ai/provider
```

**Auth:** Required | **Roles:** `Teacher`, `Student`, `Admin`

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "activeProvider": "ollama",
    "supportedProviders": ["ollama", "groq"],
    "isGroqConfigured": true
  }
}
```

---

### 19.2 Switch Provider

```
POST /api/ai/provider/switch
```

**Auth:** Required | **Roles:** `Teacher`, `Student`, `Admin`

| Field      | Type     | Required | Notes                       |
| ---------- | -------- | -------- | --------------------------- |
| `provider` | `string` | Yes      | `"ollama"` or `"groq"`     |

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "previousProvider": "ollama",
    "activeProvider": "groq",
    "message": "Successfully switched from 'ollama' to 'groq'"
  }
}
```

---

## 20. Dialogue & Audio

All endpoints are prefixed with `/api/dialogue`.

### 20.1 Get Available Voices

```
GET /api/dialogue/voices
```

**Auth:** Required

**Response:** `200 OK` — List of available TTS voices with metadata and preview URLs.

---

### 20.2 Get Voice Previews

```
GET /api/dialogue/voice-previews
```

**Auth:** Required

| Parameter    | Type      | In    | Default |
| ------------ | --------- | ----- | ------- |
| `VoiceId`    | `string?` | Query | —       |
| `SampleText` | `string?` | Query | —       |
| `Format`     | `string`  | Query | `"mp3"` |
| `SampleRate` | `int`     | Query | 24000   |

**Response:** `200 OK` — List of voices with base64-encoded audio samples.

---

### 20.3 Get Default Voice Config

```
GET /api/dialogue/voice-config/default
```

**Auth:** Required

**Response:** `200 OK` — Default teacher/student voice IDs, speeds, and names.

---

### 20.4 Get Supported Formats

```
GET /api/dialogue/supported-formats
```

**Auth:** Required

---

### 20.5 Get Supported Languages

```
GET /api/dialogue/supported-languages
```

**Auth:** Required

---

### 20.6 Get User Voice Settings

```
GET /api/dialogue/voice-settings
```

**Auth:** Required

**Response:** `200 OK` — `UserVoiceSettingsDto` (returns defaults if no custom settings saved).

---

### 20.7 Save User Voice Settings

```
PUT /api/dialogue/voice-settings
```

**Auth:** Required

| Field              | Type     | Required | Default          |
| ------------------ | -------- | -------- | ---------------- |
| `teacherVoiceId`   | `string` | No       | "Damien Black"   |
| `studentVoiceId`   | `string` | No       | "Daisy Studious" |
| `teacherSpeed`     | `double` | No       | 0.95             |
| `studentSpeed`     | `double` | No       | 1.0              |
| `outputFormat`     | `string` | No       | "mp3"            |
| `sampleRate`       | `int`    | No       | 24000            |
| `includePauses`    | `bool`   | No       | `true`           |
| `pauseDurationMs`  | `int`    | No       | 500              |
| `pauseMultiplier`  | `double` | No       | 1.0              |
| `normalizeAudio`   | `bool`   | No       | `true`           |

---

### 20.8 Delete Voice Settings

Resets voice settings to system defaults.

```
DELETE /api/dialogue/voice-settings
```

**Auth:** Required

---

## 21. Enums Reference

### QuestionType
| Value | Int | Description        |
| ----- | --- | ------------------ |
| `MultipleChoice` | 0 | MCQ with options |
| `TrueFalse`      | 1 | True/False       |
| `ShortAnswer`    | 2 | Short text answer|
| `Essay`          | 3 | Long-form essay  |
| `FillInTheBlank` | 4 | Fill in the blank|

### EnrollmentStatus
| Value       | Int |
| ----------- | --- |
| `Active`    | 0   |
| `Completed` | 1   |
| `Dropped`   | 2   |
| `Pending`   | 3   |

### MaterialType
| Value      | Int |
| ---------- | --- |
| `Video`    | 0   |
| `Document` | 1   |
| `Audio`    | 2   |
| `Image`    | 3   |

### CartStatus
| Value        | Int |
| ------------ | --- |
| `Active`     | 0   |
| `CheckedOut` | 1   |
| `Abandoned`  | 2   |

### OrderStatus
| Value                | Int |
| -------------------- | --- |
| `Pending`            | 0   |
| `Paid`               | 1   |
| `Refunded`           | 2   |
| `PartiallyRefunded`  | 3   |
| `Failed`             | 4   |

### ChatRole
| Value       | Int |
| ----------- | --- |
| `Student`   | 0   |
| `Assistant` | 1   |
| `System`    | 2   |

### QuizDifficulty
| Value    | Int |
| -------- | --- |
| `Easy`   | 0   |
| `Medium` | 1   |
| `Hard`   | 2   |

---

## 22. Error Handling

All errors follow the standard response envelope:

```json
{
  "success": false,
  "data": null,
  "message": "Descriptive error message"
}
```

### Common HTTP Status Codes

| Code | Meaning              | Typical Cause                              |
| ---- | -------------------- | ------------------------------------------ |
| 400  | Bad Request          | Validation error, business rule violation  |
| 401  | Unauthorized         | Missing or invalid JWT token               |
| 403  | Forbidden            | Insufficient role or not the owner         |
| 404  | Not Found            | Resource does not exist                    |
| 409  | Conflict             | Duplicate resource (e.g., already enrolled)|
| 429  | Too Many Requests    | Rate limit exceeded                        |
| 500  | Internal Server Error| Unexpected server error                    |

### Rate-Limited Endpoints

| Policy           | Endpoints                                    |
| ---------------- | -------------------------------------------- |
| LoginPolicy      | `POST /api/auth/login`                       |
| AiEndpointsPolicy| `POST .../grade-ai`                          |
| FileUploadPolicy | `POST .../materials` (file upload)           |
