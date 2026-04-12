import { useEffect, useRef, useCallback, useMemo } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '@/stores/authStore';
import { useNotificationStore } from '@/stores/notificationStore';
import { toast } from 'sonner';
import { generateId } from '@/utils/id';

const LOCALHOST_PATTERN = /^(https?:\/\/)?(localhost|127\.0\.0\.1)(:\d+)?(\/|$)/i;
const configuredSignalRUrl = (import.meta.env.VITE_SIGNALR_URL ?? '').trim().replace(/\/+$/, '');
const SIGNALR_URL =
  import.meta.env.PROD && LOCALHOST_PATTERN.test(configuredSignalRUrl)
    ? ''
    : configuredSignalRUrl;

export function useSignalR(enrolledCourseIds: string[] = []) {
  const { accessToken, isAuthenticated, roles } = useAuthStore();
  const { addNotification } = useNotificationStore();
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
        toast.info(`New exam "${data.examTitle}" in ${data.courseName}`);
        addNotification({ id: generateId(), title: 'New Exam', message: `New exam "${data.examTitle}" in ${data.courseName}`, type: 'exam', isRead: false, createdAt: new Date().toISOString(), relatedEntityId: null, relatedEntityType: null, readAt: null });
      });
      studentConnection.on('NewMaterialUploaded', (data) => {
        toast.info(`New material "${data.materialTitle}" in ${data.courseName}`);
        addNotification({ id: generateId(), title: 'New Material', message: `New material in ${data.courseName}`, type: 'material', isRead: false, createdAt: new Date().toISOString(), relatedEntityId: null, relatedEntityType: null, readAt: null });
      });
      studentConnection.on('NewLectureAdded', (data) => {
        toast.info(`New lecture "${data.lectureTitle}" in ${data.courseName}`);
        addNotification({ id: generateId(), title: 'New Lecture', message: `New lecture in ${data.courseName}`, type: 'lecture', isRead: false, createdAt: new Date().toISOString(), relatedEntityId: null, relatedEntityType: null, readAt: null });
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
        addNotification({ id: generateId(), title: 'Submission Graded', message: `Your ${data.examTitle} has been graded`, type: 'grade', isRead: false, createdAt: new Date().toISOString(), relatedEntityId: null, relatedEntityType: null, readAt: null });
      });
      studentConnection.on('GradeApproved', (data) => {
        toast.success(`Your grade for ${data.examTitle} has been approved`);
      });
      studentConnection.on('GradeUpdated', (data) => {
        toast.info(`Your grade for ${data.examTitle} updated to ${data.newScore}`);
      });
      studentConnection.on('EngagementAlert', (data) => {
        toast.warning(`Message from ${data.teacherName}: ${data.message}`);
        addNotification({ id: generateId(), title: 'Engagement Alert', message: data.message, type: 'alert', isRead: false, createdAt: new Date().toISOString(), relatedEntityId: null, relatedEntityType: null, readAt: null });
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
        if (data.success) {
          toast.success(`Material indexed: ${data.chunksIndexed} chunks`);
        } else {
          toast.error(`Material indexing failed: ${data.error}`);
        }
      });
      teacherConnection.on('ExamSubmitted', (data) => {
        toast.info(`${data.studentName} submitted "${data.examTitle}"`);
        addNotification({ id: generateId(), title: 'Exam Submitted', message: `${data.studentName} submitted "${data.examTitle}"`, type: 'submission', isRead: false, createdAt: new Date().toISOString(), relatedEntityId: null, relatedEntityType: null, readAt: null });
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
  ]);

  return {
    studentConnection: studentConnectionRef.current,
    teacherConnection: teacherConnectionRef.current,
  };
}
