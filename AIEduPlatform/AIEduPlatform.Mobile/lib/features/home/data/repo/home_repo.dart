import 'package:graduation_app/core/networking/api_error_handler.dart';
import 'package:graduation_app/core/networking/api_result.dart';
import 'package:graduation_app/core/networking/api_service.dart';
import 'package:graduation_app/features/home/data/models/continue_learning_course_model.dart';
import 'package:graduation_app/features/home/data/models/get_availble_exams_response_model.dart';
import 'package:graduation_app/features/home/data/models/get_exam_questions_response_model.dart';
import 'package:graduation_app/features/home/data/models/get_student_submissions_response_model.dart';
import 'package:graduation_app/features/home/data/models/submit_exam_response_model.dart';
import 'package:graduation_app/features/home/data/models/up_coming_exams_response_model.dart';

class HomeRepo {
  final ApiService apiService;

  HomeRepo({required this.apiService});

  Future<ApiResult<ContinueLearningCourseModel>>
  getContinueLearningCourses() async {
    try {
      final response = await apiService.getContinueLearningCourses();
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

  Future<ApiResult<UpComingExamsResponseModel>> getUpComingExams(
    String courseId,
    int page,
    int pageSize,
  ) async {
    try {
      final response = await apiService.getUpcomingExams(
        courseId,
        page,
        pageSize,
      );
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

  Future<ApiResult<GetAvailbleExamsResponseModel>> getAvailableExams(
    int? page,
    int? pageSize,
  ) async {
    try {
      final response = await apiService.getAvailbleExams(page, pageSize);
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

  Future<ApiResult<GetExamQuestionsResponseModel>> getExamQuestions(
    String examId,
  ) async {
    try {
      final response = await apiService.getExamQuestions(examId);
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

  Future<ApiResult<SubmitExamResponseModel>> submitExam(
    String examId,
    Map<String, String> answers,
  ) async {
    try {
      final response = await apiService.submitExam(examId, answers);
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

  Future<ApiResult<SubmissionDetailResponseModel>> getSubmissionDetails(
    String submissionId,
  ) async {
    try {
      final response = await apiService.getSubmissionDetails(submissionId);
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

  Future<ApiResult<GetStudentSubmissionsResponseModel>> getStudentSubmissions(
    int page,
    int pageSize,
  ) async {
    try {
      final response = await apiService.getStudentSubmissions(page, pageSize);
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }
}
