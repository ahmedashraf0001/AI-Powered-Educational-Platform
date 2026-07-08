import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/core/di/dependency_injection.dart';
import 'package:graduation_app/features/home/logic/home_cubit.dart';
import 'package:graduation_app/features/home/screens/exam_submission_screen_body.dart';
import 'package:graduation_app/core/theming/styles.dart';

class ExamSubmissionsScreen extends StatelessWidget {
  const ExamSubmissionsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) =>
          getIt<HomeCubit>()..getStudentSubmissions(page: 1, pageSize: 20),
      child: Scaffold(
        appBar: AppBar(
          title: Text('My Submissions', style: TextStyles.font20),
          centerTitle: true,
        ),
        body: ExamSubmissionsScreenBody(),
      ),
    );
  }
}
