import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/core/networking/api_result.dart';
import 'package:graduation_app/features/courses/data/repo/courses_repo.dart';

import 'courses_state.dart';

class CoursesCubit extends Cubit<CoursesState> {
  final CoursesRepo coursesRepo;
  CoursesCubit(this.coursesRepo) : super(CoursesState.initial());

  getAllCourses(String? categoryId, int page , int pageSize)async{
    emit(LoadingGetAllCourses());
    final data = await coursesRepo.getAllCourses(categoryId?? null, page, pageSize);
    return data.when(
        success: (courses){
          emit(SuccessGetAllCourses(courses.data?.allCoursesItemModels ?? [],));
        },
        failure: (failure){
          emit(FailureGetAllCourses(message: failure.apiErrorModel.message));
        }
    );
  }

  addCourseToCart(String courseId)async{
    emit(LoadingAddCourseToCart());
    final data = await coursesRepo.addCourseToCart(courseId);
    return data.when(
        success: (responseModel){
          emit(SuccessAddCourseToCart(responseModel));
        },
        failure: (failure){
          emit(FailureAddCourseToCart(message: failure.apiErrorModel.message));
        }
    );
  }


  getCourseLectures(String courseId)async{
    emit(LoadingCourseLectures());
    final data = await coursesRepo.getCourseLectures(courseId, true);
    return data.when(
        success: (response) {
          final materials = response.courseLecturesDataList
              .expand((lecture) => lecture.courseLectureMaterials)
              .toList();

          emit(SuccessCourseLectures(materials));
        },
        failure: (failure){
          emit(FailureCourseLectures(message: failure.apiErrorModel.message));

        }
    );

  }



  startStudySession(String courseId)async{
    emit(LoadingStartSession());
    final data = await coursesRepo.startStudySession(courseId);
    return data.when(
        success: (data){
          emit(SuccessStartSession(data));
        },
        failure: (errorMessage){
          emit(FailureStartSession(message: errorMessage.apiErrorModel.message));
        }
    );
  }


}
