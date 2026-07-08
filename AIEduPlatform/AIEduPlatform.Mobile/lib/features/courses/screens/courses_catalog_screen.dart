import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/features/courses/logic/courses_cubit.dart';
import 'package:graduation_app/features/courses/screens/Widgets/add_course_to_cart_bloc_listener.dart';
import 'package:graduation_app/features/courses/screens/Widgets/get_all_courses_bloc_builder.dart';
import '../../../core/di/dependency_injection.dart';
import '../../../core/theming/styles.dart';

class CoursesCatalogScreen extends StatelessWidget {
  const CoursesCatalogScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (BuildContext context) =>
          getIt<CoursesCubit>()..getAllCourses(null, 1, 20),
      child: Builder(
        builder: (context) {
          return Scaffold(
            appBar: AppBar(
              title: Text('Courses Catalog', style: TextStyles.font20),
              centerTitle: true,
            ),
            body: Padding(
              padding: EdgeInsets.symmetric(horizontal: 16.w, vertical: 16.h),
              child: Column(
                children: [
                  Expanded(
                    child: RefreshIndicator(
                      onRefresh: () async {
                        await context.read<CoursesCubit>().getAllCourses(
                          null,
                          1,
                          20,
                        );
                      },
                      child: GetAllCoursesBlocBuilder(),
                    ),
                  ),
                  AddCourseToCartBlocListener(),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
