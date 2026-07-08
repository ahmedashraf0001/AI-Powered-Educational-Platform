import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_svg/svg.dart';
import 'package:graduation_app/core/helpers/extensions.dart';

import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';

class HomeTopCard extends StatelessWidget {
  final Color? color;
  final Color? textColor;
  final Color? iconColor;
  final String iconName;
  final String title;
  final void Function()? onTap;
  const HomeTopCard({
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
        alignment: Alignment.center,
        height: 60.h,
        decoration: BoxDecoration(
          color: color ?? ColorsManager.mainBlue,
          borderRadius: BorderRadius.circular(24.r),
          border: Border.all(width: 1, color: ColorsManager.lightGray),
        ),
        child: Row(
          spacing: 12.w,
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            CircleAvatar(
              radius: 20.r,
              backgroundColor: iconColor ?? context.colors.surface,
              child: SvgPicture.asset(iconName, height: 22.h, width: 22.w),
            ),
            Text(
              title,
              style: TextStyles.font18.copyWith(
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
