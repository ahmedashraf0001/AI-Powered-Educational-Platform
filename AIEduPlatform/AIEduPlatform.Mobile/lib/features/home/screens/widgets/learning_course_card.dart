import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/features/cart/data/models/my_courses_response_model.dart';
import '../../../../core/helpers/space_helper.dart';
import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';

class LearningCourseCard extends StatelessWidget {
  final CoursesProgress courseModel;
  const LearningCourseCard({super.key, required this.courseModel});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDark = theme.brightness == Brightness.dark;

    // Theme-aware palette
    final cardColor = isDark ? const Color(0xFF1E1E22) : Colors.white;
    final borderColor = isDark
        ? Colors.white.withOpacity(0.08)
        : ColorsManager.lightGray;
    final titleColor = isDark ? Colors.white : Colors.black;
    final subtitleColor = isDark
        ? Colors.white.withOpacity(0.6)
        : ColorsManager.darkGray;
    final progressLabelColor = isDark
        ? Colors.white.withOpacity(0.7)
        : ColorsManager.darkGray;
    final trackColor = isDark
        ? Colors.white.withOpacity(0.1)
        : ColorsManager.lightGray.withOpacity(0.6);

    final progress = ((courseModel.progressPercentage ?? 0) / 100).clamp(
      0.0,
      1.0,
    );

    return Container(
      width: 240.w,
      padding: EdgeInsets.symmetric(horizontal: 12.w, vertical: 12.h),
      decoration: BoxDecoration(
        color: cardColor,
        borderRadius: BorderRadius.circular(20.r),
        border: Border.all(width: 1, color: borderColor),
        boxShadow: [
          BoxShadow(
            color: isDark
                ? Colors.black.withOpacity(0.3)
                : Colors.black.withOpacity(0.05),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          ClipRRect(
            borderRadius: BorderRadius.circular(14.r),
            child: Container(
              width: 240.w,
              height: 110.h,
              color: trackColor,
              child: Image.asset(
                'assets/images/ai_robot.png',
                fit: BoxFit.cover,
              ),
            ),
          ),

          VerticalSpace(height: 10),

          Text(
            courseModel.courseTitle ?? 'empty',
            style: TextStyles.font16.copyWith(
              fontWeight: FontWeight.bold,
              color: titleColor,
            ),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
          ),

          VerticalSpace(height: 4),

          Text(
            courseModel.status ?? 'empty',
            style: TextStyles.font13.copyWith(
              fontWeight: FontWeight.w500,
              color: subtitleColor,
            ),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
          ),

          VerticalSpace(height: 10),

          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Progress',
                style: TextStyles.font12.copyWith(
                  fontWeight: FontWeight.bold,
                  color: progressLabelColor,
                ),
              ),
              Text(
                '${(courseModel.progressPercentage ?? 0).toStringAsFixed(0)}%',
                style: TextStyles.font12.copyWith(
                  fontWeight: FontWeight.bold,
                  color: ColorsManager.mainBlue,
                ),
              ),
            ],
          ),

          VerticalSpace(height: 6),

          ClipRRect(
            borderRadius: BorderRadius.circular(8.r),
            child: LinearProgressIndicator(
              value: progress,
              minHeight: 6.h,
              backgroundColor: trackColor,
              valueColor: AlwaysStoppedAnimation<Color>(ColorsManager.mainBlue),
            ),
          ),
        ],
      ),
    );
  }
}
