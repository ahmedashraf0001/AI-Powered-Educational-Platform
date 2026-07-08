import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/services/navigation/navigation_service.dart';
import 'package:graduation_app/features/cart/logic/cart_cubit.dart';
import 'package:graduation_app/features/cart/logic/cart_state.dart';
import 'package:graduation_app/features/courses/screens/my_courses_screen.dart';
import 'package:skeletonizer/skeletonizer.dart';

import '../../../../core/helpers/space_helper.dart';
import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';
import 'continue_learning_list_view.dart';

class ContinueLearningBlocBuilder extends StatelessWidget {
  const ContinueLearningBlocBuilder({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<CartCubit, CartState>(
      buildWhen: (previous, current) =>
          current is SuccessGetMyCourses ||
          current is FailureGetMyCourses ||
          current is LoadingGetMyCourses,
      builder: (context, state) {
        if (state is FailureGetMyCourses) {
          return Center(child: Text(state.message ?? 'error'));
        } else if (state is SuccessGetMyCourses) {
          return Column(
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text('Continue Learning', style: TextStyles.font18),
                  GestureDetector(
                    onTap: () {
                      NavigationService.instance.navigateTo(MyCoursesScreen());
                    },
                    child: Text(
                      'See All',
                      style: TextStyles.font14.copyWith(
                        fontWeight: FontWeight.w600,
                        color: ColorsManager.mainBlue,
                      ),
                    ),
                  ),
                ],
              ),
              VerticalSpace(height: 15),
              ContinueLearningListView(
                coursesList:
                    state
                        .myCoursesReponseModel
                        .coursesData
                        .courseProgressList ??
                    [],
              ),
            ],
          );
        } else if (state is LoadingGetMyCourses) {
          return Skeletonizer(
            enabled: true,
            enableSwitchAnimation: true,
            child: ContinueLearningSkeleton(),
          );
        } else {
          return const SizedBox.shrink();
        }
      },
    );
  }
}

class ContinueLearningSkeleton extends StatelessWidget {
  const ContinueLearningSkeleton({super.key});

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Container(
              height: 28.h,
              width: 180.w,
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(8.r),
              ),
              child: Text('hhhhhhhhhhhhhhhh'),
            ),
            Container(
              height: 20.h,
              width: 60.w,
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(8.r),
              ),
              child: Text('hhhhhh'),
            ),
          ],
        ),

        VerticalSpace(height: 16),
        Container(
          padding: EdgeInsets.all(16.w),
          width: double.infinity,
          decoration: BoxDecoration(
            color: ColorsManager.gray,
            borderRadius: BorderRadius.circular(24.r),
          ),
          child: Column(
            spacing: 10.h,
            children: [
              Container(
                width: double.infinity,
                height: 120.h,
                decoration: BoxDecoration(
                  color: ColorsManager.lightGray.withValues(alpha: 0.3),
                  borderRadius: BorderRadius.circular(24.r),
                ),
              ),
              Text('hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh'),
              Text('hhhhhhhhhhhhhhhhhhhhhhh'),
            ],
          ),
        ),
        VerticalSpace(height: 30.h),
      ],
    );
  }
}
