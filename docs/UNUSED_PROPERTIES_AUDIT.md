# Unused / Unexposed Properties Audit

> **Purpose:** List all entity properties not exposed in any DTO, and identify DTOs without a backing entity. This is an audit report — no changes have been made.
>
> **Note:** `CreatedAt`/`UpdatedAt` from `BaseEntity` and navigation properties are excluded from "unused" counts since they're expected patterns. Foreign key IDs omitted from DTOs are also normal (parent context is implied). Only **significant** unexposed properties are flagged.

---

## Legend

- **Intentionally internal** — Properties that logically should not be exposed (security, implementation detail)
- **Potentially unused** — Properties that exist on the entity but are never surfaced to any client. Worth reviewing whether they should be exposed or removed.
- **No DTO** — Entity has no corresponding response DTO at all

---

## Entities with Notable Unused Properties

### Question
| Property | Status | Notes |
|----------|--------|-------|
| `ModelAnswer` | **Potentially unused** | Stored in DB, used internally for AI grading prompt, but never returned to frontend. Teachers cannot see/edit it via API. |
| `GradingCriteria` | **Potentially unused** | Same as ModelAnswer — used in AI grading but not exposed. Teachers cannot set or view grading criteria via any endpoint. |

> **Recommendation:** Consider adding `ModelAnswer` and `GradingCriteria` to `QuestionDto` (for teacher view only — hide from students during exams). This would let teachers set and review grading criteria.

---

### Material
| Property | Status | Notes |
|----------|--------|-------|
| `Summary` | **Potentially unused** | Text summary of the material. Not exposed in `MaterialDto` or `MaterialProjectionDto`. |
| `DurationSeconds` | **Potentially unused** | Duration for video/audio materials. Not exposed in any DTO. Frontend cannot display "12:35" duration. |
| `TotalPages` | **Potentially unused** | Total pages for PDF/document materials. Not exposed. Frontend cannot display "45 pages". |
| `FileUrl` | Exposed as `StreamUrl` | Renamed in DTO — this is fine. |

> **Recommendation:** Add `DurationSeconds` and `TotalPages` to `MaterialDto` so the frontend can show material metadata (duration for videos, page count for PDFs). `Summary` could also be useful in the material list.

---

### Cart
| Property | Status | Notes |
|----------|--------|-------|
| `UserId` | Intentionally internal | Resolved from auth context — not needed in response. |
| `Status` | **Potentially unused** | `CartStatus` enum (Active/CheckedOut/Abandoned) is never returned to the client. Frontend cannot distinguish cart states. |

---

### CartItem
| Property | Status | Notes |
|----------|--------|-------|
| `AddedAt` | **Potentially unused** | Timestamp of when item was added to cart. Not in `CartItemDto`. Could be useful for "Added 2 days ago" display. |

---

### Notification
| Property | Status | Notes |
|----------|--------|-------|
| `Metadata` | **Potentially unused** | JSON metadata field on notifications. Not exposed in `NotificationDto`. Could contain useful contextual data (courseId, examId, etc.) for deep-linking. |

> **Recommendation:** Expose `Metadata` in `NotificationDto` so the frontend can deep-link notifications (e.g., clicking "Your exam was graded" navigates to the submission).

---

### StudySession
| Property | Status | Notes |
|----------|--------|-------|
| `EndedAt` | **Potentially unused** | Session end timestamp. Not in `SessionSummaryDto` or `SessionDetailDto`. |
| `IsActive` | **Potentially unused** | Computed property (`EndedAt == null`). Not exposed. Frontend cannot tell if a session is still active. |

> **Recommendation:** Add `EndedAt` and/or `IsActive` to `SessionSummaryDto` so the frontend can show active vs completed sessions.

---

### Concept
| Property | Status | Notes |
|----------|--------|-------|
| `NormalizedName` | Intentionally internal | Used for deduplication/matching. |
| `Embedding` | Intentionally internal | Vector data, not human-readable. |
| `CourseId` | Intentionally internal | Foreign key. |
| `MaterialId` | Intentionally internal | Foreign key. |

> All Concept properties are intentionally internal (knowledge graph infrastructure). No action needed.

---

## Entities with No Response DTO

These entities have no dedicated DTO and are never directly returned to clients:

| Entity | Reason | Action Needed? |
|--------|--------|----------------|
| `MaterialChunk` | Internal RAG infrastructure (embeddings, content chunks). Used in vector search, never shown to users. | No |
| `OrderItem` | Indirectly represented via `CheckoutItemDto` and `EnrolledCourseInfoDto` inside payment responses. | No |
| `CourseCategory` | Join table (Course ↔ Category). Category data surfaces through `CourseDetailDto.CategoryName`. | No |
| `ConceptChunkMap` | Join table (Concept ↔ MaterialChunk). Internal knowledge graph. | No |
| `RefreshToken` | Security entity. Token value surfaces only in `AuthResponseDto.RefreshToken`. | No — **must not** have a response DTO |

---

## BaseEntity Properties Commonly Omitted

The following entities inherit `CreatedAt`/`UpdatedAt` from `BaseEntity` but don't expose both in their DTOs. This is generally fine — most DTOs only include `CreatedAt` where relevant:

- **Both omitted:** Enrollment (uses `EnrolledAt` instead), Exam, Grade, Submission
- **Only `UpdatedAt` omitted:** ChatMessage, Flashcard, GeneratedQuiz, MindMap, Category, SemanticSection

---

## Foreign Keys Omitted in DTOs (Expected)

These are parent IDs that are omitted from child DTOs because the parent context is already known:

| DTO | Omitted FK | Why |
|-----|-----------|-----|
| `ChatMessageDto` | `SessionId` | Returned within session context |
| `FlashcardDto` | `SessionId` | Returned within session context |
| `GeneratedQuizDto` | `SessionId` | Returned within session context |
| `MindMapDto` | `SessionId` | Returned within session context |
| `SemanticSectionDto` | `MaterialId` | Returned within material context |
| `NotificationDto` | `UserId` | Always user's own notifications |
| `CartDto` | `UserId` | Always user's own cart |
| `UserVoiceSettingsDto` | `UserId` | Always user's own settings |

---

## Summary of Recommended Actions

| Priority | Entity | Property | Recommendation |
|----------|--------|----------|----------------|
| **High** | Question | `ModelAnswer`, `GradingCriteria` | Expose in teacher-facing DTO so teachers can set/view grading criteria |
| **Medium** | Material | `DurationSeconds`, `TotalPages` | Add to `MaterialDto` for UI metadata display |
| **Medium** | Notification | `Metadata` | Expose for deep-linking from notification to related entity |
| **Medium** | StudySession | `EndedAt`, `IsActive` | Add to `SessionSummaryDto` for active/completed distinction |
| **Low** | Material | `Summary` | Consider exposing for material previews |
| **Low** | CartItem | `AddedAt` | Nice-to-have for "added X days ago" |
| **Low** | Cart | `Status` | Could help frontend distinguish active vs checked-out carts |

---

> **Document Version:** 1.0
> **Last Updated:** March 2025
> **Status:** Audit only — no code changes made
