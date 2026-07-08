import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/core/theming/styles.dart';
import 'package:graduation_app/features/cart/data/models/my_courses_response_model.dart';

class StreakWidget extends StatelessWidget {
  final StreakModel streak;
  const StreakWidget({super.key, required this.streak});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.symmetric(
        horizontal: 16.w,
      ).copyWith(top: 8.h, bottom: 12.h),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(24.r),
        color: ColorsManager.mainBlue,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Keep it up!',
            style: TextStyles.font13.copyWith(
              fontWeight: FontWeight.w400,
              color: ColorsManager.white,
            ),
          ),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                '${streak.currentStreak ?? 0}-Day Study Streak',
                style: TextStyles.font17.copyWith(
                  fontWeight: FontWeight.bold,
                  color: ColorsManager.white,
                ),
              ),
              CircleAvatar(
                backgroundColor: ColorsManager.lightBlue.withValues(alpha: 0.3),
                radius: 22.r,
                child: Icon(
                  Icons.local_fire_department_outlined,
                  color: ColorsManager.white,
                  size: 25.w,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
