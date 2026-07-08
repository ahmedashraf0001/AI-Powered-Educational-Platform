import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'package:graduation_app/core/di/dependency_injection.dart';

import 'package:graduation_app/features/courses/logic/courses_cubit.dart';

import 'package:graduation_app/features/courses/screens/course_learning_screen_body.dart';
import '../../../core/theming/styles.dart';

class CourseLearningScreen extends StatelessWidget {
  final String? courseId;
  final String? courseTitle;

  const CourseLearningScreen({super.key, this.courseId, this.courseTitle});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => getIt<CoursesCubit>(),
      child: Scaffold(
        appBar: AppBar(
          title: Text(courseTitle ?? '', style: TextStyles.font20),
        ),
        body: CourseLearningScreenBody(
          courseId: courseId,
          courseTitle: courseTitle,
        ),
      ),
    );
  }
}
