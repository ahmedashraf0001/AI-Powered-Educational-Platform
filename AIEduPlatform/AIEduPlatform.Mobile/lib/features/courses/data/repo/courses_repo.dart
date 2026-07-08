
import 'package:graduation_app/core/networking/api_error_handler.dart';
import 'package:graduation_app/core/networking/api_result.dart';
import 'package:graduation_app/core/networking/api_service.dart';
import 'package:graduation_app/features/courses/data/models/add_course_to_cart_request_model.dart';
import 'package:graduation_app/features/courses/data/models/add_course_to_cart_response_model.dart';
import 'package:graduation_app/features/courses/data/models/get_all_courses_response_model.dart';
import 'package:graduation_app/features/courses/data/models/get_course_lectures_response_model.dart';

import '../models/start_study_session_request_model.dart';
import '../models/start_study_session_response_model.dart';

class CoursesRepo{
  final ApiService apiService;

  CoursesRepo({required this.apiService});

  Future<ApiResult<GetAllCoursesResponseModel>> getAllCourses(String? categoryId, int page , int pageSize)async{
    try {
      final response = await apiService.getAllCourses(categoryId, page, pageSize);
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }

  }

  Future<ApiResult<AddCourseToCartResponseModel>> addCourseToCart(String courseId)async{
    try {
      final response = await apiService.addCourseToCart(AddCourseToCartRequestModel(courseId: courseId));
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }



  Future<ApiResult<GetCourseLecturesResponseModel>> getCourseLectures(String courseId,bool includeMaterials)async{
    try {
      final response = await apiService.getCourseLectures(courseId, includeMaterials);
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }



  Future<ApiResult<StartSessionResponseModel>> startStudySession(String courseId)async{
    try {
      final response = await apiService.startStudySession(StartStudySessionRequestModel(courseId: courseId));
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

}