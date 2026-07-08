import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import 'package:graduation_app/features/cart/logic/cart_cubit.dart';
import 'package:graduation_app/features/courses/screens/Widgets/my_courses_bloc_builder.dart';
import '../../../core/di/dependency_injection.dart';
import '../../../core/theming/styles.dart';

class MyCoursesScreen extends StatelessWidget {
  const MyCoursesScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => getIt<CartCubit>()..getMyCourses(),
      child: Builder(
        builder: (context) {
          return Scaffold(
            appBar: AppBar(title: Text('My Courses', style: TextStyles.font20)),
            body: Padding(
              padding: EdgeInsets.symmetric(horizontal: 16.w, vertical: 20.h),
              child: Column(
                children: [Expanded(child: MyCoursesBlocBuilder())],
              ),
            ),
          );
        },
      ),
    );
  }
}
