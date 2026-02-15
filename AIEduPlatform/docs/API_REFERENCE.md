# AIEduPlatform — API Reference

> **Base URL:** `http://localhost:5000/api` (development) | `https://your-domain.com/api` (production)
>
> **Authentication:** JWT Bearer Token — include `Authorization: Bearer <access_token>` header on all authenticated endpoints.
>
> **Content Type:** `application/json` unless otherwise noted.

---

## Table of Contents

1. [Response Envelope](#1-response-envelope)
2. [Authentication](#2-authentication)
3. [Users](#3-users)
4. [Courses](#4-courses)
5. [Enrollments](#5-enrollments)
6. [Lectures](#6-lectures)
7. [Materials](#7-materials)
8. [Exams](#8-exams)
9. [Questions](#9-questions)
10. [Submissions](#10-submissions)
11. [Grades](#11-grades)
12. [Reviews](#12-reviews)
13. [Study Sessions](#13-study-sessions)
14. [Enums Reference](#14-enums-reference)
15. [Error Handling](#15-error-handling)

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

**Default pagination:** `page=1`, `pageSize=10` when not specified.

---

## 2. Authentication

Authentication uses JWT access tokens + refresh tokens. Access tokens are short-lived (configurable); refresh tokens are long-lived and stored in the database.

### JWT Token Claims

| Claim                 | Value                                    |
| --------------------- | ---------------------------------------- |
| `sub`                 | User ID (GUID)                           |
| `email`               | User email                               |
| `name`                | Username                                 |
| `jti`                 | Unique token ID                          |
| `role`                | One entry per role (`Student`, `Teacher`) |

### Roles

| Role      | Description                          |
| --------- | ------------------------------------ |
| `Student` | Default role assigned on registration |
| `Teacher` | Assigned via "Become Teacher" endpoint; users can have both roles |

---

### 2.1 Register

Creates a new user account with the `Student` role.

```
POST /api/auth/register
```

**Auth:** None (public)

**Request Body:**

| Field             | Type     | Required | Constraints                     |
| ----------------- | -------- | -------- | ------------------------------- |
| `email`           | `string` | Yes      | Valid email format              |
| `userName`        | `string` | Yes      | Unique                          |
| `password`        | `string` | Yes      | Meets identity password rules   |
| `confirmPassword` | `string` | Yes      | Must match `password`           |
| `firstName`       | `string` | No       |                                 |
| `lastName`        | `string` | No       |                                 |

**Example Request:**
```json
{
  "email": "student@example.com",
  "userName": "john_doe",
  "password": "P@ssw0rd123",
  "confirmPassword": "P@ssw0rd123",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": null,
  "message": "Registration successful"
}
```

**Error Response:** `400 Bad Request`
```json
{
  "success": false,
  "data": null,
  "message": "Email already exists"
}
```

---

### 2.2 Login

Authenticates a user and returns JWT tokens.

```
POST /api/auth/login
```

**Auth:** None (public)

**Request Body:**

| Field      | Type     | Required |
| ---------- | -------- | -------- |
| `email`    | `string` | Yes      |
| `password` | `string` | Yes      |

**Example Request:**
```json
{
  "email": "student@example.com",
  "password": "P@ssw0rd123"
}
```

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "accessTokenExpiration": "2026-02-15T15:30:00Z",
    "refreshTokenExpiration": "2026-03-15T13:30:00Z"
  }
}
```

> **Frontend Note:** Store both tokens securely. Use the `accessToken` in the `Authorization: Bearer <token>` header. When it expires, use the refresh token endpoint.

---

### 2.3 Refresh Token

Exchanges an expired access token + valid refresh token for a new token pair.

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

> **Frontend Note:** After refreshing, replace both stored tokens. The old refresh token is revoked.

---

### 2.4 Logout

Revokes the user's refresh token, ending the session.

```
POST /api/auth/logout
```

**Auth:** Required (any authenticated user)

**Request Body:**

| Field          | Type     | Required |
| -------------- | -------- | -------- |
| `refreshToken` | `string` | Yes      |

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": null,
  "message": "Logged out successfully"
}
```

---

## 3. Users

### 3.1 Get My Profile

Returns the authenticated user's profile.

```
GET /api/users/me
```

**Auth:** Required

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "student@example.com",
    "userName": "john_doe",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["Student"],
    "createdAt": "2026-01-10T08:00:00Z",
    "updatedAt": "2026-02-01T12:00:00Z"
  }
}
```

---

### 3.2 Update My Profile

Updates the authenticated user's profile fields. Only non-null fields are updated.

```
PUT /api/users/me
```

**Auth:** Required

**Request Body:**

| Field       | Type      | Required | Notes                       |
| ----------- | --------- | -------- | --------------------------- |
| `firstName` | `string?` | No       | Only updated if provided    |
| `lastName`  | `string?` | No       | Only updated if provided    |
| `userName`  | `string?` | No       | Must be unique if provided  |

**Success Response:** `200 OK`

---

### 3.3 Get User by ID

Returns any user's public profile.

```
GET /api/users/{UserId}
```

**Auth:** Required

| Parameter | Type   | In    |
| --------- | ------ | ----- |
| `UserId`  | `Guid` | Route |

**Response:** `200 OK` — Same schema as [Get My Profile](#31-get-my-profile)

---

### 3.4 Get User Stats

Returns learning/teaching statistics for a user.

```
GET /api/users/stats
```

**Auth:** Required

| Parameter | Type    | In    | Required | Default         |
| --------- | ------- | ----- | -------- | --------------- |
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
    "lastActiveDate": "2026-02-14T18:00:00Z"
  }
}
```

---

### 3.5 Become Teacher

Adds the `Teacher` role to the authenticated user. Returns fresh tokens with the updated role claims.

```
POST /api/users/become-teacher
```

**Auth:** Required

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "tokens": {
      "accessToken": "eyJhbGciOiJIUzI1NiIs...",
      "refreshToken": "new-refresh-token"
    }
  }
}
```

> **Frontend Note:** Replace stored tokens immediately — the new access token includes the `Teacher` role claim.

**Error:** `400 Bad Request` if user is already a teacher.

---

### 3.6 Teacher Dashboard

Returns aggregated statistics for the teacher's courses, students, and grading workload.

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
    "ungradedSubmissions": 14
  }
}
```

---

## 4. Courses

### 4.1 Get All Courses

Returns a paginated list of **published** courses.

```
GET /api/courses
```

**Auth:** None (public)

| Parameter  | Type   | In    | Default |
| ---------- | ------ | ----- | ------- |
| `Page`     | `int?` | Query | 1       |
| `PageSize` | `int?` | Query | 10      |

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "title": "Introduction to Machine Learning",
        "description": "Learn the fundamentals of ML...",
        "teacherId": "8b1c2d3e-4f5a-6789-0abc-def123456789",
        "teacherName": "Dr. Smith",
        "isPublished": true,
        "lectureCount": 12,
        "enrollmentCount": 45,
        "createdAt": "2026-01-15T10:00:00Z",
        "isEnrolled": false,
        "averageRating": 4.5,
        "reviewCount": 23
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3,
    "hasPrevious": false,
    "hasNext": true
  }
}
```

> **Frontend Note:** `isEnrolled` is `false` for unauthenticated users. For authenticated users, it reflects their enrollment status.

---

### 4.2 Search Courses

Searches published courses by keyword (matches title and description).

```
GET /api/courses/search
```

**Auth:** None (public)

| Parameter  | Type     | In    | Required |
| ---------- | -------- | ----- | -------- |
| `Keyword`  | `string` | Query | Yes      |
| `Page`     | `int?`   | Query | No       |
| `PageSize` | `int?`   | Query | No       |

**Response:** Same schema as [Get All Courses](#41-get-all-courses)

---

### 4.3 Get Course Details

Returns detailed info about a single course including lecture summaries.

```
GET /api/courses/{CourseId}
```

**Auth:** None (public)

| Parameter  | Type   | In    |
| ---------- | ------ | ----- |
| `CourseId`  | `Guid` | Route |

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-...",
    "title": "Introduction to Machine Learning",
    "description": "Learn the fundamentals...",
    "teacherId": "8b1c2d3e-...",
    "teacherName": "Dr. Smith",
    "isPublished": true,
    "createdAt": "2026-01-15T10:00:00Z",
    "updatedAt": "2026-02-01T12:00:00Z",
    "lectureCount": 12,
    "enrollmentCount": 45,
    "isEnrolled": true,
    "hasReviewed": false,
    "averageRating": 4.5,
    "reviewCount": 23,
    "lectures": [
      { "id": "...", "title": "What is ML?", "orderIndex": 1 },
      { "id": "...", "title": "Supervised Learning", "orderIndex": 2 }
    ]
  }
}
```

---

### 4.4 Get Instructor's Courses

Returns all courses by a specific instructor.

```
GET /api/courses/instructor/{InstructorId}
```

**Auth:** Required

| Parameter            | Type     | In    | Required | Default |
| -------------------- | -------- | ----- | -------- | ------- |
| `InstructorId`       | `Guid`   | Route | Yes      |         |
| `IncludeUnpublished` | `bool?`  | Query | No       | `false` |
| `Page`               | `int?`   | Query | No       | 1       |
| `PageSize`           | `int?`   | Query | No       | 10      |

**Response:** Same item schema as [Get All Courses](#41-get-all-courses)

---

### 4.5 Get My Courses (Teacher)

Returns the authenticated teacher's own courses.

```
GET /api/courses/my-courses
```

**Auth:** Required | **Role:** `Teacher`

| Parameter            | Type    | In    | Default |
| -------------------- | ------- | ----- | ------- |
| `IncludeUnpublished` | `bool?` | Query | `true`  |
| `Page`               | `int?`  | Query | 1       |
| `PageSize`           | `int?`  | Query | 10      |

**Response:** Same item schema as [Get All Courses](#41-get-all-courses)

---

### 4.6 Create Course

Creates a new course with the authenticated teacher as instructor. Course starts **unpublished**.

```
POST /api/courses
```

**Auth:** Required | **Role:** `Teacher`

**Request Body:**

| Field         | Type     | Required |
| ------------- | -------- | -------- |
| `title`       | `string` | Yes      |
| `description` | `string` | Yes      |

**Success Response:** `201 Created`
```json
{
  "success": true,
  "data": {
    "courseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }
}
```

---

### 4.7 Update Course

Updates a course's title and description.

```
PUT /api/courses/{CourseId}
```

**Auth:** Required | **Role:** `Teacher` (must be the course instructor)

| Parameter  | Type   | In    |
| ---------- | ------ | ----- |
| `CourseId`  | `Guid` | Route |

**Request Body:**

| Field         | Type     | Required |
| ------------- | -------- | -------- |
| `title`       | `string` | Yes      |
| `description` | `string` | Yes      |

**Success Response:** `204 No Content`

---

### 4.8 Delete Course

Deletes a course and all its lectures, materials, exams, enrollments, and study sessions.

```
DELETE /api/courses/{CourseId}
```

**Auth:** Required | **Role:** `Teacher` (must be the course instructor)

**Success Response:** `204 No Content`

> **Warning:** This action is irreversible and cascades to all related data.

---

### 4.9 Publish Course

Makes a course visible to students for enrollment.

```
POST /api/courses/{CourseId}/publish
```

**Auth:** Required | **Role:** `Teacher` (must be the course instructor)

| Parameter  | Type   | In    |
| ---------- | ------ | ----- |
| `CourseId`  | `Guid` | Route |

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": null,
  "message": "Course published successfully"
}
```

---

## 5. Enrollments

### 5.1 Enroll in Course

Enrolls the authenticated user in a published course.

```
POST /api/courses/{CourseId}/enroll
```

**Auth:** Required

| Parameter | Type   | In    |
| --------- | ------ | ----- |
| `CourseId` | `Guid` | Route |

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "enrollmentId": "3fa85f64-..."
  }
}
```

**Error Cases:**
- `400` — Already enrolled
- `404` — Course not found or not published

---

### 5.2 Unenroll from Course

Removes the authenticated user's enrollment from a course.

```
DELETE /api/courses/{CourseId}/unenroll
```

**Auth:** Required

| Parameter | Type   | In    |
| --------- | ------ | ----- |
| `CourseId` | `Guid` | Route |

**Success Response:** `200 OK`

---

### 5.3 Get My Enrollments

Returns all courses the authenticated user is enrolled in.

```
GET /api/courses/enrolled
```

**Auth:** Required

| Parameter  | Type   | In    | Default |
| ---------- | ------ | ----- | ------- |
| `Page`     | `int?` | Query | 1       |
| `PageSize` | `int?` | Query | 10      |

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "enrollment-guid",
        "studentId": "student-guid",
        "studentName": "John Doe",
        "courseId": "course-guid",
        "courseTitle": "Introduction to ML",
        "enrolledAt": "2026-01-20T09:00:00Z",
        "status": "Active"
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 5,
    "totalPages": 1,
    "hasPrevious": false,
    "hasNext": false
  }
}
```

---

### 5.4 Get Course Enrollments (Teacher)

Returns all students enrolled in a specific course.

```
GET /api/courses/{CourseId}/enrollments
```

**Auth:** Required | **Role:** `Teacher` (must be the course instructor)

| Parameter  | Type   | In    |
| ---------- | ------ | ----- |
| `CourseId`  | `Guid` | Route |
| `Page`     | `int?` | Query |
| `PageSize` | `int?` | Query |

**Response:** Same schema as [Get My Enrollments](#53-get-my-enrollments)

---

## 6. Lectures

### 6.1 Add Lecture

Adds a lecture to a course.

```
POST /api/courses/{CourseId}/lectures
```

**Auth:** Required | **Role:** `Teacher` (must be course instructor)

**Request Body:**

| Field         | Type     | Required | Notes                      |
| ------------- | -------- | -------- | -------------------------- |
| `title`       | `string` | Yes      |                            |
| `description` | `string` | Yes      |                            |
| `orderIndex`  | `int`    | Yes      | Position in course outline |

**Success Response:** `201 Created`
```json
{
  "success": true,
  "data": {
    "lectureId": "3fa85f64-..."
  }
}
```

---

### 6.2 Get Course Lectures

Returns all lectures for a course with optional materials.

```
GET /api/courses/{CourseId}/lectures
```

**Auth:** Required (must be enrolled or course instructor)

| Parameter          | Type    | In    | Default |
| ------------------ | ------- | ----- | ------- |
| `CourseId`          | `Guid`  | Route |         |
| `IncludeMaterials` | `bool?` | Query | `true`  |

**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "id": "lecture-guid",
      "courseId": "course-guid",
      "title": "Introduction to Neural Networks",
      "description": "Understanding the basics...",
      "orderIndex": 1,
      "createdAt": "2026-01-20T10:00:00Z",
      "updatedAt": "2026-01-20T10:00:00Z",
      "materials": [
        {
          "id": "material-guid",
          "lectureId": "lecture-guid",
          "type": "Document",
          "title": "Lecture Notes - Neural Networks.pdf",
          "streamUrl": "/api/materials/material-guid/stream",
          "indexed": true,
          "createdAt": "2026-01-20T10:00:00Z",
          "updatedAt": "2026-01-20T10:00:00Z"
        }
      ]
    }
  ]
}
```

---

### 6.3 Get Lecture Details

Returns detailed lecture info with materials categorized by type.

```
GET /api/lectures/{LectureId}
```

**Auth:** Required (must be enrolled or course instructor)

| Parameter   | Type   | In    |
| ----------- | ------ | ----- |
| `LectureId` | `Guid` | Route |

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "id": "lecture-guid",
    "courseId": "course-guid",
    "courseTitle": "Introduction to ML",
    "title": "Neural Networks Basics",
    "description": "...",
    "orderIndex": 1,
    "createdAt": "2026-01-20T10:00:00Z",
    "updatedAt": "2026-01-20T10:00:00Z",
    "materialsByType": {
      "Document": [
        { "id": "...", "title": "Notes.pdf", "streamUrl": "/api/materials/.../stream", ... }
      ],
      "Video": [
        { "id": "...", "title": "Lecture Recording.mp4", "streamUrl": "/api/materials/.../stream", ... }
      ]
    },
    "totalMaterials": 3
  }
}
```

---

### 6.4 Update Lecture

```
PUT /api/courses/lectures/{LectureId}
```

**Auth:** Required | **Role:** `Teacher` (must be course instructor)

**Request Body:**

| Field         | Type     | Required |
| ------------- | -------- | -------- |
| `title`       | `string` | Yes      |
| `description` | `string` | Yes      |
| `orderIndex`  | `int`    | Yes      |

**Success Response:** `204 No Content`

---

### 6.5 Delete Lecture

Deletes a lecture and all its materials (including files from storage).

```
DELETE /api/courses/lectures/{LectureId}
```

**Auth:** Required | **Role:** `Teacher` (must be course instructor)

**Success Response:** `204 No Content`

---

## 7. Materials

### 7.1 Upload Materials

Bulk upload files to a lecture. Material type (`Video`, `Document`, `Audio`, `Image`) is automatically inferred from the file extension.

```
POST /api/courses/lectures/{LectureId}/materials
```

**Auth:** Required | **Role:** `Teacher` (must be course instructor)

**Content-Type:** `multipart/form-data`

| Parameter   | Type       | In        | Required | Notes                                     |
| ----------- | ---------- | --------- | -------- | ----------------------------------------- |
| `LectureId` | `Guid`    | Route     | Yes      |                                           |
| `Files`     | `File[]`   | Form Data | Yes      | One or more files                         |
| `Titles`    | `string`   | Query     | No       | Comma-separated titles matching file order |

**Supported Formats:**

| Type     | Extensions                                       |
| -------- | ------------------------------------------------ |
| Document | `.pdf`                                            |
| Video    | `.mp4`, `.avi`, `.mov`, `.mkv`, `.webm`, `.flv`, `.wmv`, `.m4v` |
| Audio    | `.wav`, `.mp3`                                    |
| Image    | `.png`, `.jpg`, `.jpeg`, `.gif`, `.bmp`, `.webp`, `.tiff` |

**Success Response:** `201 Created`
```json
{
  "success": true,
  "data": {
    "materialIds": [
      "material-guid-1",
      "material-guid-2"
    ]
  }
}
```

> **Frontend Note:** After upload, materials are automatically queued for AI indexing (text extraction, embedding, RAG). The `indexed` field on `MaterialDto` will become `true` once processing completes.

---

### 7.2 Get Lecture Materials

Returns all materials for a lecture.

```
GET /api/courses/lectures/{LectureId}/materials
```

**Auth:** Required (must be enrolled or course instructor)

| Parameter   | Type   | In    |
| ----------- | ------ | ----- |
| `LectureId` | `Guid` | Route |

**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "id": "material-guid",
      "lectureId": "lecture-guid",
      "type": "Video",
      "title": "Lecture 1 Recording.mp4",
      "streamUrl": "/api/materials/material-guid/stream",
      "indexed": true,
      "createdAt": "2026-01-20T10:00:00Z",
      "updatedAt": "2026-01-20T10:00:00Z"
    }
  ]
}
```

> **Frontend Note:** Use the `streamUrl` to access file content. For videos/audio, use it as the `<video>` or `<audio>` `src` attribute. For documents, use it for download links or embedded viewers.

---

### 7.3 Stream / Download Material

Streams a material file with full HTTP Range support for video/audio seeking.

```
GET /api/materials/{MaterialId}/stream
```

**Auth:** Required (must be enrolled or course instructor)

| Parameter    | Type   | In    |
| ------------ | ------ | ----- |
| `MaterialId` | `Guid` | Route |

**Request Headers (Optional):**

| Header  | Example                  | Purpose              |
| ------- | ------------------------ | -------------------- |
| `Range` | `bytes=0-1048575`        | Request partial content for seeking |

**Response Headers:**

| Header              | Value                                 |
| ------------------- | ------------------------------------- |
| `Content-Type`      | MIME type (e.g., `video/mp4`, `application/pdf`) |
| `Content-Length`    | File size in bytes                     |
| `Accept-Ranges`     | `bytes`                               |
| `Content-Disposition` | `inline` for viewable types, `attachment` for documents |
| `Cache-Control`     | `public, max-age=3600`                |

**Response Codes:**
- `200 OK` — Full file returned
- `206 Partial Content` — Partial file returned (Range request)
- `404 Not Found` — Material doesn't exist or file missing

> **Frontend Integration Guide:**
>
> **Video/Audio Player:**
> ```html
> <video controls>
>   <source src="/api/materials/{id}/stream" type="video/mp4">
> </video>
> ```
> Include the `Authorization` header via a service worker or use a token-authenticated proxy.
>
> **PDF Viewer:** Use the stream URL with a PDF viewer library (e.g., PDF.js).
>
> **Download Link:** The endpoint returns `Content-Disposition: attachment` for document types.

---

### 7.4 Delete Material

Deletes a material record and its physical file from storage.

```
DELETE /api/courses/materials/{MaterialId}
```

**Auth:** Required | **Role:** `Teacher` (must be course instructor)

| Parameter    | Type   | In    |
| ------------ | ------ | ----- |
| `MaterialId` | `Guid` | Route |

**Success Response:** `204 No Content`

---

## 8. Exams

### 8.1 Create Exam

Creates an exam for a course with a time window and duration.

```
POST /api/courses/{CourseId}/exams
```

**Auth:** Required | **Role:** `Teacher` (must be course instructor)

**Request Body:**

| Field             | Type       | Required | Notes                                    |
| ----------------- | ---------- | -------- | ---------------------------------------- |
| `title`           | `string`   | Yes      |                                          |
| `startTime`       | `DateTime` | Yes      | When students can start the exam (UTC)   |
| `endTime`         | `DateTime` | Yes      | Deadline for submissions (UTC)           |
| `durationMinutes` | `int`      | Yes      | Time limit once a student starts         |

**Example Request:**
```json
{
  "title": "Midterm Exam - Machine Learning",
  "startTime": "2026-03-01T09:00:00Z",
  "endTime": "2026-03-01T12:00:00Z",
  "durationMinutes": 90
}
```

**Success Response:** `201 Created`
```json
{
  "success": true,
  "data": {
    "examId": "exam-guid"
  }
}
```

---

### 8.2 Get Exam Details

Returns full exam details including all questions.

```
GET /api/exams/{ExamId}
```

**Auth:** Required

| Parameter | Type   | In    |
| --------- | ------ | ----- |
| `ExamId`  | `Guid` | Route |

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "id": "exam-guid",
    "courseId": "course-guid",
    "title": "Midterm Exam",
    "startTime": "2026-03-01T09:00:00Z",
    "endTime": "2026-03-01T12:00:00Z",
    "durationMinutes": 90,
    "questions": [
      {
        "id": "question-guid",
        "examId": "exam-guid",
        "type": "MultipleChoice",
        "text": "Which algorithm is used for classification?",
        "options": "[\"SVM\", \"K-Means\", \"PCA\", \"DBSCAN\"]",
        "correctAnswer": "SVM",
        "points": 5,
        "order": 1
      }
    ],
    "submissionCount": 15
  }
}
```

> **Frontend Note:** For students taking the exam, hide `correctAnswer` on the client side until after submission. The API returns all question details — the frontend controls what's visible.

---

### 8.3 Update Exam

```
PUT /api/exams/{ExamId}
```

**Auth:** Required | **Role:** `Teacher` (must be course instructor)

**Request Body:** Same as [Create Exam](#81-create-exam) request.

**Success Response:** `204 No Content`

---

### 8.4 Delete Exam

Deletes an exam, all its questions, and all submissions/grades.

```
DELETE /api/exams/{ExamId}
```

**Auth:** Required | **Role:** `Teacher`

**Success Response:** `204 No Content`

---

### 8.5 Get Course Exams

Returns all exams for a course.

```
GET /api/exams/course/{CourseId}
```

**Auth:** Required

| Parameter  | Type   | In    |
| ---------- | ------ | ----- |
| `CourseId`  | `Guid` | Route |
| `Page`     | `int?` | Query |
| `PageSize` | `int?` | Query |

**Response:** `200 OK` — Paginated list of:
```json
{
  "id": "exam-guid",
  "courseId": "course-guid",
  "title": "Midterm Exam",
  "startTime": "2026-03-01T09:00:00Z",
  "endTime": "2026-03-01T12:00:00Z",
  "durationMinutes": 90,
  "questionCount": 20
}
```

---

### 8.6 Get Active Exams

Returns exams currently within their start/end time window.

```
GET /api/exams/active/{CourseId}
```

**Auth:** Required

---

### 8.7 Get Upcoming Exams

Returns exams that haven't started yet.

```
GET /api/exams/upcoming/{CourseId}
```

**Auth:** Required

---

### 8.8 Get Past Exams

Returns exams whose end time has passed.

```
GET /api/exams/past/{CourseId}
```

**Auth:** Required

---

### 8.9 Get Available Exams (Student)

Returns all active exams from the student's enrolled courses.

```
GET /api/exams/available
```

**Auth:** Required

| Parameter  | Type   | In    |
| ---------- | ------ | ----- |
| `Page`     | `int?` | Query |
| `PageSize` | `int?` | Query |

---

### 8.10 Get Exam Total Points

Returns the sum of all question points for an exam.

```
GET /api/exams/{ExamId}/total-points
```

**Auth:** Required

**Response:** `200 OK`
```json
{
  "success": true,
  "data": 100
}
```

---

## 9. Questions

### 9.1 Add Question

Adds a single question to an exam.

```
POST /api/exams/{ExamId}/questions
```

**Auth:** Required | **Role:** `Teacher`

**Request Body:**

| Field           | Type           | Required | Notes                            |
| --------------- | -------------- | -------- | -------------------------------- |
| `type`          | `QuestionType` | Yes      | See [Enums](#14-enums-reference) |
| `text`          | `string`       | Yes      | The question text                |
| `options`       | `string[]?`    | Depends  | Required for `MultipleChoice`    |
| `correctAnswer` | `string`       | Yes      | Expected answer                  |
| `points`        | `int`          | Yes      | Point value                      |

**Example (Multiple Choice):**
```json
{
  "type": "MultipleChoice",
  "text": "What is the capital of France?",
  "options": ["London", "Paris", "Berlin", "Madrid"],
  "correctAnswer": "Paris",
  "points": 5
}
```

**Example (True/False):**
```json
{
  "type": "TrueFalse",
  "text": "The Earth is flat.",
  "correctAnswer": "False",
  "points": 2
}
```

**Example (Essay):**
```json
{
  "type": "Essay",
  "text": "Explain the concept of backpropagation in neural networks.",
  "correctAnswer": "Model answer for AI grading reference...",
  "points": 20
}
```

**Success Response:** `201 Created`
```json
{
  "success": true,
  "data": {
    "questionId": "question-guid"
  }
}
```

---

### 9.2 Add Bulk Questions

Adds multiple questions at once.

```
POST /api/exams/{ExamId}/questions/bulk
```

**Auth:** Required | **Role:** `Teacher`

**Request Body:**
```json
{
  "questions": [
    {
      "type": "MultipleChoice",
      "text": "Question 1?",
      "options": ["A", "B", "C", "D"],
      "correctAnswer": "B",
      "points": 5
    },
    {
      "type": "TrueFalse",
      "text": "Question 2?",
      "correctAnswer": "True",
      "points": 2
    }
  ]
}
```

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "questionIds": ["guid-1", "guid-2"]
  }
}
```

---

### 9.3 Generate AI Questions

Uses AI to generate exam questions from course materials (RAG-powered).

```
POST /api/exams/{ExamId}/questions/generate-ai
```

**Auth:** Required | **Role:** `Teacher`

**Request Body:**

| Field               | Type              | Required | Default   | Notes                           |
| ------------------- | ----------------- | -------- | --------- | ------------------------------- |
| `numberOfQuestions`  | `int`             | Yes      |           | How many questions to generate  |
| `difficulty`        | `string?`         | No       | `"Mixed"` | `Easy`, `Medium`, `Hard`, `Mixed` |
| `questionTypes`     | `QuestionType[]?` | No       | All types | Filter by type                  |
| `focusTopics`       | `string[]?`       | No       |           | Specific topics to focus on     |
| `lectureIds`        | `Guid[]?`         | No       |           | Limit source material           |
| `materialIds`       | `Guid[]?`         | No       |           | Limit source material           |

**Example Request:**
```json
{
  "numberOfQuestions": 10,
  "difficulty": "Medium",
  "questionTypes": ["MultipleChoice", "TrueFalse"],
  "focusTopics": ["Neural Networks", "Backpropagation"],
  "lectureIds": ["lecture-guid-1", "lecture-guid-2"]
}
```

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "examId": "exam-guid",
    "questionsGenerated": 10,
    "questionIds": ["guid-1", "guid-2", "..."],
    "generationTimeMs": 15000,
    "model": "qwen2.5:14b"
  }
}
```

> **Frontend Note:** This operation may take 10-30 seconds depending on the number of questions and material volume. Show a loading indicator.

---

### 9.4 Get Exam Questions

```
GET /api/exams/{ExamId}/questions
```

**Auth:** Required

**Response:** `200 OK` — Array of `QuestionDto` (see [Exam Details](#82-get-exam-details))

---

### 9.5 Update Question

```
PUT /api/exams/questions/{QuestionId}
```

**Auth:** Required | **Role:** `Teacher`

**Request Body:** Same as [Add Question](#91-add-question)

**Success Response:** `204 No Content`

---

### 9.6 Delete Question

```
DELETE /api/exams/questions/{QuestionId}
```

**Auth:** Required | **Role:** `Teacher`

**Success Response:** `204 No Content`

---

### 9.7 Reorder Questions

Changes the display order of questions within an exam.

```
POST /api/exams/{ExamId}/questions/reorder
```

**Auth:** Required | **Role:** `Teacher`

**Request Body:**
```json
{
  "questionOrders": {
    "question-guid-1": 1,
    "question-guid-2": 2,
    "question-guid-3": 3
  }
}
```

**Success Response:** `204 No Content`

---

## 10. Submissions

### 10.1 Submit Exam

Submits the student's answers for an exam.

```
POST /api/exams/{ExamId}/submit
```

**Auth:** Required

**Request Body:**

| Field     | Type                      | Required | Notes                          |
| --------- | ------------------------- | -------- | ------------------------------ |
| `answers` | `Dictionary<Guid, string>` | Yes      | Map: questionId → student answer |

**Example Request:**
```json
{
  "answers": {
    "question-guid-1": "Paris",
    "question-guid-2": "True",
    "question-guid-3": "Backpropagation is an algorithm..."
  }
}
```

**Success Response:** `201 Created`
```json
{
  "success": true,
  "data": {
    "submissionId": "submission-guid"
  }
}
```

**Error Cases:**
- `400` — Exam not active, already submitted, or time expired.

---

### 10.2 Get Exam Submissions (Teacher)

```
GET /api/exams/{ExamId}/submissions
```

**Auth:** Required | **Role:** `Teacher`

| Parameter  | Type   | In    |
| ---------- | ------ | ----- |
| `ExamId`   | `Guid` | Route |
| `Page`     | `int?` | Query |
| `PageSize` | `int?` | Query |

**Response:** `200 OK` — Paginated list of:
```json
{
  "id": "submission-guid",
  "examId": "exam-guid",
  "studentId": "student-guid",
  "submittedAt": "2026-03-01T10:30:00Z",
  "isGraded": false
}
```

---

### 10.3 Get Submission Details

```
GET /api/exams/submissions/{SubmissionId}
```

**Auth:** Required (student who submitted or course instructor)

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "id": "submission-guid",
    "examId": "exam-guid",
    "studentId": "student-guid",
    "answers": "{\"question-guid-1\": \"Paris\", ...}",
    "submittedAt": "2026-03-01T10:30:00Z",
    "grade": {
      "id": "grade-guid",
      "submissionId": "submission-guid",
      "score": 85.0,
      "feedback": "Good work! Review question 3.",
      "isAiGraded": false,
      "isApproved": true
    }
  }
}
```

