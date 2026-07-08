import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';

class LoginWelcomeBack extends StatelessWidget {
  const LoginWelcomeBack({super.key});

  @override
  Widget build(BuildContext context) {
    return Column(
      spacing: 10.h,
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Icon(
          Icons.auto_stories_rounded,
          size: 70.w,
          color: ColorsManager.mainBlue,
        ),
        Text('Welcome Back', style: TextStyles.font24),
        Text(
          'Enter your details to access your courses',
          style: TextStyles.font16.copyWith(color: ColorsManager.darkGray),
        ),
      ],
    );
  }
}
