import type { NotificationDto } from '@/types';

type NotificationMetadata = Record<string, unknown>;

const GRADE_NOTIFICATION_TYPES = new Set(['submissiongraded', 'gradeapproved', 'gradeupdated']);
const COURSE_CONTENT_NOTIFICATION_TYPES = new Set(['newmaterialuploaded', 'newlectureadded']);

function normalizeEntityType(value: string | null | undefined): string {
  return (value ?? '').trim().toLowerCase();
}

function parseMetadata(raw: string | null | undefined): NotificationMetadata | null {
  if (!raw) {
    return null;
  }

  try {
    const parsed = JSON.parse(raw) as NotificationMetadata;
    return parsed && typeof parsed === 'object' ? parsed : null;
  } catch {
    return null;
  }
}

function pickMetadataString(metadata: NotificationMetadata | null, ...keys: string[]): string | undefined {
  if (!metadata) {
    return undefined;
  }

  for (const key of keys) {
    const value = metadata[key];
    if (typeof value === 'string' && value.trim().length > 0) {
      return value;
    }
  }

  return undefined;
}

export function getNotificationNavigationPath(notification: NotificationDto): string {
  const type = (notification.type ?? '').trim().toLowerCase();
  const relatedEntityType = normalizeEntityType(notification.relatedEntityType);
  const relatedEntityId = notification.relatedEntityId ?? undefined;
  const title = (notification.title ?? '').trim().toLowerCase();
  const metadata = parseMetadata(notification.metadata);

  const courseIdFromMetadata = pickMetadataString(
    metadata,
    'courseId',
    'CourseId',
    'course_id',
    'CourseID',
    'courseID'
  );
  const examIdFromMetadata = pickMetadataString(
    metadata,
    'examId',
    'ExamId',
    'exam_id',
    'ExamID',
    'examID'
  );
  const lectureIdFromMetadata = pickMetadataString(
    metadata,
    'lectureId',
    'LectureId',
    'lecture_id',
    'LectureID',
    'lectureID',
    'lessonId',
    'LessonId',
    'lesson_id'
  );
  const orderIdFromMetadata = pickMetadataString(metadata, 'orderId', 'OrderId', 'order_id', 'OrderID', 'orderID');

  const courseId = courseIdFromMetadata ?? (relatedEntityType === 'course' ? relatedEntityId : undefined);

  const orderId = orderIdFromMetadata ?? (relatedEntityType === 'order' ? relatedEntityId : undefined);
  if (orderId) {
    return `/checkout/${orderId}`;
  }

  if (type === 'courseaddedtocart') {
    return '/checkout';
  }

  const lectureId =
    lectureIdFromMetadata ??
    (relatedEntityType === 'lecture' || relatedEntityType === 'material' ? relatedEntityId : undefined);

  if (
    COURSE_CONTENT_NOTIFICATION_TYPES.has(type) ||
    title.startsWith('new material') ||
    title.startsWith('new lecture')
  ) {
    if (lectureId && courseId) {
      return `/courses/${courseId}/lectures/${lectureId}`;
    }

    if (courseId) {
      return `/courses/${courseId}/learn`;
    }
  }

  const examId = examIdFromMetadata ?? (relatedEntityType === 'exam' ? relatedEntityId : undefined);
  if (examId) {
    return `/exams/${examId}`;
  }

  if (GRADE_NOTIFICATION_TYPES.has(type)) {
    return '/my-grades';
  }

  if (type === 'examsubmitted' || type === 'aigradingneedsreview') {
    return '/teacher/dashboard';
  }

  const courseRelatedId = relatedEntityType === 'course' ? relatedEntityId : courseId;
  if (courseRelatedId) {
    if (
      type === 'newexamposted' ||
      title.startsWith('new exam') ||
      COURSE_CONTENT_NOTIFICATION_TYPES.has(type)
    ) {
      return `/courses/${courseRelatedId}/learn`;
    }

    return `/courses/${courseRelatedId}`;
  }

  if (type === 'paymentsuccess') {
    return '/my-enrollments';
  }

  return '/notifications';
}
