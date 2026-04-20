You are an AI software engineer working on a .NET Clean Architecture project.

Your task is to implement a scalable and efficient course tag updating system based on content changes.

---

## Goal

Ensure that course tags stay up-to-date when course content changes (lectures, materials), while avoiding unnecessary expensive operations (LLM calls).

---

## Requirements

### 1. Extend Course Entity

Modify the Course entity to include:

- bool NeedsTagRebuild
- int PendingContentChanges
- DateTime? LastTagUpdatedAt

---

### 2. Track Content Changes

Whenever any of the following occurs:

- Lecture added
- Lecture removed
- Material added
- Material removed
- Material summary updated

You MUST:

- Set course.NeedsTagRebuild = true
- Increment course.PendingContentChanges by 1

This should be implemented in the appropriate command handlers or domain services.

---

### 3. Background Processing

Create a background worker (or scheduled job) that:

1. Queries all courses where:
   NeedsTagRebuild == true

2. For each course, decide:

---

### 4. Decision Logic

#### FULL REBUILD

Trigger full tag extraction if:

- PendingContentChanges >= 5
- Any deletion occurred (lecture/material removed)
- Course has no existing tags

Action:
- Call TagExtractionService.ExtractCourseTagsAsync(courseId)
- Replace all existing CourseTags

---

#### DELTA UPDATE

Trigger delta update if:

- PendingContentChanges < 5
- Only additions occurred (no deletions)

Action:
- Extract tags ONLY from newly added lectures/materials
- Merge with existing tags
- Normalize and deduplicate tags
- Do NOT remove existing tags unless clearly invalid

---

### 5. Reset State

After processing:

- course.NeedsTagRebuild = false
- course.PendingContentChanges = 0
- course.LastTagUpdatedAt = DateTime.UtcNow

Persist changes using UnitOfWork.

---

### 6. Tag Persistence

- Use TagRepository.GetOrCreateAsync for tag creation
- Maintain CourseTag relationships
- Avoid duplicate CourseTag entries

---

### 7. Constraints

- Do NOT block user requests (must run async/background)
- Ensure idempotency (safe to run multiple times)
- Handle nulls and edge cases safely
- Keep implementation clean and aligned with existing architecture (UnitOfWork, repositories, services)

---

## Expected Output

Implement:

1. Updated Course entity
2. Modifications in content-related handlers (lecture/material changes)
3. Background worker/service
4. Integration with TagExtractionService
5. Delta merge logic for tags

---

## Notes

- Prefer simple and maintainable code over over-engineering
- Full rebuild is acceptable when unsure
- Delta logic should be safe and not corrupt tag state

---

## Objective

Produce a robust tagging pipeline that is:

- Efficient
- Scalable
- Eventually consistent
- Easy to extend in the future