---

### 10.4 Get My Submissions (Student)

```
GET /api/exams/submissions/student
```

**Auth:** Required

| Parameter  | Type   | In    |
| ---------- | ------ | ----- |
| `Page`     | `int?` | Query |
| `PageSize` | `int?` | Query |

---

### 10.5 Get Ungraded Submissions (Teacher)

```
GET /api/exams/submissions/ungraded
```

**Auth:** Required | **Role:** `Teacher`

| Parameter  | Type    | In    | Notes                      |
| ---------- | ------- | ----- | -------------------------- |
| `ExamId`   | `Guid?` | Query | Optional filter by exam    |
| `Page`     | `int?`  | Query |                            |
| `PageSize` | `int?`  | Query |                            |

---

### 10.6 Get Submission Statistics (Teacher)

```
GET /api/submissions/stats/{ExamId}
```

**Auth:** Required | **Role:** `Teacher`

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "totalSubmissions": 30,
    "gradedCount": 25,
    "pendingGradeCount": 5,
    "aiGradedCount": 15,
    "approvedCount": 20,
    "averageScore": 78.5,
    "highestScore": 98.0,
    "lowestScore": 42.0
  }
}
```

---

## 11. Grades

### 11.1 Grade Submission (Manual)

Manually assigns a grade to a submission.

```
POST /api/exams/submissions/{SubmissionId}/grade
```

**Auth:** Required | **Role:** `Teacher`

**Request Body:**

| Field      | Type     | Required |
| ---------- | -------- | -------- |
| `score`    | `float`  | Yes      |
| `feedback` | `string` | Yes      |

**Success Response:** `201 Created`
```json
{
  "success": true,
  "data": {
    "gradeId": "grade-guid"
  }
}
```

---

### 11.2 Grade with AI

Uses AI to automatically grade a submission (essay questions use AI rubric evaluation).

```
POST /api/exams/submissions/{SubmissionId}/grade-ai
```

**Auth:** Required | **Role:** `Teacher`

**Request Body:** None

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "gradeId": "grade-guid",
    "score": 82.0,
    "feedback": "AI-generated feedback...",
    "isAiGraded": true,
    "isApproved": false,
    "essayGrades": [
      {
        "questionId": "question-guid",
        "score": 16.0,
        "maxPoints": 20,
        "percentage": 80.0,
        "feedback": "Good understanding of the core concept...",
        "strengths": ["Clear structure", "Good examples"],
        "areasForImprovement": ["Needs more depth on backpropagation"]
      }
    ]
  }
}
```

