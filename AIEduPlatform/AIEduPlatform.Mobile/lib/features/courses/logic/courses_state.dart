import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:graduation_app/features/courses/data/models/add_course_to_cart_response_model.dart';
import 'package:graduation_app/features/courses/data/models/get_course_lectures_response_model.dart';
import 'package:graduation_app/features/courses/data/models/start_study_session_response_model.dart';

import '../data/models/get_all_courses_response_model.dart';

part 'courses_state.freezed.dart';

@freezed
class CoursesState<T> with _$CoursesState<T> {
  const factory CoursesState.initial() = _Initial;

  const factory CoursesState.loadingGetAllCourses() = LoadingGetAllCourses;

  const factory CoursesState.successGetAllCourses(
    List<AllCoursesItemModel> coursesData,
  ) = SuccessGetAllCourses;

  const factory CoursesState.failureGetAllCourses({String? message}) =
      FailureGetAllCourses;

  const factory CoursesState.loadingAddCourseToCart() = LoadingAddCourseToCart;

  const factory CoursesState.successAddCourseToCart(
    AddCourseToCartResponseModel responseModel,
  ) = SuccessAddCourseToCart;

  const factory CoursesState.failureAddCourseToCart({String? message}) =
      FailureAddCourseToCart;

  const factory CoursesState.loadingCourseLectures() = LoadingCourseLectures;

  const factory CoursesState.successCourseLectures(
    List<CourseLectureMaterials> courseLecturerMaterials,
  ) = SuccessCourseLectures;

  const factory CoursesState.failureCourseLectures({String? message}) =
      FailureCourseLectures;

  //study session
  const factory CoursesState.loadingStartSession() = LoadingStartSession;
  const factory CoursesState.successStartSession(
    StartSessionResponseModel dataModel,
  ) = SuccessStartSession;
  const factory CoursesState.failureStartSession({String? message}) =
      FailureStartSession;
}
