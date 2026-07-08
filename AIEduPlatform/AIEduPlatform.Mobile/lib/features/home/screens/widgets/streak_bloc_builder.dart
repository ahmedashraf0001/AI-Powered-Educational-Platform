import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/features/cart/logic/cart_cubit.dart';
import 'package:graduation_app/features/cart/logic/cart_state.dart';
import 'package:graduation_app/features/home/screens/widgets/streak_widget.dart';
import 'package:skeletonizer/skeletonizer.dart';

class StreakBlocBuilder extends StatelessWidget {
  const StreakBlocBuilder({super.key});

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
          return StreakWidget(
            streak: state.myCoursesReponseModel.coursesData.streak!,
          );
        } else if (state is LoadingGetMyCourses) {
          return Skeletonizer(
            enabled: true,
            enableSwitchAnimation: true,
            child: StreakSkeleton(),
          );
        } else {
          return const SizedBox.shrink();
        }
      },
    );
  }
}

class StreakSkeleton extends StatelessWidget {
  const StreakSkeleton({super.key});

  @override
  Widget build(BuildContext context) {
    return Container(
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
    );
  }
}
