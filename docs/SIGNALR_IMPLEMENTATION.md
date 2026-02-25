# SignalR Implementation Guide

> Real-time notifications in AIEduPlatform are delivered via **two SignalR hubs**.
> Both require JWT Bearer authentication.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Hubs](#2-hubs)
3. [Events Reference](#3-events-reference)
4. [Frontend Integration (JavaScript/TypeScript)](#4-frontend-integration)
5. [Flutter/Dart Integration](#5-flutterdart-integration)
6. [Configuration](#6-configuration)
7. [Testing](#7-testing)
8. [Troubleshooting](#8-troubleshooting)

---

## 1. Architecture Overview

```
┌─────────────────┐         ┌──────────────────────────┐
│  Frontend (SPA)  │ ◄──── │  StudentNotificationHub   │
│                  │  SSE   │  /hubs/student-notifications│
│  React / JS     │ ◄──── │  MaterialIndexingHub       │
│                  │        │  /hubs/material-indexing   │
└─────────────────┘         └──────────────────────────┘
         ▲                              ▲
         │ WebSocket                    │ IHubContext<T>
         │ (auto-negotiated)            │
         │                    ┌─────────┴──────────┐
         └────────────────────│  NotificationService │
                              │  (INotificationService)│
                              └────────────────────────┘
```

### Key Components

| Component                 | Location                                                    | Purpose                               |
| ------------------------- | ----------------------------------------------------------- | ------------------------------------- |
| `MaterialIndexingHub`     | `Application/SignalR/IndexingHub.cs`                        | Teacher notifications (indexing, etc.) |
| `StudentNotificationHub`  | `Application/SignalR/StudentNotificationHub.cs`             | Student notifications (all types)      |
| `CustomUserIdProvider`    | `Application/SignalR/CustomUserIdProvider.cs`               | Extracts user ID from JWT claims       |
| `NotificationService`     | `Application/Common/Services/NotificationService.cs`        | Sends events through hub contexts     |

### User ID Resolution

`CustomUserIdProvider` extracts the user ID from JWT claims in this order:
1. `ClaimTypes.NameIdentifier`
2. `"sub"`
3. `"userId"`
4. `"nameid"`

---

## 2. Hubs

### 2.1 MaterialIndexingHub

| Property        | Value                           |
| --------------- | ------------------------------- |
| **Endpoint**    | `/hubs/material-indexing`       |
| **Auth**        | `[Authorize(Roles = "Teacher")]`|
| **Target Users**| Teachers only                   |

No client-invokable methods — this hub only pushes server-to-client events.

**Events Received:**

| Event                        | When                                     | Payload                                                    |
| ---------------------------- | ---------------------------------------- | ---------------------------------------------------------- |
| `ReceiveIndexingNotification`| Material indexing completes              | `{ success, courseId, chunksIndexed, indexTimeMs, embeddingTimeMs, chunksFailed, failureRatio, error }` |
| `ExamSubmitted`              | A student submits an exam                | `{ studentName, examTitle, courseName }`                   |
| `NewEnrollment`              | A student enrolls in teacher's course    | `{ studentName, courseName }`                              |
| `NewReview`                  | A student reviews teacher's course       | `{ courseName, rating }`                                   |
| `EnrollmentCompleted`        | A student completes teacher's course     | `{ studentName, courseName }`                              |
| `StudentUnenrolled`          | A student unenrolls from teacher's course| `{ studentName, courseName }`                              |

All teacher events are sent via `.User(teacherId)` — only the specific teacher receives them.

---

### 2.2 StudentNotificationHub

| Property        | Value                              |
| --------------- | ---------------------------------- |
| **Endpoint**    | `/hubs/student-notifications`      |
| **Auth**        | `[Authorize]` (any authenticated)  |
| **Target Users**| Students (enrolled in courses)     |

**Client-Invokable Methods:**

| Method             | Parameter       | Description                                         |
| ------------------ | --------------- | --------------------------------------------------- |
| `JoinCourseGroup`  | `courseId: string` | Joins the `"course-{courseId}"` group for notifications |
| `LeaveCourseGroup` | `courseId: string` | Leaves the `"course-{courseId}"` group               |

> **Important:** Students must call `JoinCourseGroup` for each enrolled course to receive course-wide notifications. Call `LeaveCourseGroup` when unenrolling or navigating away.

---

## 3. Events Reference

### 3.1 Course-Wide Events (Group-based)

These events are broadcast to all connected students in the `"course-{courseId}"` group.

| Event                  | Trigger                                   | Payload                                                   |
| ---------------------- | ----------------------------------------- | --------------------------------------------------------- |
| `NewExamPosted`        | Teacher creates an exam                   | `{ courseId, courseName, examTitle }`                      |
| `NewMaterialUploaded`  | Teacher uploads material                  | `{ courseId, courseName, materialTitle }`                  |
| `NewLectureAdded`      | Teacher adds a lecture                    | `{ courseId, courseName, lectureTitle }`                   |
| `CourseUpdated`        | Teacher updates course details            | `{ courseId, courseName }`                                |
| `CoursePublished`      | Teacher publishes a course                | `{ courseId, courseName, isPublished: true }`             |
| `CourseUnpublished`    | Teacher unpublishes a course              | `{ courseId, courseName, isPublished: false }`            |
| `ExamUpdated`          | Teacher updates an exam                   | `{ courseId, courseName, examTitle }`                      |
| `ExamDeleted`          | Teacher deletes an exam                   | `{ courseId, courseName, examTitle }`                      |

### 3.2 Individual Events (User-based)

These events are sent to a specific student via `.User(studentId)`.

| Event               | Trigger                                    | Payload                                                              |
| -------------------- | ----------------------------------------- | -------------------------------------------------------------------- |
| `SubmissionGraded`   | Teacher grades a submission               | `{ courseName, examTitle, score }`                                   |
| `GradeApproved`      | Teacher approves an AI grade              | `{ courseName, examTitle }`                                          |
| `GradeUpdated`       | Teacher updates a grade                   | `{ courseName, examTitle, newScore }`                                |
| `EngagementAlert`    | Teacher sends engagement alert            | `{ courseName, teacherName, engagementLevel, message, sentAt }`      |

### 3.3 Teacher Events (User-based, via MaterialIndexingHub)

| Event                        | Trigger                                  | Payload                                                    |
| ---------------------------- | ---------------------------------------- | ---------------------------------------------------------- |
| `ReceiveIndexingNotification`| Material indexing completes              | `RagIndexResponse` object                                  |
| `ExamSubmitted`              | Student submits an exam                  | `{ studentName, examTitle, courseName }`                   |
| `NewEnrollment`              | Student enrolls                          | `{ studentName, courseName }`                              |
| `NewReview`                  | Student reviews a course                 | `{ courseName, rating }`                                   |
| `EnrollmentCompleted`        | Student completes a course               | `{ studentName, courseName }`                              |
| `StudentUnenrolled`          | Student unenrolls                        | `{ studentName, courseName }`                              |

---

## 4. Frontend Integration

### 4.1 Installation

```bash
npm install @microsoft/signalr
```

### 4.2 Student Notifications (Full Example)

```typescript
import * as signalR from "@microsoft/signalr";

// Build the connection
const studentConnection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7205/hubs/student-notifications", {
    accessTokenFactory: () => getAccessToken(), // Return stored JWT
  })
  .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Retry intervals
  .configureLogging(signalR.LogLevel.Information)
  .build();

// ── Course-wide events ──
studentConnection.on("NewExamPosted", (data) => {
  // data: { courseId, courseName, examTitle }
  showNotification(`New exam "${data.examTitle}" in ${data.courseName}`);
});

studentConnection.on("NewMaterialUploaded", (data) => {
  showNotification(`New material "${data.materialTitle}" in ${data.courseName}`);
});

studentConnection.on("NewLectureAdded", (data) => {
  showNotification(`New lecture "${data.lectureTitle}" in ${data.courseName}`);
});

studentConnection.on("CourseUpdated", (data) => {
  showNotification(`${data.courseName} has been updated`);
});

studentConnection.on("CoursePublished", (data) => {
  showNotification(`${data.courseName} is now published`);
});

studentConnection.on("ExamUpdated", (data) => {
  showNotification(`Exam "${data.examTitle}" has been updated in ${data.courseName}`);
});

studentConnection.on("ExamDeleted", (data) => {
  showNotification(`Exam "${data.examTitle}" has been removed from ${data.courseName}`);
});

// ── Individual events ──
studentConnection.on("SubmissionGraded", (data) => {
  // data: { courseName, examTitle, score }
  showNotification(`Your ${data.examTitle} has been graded: ${data.score}`);
});

studentConnection.on("GradeApproved", (data) => {
  showNotification(`Your grade for ${data.examTitle} has been approved`);
});

studentConnection.on("GradeUpdated", (data) => {
  showNotification(`Your grade for ${data.examTitle} updated to ${data.newScore}`);
});

studentConnection.on("EngagementAlert", (data) => {
  // data: { courseName, teacherName, engagementLevel, message, sentAt }
  showAlert(`Message from ${data.teacherName}: ${data.message}`);
});

// ── Connection lifecycle ──
studentConnection.onreconnecting(() => {
  console.warn("SignalR reconnecting...");
});

studentConnection.onreconnected(() => {
  console.log("SignalR reconnected, rejoining course groups...");
  joinAllCourseGroups(); // Re-join groups after reconnect
});

studentConnection.onclose(() => {
  console.error("SignalR connection closed");
});

// ── Start & join groups ──
async function startStudentConnection(enrolledCourseIds: string[]) {
  try {
    await studentConnection.start();
    console.log("Connected to StudentNotificationHub");

    // Join groups for all enrolled courses
    for (const courseId of enrolledCourseIds) {
      await studentConnection.invoke("JoinCourseGroup", courseId);
    }
  } catch (err) {
    console.error("Failed to connect:", err);
    setTimeout(() => startStudentConnection(enrolledCourseIds), 5000);
  }
}

// ── Leave groups on cleanup ──
async function leaveAllCourseGroups(courseIds: string[]) {
  for (const courseId of courseIds) {
    await studentConnection.invoke("LeaveCourseGroup", courseId);
  }
}
```

### 4.3 Teacher Notifications (Material Indexing Hub)

```typescript
const teacherConnection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7205/hubs/material-indexing", {
    accessTokenFactory: () => getAccessToken(),
  })
  .withAutomaticReconnect()
  .configureLogging(signalR.LogLevel.Information)
  .build();

// Material indexing results
teacherConnection.on("ReceiveIndexingNotification", (notification) => {
  // notification: { success, courseId, chunksIndexed, indexTimeMs, ... }
  if (notification.success) {
    showNotification(`Material indexed: ${notification.chunksIndexed} chunks processed`);
  } else {
    showError(`Indexing failed: ${notification.error}`);
  }
});

// Student activity notifications
teacherConnection.on("ExamSubmitted", (data) => {
  showNotification(`${data.studentName} submitted "${data.examTitle}"`);
});

teacherConnection.on("NewEnrollment", (data) => {
  showNotification(`${data.studentName} enrolled in ${data.courseName}`);
});

teacherConnection.on("NewReview", (data) => {
  showNotification(`New ${data.rating}-star review for ${data.courseName}`);
});

teacherConnection.on("EnrollmentCompleted", (data) => {
  showNotification(`${data.studentName} completed ${data.courseName}`);
});

teacherConnection.on("StudentUnenrolled", (data) => {
  showNotification(`${data.studentName} unenrolled from ${data.courseName}`);
});

await teacherConnection.start();
```

### 4.4 Connection Strategy

```typescript
// Determine which hubs to connect based on user roles
function connectSignalR(user: { roles: string[] }, enrolledCourseIds: string[]) {
  // All authenticated users connect to student hub
  startStudentConnection(enrolledCourseIds);

  // Only teachers connect to the material indexing hub
  if (user.roles.includes("Teacher")) {
    teacherConnection.start();
  }
}
```

---

## 5. Flutter/Dart Integration

> **What is SignalR?** SignalR is a Microsoft library for adding real-time web functionality to applications. It uses WebSockets by default (with fallbacks to Server-Sent Events and Long Polling). The server pushes messages to connected clients instantly — no polling required. Think of it like Firebase Realtime Database notifications, but for custom events over WebSockets.

### 5.1 Installation

Add the `signalr_netcore` package to your `pubspec.yaml`:

```yaml
dependencies:
  signalr_netcore: ^1.3.7
```

Then run:

```bash
flutter pub get
```

> **Note:** The package name is `signalr_netcore` (not `signalr_core`). This is the most maintained Dart SignalR client that supports .NET SignalR hubs with JWT auth.

### 5.2 How It Works (Overview)

1. Your Flutter app opens a **persistent WebSocket connection** to the server hub URL (e.g., `/hubs/student-notifications`).
2. The server sends **events** (e.g., `NewExamPosted`, `SubmissionGraded`) through this connection in real-time.
3. You register **listeners** (callbacks) for each event name you care about.
4. For course-scoped events, you **join a group** by calling a method on the hub (e.g., `JoinCourseGroup`). Only members of that group receive those events.
5. The connection authenticates using your **JWT token** sent as a query parameter.

### 5.3 Student Notification Connection

```dart
import 'package:signalr_netcore/signalr_client.dart';

class StudentSignalRService {
  late HubConnection _connection;
  final String _baseUrl;
  final String Function() _getAccessToken;
  final List<String> _enrolledCourseIds;

  StudentSignalRService({
    required String baseUrl,           // e.g., "https://localhost:7205"
    required String Function() getAccessToken,
    required List<String> enrolledCourseIds,
  })  : _baseUrl = baseUrl,
        _getAccessToken = getAccessToken,
        _enrolledCourseIds = enrolledCourseIds;

  Future<void> connect() async {
    // Build the hub connection with JWT auth
    _connection = HubConnectionBuilder()
        .withUrl(
          '$_baseUrl/hubs/student-notifications',
          options: HttpConnectionOptions(
            accessTokenFactory: () async => _getAccessToken(),
            // SignalR sends the token as ?access_token= query param
            // The server reads it from the query string automatically
          ),
        )
        .withAutomaticReconnect(
          retryDelays: [0, 2000, 5000, 10000, 30000],
        )
        .build();

    // ── Register event listeners BEFORE connecting ──

    // Course-wide events (received via group membership)
    _connection.on('NewExamPosted', (arguments) {
      final data = arguments![0] as Map<String, dynamic>;
      // data contains: { courseId, courseName, examTitle }
      print('New exam: ${data["examTitle"]} in ${data["courseName"]}');
      // Show a local notification or update state
    });

    _connection.on('NewMaterialUploaded', (arguments) {
      final data = arguments![0] as Map<String, dynamic>;
      // data: { courseId, courseName, materialTitle }
      print('New material: ${data["materialTitle"]}');
    });

    _connection.on('NewLectureAdded', (arguments) {
      final data = arguments![0] as Map<String, dynamic>;
      // data: { courseId, courseName, lectureTitle }
      print('New lecture: ${data["lectureTitle"]}');
    });

    _connection.on('CourseUpdated', (arguments) {
      final data = arguments![0] as Map<String, dynamic>;
      print('Course updated: ${data["courseName"]}');
    });

    _connection.on('ExamUpdated', (arguments) {
      final data = arguments![0] as Map<String, dynamic>;
      print('Exam updated: ${data["examTitle"]}');
    });

    _connection.on('ExamDeleted', (arguments) {
      final data = arguments![0] as Map<String, dynamic>;
      print('Exam deleted: ${data["examTitle"]}');
    });

    // Individual events (sent directly to this user)
    _connection.on('SubmissionGraded', (arguments) {
      final data = arguments![0] as Map<String, dynamic>;
      // data: { courseName, examTitle, score }
      print('Graded: ${data["examTitle"]} — Score: ${data["score"]}');
    });

    _connection.on('GradeApproved', (arguments) {
      final data = arguments![0] as Map<String, dynamic>;
      print('Grade approved for ${data["examTitle"]}');
    });

    _connection.on('GradeUpdated', (arguments) {
      final data = arguments![0] as Map<String, dynamic>;
      print('Grade updated: ${data["newScore"]}');
    });

    _connection.on('EngagementAlert', (arguments) {
      final data = arguments![0] as Map<String, dynamic>;
      // data: { courseName, teacherName, engagementLevel, message, sentAt }
      print('Alert from ${data["teacherName"]}: ${data["message"]}');
      // Show a prominent alert/dialog to the student
    });

    // ── Handle reconnection ──
    _connection.onreconnected(({connectionId}) async {
      // Groups are LOST on disconnect — must rejoin after reconnect
      await _joinAllCourseGroups();
    });

    // ── Start the connection ──
    await _connection.start();

    // ── Join course groups ──
    await _joinAllCourseGroups();
  }

  Future<void> _joinAllCourseGroups() async {
    for (final courseId in _enrolledCourseIds) {
      // This calls the server hub method "JoinCourseGroup"
      // which adds this connection to the "course-{courseId}" group
      await _connection.invoke('JoinCourseGroup', args: [courseId]);
    }
  }

  Future<void> leaveCourseGroup(String courseId) async {
    await _connection.invoke('LeaveCourseGroup', args: [courseId]);
  }

  Future<void> disconnect() async {
    await _connection.stop();
  }
}
```

### 5.4 Teacher Notification Connection

```dart
class TeacherSignalRService {
  late HubConnection _connection;
  final String _baseUrl;
  final String Function() _getAccessToken;

  TeacherSignalRService({
    required String baseUrl,
    required String Function() getAccessToken,
  })  : _baseUrl = baseUrl,
        _getAccessToken = getAccessToken;

  Future<void> connect() async {
    _connection = HubConnectionBuilder()
        .withUrl(
          '$_baseUrl/hubs/material-indexing',
          options: HttpConnectionOptions(
            accessTokenFactory: () async => _getAccessToken(),
          ),
        )
        .withAutomaticReconnect()
        .build();

    // Material indexing result
    _connection.on('ReceiveIndexingNotification', (arguments) {
      final data = arguments![0] as Map<String, dynamic>;
      if (data['success'] == true) {
        print('Material indexed: ${data["chunksIndexed"]} chunks');
      } else {
        print('Indexing failed: ${data["error"]}');
      }
    });

    // Student submitted an exam
    _connection.on('ExamSubmitted', (arguments) {
      final data = arguments![0] as Map<String, dynamic>;
      print('${data["studentName"]} submitted ${data["examTitle"]}');
    });

    // New enrollment
    _connection.on('NewEnrollment', (arguments) {
      final data = arguments![0] as Map<String, dynamic>;
      print('${data["studentName"]} enrolled in ${data["courseName"]}');
    });

    // New review
    _connection.on('NewReview', (arguments) {
      final data = arguments![0] as Map<String, dynamic>;
      print('New ${data["rating"]}★ review for ${data["courseName"]}');
    });

    // Student completed course
    _connection.on('EnrollmentCompleted', (arguments) {
      final data = arguments![0] as Map<String, dynamic>;
      print('${data["studentName"]} completed ${data["courseName"]}');
    });

    // Student unenrolled
    _connection.on('StudentUnenrolled', (arguments) {
      final data = arguments![0] as Map<String, dynamic>;
      print('${data["studentName"]} left ${data["courseName"]}');
    });

    await _connection.start();
  }

  Future<void> disconnect() async {
    await _connection.stop();
  }
}
```

### 5.5 Usage in Flutter App

```dart
// In your app initialization (e.g., after login)
final studentSignalR = StudentSignalRService(
  baseUrl: 'https://localhost:7205',
  getAccessToken: () => authProvider.accessToken, // Your auth state
  enrolledCourseIds: ['course-id-1', 'course-id-2'],
);

await studentSignalR.connect();

// If the user is also a teacher, connect the teacher hub too
if (authProvider.isTeacher) {
  final teacherSignalR = TeacherSignalRService(
    baseUrl: 'https://localhost:7205',
    getAccessToken: () => authProvider.accessToken,
  );
  await teacherSignalR.connect();
}

// On logout
await studentSignalR.disconnect();
```

### 5.6 Integration with State Management

If you're using **Riverpod**, **Provider**, or **Bloc**, pass callbacks that update your state instead of just printing:

```dart
// Example with a callback approach
_connection.on('SubmissionGraded', (arguments) {
  final data = arguments![0] as Map<String, dynamic>;
  // Call your state management callback
  onNotificationReceived(Notification(
    type: NotificationType.grade,
    title: '${data["examTitle"]} graded: ${data["score"]}',
    courseName: data['courseName'],
  ));
});
```

### 5.7 Important Notes for Flutter

| Topic | Detail |
|-------|--------|
| **Auth mechanism** | SignalR in browsers sends the JWT as a query parameter (`?access_token=...`). The `signalr_netcore` package handles this via `accessTokenFactory`. |
| **No client methods on MaterialIndexingHub** | Teachers only **receive** events — there are no methods to invoke. |
| **Group membership is not persistent** | If the WebSocket disconnects and reconnects, you must call `JoinCourseGroup` again for each course. Handle this in `onreconnected`. |
| **Connection lifecycle** | Connect after login, disconnect on logout. Avoid connecting before you have a valid JWT. |
| **HTTPS in development** | If using a self-signed certificate for `localhost`, you may need to configure your HTTP client to accept it, or use the HTTP endpoint (`http://localhost:5069`) during development. |
| **Background behavior** | Mobile apps may close WebSocket connections when backgrounded. Consider reconnecting in `didChangeAppLifecycleState` when the app returns to foreground. |

---

## 6. Configuration

### Hub Endpoints

| Hub                    | Path                          | Auth                    |
| ---------------------- | ----------------------------- | ----------------------- |
| MaterialIndexingHub    | `/hubs/material-indexing`     | JWT + `Teacher` role    |
| StudentNotificationHub | `/hubs/student-notifications` | JWT (any authenticated) |

### CORS

CORS is configured in `AIEduPlatform.Api/Extensions/CorsExtensions.cs`:

```csharp
policy.WithOrigins(
    "http://localhost:3000",
    "https://localhost:3000",
    "http://localhost:5069",
    "https://localhost:7205")
.AllowAnyMethod()
.AllowAnyHeader()
.AllowCredentials(); // Required for SignalR
```

> **Important:** `AllowCredentials()` is required for SignalR WebSocket connections. `AllowAnyOrigin()` is **incompatible** with credentials — always specify exact origins.

### API Endpoints

| Environment | HTTP                    | HTTPS                    |
| ----------- | ----------------------- | ------------------------ |
| Development | `http://localhost:5069`  | `https://localhost:7205`  |

---

## 7. Testing

### Quick Start

```powershell
# Terminal 1: Start the API
cd AIEduPlatform.Api
dotnet run

# Terminal 2: Run the SignalR test client
cd ../Testing/AIEduPlatform.SignalRTestClient
.\RunTestClient.ps1
```

The helper script will:
1. Check if the API is running
2. Prompt for credentials (or use defaults)
3. Register/Login to get a JWT token
4. Start the SignalR test client
5. Save the token to `token.txt`

### Manual Testing

#### 1. Get a JWT Token

```powershell
# Register a teacher account
$body = @{
    email = "teacher@test.com"
    userName = "teacher_test"
    password = "Teacher123!"
    confirmPassword = "Teacher123!"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7205/api/auth/register" `
  -Method Post -Body $body -ContentType "application/json" -SkipCertificateCheck

# Login
$body = @{
    email = "teacher@test.com"
    password = "Teacher123!"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:7205/api/auth/login" `
  -Method Post -Body $body -ContentType "application/json" -SkipCertificateCheck

$token = $response.data.accessToken
```

#### 2. Run the Test Client

```powershell
cd Testing/AIEduPlatform.SignalRTestClient
dotnet run
```

Enter the API URL and paste the JWT token when prompted.

#### 3. Trigger Notifications

Upload a material to trigger an indexing notification, or use the notification test endpoint if available.

---

## 8. Troubleshooting

| Symptom                                          | Cause                                               | Fix                                                       |
| ------------------------------------------------ | --------------------------------------------------- | --------------------------------------------------------- |
| `401 Unauthorized`                               | Invalid or expired JWT token                        | Refresh the token and reconnect                           |
| `403 Forbidden` on material-indexing hub         | User doesn't have `Teacher` role                    | Use `POST /api/users/become-teacher` first                |
| No course-wide notifications received            | Student didn't join the course group                | Call `JoinCourseGroup(courseId)` after connecting          |
| No notifications after reconnect                 | Groups are lost on reconnect                        | Re-join all groups in `onreconnected` callback            |
| CORS errors                                      | Frontend origin not in CORS whitelist               | Add the origin to `CorsExtensions.cs` `WithOrigins()`    |
| Connection drops silently                        | Network issues or server restart                    | Enable `withAutomaticReconnect()` with retry intervals    |
| API not responding                               | API not running or wrong port                       | Check `launchSettings.json` for correct ports             |

### Security Notes

- Always use HTTPS in production
- Update CORS origins for production domains
- JWT tokens expire — implement token refresh in the frontend
- Consider rate limiting hub connections for production
- Notifications are scoped to groups/users — no data leaks across courses

---

## Notification Flow Diagram

```
Teacher uploads material
  └─► BackgroundService processes material
        └─► NotificationService.NotifyIndexingCompletedAsync()
              └─► MaterialIndexingHub → Teacher receives "ReceiveIndexingNotification"

Teacher creates exam
  └─► NotificationService.NotifyNewExamPostedAsync()
        └─► StudentNotificationHub → Group "course-{id}" receives "NewExamPosted"

Student submits exam
  └─► NotificationService.NotifyExamSubmittedAsync()
        └─► MaterialIndexingHub → Teacher receives "ExamSubmitted"

Teacher grades submission
  └─► NotificationService.NotifySubmissionGradedAsync()
        └─► StudentNotificationHub → Student receives "SubmissionGraded"

Teacher sends engagement alerts
  └─► NotificationService.NotifyLowEngagementAlertAsync() (per student)
        └─► StudentNotificationHub → Each student receives "EngagementAlert"
```

---

> **Document Version:** 2.0
> **Last Updated:** February 25, 2026
