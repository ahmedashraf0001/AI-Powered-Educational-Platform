import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/features/profile/data/models/get_user_statistics_model.dart';

import '../../../../core/helpers/space_helper.dart';
import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';

class ProfileMyProgress extends StatelessWidget {
  final UserStatisticsData userStatisticsData;
  const ProfileMyProgress({super.key, required this.userStatisticsData});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.symmetric(horizontal: 16.w, vertical: 24.h),
      width: double.infinity,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('My Progress', style: TextStyles.font18),
          VerticalSpace(height: 16),
          Row(
            children: [
              Expanded(
                child: ProgressCard(
                  title: 'Enrolled',
                  icon: Icons.assignment,
                  contentText: '${userStatisticsData.coursesEnrolled}\nCourses',
                  iconColor: Colors.orange,
                ),
              ),
              HorizontalSpace(width: 16),
              Expanded(
                child: ProgressCard(
                  title: 'Completed ',
                  icon: Icons.incomplete_circle,
                  contentText:
                      '${userStatisticsData.coursesCompleted}\nCourses',
                  iconColor: ColorsManager.mainBlue,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class ProgressCard extends StatelessWidget {
  final String title;
  final String contentText;
  final IconData icon;
  final Color iconColor;

  const ProgressCard({
    super.key,
    required this.title,
    required this.contentText,
    required this.icon,
    required this.iconColor,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.symmetric(horizontal: 16.w, vertical: 16.h),
      width: double.infinity,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(24.r),
        border: Border.all(width: 1, color: ColorsManager.lightGray),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(icon, size: 25.w, color: iconColor),
              SizedBox(width: 8.w),
              Text(
                title,
                style: TextStyles.font15.copyWith(
                  fontWeight: FontWeight.w500,
                  color: ColorsManager.darkGray,
                ),
              ),
            ],
          ),
          VerticalSpace(height: 12),
          Text(contentText, style: TextStyles.font20),
        ],
      ),
    );
  }
}