> **Frontend Note:** AI grades have `isApproved: false` by default. The teacher must review and approve them.

---

### 11.3 Approve AI Grade

Approves an AI-generated grade, making it final.

```
POST /api/exams/grades/{GradeId}/approve
```

**Auth:** Required | **Role:** `Teacher`

**Success Response:** `204 No Content`

---

### 11.4 Update Grade

Modifies an existing grade's score and feedback.

```
PUT /api/exams/grades/{GradeId}
```

**Auth:** Required | **Role:** `Teacher`

**Request Body:**

| Field      | Type     | Required |
| ---------- | -------- | -------- |
| `score`    | `float`  | Yes      |
| `feedback` | `string` | Yes      |

**Success Response:** `204 No Content`

---

### 11.5 Get Exam Grades (Teacher)

```
GET /api/exams/{ExamId}/grades
```

**Auth:** Required | **Role:** `Teacher`

| Parameter  | Type   | In    |
| ---------- | ------ | ----- |
| `ExamId`   | `Guid` | Route |
| `Page`     | `int?` | Query |
| `PageSize` | `int?` | Query |

**Response:** `200 OK` — Paginated list of `GradeDto`

---

### 11.6 Get Pending Approval Grades (Teacher)

```
GET /api/exams/grades/pending-approval
```

