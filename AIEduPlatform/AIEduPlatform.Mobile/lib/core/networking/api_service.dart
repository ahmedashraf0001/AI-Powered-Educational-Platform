import 'package:dio/dio.dart';
import 'package:graduation_app/features/ai/data/models/flash_cards_request_model.dart';
import 'package:graduation_app/features/ai/data/models/flash_cards_response_model.dart';
import 'package:graduation_app/features/ai/data/models/generate_mind_map_request_model.dart';
import 'package:graduation_app/features/ai/data/models/generate_mind_map_response_model.dart';
import 'package:graduation_app/features/ai/data/models/generate_quiz_request_model.dart';
import 'package:graduation_app/features/ai/data/models/generate_quiz_response_model.dart';
import 'package:graduation_app/features/ai/data/models/submit_quiz_request_model.dart';
import 'package:graduation_app/features/ai/data/models/submit_quiz_response_model.dart';
import 'package:graduation_app/features/ai/data/models/summary_topic_request_model.dart';
import 'package:graduation_app/features/ai/data/models/summary_topic_response_model.dart';
import 'package:graduation_app/features/cart/data/models/checkout_response_model.dart';
import 'package:graduation_app/features/cart/data/models/get_my_cart_response_model.dart';
import 'package:graduation_app/features/cart/data/models/my_courses_response_model.dart';
import 'package:graduation_app/features/cart/data/models/remove_course_from_cart_response_model.dart';
import 'package:graduation_app/features/courses/data/models/add_course_to_cart_request_model.dart';
import 'package:graduation_app/features/courses/data/models/get_all_courses_response_model.dart';
import 'package:graduation_app/features/courses/data/models/get_course_lectures_response_model.dart';
import 'package:graduation_app/features/home/data/models/continue_learning_course_model.dart';
import 'package:graduation_app/features/courses/data/models/start_study_session_response_model.dart';
import 'package:graduation_app/features/home/data/models/get_availble_exams_response_model.dart';
import 'package:graduation_app/features/home/data/models/get_exam_questions_response_model.dart';
import 'package:graduation_app/features/home/data/models/get_student_submissions_response_model.dart';
import 'package:graduation_app/features/home/data/models/submit_exam_response_model.dart';
import 'package:graduation_app/features/home/data/models/up_coming_exams_response_model.dart';
import 'package:graduation_app/features/login/data/models/login_request_body_model.dart';
import 'package:graduation_app/features/login/data/models/login_response_model.dart';
import 'package:graduation_app/features/profile/data/models/get_user_statistics_model.dart';
import 'package:graduation_app/features/profile/data/models/my_profile_model.dart';
import 'package:graduation_app/features/profile/data/models/update_profile_response_model.dart';
import 'package:graduation_app/features/register/data/models/register_request_body_model.dart';
import 'package:graduation_app/features/register/data/models/register_response_model.dart';
import 'package:retrofit/error_logger.dart';
import 'package:retrofit/http.dart';
import '../../features/cart/data/models/clear_cart_model.dart';
import '../../features/cart/data/models/get_order_status_response_model.dart';
import '../../features/courses/data/models/add_course_to_cart_response_model.dart';
import '../../features/courses/data/models/start_study_session_request_model.dart';
import '../../features/profile/data/models/logout_request_model.dart';
import '../models/refresh_response.dart';
import '../models/refresh_token_model.dart';
import 'api_constants.dart';

part 'api_service.g.dart';

@RestApi(baseUrl: ApiConstants.apiBaseUrl)
abstract class ApiService {
  factory ApiService(Dio dio, {String baseUrl}) = _ApiService;

  //login
  @POST(ApiConstants.login)
  Future<LoginResponseModel> login(
    @Body() LoginRequestBodyModel loginRequestModel,
  );

  //register
  @POST(ApiConstants.register)
  Future<RegisterResponseModel> register(
    @Body() RegisterRequestBodyModel registerRequestModel,
  );

  //logout
  @POST(ApiConstants.logout)
  Future<void> logout(@Body() LogoutRequestModel logoutRequestModel);

  //refresh token
  @POST(ApiConstants.refreshEndpoint)
  Future<RefreshResponse> refreshToken(@Body() RefreshTokenRequest request);

  //getContinueLearning
  @GET(ApiConstants.getContinueLearningCourses)
  Future<ContinueLearningCourseModel> getContinueLearningCourses();

  //upComingExams
  @GET(ApiConstants.getUpComingExams)
  Future<UpComingExamsResponseModel> getUpcomingExams(
    @Path('CourseId') String courseId,
    @Query('Page') int page,
    @Query('PageSize') int pageSize,
  );

