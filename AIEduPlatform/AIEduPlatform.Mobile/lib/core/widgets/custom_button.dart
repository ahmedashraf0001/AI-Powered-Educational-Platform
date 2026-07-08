import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../theming/colors.dart';
import '../theming/styles.dart';

class CustomButton extends StatelessWidget {
  final String title;
  final void Function()? onPressed;
  final double? width;
  final double? height;
  final Color? color;
  final BorderRadiusGeometry? borderRadius;
  final Color? textColor;
  final Widget? body;

  const CustomButton({
    super.key,
    required this.title,
    this.onPressed,
    this.width,
    this.height,
    this.color,
    this.borderRadius,
    this.textColor,
    this.body,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onPressed,
      child: Container(
        alignment: Alignment.center,
        width: width ?? double.infinity,
        height: height ?? 52.h,
        decoration: BoxDecoration(
          color: color ?? ColorsManager.mainBlue,
          borderRadius: borderRadius ?? BorderRadius.circular(16.r),
        ),
        child: body != null
            ? body
            : Text(
                title,
                style: TextStyles.font17.copyWith(
                  fontWeight: FontWeight.bold,
                  color: textColor ?? ColorsManager.white,
                ),
              ),
      ),
    );
  }
}
