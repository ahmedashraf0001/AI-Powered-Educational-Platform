# AI Agent Task Prompt: Full-Stack LMS Integration, Dashboard Improvements & End-to-End Testing

---

## CONTEXT

You are working on a **Learning Management System (LMS)** with a backend (ASP.NET Core / C#) and a frontend. The system has **recently added new features** including:

- Course **Categories**
- Course **Pricing**
- User **Progress Tracking**
- Enhanced **Teacher Analytics**
- Extended **User Profile Fields**

Many of these new features are **not yet reflected in the DTOs, API responses, frontend dashboards, or forms**. Your job is to find every gap, fix it, and validate the full system end-to-end.

---

## STEP 1 — CODEBASE DISCOVERY

Before doing anything else, fully map the project structure:

```
1. List all files and directories from the project root.
2. Identify the backend project (Controllers, DTOs, Models, Services, Repositories, DbContext).
3. Identify the frontend project (components, pages, API service files, forms).
4. Identify the database migration files and the current schema.
5. Note the base API URL used by the frontend.
```

Do **not** skip this step. Every fix in later steps depends on understanding the full structure.

---

## STEP 2 — DTO AUDIT & FIXES

Go through **every DTO** in the backend. For each one, check whether it is missing fields from the new features (Categories, Price, Progress Tracking). Apply the following fixes:

### 2a. Course-Related DTOs

**`CourseListItemDto` / `BrowseCoursesResponseDto`** (used in Browse All Courses endpoint)
- Add `CategoryId` (Guid or int)
- Add `CategoryName` (string)
- Add `Price` (decimal)
- Add `IsFree` (bool, derived: Price == 0)

**`CreateCourseDto` / `CreateCourseRequest`** (used in POST /courses)
- Add `CategoryId` (required, Guid or int)
- Add `Price` (required, decimal, default 0)

**`UpdateCourseDto`**
- Add `CategoryId`
- Add `Price`

**`CourseDetailsDto`** (used in GET /courses/{id})
- Add `CategoryId`
- Add `CategoryName`
- Add `Price`
- Add `IsFree`

### 2b. Enrollment / Progress DTOs

**`EnrollmentDto`** or **`UserEnrollmentDto`**
- Add `ProgressPercentage` (double, 0–100)
- Add `CompletedLectures` (int)
- Add `TotalLectures` (int)
- Add `LastAccessedAt` (DateTime?)
- Add `IsCompleted` (bool)

**`LectureProgressDto`** (if it exists, or create it)
- `LectureId`
- `IsCompleted`
- `CompletedAt` (DateTime?)

### 2c. User/Profile DTOs

**`UserProfileDto`** (used in GET /users/me or /profile)
- Add `Bio` (string)
- Add `AvatarUrl` (string)
- Add `Website` (string)
- Add `LinkedInUrl` (string)
- Add `Title` (string, e.g. "Senior Engineer")
- Add `ExpertiseAreas` (List<string>)
- Add `Location` (string)

**`UpdateProfileDto`** (used in PUT /users/me or /profile)
- Mirror all fields from `UserProfileDto` above, all optional.

### 2d. Teacher Dashboard DTOs

Create or update **`TeacherDashboardDto`**:
```json
{
  "TotalCourses": 5,
  "PublishedCourses": 3,
  "DraftCourses": 2,
  "TotalEnrollments": 142,
  "TotalStudents": 98,
  "TotalRevenue": 2450.00,
  "AverageRating": 4.3,
  "TotalReviews": 67,
  "TotalLectures": 34,
  "CompletionRate": 61.5,
  "RecentEnrollments": [
    {
      "StudentName": "...",
      "CourseName": "...",
      "EnrolledAt": "..."
    }
  ],
  "CoursePerformance": [
    {
      "CourseId": "...",
      "Title": "...",
      "EnrollmentCount": 30,
      "AverageRating": 4.5,
      "CompletionRate": 70.0,
      "Revenue": 600.00
    }
  ],
  "EnrollmentTrend": [
    { "Month": "Jan 2026", "Count": 12 },
    { "Month": "Feb 2026", "Count": 19 }
  ]
}
```

### 2e. Student Stats DTO

Create or update **`StudentStatsDashboardDto`**:
```json
{
  "TotalEnrolledCourses": 4,
  "CompletedCourses": 1,
  "InProgressCourses": 3,
  "TotalLecturesCompleted": 22,
  "TotalLectures": 48,
  "OverallProgressPercentage": 45.8,
  "CertificatesEarned": 1,
  "AverageScore": 83.0,
  "RecentActivity": [
    {
      "CourseTitle": "...",
      "LectureTitle": "...",
      "CompletedAt": "..."
    }
  ],
  "EnrolledCourses": [
    {
      "CourseId": "...",
      "Title": "...",
      "ProgressPercentage": 60.0,
      "LastAccessedAt": "...",
      "IsCompleted": false
    }
  ]
}
```

> **After every DTO change**, find all places in the codebase where that DTO is mapped (AutoMapper profiles, manual mapping in services/controllers) and update those mappings to populate the new fields.

---

## STEP 3 — BACKEND SERVICE & CONTROLLER FIXES

For every DTO field added in Step 2, trace it through the backend:

```
DTO field added → Find the mapping source (Entity/Model) → 
Update the query (EF Core LINQ / raw SQL) to include that data → 
Update the mapping logic → Verify the controller returns the updated DTO
```

Specific things to verify and fix:

1. **Browse Courses** (`GET /api/courses`) — confirm `CategoryName` and `Price` are included via a JOIN or `.Include()`.
2. **Create Course** (`POST /api/courses`) — confirm `CategoryId` and `Price` are read from the request body and saved to the database.
3. **Teacher Dashboard** (`GET /api/dashboard/teacher` or similar) — rebuild the query to return all metrics defined in Step 2d.
4. **Student Dashboard / Stats** (`GET /api/dashboard/student` or `/api/users/me/stats`) — rebuild the query to return all metrics in Step 2e.
5. **Update Profile** (`PUT /api/users/me`) — ensure all new profile fields are accepted and persisted.
6. **Get Profile** (`GET /api/users/me`) — ensure all new profile fields are returned.

---

## STEP 4 — FRONTEND FIXES

### 4a. Teacher Dashboard Page

Locate the teacher dashboard component/page and completely revamp the metrics section:

**Add or update the following metric cards:**
- Total Courses (Published vs Draft breakdown)
- Total Students (unique across all courses)
- Total Enrollments
- Total Revenue (if pricing is enabled)
- Average Course Rating
- Overall Completion Rate

**Add or update the following sections:**
- **Course Performance Table**: columns for Course Name, Enrollments, Avg Rating, Completion Rate, Revenue
- **Enrollment Trend Chart**: monthly bar or line chart using last 6 months of data
- **Recent Enrollments Feed**: last 5–10 student enrollments with student name, course, and date

Pull all data from the updated Teacher Dashboard API endpoint. If charts are used, use whatever charting library is already in the project (Chart.js, Recharts, etc.) or implement a clean CSS-only solution.

### 4b. Student Stats / User Dashboard Page

Locate the student stats or user dashboard component and update it:

**Add or update the following:**
- Overall progress bar (TotalLecturesCompleted / TotalLectures)
- Enrolled courses list with per-course progress bars
- Completed vs In-Progress course counts
- Recent activity feed (last completed lectures)
- Certificates earned count

### 4c. Update Profile Form

Locate the profile update form/page and add fields for every new property in `UpdateProfileDto`:
- Bio (textarea)
- Avatar URL (text input or file upload if supported)
- Website URL
- LinkedIn URL
- Job Title / Position
- Location
- Expertise Areas (tag input or comma-separated text)

Ensure the form calls the correct `PUT /api/users/me` endpoint and sends all new fields.

### 4d. Course Forms

**Create Course Form:**
- Add `Category` dropdown (populated from `GET /api/categories`)
- Add `Price` number input (with a "Free" checkbox that sets price to 0)

**Edit Course Form:**
- Same additions as Create Course Form
- Pre-populate with existing category and price values

### 4e. Browse Courses / Course Cards

Update course card components to display:
- Category badge/tag
- Price (or "Free" label)

---

## STEP 5 — INTEGRATION VERIFICATION CHECKLIST

Go through each new feature and verify full integration from database → backend → DTO → frontend:

| Feature | DB Column Exists | Entity Updated | DTO Updated | Mapping Updated | Controller Updated | Frontend Updated |
|---|---|---|---|---|---|---|
| Course Category | ? | ? | ? | ? | ? | ? |
| Course Price | ? | ? | ? | ? | ? | ? |
| Lecture Progress Tracking | ? | ? | ? | ? | ? | ? |
| User Bio | ? | ? | ? | ? | ? | ? |
| User Avatar | ? | ? | ? | ? | ? | ? |
| User Website/LinkedIn | ? | ? | ? | ? | ? | ? |
| Teacher Revenue Metric | ? | ? | ? | ? | ? | ? |
| Teacher Completion Rate | ? | ? | ? | ? | ? | ? |
| Student Overall Progress | ? | ? | ? | ? | ? | ? |

For any row with a `?` that resolves to **No**, fix it before moving on.

---

## STEP 6 — END-TO-END WORKFLOW TESTING

Start the backend and frontend (or use existing running instances). Execute the following test scenarios in order and **record the result of each step**.

### TEACHER WORKFLOW

```
SCENARIO: Teacher registers, creates a course, adds lectures, and monitors their dashboard.

T-01: POST /api/auth/register — Register a new teacher account.
      EXPECT: 201 Created, user created with Teacher role.

T-02: POST /api/auth/login — Login with the new teacher account.
      EXPECT: 200 OK, JWT token returned.

T-03: GET /api/users/me — Fetch the teacher's profile.
      EXPECT: 200 OK, all profile fields present (including new fields, even if null).

T-04: PUT /api/users/me — Update teacher profile with bio, title, website, LinkedIn, location.
      EXPECT: 200 OK, changes persisted.

T-05: GET /api/users/me — Refetch profile.
      EXPECT: All updated fields are returned correctly.

T-06: GET /api/categories — Fetch available categories.
      EXPECT: 200 OK, list of categories returned.

T-07: POST /api/courses — Create a new course with Title, Description, CategoryId, Price.
      EXPECT: 201 Created, course returned with CategoryId and Price.

T-08: POST /api/courses/{id}/lectures — Add at least 2 lectures to the course.
      EXPECT: 201 Created for each.

T-09: POST /api/courses/{id}/publish — Publish the course.
      EXPECT: 200 OK, IsPublished = true.

T-10: GET /api/courses — Browse all courses.
      EXPECT: Course appears in list with CategoryName and Price fields populated.

T-11: GET /api/dashboard/teacher (or equivalent endpoint).
      EXPECT: 200 OK, all metrics present: TotalCourses, PublishedCourses, TotalEnrollments,
              TotalStudents, TotalRevenue, AverageRating, CompletionRate, CoursePerformance,
              EnrollmentTrend, RecentEnrollments.
```

### STUDENT WORKFLOW

```
SCENARIO: Student registers, browses courses, enrolls, tracks progress, and views their dashboard.

S-01: POST /api/auth/register — Register a new student account.
      EXPECT: 201 Created.

S-02: POST /api/auth/login — Login as student.
      EXPECT: 200 OK, JWT token returned.

S-03: GET /api/courses — Browse all courses.
      EXPECT: CategoryName and Price are present on each course item.

S-04: GET /api/courses/{id} — View full course details.
      EXPECT: CategoryId, CategoryName, Price, IsFree, LectureCount, AverageRating all present.

S-05: POST /api/courses/{id}/enroll — Enroll in the published course.
      EXPECT: 200 OK, enrollment created.

S-06: GET /api/users/me/enrollments (or equivalent).
      EXPECT: Enrolled course appears with ProgressPercentage = 0, CompletedLectures = 0.

S-07: POST /api/courses/{id}/lectures/{lectureId}/complete — Mark first lecture as complete.
      EXPECT: 200 OK.

S-08: GET /api/users/me/enrollments — Refetch enrollments.
      EXPECT: ProgressPercentage > 0, CompletedLectures = 1.

S-09: POST /api/courses/{id}/lectures/{lectureId}/complete — Mark second lecture as complete.
      EXPECT: 200 OK.

S-10: GET /api/dashboard/student (or /api/users/me/stats or equivalent).
      EXPECT: TotalEnrolledCourses = 1, TotalLecturesCompleted = 2, OverallProgressPercentage
              reflects completed lectures, RecentActivity populated.

S-11: POST /api/courses/{id}/reviews — Leave a review with rating and comment.
      EXPECT: 201 Created.

S-12: GET /api/courses/{id} — Refetch course.
      EXPECT: AverageRating updated, ReviewCount = 1.
```

### CROSS-CHECK: TEACHER DASHBOARD AFTER STUDENT ACTIVITY

```
X-01: GET /api/dashboard/teacher — Fetch teacher dashboard AFTER the student enrolled and completed lectures.
      EXPECT: TotalEnrollments = 1, TotalStudents = 1, CompletionRate reflects student progress,
              RecentEnrollments shows the student, EnrollmentTrend updated for current month.

X-02: GET /api/courses — Browse all courses as teacher.
      EXPECT: EnrollmentCount = 1 on the teacher's course.
```

---

## STEP 7 — REPORT & FIX

After completing all test scenarios, produce a structured report in this exact format:

```markdown
## TEST REPORT

### ✅ PASSED
- List each passing test step with the endpoint, HTTP status, and a one-line summary of what was verified.

### ❌ FAILED
- List each failing test step with:
  - Endpoint called
  - Expected result
  - Actual result (error message, wrong status code, missing fields, etc.)
  - Root cause (if identifiable)
  - Fix applied (if you fixed it) OR Action required (if it requires developer attention)

### ⚠️ REQUIRES DEVELOPER ATTENTION
- List all issues that could NOT be fixed automatically, with a clear description of:
  - What the problem is
  - Where in the codebase it originates
  - What the developer needs to do to fix it
  - Estimated complexity (Low / Medium / High)

### 🔧 FIXES APPLIED
- List every fix you applied during testing:
  - File modified
  - What was changed and why
  - Whether it was tested and confirmed working after the fix

### 📋 INTEGRATION GAPS REMAINING
- List any features that are still not fully integrated after your fixes,
  with the exact gap (e.g., "Price field is saved to DB but not returned in Browse Courses response")
```

---

## EXECUTION RULES FOR THE AGENT

1. **Work in order.** Complete Step 1 before Step 2, Step 2 before Step 3, etc.
2. **Never assume.** If a file might exist, check before acting. If a field might be there, verify before adding.
3. **Always trace the full chain.** Adding a DTO field means nothing unless it is also populated in the query and mapping.
4. **Test after every fix.** After changing a DTO or service method, re-run the relevant test step to confirm the fix works.
5. **Be specific in your report.** Vague entries like "something failed" are not acceptable. Include the exact endpoint, exact response, and exact file where the issue lives.
6. **Do not skip frontend fixes.** Backend-only fixes are half-done. Every data field that is now returned from the API must also be rendered somewhere meaningful in the UI.
7. **Preserve existing functionality.** Do not break currently working endpoints while adding new fields. All existing tests should still pass after your changes.
8. **Document your assumptions.** If you make a judgment call (e.g., assuming a specific endpoint path), note it clearly in the report.