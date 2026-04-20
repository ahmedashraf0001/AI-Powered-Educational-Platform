import { lazy, Suspense } from 'react';
import { Routes, Route } from 'react-router-dom';
import { AppLayout } from '@/components/layout/AppLayout';
import { ProtectedRoute } from '@/components/auth/ProtectedRoute';
import { PageSpinner } from '@/components/ui/Spinner';

// Public pages
const LandingPage = lazy(() => import('@/pages/public/LandingPage'));
const LoginPage = lazy(() => import('@/pages/public/LoginPage'));
const RegisterPage = lazy(() => import('@/pages/public/RegisterPage'));
const VerifyEmailPage = lazy(() => import('@/pages/public/VerifyEmailPage'));
const CourseCatalogPage = lazy(() => import('@/pages/public/CourseCatalogPage'));
const CourseDetailPage = lazy(() => import('@/pages/public/CourseDetailPage'));
const InstructorProfilePage = lazy(() => import('@/pages/public/InstructorProfilePage'));

// Student pages
const StudentDashboard = lazy(() => import('@/pages/student/StudentDashboard'));
const MyEnrollmentsPage = lazy(() => import('@/pages/student/MyEnrollmentsPage'));
const CourseLearningPage = lazy(() => import('@/pages/student/CourseLearningPage'));
const LecturePage = lazy(() => import('@/pages/student/LecturePage'));
const StudioPage = lazy(() => import('@/pages/student/StudioPage'));
const ExamPreAssessmentPage = lazy(() => import('@/pages/student/ExamPreAssessmentPage'));
const ExamTakingPage = lazy(() => import('@/pages/student/ExamTakingPage'));
const MySubmissionsPage = lazy(() => import('@/pages/student/MySubmissionsPage'));
const MyGradesPage = lazy(() => import('@/pages/student/MyGradesPage'));

const CheckoutPage = lazy(() => import('@/pages/student/CheckoutPage'));

// Teacher pages
const TeacherDashboard = lazy(() => import('@/pages/teacher/TeacherDashboard'));
const TeacherCoursesPage = lazy(() => import('@/pages/teacher/TeacherCoursesPage'));
const CreateCoursePage = lazy(() => import('@/pages/teacher/CreateCoursePage'));
const CourseManagementPage = lazy(() => import('@/pages/teacher/CourseManagementPage'));
const TeacherExamsPage = lazy(() => import('@/pages/teacher/TeacherExamsPage'));
const TeacherExamDetailPage = lazy(() => import('@/pages/teacher/TeacherExamDetailPage'));
const QuestionEditorPage = lazy(() => import('@/pages/teacher/QuestionEditorPage'));
const GradingPage = lazy(() => import('@/pages/teacher/GradingPage'));
const EngagementPage = lazy(() => import('@/pages/teacher/EngagementPage'));

// Shared pages
const NotificationsPage = lazy(() => import('@/pages/NotificationsPage'));
const ProfilePage = lazy(() => import('@/pages/ProfilePage'));

// 404 page
const NotFoundPage = lazy(() => import('@/pages/NotFoundPage'));

function App() {
  return (
    <Suspense fallback={<PageSpinner />}>
      <Routes>
        {/* Public routes (no layout) */}
        <Route path="/" element={<LandingPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/register/teacher" element={<RegisterPage />} />
        <Route path="/verify-email" element={<VerifyEmailPage />} />

        {/* Public routes with layout */}
        <Route element={<AppLayout />}>
          <Route path="/courses" element={<CourseCatalogPage />} />
          <Route path="/courses/:courseId" element={<CourseDetailPage />} />
          <Route path="/profile/:instructorId" element={<InstructorProfilePage />} />        </Route>

        {/* Authenticated routes */}
        <Route element={<ProtectedRoute />}>          <Route element={<AppLayout />}>
            <Route path="/dashboard" element={<StudentDashboard />} />
            <Route path="/my-enrollments" element={<MyEnrollmentsPage />} />
            <Route path="/courses/:courseId/learn" element={<CourseLearningPage />} />
            <Route path="/courses/:courseId/lectures/:lectureId" element={<LecturePage />} />
            <Route path="/courses/:courseId/studio/:sessionId" element={<StudioPage />} />
            <Route path="/exams/:examId" element={<ExamPreAssessmentPage />} />
            <Route path="/exams/:examId/take" element={<ExamTakingPage />} />
            <Route path="/my-submissions" element={<MySubmissionsPage />} />
            <Route path="/my-grades" element={<MyGradesPage />} />
              <Route path="/checkout" element={<CheckoutPage />} />
            <Route path="/checkout/:orderId" element={<CheckoutPage />} />
            <Route path="/notifications" element={<NotificationsPage />} />
            <Route path="/profile" element={<ProfilePage />} />
          </Route>
        </Route>

        {/* Teacher routes */}
        <Route element={<ProtectedRoute requiredRole="Teacher" />}>
          <Route element={<AppLayout />}>
            <Route path="/teacher/dashboard" element={<TeacherDashboard />} />
            <Route path="/teacher/courses" element={<TeacherCoursesPage />} />
            <Route path="/teacher/courses/create" element={<CreateCoursePage />} />
            <Route path="/teacher/courses/:courseId" element={<CourseManagementPage />} />
            <Route path="/teacher/exams" element={<TeacherExamsPage />} />
            <Route path="/teacher/exams/:examId" element={<TeacherExamDetailPage />} />
            <Route path="/teacher/exams/:examId/questions" element={<QuestionEditorPage />} />
            <Route path="/teacher/grading" element={<GradingPage />} />
            <Route path="/teacher/courses/:courseId/engagement" element={<EngagementPage />} />
          </Route>
        </Route>

        {/* Fallback */}
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </Suspense>
  );
}

export default App;