  //get all courses
  @GET(ApiConstants.getAllCourses)
  Future<GetAllCoursesResponseModel> getAllCourses(
    @Query('CategoryId') String? categoryId,
    @Query('Page') int page,
    @Query('PageSize') int pageSize,
  );

  // get available exams
  @GET(ApiConstants.getAvailbleExams)
  Future<GetAvailbleExamsResponseModel> getAvailbleExams(
    @Query('Page') int? page,
    @Query('PageSize') int? pageSize,
  );

  // get my profile
  @GET(ApiConstants.getMyProfile)
  Future<MyProfileModel> getMyProfile();

  // update my profile
  @PUT(ApiConstants.updateMyProfile)
  @MultiPart()
  Future<UpdateProfileResponseModel> updateMyProfile(
    @Part(name: 'firstName') String? firstName,
    @Part(name: 'lastName') String? lastName,
    @Part(name: 'userName') String? userName,
    @Part(name: 'bio') String? bio,
  );

  // get user statistics
  @GET(ApiConstants.getUserStatistics)
  Future<GetUserStatisticsModel> getUserStatistics(
    @Query('UserId') String userId,
  );

  // add course to cart
  @POST(ApiConstants.addCourseToCart)
  Future<AddCourseToCartResponseModel> addCourseToCart(
    @Body() AddCourseToCartRequestModel addCourseToCartRequestModel,
  );

  // get my cart
  @GET(ApiConstants.getMyCart)
  Future<GetMyCartResponseModel> getMyCart();

  //remove course from cart
  @DELETE(ApiConstants.removeCourseFromCart)
  Future<RemoveCourseFromCartResponseModel> removeCourseFromCart(
    @Path('CourseId') String courseId,
  );

  // clear my cart
  @DELETE(ApiConstants.clearMyCart)
  Future<ClearMyCartModel> clearMyCart();

  // checkout
  @POST(ApiConstants.checkoutEndpoint)
  Future<CheckoutResponseModel> startCheckout();

  // get order status
  @GET(ApiConstants.getOrderStatus)
  Future<GetOrderStatusResponseModel> getOrderStatus(
    @Path('OrderId') String orderId,
  );

  // get user dashboard
  @GET(ApiConstants.getUserDashboard)
  Future<MyCoursesResponseModel> getMyCourses();

  // get course lectures
  @GET(ApiConstants.getCourseLectures)
  Future<GetCourseLecturesResponseModel> getCourseLectures(
    @Path('CourseId') String courseId,
    @Query('IncludeMaterials') bool includeMaterials,
  );

  //startStudySession
  @POST(ApiConstants.startStudySession)
  Future<StartSessionResponseModel> startStudySession(
    @Body() StartStudySessionRequestModel startStudySessionRequestModel,
  );

  // generate flash cards
  @POST(ApiConstants.generateFlashCards)
  Future<FlashCardsResponseModel> generateFlashCards(
    @Body() FlashCardsRequestModel flashCardsRequestModel,
    @Path('SessionId') String sessionId,
  );

  // summary topic
  @POST(ApiConstants.summaryTopic)
  Future<SummaryTopicResponseModel> summaryTopic(
    @Body() SummaryTopicRequestModel sumaaryTopicRequestModel,
    @Path('SessionId') String sessionId,
  );

  //generate quiz
  @GET(ApiConstants.generateQuiz)
  Future<GenerateQuizResponseModel> generateQuiz(
    @Body() GenerateQuizRequestModel generateQuizRequestModel,
    @Path('SessionId') String sessionId,
  );

  //submit quiz
  @POST(ApiConstants.submitQuizAnswers)
  Future<SubmitQuizResponseModel> submitQuizAnswers(
    @Body() SubmitQuizRequestModel submitQuizRequestModel,
    @Path('SessionId') String sessionId,
    @Path('QuizId') String quizId,
  );

  @POST(ApiConstants.generateMindMap)
  Future<CreateMindMapResponseModel> generateMindMap(
    @Body() GenerateMindMapRequestModel generateMindMapRequestModel,
    @Path('SessionId') String sessionId,
  );

  @GET(ApiConstants.getExamQuestions)
  Future<GetExamQuestionsResponseModel> getExamQuestions(
    @Path('ExamId') String examId,
  );

  @POST(ApiConstants.submitExam)
  Future<SubmitExamResponseModel> submitExam(
    @Path('ExamId') String examId,
    @Body() Map<String, String> answers,
  );

  @GET(ApiConstants.getSubmitionDetails)
  Future<SubmissionDetailResponseModel> getSubmissionDetails(
    @Path('SubmissionId') String submissionId,
  );

  @GET(ApiConstants.getStudentSubmissions)
  Future<GetStudentSubmissionsResponseModel> getStudentSubmissions(
    @Query('Page') int page,
    @Query('PageSize') int pageSize,
  );
}
