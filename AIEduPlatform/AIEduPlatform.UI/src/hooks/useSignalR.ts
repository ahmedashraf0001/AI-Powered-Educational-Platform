import { useEffect, useRef, useCallback, useMemo } from 'react';
import * as signalR from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { useAuthStore } from '@/stores/authStore';
import { useNotificationStore } from '@/stores/notificationStore';
import { toast } from 'sonner';
import { generateId } from '@/utils/id';
import { showNotification } from '@/utils/notifications';

const LOCALHOST_PATTERN = /^(https?:\/\/)?(localhost|127\.0\.0\.1)(:\d+)?(\/|$)/i;
const configuredSignalRUrl = (import.meta.env.VITE_SIGNALR_URL ?? '').trim().replace(/\/+$/, '');
const SIGNALR_URL =
  import.meta.env.PROD && LOCALHOST_PATTERN.test(configuredSignalRUrl)
    ? ''
    : configuredSignalRUrl;

export function useSignalR(enrolledCourseIds: string[] = []) {
  const { accessToken, isAuthenticated, roles } = useAuthStore();
  const { addNotification } = useNotificationStore();
  const queryClient = useQueryClient();
  const isStudent = roles.includes('Student');
  const isTeacher = roles.includes('Teacher');

  // Stabilize the array reference so the effect doesn't re-run on every render
  const stableEnrolledCourseIds = useMemo(() => {
    const sorted = [...enrolledCourseIds].sort();
    return sorted;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [JSON.stringify(enrolledCourseIds)]);
  const studentConnectionRef = useRef<signalR.HubConnection | null>(null);
  const teacherConnectionRef = useRef<signalR.HubConnection | null>(null);

  const joinCourseGroups = useCallback(
    async (connection: signalR.HubConnection, courseIds: string[]) => {
      for (const courseId of courseIds) {
        try {
          await connection.invoke('JoinCourseGroup', courseId);
        } catch (err) {
          console.error(`Failed to join course group ${courseId}:`, err);
        }
      }
    },
    []
  );

  useEffect(() => {
    if (!isAuthenticated || !accessToken) return;
    studentConnectionRef.current = null;
    teacherConnectionRef.current = null;

    let studentConnection: signalR.HubConnection | null = null;
    if (isStudent) {
      studentConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${SIGNALR_URL}/hubs/student-notifications`, {
          accessTokenFactory: () => useAuthStore.getState().accessToken || '',
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .build();

      // Student events
      studentConnection.on('NewExamPosted', (data) => {
        const courseId = data.courseId ?? data.CourseId ?? null;
        const examId = data.examId ?? data.ExamId ?? null;
        const metadata = JSON.stringify({
          courseId,
          examId,
          examTitle: data.examTitle,
        });
        toast.info(`New exam "${data.examTitle}" in ${data.courseName}`);
        addNotification({ id: generateId(), title: 'New Exam', message: `New exam "${data.examTitle}" in ${data.courseName}`, type: 'NewExamPosted', isRead: false, createdAt: new Date().toISOString(), relatedEntityId: examId, relatedEntityType: examId ? 'Exam' : null, metadata, readAt: null });
      });
      studentConnection.on('NewMaterialUploaded', (data) => {
        const courseId = data.courseId ?? data.CourseId ?? null;
        const lectureId = data.lectureId ?? data.LectureId ?? null;
        const metadata = JSON.stringify({
          courseId,
          lectureId,
          materialTitle: data.materialTitle,
        });
        toast.info(`New material "${data.materialTitle}" in ${data.courseName}`);
        addNotification({ id: generateId(), title: 'New Material', message: `New material in ${data.courseName}`, type: 'NewMaterialUploaded', isRead: false, createdAt: new Date().toISOString(), relatedEntityId: lectureId, relatedEntityType: lectureId ? 'Lecture' : null, metadata, readAt: null });
      });
      studentConnection.on('NewLectureAdded', (data) => {
        const courseId = data.courseId ?? data.CourseId ?? null;
        const lectureId = data.lectureId ?? data.LectureId ?? null;
        const metadata = JSON.stringify({
          courseId,
          lectureId,
          lectureTitle: data.lectureTitle,
        });
        toast.info(`New lecture "${data.lectureTitle}" in ${data.courseName}`);
        addNotification({ id: generateId(), title: 'New Lecture', message: `New lecture in ${data.courseName}`, type: 'NewLectureAdded', isRead: false, createdAt: new Date().toISOString(), relatedEntityId: lectureId, relatedEntityType: lectureId ? 'Lecture' : null, metadata, readAt: null });
      });
      studentConnection.on('CourseUpdated', (data) => {
        toast.info(`${data.courseName} has been updated`);
      });
      studentConnection.on('CoursePublished', (data) => {
        toast.info(`${data.courseName} is now published`);
      });
      studentConnection.on('ExamUpdated', (data) => {
        toast.info(`Exam "${data.examTitle}" has been updated in ${data.courseName}`);
      });
      studentConnection.on('ExamDeleted', (data) => {
        toast.info(`Exam "${data.examTitle}" has been removed from ${data.courseName}`);
      });
      studentConnection.on('SubmissionGraded', (data) => {
        toast.success(`Your ${data.examTitle} has been graded: ${data.score}`);
        addNotification({ id: generateId(), title: 'Submission Graded', message: `Your ${data.examTitle} has been graded`, type: 'SubmissionGraded', isRead: false, createdAt: new Date().toISOString(), relatedEntityId: null, relatedEntityType: null, readAt: null });
      });
      studentConnection.on('GradeApproved', (data) => {
        toast.success(`Your grade for ${data.examTitle} has been approved`);
      });
      studentConnection.on('GradeUpdated', (data) => {
        toast.info(`Your grade for ${data.examTitle} updated to ${data.newScore}`);
      });
      studentConnection.on('EngagementAlert', (data) => {
        toast.warning(`Message from ${data.teacherName}: ${data.message}`);
        addNotification({ id: generateId(), title: 'Engagement Alert', message: data.message, type: 'EngagementAlert', isRead: false, createdAt: new Date().toISOString(), relatedEntityId: null, relatedEntityType: null, readAt: null });
      });

      studentConnection.onreconnected(() => {
        joinCourseGroups(studentConnection!, stableEnrolledCourseIds);
      });

      studentConnection
        .start()
        .then(() => joinCourseGroups(studentConnection!, stableEnrolledCourseIds))
        .catch((err) => console.error('Student hub connection failed:', err));

      studentConnectionRef.current = studentConnection;
    }

    // Teacher hub (only for teachers)
    let teacherConnection: signalR.HubConnection | null = null;
    if (isTeacher) {
      teacherConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${SIGNALR_URL}/hubs/material-indexing`, {
          accessTokenFactory: () => useAuthStore.getState().accessToken || '',
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .build();

      teacherConnection.on('ReceiveIndexingNotification', (data) => {
        const success = Boolean(data.success ?? data.Success);
        const courseId = data.courseId ?? data.CourseId;

        if (success) {
          showNotification({
            type: 'success',
            message: `Material indexed: ${data.chunksIndexed} chunks`,
          });
        } else {
          const courseLabel = data.courseTitle ?? data.CourseTitle ?? data.courseId ?? data.CourseId ?? 'this course';
          showNotification({
            type: 'warning',
            message: `Indexing failed for "${courseLabel}". Students cannot use this content in AI study sessions.`,
            persistent: true,
          });
        }

        if (courseId) {
          queryClient.invalidateQueries({ queryKey: ['course-lectures', courseId] });
          queryClient.invalidateQueries({ queryKey: ['course', courseId] });
        }

        window.dispatchEvent(
          new CustomEvent('aiedu:indexing-status', {
            detail: {
              courseId,
              success,
            },
          })
        );
      });
      teacherConnection.on('ReceiveTagExtractionNotification', (data) => {
        const courseTitle = data.courseTitle ?? data.CourseTitle ?? 'this course';
        const isSuccess = Boolean(data.success ?? data.Success);

        if (isSuccess) {
          showNotification({
            type: 'success',
            message: `Tag extraction completed for "${courseTitle}".`,
          });
          return;
        }

        showNotification({
          type: 'warning',
          message: `Tag extraction failed for "${courseTitle}". This may reduce course discoverability and engagement.`,
          persistent: true,
        });
      });
      teacherConnection.on('ExamSubmitted', (data) => {
        toast.info(`${data.studentName} submitted "${data.examTitle}"`);
        addNotification({ id: generateId(), title: 'Exam Submitted', message: `${data.studentName} submitted "${data.examTitle}"`, type: 'ExamSubmitted', isRead: false, createdAt: new Date().toISOString(), relatedEntityId: null, relatedEntityType: null, readAt: null });
      });
      teacherConnection.on('NewEnrollment', (data) => {
        toast.info(`${data.studentName} enrolled in ${data.courseName}`);
      });
      teacherConnection.on('NewReview', (data) => {
        toast.info(`New ${data.rating}-star review for ${data.courseName}`);
      });
      teacherConnection.on('EnrollmentCompleted', (data) => {
        toast.success(`${data.studentName} completed ${data.courseName}`);
      });
      teacherConnection.on('StudentUnenrolled', (data) => {
        toast.info(`${data.studentName} unenrolled from ${data.courseName}`);
      });

      teacherConnection
        .start()
        .catch((err) => console.error('Teacher hub connection failed:', err));

      teacherConnectionRef.current = teacherConnection;
    }

    return () => {
      studentConnection?.stop();
      teacherConnection?.stop();
    };
  }, [
    isAuthenticated,
    accessToken,
    isStudent,
    isTeacher,
    stableEnrolledCourseIds,
    addNotification,
    joinCourseGroups,
    queryClient,
  ]);

  return {
    studentConnection: studentConnectionRef.current,
    teacherConnection: teacherConnectionRef.current,
  };
}