**Auth:** Required | **Role:** `Teacher`

| Parameter  | Type    | In    |
| ---------- | ------- | ----- |
| `ExamId`   | `Guid?` | Query |
| `Page`     | `int?`  | Query |
| `PageSize` | `int?`  | Query |

---

### 11.7 Get My Grades (Student)

```
GET /api/exams/grades/student
```

**Auth:** Required

---

### 11.8 Get Submission Grade

```
GET /api/exams/submissions/{SubmissionId}/grade
```

**Auth:** Required

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "id": "grade-guid",
    "submissionId": "submission-guid",
    "score": 85.0,
    "feedback": "Great work on the essay section!",
    "isAiGraded": true,
    "isApproved": true
  }
}
```

---

### 11.9 Get Exam Grade Statistics (Teacher)

```
GET /api/grades/stats/exam/{ExamId}
```

**Auth:** Required | **Role:** `Teacher`

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "totalGraded": 28,
    "pendingApproval": 3,
    "averageScore": 76.4,
    "medianScore": 78.0,
    "highestScore": 98.0,
    "lowestScore": 35.0,
    "passRate": 85.7
  }
}
```

---

### 11.10 Get Student Grade Statistics

```
GET /api/grades/stats/student/{StudentId}
```

**Auth:** Required

| Parameter   | Type    | In    | Notes                     |
| ----------- | ------- | ----- | ------------------------- |
| `StudentId` | `Guid`  | Route |                           |
| `CourseId`  | `Guid?` | Query | Optional filter by course |

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "totalExamsTaken": 8,
    "averageScore": 82.5,
    "highestScore": 98.0,
    "lowestScore": 65.0,
    "totalPointsEarned": 660,
    "totalPointsPossible": 800,
    "overallPercentage": 82.5
  }
}
```

---

### 11.11 Get Grade Distribution (Teacher)

```
GET /api/grades/distribution/{ExamId}
```

**Auth:** Required | **Role:** `Teacher`

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "A": 8,
    "B": 12,
    "C": 6,
    "D": 3,
    "F": 1
  }
}
```

