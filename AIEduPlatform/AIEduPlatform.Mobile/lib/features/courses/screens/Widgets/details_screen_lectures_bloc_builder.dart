import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/features/courses/logic/courses_cubit.dart';
import 'package:graduation_app/features/courses/logic/courses_state.dart';
import 'package:graduation_app/features/courses/screens/Widgets/lectures_item_card.dart';
import '../../../../core/helpers/space_helper.dart';
import '../../../../core/theming/styles.dart';

class DetailsScreenLecturesBlocBuilder extends StatelessWidget {
  const DetailsScreenLecturesBlocBuilder({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<CoursesCubit, CoursesState>(
      buildWhen: (previous, current) =>
          current is SuccessCourseLectures ||
          current is FailureCourseLectures ||
          current is LoadingCourseLectures,
      builder: (context, state) {
        if (state is FailureCourseLectures) {
          return Center(child: Text(state.message ?? 'error'));
        } else if (state is SuccessCourseLectures) {
          final dataList = state.courseLecturerMaterials;

          if (state.courseLecturerMaterials.isEmpty) {
            return Center(
              child: Padding(
                padding: EdgeInsets.all(24.w),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(
                      Icons.menu_book_outlined,
                      size: 64.sp,
                      color: Colors.grey,
                    ),
                    VerticalSpace(height: 12),
                    Text(
                      'No lesson materials yet',
                      style: TextStyles.font16.copyWith(
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    VerticalSpace(height: 6),
                    Text(
                      'Materials will appear here when they are added.',
                      textAlign: TextAlign.center,
                      style: TextStyles.font14.copyWith(color: Colors.grey),
                    ),
                  ],
                ),
              ),
            );
          }

          return ListView.builder(
            padding: EdgeInsets.zero,
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            itemCount: dataList.length,
            itemBuilder: (context, index) {
              return Padding(
                padding: EdgeInsets.only(bottom: 16.h),
                child: LecturesItemCard(lessonMaterial: dataList[index]),
              );
            },
          );
        } else if (state is LoadingCourseLectures) {
          return const Center(child: CircularProgressIndicator());
        } else {
          return const SizedBox.shrink();
        }
      },
    );
  }
}
