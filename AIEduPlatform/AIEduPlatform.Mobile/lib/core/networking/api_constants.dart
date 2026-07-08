class ApiConstants {
  static const String apiBaseUrl = 'http://20.199.107.154/api/';
  static const String pdfBaseUrl = 'http://20.199.107.154';
  static const String baseImageUrl = "http://20.199.107.154";
  static const String login = 'auth/login';
  static const String register = 'auth/register/student';
  static const String getContinueLearningCourses = 'courses/continue-learning';
  static const String startStudySession = 'study-sessions';
  static const String getUpComingExams = 'exams/upcoming/{courseId}';
  static const String getAllCourses = 'courses';
  static const String getAvailbleExams = 'exams/available';
  static const String refreshEndpoint = 'auth/refresh-token';
  static const String getMyProfile = 'users/me';
  static const String updateMyProfile = 'users/me';
  static const String logout = 'auth/logout';
  static const String getUserStatistics = 'users/stats';
  static const String addCourseToCart = 'cart/items';
  static const String getMyCart = 'cart';
  static const String removeCourseFromCart = 'cart/items/{CourseId}';
  static const String clearMyCart = 'cart';
  static const String checkoutEndpoint = 'checkout';
  static const String getOrderStatus = 'checkout/{OrderId}';
  static const String getUserDashboard = 'users/dashboard';
  static const String getCourseLectures = 'courses/{CourseId}/lectures';
  static const String openPdfWithId = 'materials/{id}/stream';
  static const String sendChatMessage = 'study-sessions/{SessionId}/chat';
  static const String generateFlashCards =
      'study-sessions/{SessionId}/flashcards';
  static const String summaryTopic = 'study-sessions/{SessionId}/summary';
  static const String generateQuiz = 'study-sessions/{SessionId}/quizzes';
  static const String submitQuizAnswers =
      'study-sessions/{SessionId}/quizzes/{QuizId}/submit';

  static const String generateMindMap = 'study-sessions/{SessionId}/mindmaps';
  static const String getExamQuestions = 'exams/{ExamId}/questions';
  static const String submitExam = 'exams/{ExamId}/submit';
  static const String getSubmitionDetails = 'exams/submissions/{SubmissionId}';
  static const String getStudentSubmissions = 'exams/submissions/student';

  static const String getAllNotifications = 'notifications';
  static const String deleteAllNotifications = 'notifications';
  static const String markNotificationAsRead = 'notifications/{Id}/read';
  static const String getUnreadCount = 'notifications/unread-count';
  static const String markAllNotificationsAsRead = 'notifications/read-all';
  static const String deleteNotification = 'notifications/{Id}';
}

class ApiKeys {
  static const String token = 'accessToken'; // accessToken
  static const String refreshToken = 'refreshToken'; // refreshToken
}

class AppConstants {
  static const String userFirstName = 'firstName';
}

class ApiErrors {
  static const String badRequestError = "badRequestError";
  static const String noContent = "noContent";
  static const String forbiddenError = "forbiddenError";
  static const String unauthorizedError = "unauthorizedError";
  static const String notFoundError = "notFoundError";
  static const String conflictError = "conflictError";
  static const String internalServerError = "internalServerError";
  static const String unknownError = "unknownError";
  static const String timeoutError = "timeoutError";
  static const String defaultError = "defaultError";
  static const String cacheError = "cacheError";
  static const String noInternetError = "noInternetError";
  static const String loadingMessage = "loading_message";
  static const String retryAgainMessage = "retry_again_message";
  static const String ok = "Ok";
}
