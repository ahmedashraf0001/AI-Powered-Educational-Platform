import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/features/courses/logic/courses_cubit.dart';
import 'package:graduation_app/features/courses/logic/courses_state.dart';

import 'course_catalog_list_view.dart';

class GetAllCoursesBlocBuilder extends StatelessWidget {
  const GetAllCoursesBlocBuilder({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<CoursesCubit, CoursesState>(
        buildWhen: (previous,current)=> current is SuccessGetAllCourses || current is FailureGetAllCourses || current is LoadingGetAllCourses,
        builder: (context, state) {
          if (state is FailureGetAllCourses){
            return Center(child: Text(state.message ?? 'error'),);
          }

          else if (state is SuccessGetAllCourses){
            return CourseCatalogListView(coursesList: state.coursesData,);

          }
          else if (state is LoadingGetAllCourses) {
            return const Center(
              child: CircularProgressIndicator(),
            );
          }

          else{
            return const SizedBox.shrink();
          }
        }
    );
  }
}
