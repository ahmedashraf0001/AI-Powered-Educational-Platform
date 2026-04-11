// ──── Response Envelope ────
export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  message?: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface PaginationParams {
  page?: number;
  pageSize?: number;
}

// ──── Auth ────
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterStudentRequest {
  email: string;
  userName: string;
  password: string;
  confirmPassword: string;
  fullName: string;
}

export interface RegisterTeacherRequest extends RegisterStudentRequest {
  bio: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiration?: string;
  refreshTokenExpiration?: string;
}

export interface RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
}

export interface JwtPayload {
  sub: string;
  email: string;
  name: string;
  jti: string;
  role?: string | string[];
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string | string[];
  exp: number;
  iat: number;
}

// ──── Users ────
export interface UserProfile {
  id: string;
  email: string;
  userName: string;
  firstName: string | null;
  lastName: string | null;
  roles: string[];
  bio: string | null;
  qualifications: string | null;
  subjects: string | null;
  gradeLevel: string | null;
  interests: string | null;
  avatarUrl: string | null;
  website: string | null;
  linkedInUrl: string | null;
  title: string | null;
  location: string | null;
  expertiseAreas: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface UserStats {
  coursesEnrolled: number;
  coursesCompleted: number;
  coursesTaught: number;
  totalStudySessions: number;
  examsTaken: number;
  averageExamScore: number;
  flashcardsCreated: number;
  quizzesTaken: number;
  totalStudyTime: string;
  lastActiveDate: string;
}

export interface TeacherDashboard {
  totalCourses: number;
  publishedCourses: number;
  draftCourses: number;
  totalEnrollments: number;
  totalStudents: number;
  totalRevenue: number;
  averageRating: number;
  totalReviews: number;
  totalLectures: number;
  completionRate: number;
  totalExamsCreated: number;
  pendingGradeApprovals: number;
  ungradedSubmissions: number;
  recentEnrollments: { studentName: string; courseName: string; enrolledAt: string }[];
  coursePerformance: { courseId: string; title: string; enrollmentCount: number; averageRating: number; completionRate: number; revenue: number }[];
  enrollmentTrend: { month: string; count: number }[];
}

export interface StudentDashboard {
  totalEnrolledCourses: number;
  completedCourses: number;
  inProgressCourses: number;
  totalLecturesCompleted: number;
  totalLectures: number;
  overallProgressPercentage: number;
  certificatesEarned: number;
  courseProgress: { courseId: string; courseTitle: string; status: string; completedMaterials: number; totalMaterials: number; progressPercentage: number; enrolledAt: string }[];
  engagement: { totalStudySessions: number; totalMaterialsViewed: number; totalTimeSpentMinutes: number; totalQuizzesGenerated: number; totalFlashcardsGenerated: number; coursesEnrolled: number; coursesCompleted: number };
  performance: { examsTaken: number; averageScore: number; highestScore: number; lowestScore: number };
  gradeTrend: { month: string; averageScore: number; examCount: number }[];
  submissionHistory: { submissionId: string; examTitle: string; courseName: string; score: number | null; submittedAt: string; isGraded: boolean }[];
  recentActivity: { courseTitle: string; lectureTitle: string; completedAt: string | null }[];
}

// ──── Courses ────
export interface CourseListDto {
  courseId: string;
  title: string;
  description: string;
  price: number;
  thumbnailUrl: string | null;
  teacherId: string;
  teacherName: string;
  averageRating: number;
  reviewCount: number;
  lectureCount: number;
  enrollmentCount: number;
  isPublished: boolean;
  isEnrolled: boolean;
  isFree: boolean;
  categoryId: string | null;
  categoryName: string | null;
  createdAt: string;
}

export interface CourseDetailDto {
  courseId: string;
  title: string;
  description: string;
  price: number;
  thumbnailUrl: string | null;
  teacherId: string;
  teacherName: string;
  averageRating: number;
  reviewCount: number;
  lectureCount: number;
  enrollmentCount: number;
  isPublished: boolean;
  isEnrolled: boolean;
  hasReviewed: boolean;
  isFree: boolean;
  categoryId: string | null;
  categoryName: string | null;
  lectures: LectureDto[];
  categories: CategoryDto[];
  createdAt: string;
  updatedAt: string;
}

export interface ContinueLearningDto {
  courseId: string;
  courseTitle: string;
  progressPercentage: number;
  lastMaterialId: string | null;
  lastMaterialTitle: string | null;
  resumePosition: number | null;
}

export interface CourseProgressDto {
  courseId: string;
  courseTitle: string;
  completedLessons: number;
  totalLessons: number;
  progressPercentage: number;
  isCompleted?: boolean;
}

// ──── Categories ────
export interface CategoryDto {
  id: string;
  name: string;
  description: string | null;
  courseCount: number;
}

// ──── Enrollments ────
export interface EnrollmentDto {
  id: string;
  studentId: string;
  studentName: string;
  courseId: string;
  courseTitle: string;
  enrolledAt: string;
  status: EnrollmentStatus;
  progressPercentage: number;
  completedLectures: number;
  totalLectures: number;
  lastAccessedAt: string | null;
  isCompleted: boolean;
  orderId: string | null;
  amountPaid: number;
  refundedAt: string | null;
  refundAmount: number | null;
  stripeRefundId: string | null;
  unenrolledAt: string | null;
}

export enum EnrollmentStatus {
  Active = "Active",
  Completed = "Completed",
  Dropped = "Dropped",
  Pending = "Pending",
}

// ──── Lectures ────
export interface LectureDto {
  id: string;
  courseId: string;
  title: string;
  description: string;
  orderIndex: number;
  createdAt: string;
  updatedAt: string;
  materials: MaterialDto[];
}

export interface LectureDetailDto extends LectureDto {
  videoMaterials: MaterialDto[];
  documentMaterials: MaterialDto[];
  audioMaterials: MaterialDto[];
  imageMaterials: MaterialDto[];
}

// ──── Materials ────
export interface MaterialDto {
  id: string;
  lectureId: string;
  type: MaterialType;
  title: string;
  streamUrl: string;
  indexed: boolean;
  createdAt: string;
  updatedAt: string;
}

export enum MaterialType {
  Video = "Video",
  Document = "Document",
  Audio = "Audio",
  Image = "Image",
}

export interface MaterialProgressDto {
  current: number;
  total: number;
  percentage: number;
}

export interface MaterialProjectionDto {
  lessonId: string;
  title: string;
  materialType: string;
  materialUrl: string;
  progress: MaterialProgressDto | null;
  isCompleted: boolean;
  currentSection: SemanticSectionDto | null;
}

// ──── Cart ────
export interface CartDto {
  cartId: string;
  items: CartItemDto[];
  itemCount: number;
  subtotal: number;
  currency: string;
}

export interface CartItemDto {
  cartItemId: string;
  courseId: string;
  courseTitle: string;
  courseThumbnailUrl: string | null;
  teacherName: string;
  originalPrice: number;
  priceAtTimeOfAdding: number;
}

export enum CartStatus {
  Active = 0,
  CheckedOut = 1,
  Abandoned = 2,
}

// ──── Checkout ────
export interface CheckoutResponseDto {
  orderId: string;
  clientSecret: string | null;
  paymentIntentId: string | null;
  publishableKey: string | null;
  requiresPayment: boolean;
  totalAmount: number;
  currency: string;
  items: CheckoutItemDto[];
  status: string;
}

export interface CheckoutItemDto {
  courseId: string;
  courseTitle: string;
  price: number;
}

export interface OrderStatusDto {
  orderId: string;
  status: OrderStatus;
  paidAt: string | null;
  totalAmount: number;
  currency: string;
  enrolledCourses: EnrolledCourseInfoDto[];
}

export interface EnrolledCourseInfoDto {
  courseId: string;
  courseTitle: string;
  price: number;
}

export enum OrderStatus {
  Pending = 'Pending',
  Paid = 'Paid',
  Refunded = 'Refunded',
  PartiallyRefunded = 'PartiallyRefunded',
  Failed = 'Failed',
}

// ──── Exams ────
export interface ExamDto {
  id: string;
  title: string;
  courseId: string;
  startTime: string;
  endTime: string;
  durationMinutes: number;
  questionCount: number;
  hasSubmitted?: boolean;
}

export interface ExamDetailDto {
  id: string;
  title: string;
  courseId: string;
  startTime: string;
  endTime: string;
  durationMinutes: number;
  questions: QuestionDto[];
  submissionCount: number;
  hasSubmitted?: boolean;
}

export interface ExamAttemptDto {
  id: string;
  examId: string;
  studentId: string;
  startedAt: string;
  isSubmitted: boolean;
  remainingSeconds: number;
  savedAnswers: Record<string, string> | null;
}

// ──── Questions ────
export interface QuestionDto {
  id: string;
  examId: string;
  type: QuestionType;
  text: string;
  options: string;
  correctAnswer: string;
  points: number;
  order: number;
}

export enum QuestionType {
  MultipleChoice = "MultipleChoice",
  TrueFalse = "TrueFalse",
  ShortAnswer = "ShortAnswer",
  Essay = "Essay",
  FillInTheBlank = "FillInTheBlank",
}

// ──── Submissions ────
export interface SubmissionDto {
  id: string;
  examId: string;
  studentId: string;
  examTitle: string;
  courseName: string;
  studentName: string;
  submittedAt: string;
  isGraded: boolean;
  score: number | null;
}

export interface SubmissionDetailDto extends SubmissionDto {
  answers: SubmissionAnswerDto[];
  grade: GradeDto | null;
}

export interface SubmissionAnswerDto {
  questionId: string;
  questionText: string;
  questionType: QuestionType;
  answer: string;
  correctAnswer: string;
  options: string[];
  points: number;
  order: number;
}

// ──── Grades ────
export interface GradeDto {
  id: string;
  submissionId: string;
  score: number;
  feedback: string;
  isAiGraded: boolean;
  isApproved: boolean;
}

export interface ExamGradeStats {
  averageScore: number;
  medianScore: number;
  highestScore: number;
  lowestScore: number;
  passRate: number;
  totalGraded: number;
}

// ──── Reviews ────
export interface ReviewDto {
  id: string;
  courseId: string;
  studentId: string;
  studentName: string;
  rating: number;
  comment: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CourseRatingSummaryDto {
  averageRating: number;
  totalReviews: number;
  ratingDistribution: Record<string, number>;
}

// ──── Notifications ────
export interface NotificationDto {
  id: string;
  type: string;
  title: string;
  message: string;
  isRead: boolean;
  relatedEntityId: string | null;
  relatedEntityType: string | null;
  createdAt: string;
  readAt: string | null;
}

export interface NotificationListDto {
  items: NotificationDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

// ──── Study Sessions ────
export interface SessionSummaryDto {
  id: string;
  courseId: string;
  courseName: string;
  startedAt: string;
  lastActivity: string;
}

export interface SessionDetailDto {
  id: string;
  courseId: string;
  courseName: string;
  startedAt: string;
  lastActivity: string;
  messageCount: number;
  flashcardCount: number;
  quizCount: number;
  mindMapCount: number;
}

export interface ChatMessageDto {
  id: string;
  role: string;
  content: string;
  sources: string | null;
  createdAt: string;
}

export enum ChatRole {
  Student = 0,
  Assistant = 1,
  System = 2,
}

export interface FlashcardDto {
  id: string;
  topic: string;
  frontText: string;
  backText: string;
  createdAt: string;
}

export interface GeneratedQuizDto {
  id: string;
  topic: string;
  questions: string;
  difficulty: string;
  studentAnswers: string | null;
  score: number | null;
  createdAt: string;
}

export interface QuizResultDto {
  quizId: string;
  score: number;
  totalQuestions: number;
  percentage: number;
  results: QuizQuestionResult[];
}

export interface QuizQuestionResult {
  questionIndex: number;
  questionText: string;
  userAnswer: string;
  correctAnswer: string;
  isCorrect: boolean;
  explanation: string;
}

export interface MindMapDto {
  id: string;
  topic: string;
  nodes: string;
  connections: string;
  createdAt: string;
}

export interface SummaryDto {
  summary: string;
  keyPoints: string[];
  keyTerms: Record<string, string>;
}

export interface DialogueAudioResponseDto {
  audioBase64: string;
  turnTimestamps: TurnTimestamp[];
  exchanges: DialogueExchange[];
}

export interface TurnTimestamp {
  startTime: number;
  endTime: number;
  speaker: string;
  text: string;
}

export interface DialogueExchange {
  speaker: string;
  text: string;
}

// ──── Semantic Sections ────
export interface SemanticSectionDto {
  id: string;
  title: string;
  summary: string;
  startSeconds: number | null;
  endSeconds: number | null;
  startPage: number | null;
  endPage: number | null;
  orderIndex: number;
}

// ──── AI Provider ────
export interface AiProviderStatus {
  activeProvider: string;
  supportedProviders: string[];
  isGroqConfigured: boolean;
}

// ──── Dialogue / Voice ────
export interface VoiceDto {
  voiceId: string;
  name: string;
  description: string;
  previewUrl: string | null;
}

export interface UserVoiceSettingsDto {
  teacherVoiceId: string;
  studentVoiceId: string;
  teacherSpeed: number;
  studentSpeed: number;
  outputFormat: string;
  sampleRate: number;
  includePauses: boolean;
  pauseDurationMs: number;
  pauseMultiplier: number;
  normalizeAudio: boolean;
}

// ──── Engagement ────
export interface CourseEngagementReport {
  totalEnrolled: number;
  activeStudents: number;
  atRiskStudents: number;
  averageEngagementScore: number;
  students: StudentEngagementDto[];
}

export interface StudentEngagementDto {
  studentId: string;
  studentName: string;
  email: string;
  enrolledAt: string;
  enrollmentStatus: string;
  totalStudySessions: number;
  totalStudyHours: number;
  lastStudySessionDate: string | null;
  daysSinceLastActivity: number;
  totalChatMessages: number;
  totalFlashcardsGenerated: number;
  totalQuizzesTaken: number;
  totalMindMapsGenerated: number;
  examsTaken: number;
  examsAvailable: number;
  averageExamScore: number;
  pendingSubmissions: number;
  engagementScore: number;
  engagementLevel: string;
}

