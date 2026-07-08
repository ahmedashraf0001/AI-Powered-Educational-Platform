import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/extensions.dart';

import '../theming/colors.dart';
import '../theming/styles.dart';

class CustomTextField extends StatelessWidget {
  final EdgeInsetsGeometry? contentPadding;
  final InputBorder? focusedBorder;
  final InputBorder? enabledBorder;
  final String hintText;
  final bool? isObscureText;
  final Widget? suffixIcon;
  final Function(String?) validator;
  final TextEditingController? controller;
  const CustomTextField({
    super.key,
    this.contentPadding,
    this.focusedBorder,
    this.enabledBorder,
    required this.hintText,
    this.isObscureText,
    required this.validator,
    this.controller,
    this.suffixIcon,
  });

  @override
  Widget build(BuildContext context) {
    return TextFormField(
      obscureText: isObscureText ?? false,
      style: TextStyles.font14.copyWith(
        color: context.colors.onSurface,
        fontWeight: FontWeight.w500,
      ),
      decoration: InputDecoration(
        isDense: true,
        filled: true,
        suffixIcon: suffixIcon,
        fillColor: context.colors.surface,
        hintText: hintText,
        hintStyle: TextStyles.font14.copyWith(
          color: context.colors.onSurface.withValues(alpha: 0.5),
          fontWeight: FontWeight.w500,
        ),
        contentPadding:
            contentPadding ??
            EdgeInsets.symmetric(horizontal: 20.w, vertical: 20.h),
        focusedBorder:
            focusedBorder ??
            OutlineInputBorder(
              borderRadius: BorderRadius.circular(16.0),
              borderSide: BorderSide(
                color: ColorsManager.lightBlue,
                width: 1.3,
              ),
            ),
        enabledBorder:
            enabledBorder ??
            OutlineInputBorder(
              borderRadius: BorderRadius.circular(16.0),
              borderSide: BorderSide(
                color: ColorsManager.lighterGray,
                width: 1.3,
              ),
            ),
        errorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(16.0),
          borderSide: BorderSide(color: Colors.red, width: 1.3),
        ),
        focusedErrorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(16.0),
          borderSide: BorderSide(color: Colors.red, width: 1.3),
        ),
      ),
      validator: (value) {
        return validator(value);
      },
      controller: controller,
    );
  }
}