---

## 12. Reviews

### 12.1 Add Review

Adds a review to a course (must be enrolled, one review per course per student).

```
POST /api/courses/{CourseId}/reviews
```

**Auth:** Required | **Role:** `Student`

**Request Body:**

| Field     | Type      | Required | Constraints |
| --------- | --------- | -------- | ----------- |
| `rating`  | `int`     | Yes      | 1–5         |
| `comment` | `string?` | No       |             |

**Example:**
```json
{
  "rating": 5,
  "comment": "Excellent course! The AI study tools are amazing."
}
```

**Success Response:** `201 Created`
```json
{
  "success": true,
  "data": {
    "reviewId": "review-guid"
  }
}
```

**Error:** `409 Conflict` if student already reviewed this course.

---

### 12.2 Get Course Reviews

```
GET /api/courses/{CourseId}/reviews
```

**Auth:** None (public)

| Parameter  | Type   | In    |
| ---------- | ------ | ----- |
| `CourseId`  | `Guid` | Route |
| `Page`     | `int?` | Query |
| `PageSize` | `int?` | Query |

**Response:** `200 OK` — Paginated list of:
```json
{
  "id": "review-guid",
  "courseId": "course-guid",
  "studentId": "student-guid",
  "studentName": "John Doe",
  "rating": 5,
  "comment": "Excellent course!",
  "createdAt": "2026-02-10T14:00:00Z",
  "updatedAt": "2026-02-10T14:00:00Z"
}
```

