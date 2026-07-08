import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/space_helper.dart';
import 'package:graduation_app/core/services/navigation/navigation_service.dart';

import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/core/theming/styles.dart';

import 'package:graduation_app/features/home/logic/home_cubit.dart';
import 'package:graduation_app/features/home/logic/home_state.dart';
import 'package:graduation_app/features/home/screens/available_exams_screen.dart';

import 'package:graduation_app/features/home/screens/widgets/home_available_exams_list_view.dart';
import 'package:skeletonizer/skeletonizer.dart';

class AvailableExamsBlocBuilder extends StatelessWidget {
  const AvailableExamsBlocBuilder({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<HomeCubit, HomeState>(
      buildWhen: (previous, current) =>
          current is SuccessGetAvailableExams ||
          current is FailureContinueLearning ||
          current is LoadingGetAvailableExams,

      builder: (context, state) {
        if (state is FailureGetAvailableExams) {
          return Center(child: Text(state.message ?? 'error'));
        } else if (state is SuccessGetAvailableExams) {
          return Column(
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text('Available Exams', style: TextStyles.font18),
                  GestureDetector(
                    onTap: () {
                      NavigationService.instance.navigateTo(
                        AvailableExamsScreen(
                          examsItemsList:
                              state.availableExamsDataModel.examsItemsList ??
                              [],
                        ),
                      );
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
              VerticalSpace(height: 10),
              HomeAvailableExamsListView(
                examsList: state.availableExamsDataModel.examsItemsList ?? [],
              ),
            ],
          );
        } else if (state is LoadingGetAvailableExams) {
          return Skeletonizer(
            enabled: true,
            enableSwitchAnimation: true,
            child: AvailableExamsSkeleton(),
          );
        } else {
          return SizedBox.shrink();
        }
      },
    );
  }
}

class AvailableExamsSkeleton extends StatelessWidget {
  const AvailableExamsSkeleton({super.key});

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
              Text('hhhhhhhhhhhhhhhhhhhhhhhhhh'),
              Text('hhhhhhhhhhhhhhhhhhhhhhhhhh'),
            ],
          ),
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
              Text('hhhhhhhhhhhhhhhhhhhhhhhhhh'),
              Text('hhhhhhhhhhhhhhhhhhhhhhhhhh'),
            ],
          ),
        ),
        VerticalSpace(height: 30.h),
      ],
    );
  }
}
