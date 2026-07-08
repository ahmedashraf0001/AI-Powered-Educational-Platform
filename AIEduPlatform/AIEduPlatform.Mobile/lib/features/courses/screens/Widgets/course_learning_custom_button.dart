import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_svg/svg.dart';

import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';

class CourseLearningCustomButton extends StatelessWidget {
  final Color? color;
  final Color? textColor;
  final Color? iconColor;
  final String iconName;
  final String title;
  final void Function()? onTap;
  const CourseLearningCustomButton({
    super.key,
    this.color,
    required this.iconName,
    required this.title,
    this.textColor,
    this.iconColor,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: EdgeInsets.symmetric(horizontal: 16.w, vertical: 8.h),
        alignment: Alignment.center,

        decoration: BoxDecoration(
          color: color ?? ColorsManager.mainBlue,
          borderRadius: BorderRadius.circular(24.r),
        ),
        child: Row(
          spacing: 12.w,
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            CircleAvatar(
              radius: 20.r,
              backgroundColor:
                  iconColor ?? ColorsManager.white.withValues(alpha: 0.20),
              child: SvgPicture.asset(iconName, height: 22.h, width: 22.w),
            ),
            Text(
              title,
              style: TextStyles.font20.copyWith(
                fontWeight: FontWeight.bold,
                color: textColor ?? ColorsManager.white,
              ),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }
}