---

### 12.3 Get Course Rating Summary

```
GET /api/courses/{CourseId}/rating
```

**Auth:** None (public)

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "courseId": "course-guid",
    "averageRating": 4.5,
    "totalReviews": 23,
    "ratingDistribution": [1, 2, 3, 7, 10]
  }
}
```

> **Note:** `ratingDistribution` is an array of 5 elements where index 0 = 1-star count, index 4 = 5-star count.

---

### 12.4 Update Review

```
PUT /api/reviews/{ReviewId}
```

**Auth:** Required | **Role:** `Student` (must be review author)

**Request Body:**

| Field     | Type      | Required |
| --------- | --------- | -------- |
| `rating`  | `int`     | Yes      |
| `comment` | `string?` | No       |

**Success Response:** `200 OK`

---

### 12.5 Delete Review

Deletes a review. Can be done by the review author or the course instructor.

```
DELETE /api/reviews/{ReviewId}
```

**Auth:** Required

**Success Response:** `200 OK`

---

## 13. Study Sessions

Study sessions are AI-powered learning tools. Each session is linked to a course and provides:
- **AI Chat** — RAG-powered Q&A about course materials
- **Flashcards** — AI-generated study cards
- **Mind Maps** — AI-generated visual concept maps
- **Practice Quizzes** — AI-generated quizzes with auto-grading
- **Summaries** — AI-generated topic summaries

### 13.1 Start Study Session

Creates a new study session for a course.

```
POST /api/study-sessions
```

**Auth:** Required (must be enrolled in the course)

**Request Body:**

| Field      | Type   | Required |
| ---------- | ------ | -------- |
| `courseId`  | `Guid` | Yes      |

**Success Response:** `201 Created`
```json
{
  "success": true,
  "data": {
    "sessionId": "session-guid"
  }
}
```

---

### 13.2 Get Study Sessions

Returns all study sessions, optionally filtered by course.

```
GET /api/study-sessions
```

**Auth:** Required

| Parameter  | Type    | In    |
| ---------- | ------- | ----- |
| `CourseId`  | `Guid?` | Query |
| `Page`     | `int?`  | Query |
| `PageSize` | `int?`  | Query |

**Response:** `200 OK` — Paginated list of:
```json
{
  "id": "session-guid",
  "courseId": "course-guid",
  "courseName": "Introduction to ML",
  "startedAt": "2026-02-14T10:00:00Z",
  "lastActivity": "2026-02-14T11:30:00Z"
}
```

---

### 13.3 Get Session Details

Returns session metadata with counts of all generated content.

```
GET /api/study-sessions/{SessionId}
```

**Auth:** Required (must be session owner)

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "id": "session-guid",
    "courseId": "course-guid",
    "courseName": "Introduction to ML",
    "startedAt": "2026-02-14T10:00:00Z",
    "lastActivity": "2026-02-14T11:30:00Z",
    "messageCount": 12,
    "flashcardCount": 20,
    "quizCount": 3,
    "mindMapCount": 1
  }
}
```

---

### 13.4 Get Study Session Stats

Returns aggregated statistics across all study sessions.

```
GET /api/study-sessions/stats
```

**Auth:** Required

| Parameter  | Type    | In    |
| ---------- | ------- | ----- |
| `CourseId`  | `Guid?` | Query |

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "totalSessions": 15,
    "totalMessages": 120,
    "totalFlashcards": 80,
    "totalQuizzes": 25,
    "totalMindMaps": 5,
    "totalStudyTime": "12:30:00",
    "lastSessionDate": "2026-02-14T11:30:00Z"
  }
}
```

---

### 13.5 Send Chat Message (Streaming)

Sends a message to the AI study assistant. The response is **streamed via Server-Sent Events (SSE)**.

```
POST /api/study-sessions/{SessionId}/chat
```

**Auth:** Required (must be session owner)

**Request Body:**

| Field         | Type      | Required | Notes                          |
| ------------- | --------- | -------- | ------------------------------ |
| `message`     | `string`  | Yes      | The student's question         |
| `lectureId`   | `Guid?`   | No       | Focus on a specific lecture    |
| `materialIds` | `Guid[]?` | No       | Focus on specific materials    |

**Example Request:**
```json
{
  "message": "Explain backpropagation in simple terms",
  "lectureId": "lecture-guid"
}
```

**Response:** `200 OK` — `Content-Type: text/event-stream`

The response is streamed as SSE events:
```
data: {"content": "Back"}

data: {"content": "propagation"}

data: {"content": " is"}

data: {"content": " an algorithm"}

data: {"content": "..."}

data: [DONE]
```

> **Frontend Integration (SSE):**
> ```javascript
> const response = await fetch('/api/study-sessions/{sessionId}/chat', {
>   method: 'POST',
>   headers: {
>     'Content-Type': 'application/json',
>     'Authorization': `Bearer ${accessToken}`
>   },
>   body: JSON.stringify({ message: "Explain backpropagation" })
> });
>
> const reader = response.body.getReader();
> const decoder = new TextDecoder();
>
> while (true) {
>   const { done, value } = await reader.read();
>   if (done) break;
>   const text = decoder.decode(value);
>   // Parse SSE lines and append to UI
> }
> ```

---

### 13.6 Get Chat History

Returns the full chat message history for a session.

```
GET /api/study-sessions/{SessionId}/chat
```

**Auth:** Required (must be session owner)

**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "id": "message-guid",
      "role": "Student",
      "content": "Explain backpropagation in simple terms",
      "sources": null,
      "createdAt": "2026-02-14T10:05:00Z"
    },
    {
      "id": "message-guid-2",
      "role": "System",
      "content": "Backpropagation is an algorithm used to train neural networks...",
      "sources": "[\"Lecture 3 - Neural Networks.pdf (Page 12)\"]",
      "createdAt": "2026-02-14T10:05:05Z"
    }
  ]
}
```

---

### 13.7 Generate Flashcards

AI-generates flashcards from course materials.

```
POST /api/study-sessions/{SessionId}/flashcards
```

**Auth:** Required (must be session owner)

**Request Body:**

| Field           | Type      | Required | Default |
| --------------- | --------- | -------- | ------- |
| `topic`         | `string`  | Yes      |         |
| `numberOfCards` | `int?`    | No       | 10      |
| `lectureId`     | `Guid?`   | No       |         |
| `materialIds`   | `Guid[]?` | No       |         |

**Example:**
```json
{
  "topic": "Neural Network Architectures",
  "numberOfCards": 15,
  "lectureId": "lecture-guid"
}
```

**Success Response:** `201 Created`
```json
{
  "success": true,
  "data": [
    {
      "id": "flashcard-guid",
      "topic": "Neural Network Architectures",
      "frontText": "What is a Convolutional Neural Network (CNN)?",
      "backText": "A CNN is a type of neural network designed for processing structured grid data like images. It uses convolutional layers to automatically learn spatial hierarchies of features.",
      "createdAt": "2026-02-14T10:10:00Z"
    }
  ]
}
```

---

### 13.8 Get Session Flashcards

```
GET /api/study-sessions/{SessionId}/flashcards
```

**Auth:** Required (must be session owner)

**Response:** `200 OK` — Array of `FlashcardDto`

---

### 13.9 Generate Mind Map

AI-generates a hierarchical mind map from course materials.

```
POST /api/study-sessions/{SessionId}/mindmaps
```

**Auth:** Required (must be session owner)

**Request Body:**

| Field          | Type      | Required | Default |
| -------------- | --------- | -------- | ------- |
| `centralTopic` | `string`  | Yes      |         |
| `maxDepth`     | `int?`    | No       | 3       |
| `lectureId`    | `Guid?`   | No       |         |
| `materialIds`  | `Guid[]?` | No       |         |

**Success Response:** `201 Created`
```json
{
  "success": true,
  "data": {
    "id": "mindmap-guid",
    "topic": "Machine Learning",
    "nodes": "{\"id\": \"root\", \"label\": \"Machine Learning\", \"children\": [...]}",
    "connections": "[{\"from\": \"node1\", \"to\": \"node2\"}]",
    "createdAt": "2026-02-14T10:15:00Z"
  }
}
```

> **Frontend Note:** `nodes` is a JSON string containing a recursive tree structure. Parse it to render the mind map. `connections` is a JSON array of edge objects.

---

### 13.10 Get Session Mind Maps

```
GET /api/study-sessions/{SessionId}/mindmaps
```

**Auth:** Required (must be session owner)

**Response:** `200 OK` — Array of `MindMapDto`

---

### 13.11 Generate Practice Quiz

AI-generates a practice quiz with questions from course materials.

```
POST /api/study-sessions/{SessionId}/quizzes
```

**Auth:** Required (must be session owner)

**Request Body:**

| Field               | Type        | Required | Default       |
| ------------------- | ----------- | -------- | ------------- |
| `topic`             | `string`    | Yes      |               |
| `numberOfQuestions` | `int?`      | No       | 5             |
| `difficulty`        | `string?`   | No       | `"medium"`    |
| `questionTypes`     | `string[]?` | No       | `["mcq"]`     |
| `lectureId`         | `Guid?`     | No       |               |
| `materialIds`       | `Guid[]?`   | No       |               |

**Example:**
```json
{
  "topic": "Neural Networks",
  "numberOfQuestions": 10,
  "difficulty": "hard",
  "questionTypes": ["mcq", "true_false"],
  "lectureId": "lecture-guid"
}
```

**Success Response:** `201 Created`
```json
{
  "success": true,
  "data": {
    "id": "quiz-guid",
    "topic": "Neural Networks",
    "difficulty": "Hard",
    "questions": "[{\"questionText\": \"...\", \"questionType\": \"mcq\", \"options\": [...], ...}]",
    "studentAnswers": null,
    "score": 0,
    "createdAt": "2026-02-14T10:20:00Z"
  }
}
```

> **Frontend Note:** `questions` is a JSON string. Parse it to render the quiz UI. Each question object contains `questionText`, `questionType`, `options`, `correctAnswer`, `explanation`, `difficulty`.

---

### 13.12 Get Session Quizzes

```
GET /api/study-sessions/{SessionId}/quizzes
```

**Auth:** Required (must be session owner)

**Response:** `200 OK` — Array of `GeneratedQuizDto`

---

### 13.13 Submit Quiz Answers

Submits answers for a practice quiz. MCQ and True/False are auto-graded; Essay answers are AI-graded.

```
POST /api/study-sessions/{SessionId}/quizzes/{QuizId}/submit
```

**Auth:** Required (must be session owner)

**Request Body:**

| Field     | Type                     | Required | Notes                              |
| --------- | ------------------------ | -------- | ---------------------------------- |
| `answers` | `Dictionary<int, string>` | Yes      | Map: question index (0-based) → answer |

**Example:**
```json
{
  "answers": {
    "0": "B",
    "1": "True",
    "2": "Neural networks learn through..."
  }
}
```

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "quizId": "quiz-guid",
    "score": 80.0,
    "totalQuestions": 5,
    "correctCount": 4,
    "results": [
      {
        "questionIndex": 0,
        "studentAnswer": "B",
        "correctAnswer": "B",
        "isCorrect": true,
        "explanation": "Correct! B is the right answer because...",
        "aiScore": null,
        "aiFeedback": null
      },
      {
        "questionIndex": 2,
        "studentAnswer": "Neural networks learn through...",
        "correctAnswer": "Model answer...",
        "isCorrect": false,
        "explanation": "Partial answer.",
        "aiScore": 7.5,
        "aiFeedback": "Good understanding but missing key points about..."
      }
    ]
  }
}
```

---

### 13.14 Generate Summary

AI-generates a summary of a topic from course materials.

```
POST /api/study-sessions/{SessionId}/summary
```

**Auth:** Required (must be session owner)

**Request Body:**

| Field             | Type      | Required | Default |
| ----------------- | --------- | -------- | ------- |
| `topic`           | `string`  | Yes      |         |
| `summaryLength`   | `int?`    | No       | 500     |
| `includeKeyPoints`| `bool?`   | No       | `true`  |
| `lectureId`       | `Guid?`   | No       |         |
| `materialIds`     | `Guid[]?` | No       |         |

**Example:**
```json
{
  "topic": "Convolutional Neural Networks",
  "summaryLength": 800,
  "includeKeyPoints": true,
  "lectureId": "lecture-guid"
}
```

**Success Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "content": "Convolutional Neural Networks (CNNs) are a class of deep learning models...",
    "keyPoints": [
      "CNNs use convolutional layers for feature extraction",
      "Pooling layers reduce spatial dimensions",
      "Fully connected layers perform classification"
    ],
    "keyTerms": {
      "Convolution": "A mathematical operation that combines two functions...",
      "Pooling": "A downsampling technique..."
    },
    "sourceTitle": "Lecture 3 - Neural Networks.pdf",
    "originalLength": 5000,
    "summaryLength": 800
  }
}
```

---

## 14. Enums Reference

### QuestionType (Exams)
| Value             | Description                  |
| ----------------- | ---------------------------- |
| `MultipleChoice`  | Select one from options      |
| `TrueFalse`       | True or False answer         |
| `ShortAnswer`     | Brief text answer            |
| `Essay`           | Long-form written response   |
| `FillInTheBlank`  | Complete the sentence        |

### MaterialType
| Value      | Extensions                                                    |
| ---------- | ------------------------------------------------------------- |
| `Document` | `.pdf`                                                        |
| `Video`    | `.mp4`, `.avi`, `.mov`, `.mkv`, `.webm`, `.flv`, `.wmv`, `.m4v` |
| `Audio`    | `.wav`, `.mp3`                                                |
| `Image`    | `.png`, `.jpg`, `.jpeg`, `.gif`, `.bmp`, `.webp`, `.tiff`     |

### EnrollmentStatus
| Value       | Description                    |
| ----------- | ------------------------------ |
| `Active`    | Currently enrolled             |
| `Completed` | Course completed               |
| `Dropped`   | Student dropped the course     |
| `Pending`   | Enrollment pending approval    |

### ChatRole
| Value     | Description                   |
| --------- | ----------------------------- |
| `Student` | Message from the student      |
| `Teacher` | Message from the teacher      |
| `System`  | AI-generated response         |

### QuizDifficulty
| Value    |
| -------- |
| `Easy`   |
| `Medium` |
| `Hard`   |

### User Roles
| Role      | Description                         |
| --------- | ----------------------------------- |
| `Student` | Default — can enroll, study, submit |
| `Teacher` | Can create courses, exams, grade    |

> Users can hold both roles simultaneously.

---

## 15. Error Handling

### Error Response Format

All errors follow the standard response envelope:

```json
{
  "success": false,
  "data": null,
  "message": "Descriptive error message"
}
```

### Common HTTP Status Codes

| Code  | Meaning               | When                                                   |
| ----- | --------------------- | ------------------------------------------------------ |
| `200` | OK                    | Successful read or action                              |
| `201` | Created               | Resource successfully created                          |
| `204` | No Content            | Successful update or delete (no body)                  |
| `400` | Bad Request           | Validation error, invalid input, business rule violation |
| `401` | Unauthorized          | Missing or invalid JWT token                           |
| `403` | Forbidden             | Valid token but insufficient role/permissions           |
| `404` | Not Found             | Resource doesn't exist                                 |
| `409` | Conflict              | Duplicate resource (e.g., already reviewed)             |
| `500` | Internal Server Error | Unexpected server error                                |

### Authentication Errors

| Scenario                    | Status | Message                                |
| --------------------------- | ------ | -------------------------------------- |
| No `Authorization` header   | `401`  | "Authentication required"              |
| Expired access token        | `401`  | "Token has expired"                    |
| Invalid token               | `401`  | "Invalid token"                        |
| Missing required role        | `403`  | "You don't have permission..."         |

> **Frontend Note:** On receiving `401`, attempt a token refresh using the [Refresh Token](#23-refresh-token) endpoint. If that also fails, redirect to login.
