import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/cubit/theme_cubit.dart';
import 'package:graduation_app/core/di/dependency_injection.dart';
import 'package:graduation_app/core/services/navigation/navigation_service.dart';
import 'package:graduation_app/features/privacy_security/privacy_and_security_screen.dart';

import '../../../../core/helpers/space_helper.dart';
import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';

class ProfileSettings extends StatelessWidget {
  const ProfileSettings({super.key});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.symmetric(horizontal: 16.w, vertical: 24.h),
      width: double.infinity,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Settings', style: TextStyles.font18),
          VerticalSpace(height: 16),
          /*SettingsCard(
            title: 'Notifications',
            icon: Icons.notifications_outlined,
            onPressed: () {},
          ),
          VerticalSpace(height: 12),*/
          SettingsCard(
            title: 'Privacy & Security',
            icon: Icons.lock_outline,
            onPressed: () {
              NavigationService.instance.navigateTo(PrivacySecurityScreen());
            },
          ),
          VerticalSpace(height: 12),
          BlocBuilder<ThemeCubit, ThemeMode>(
            bloc: getIt<ThemeCubit>(),
            builder: (context, themeMode) {
              return SettingsCard(
                title: themeMode == ThemeMode.dark ? 'Light Mode' : 'Dark Mode',
                icon: themeMode == ThemeMode.dark
                    ? Icons.light_mode_outlined
                    : Icons.dark_mode_outlined,
                onPressed: () {
                  getIt<ThemeCubit>().toggleTheme();
                },
              );
            },
          ),
        ],
      ),
    );
  }
}

class SettingsCard extends StatelessWidget {
  final String title;
  final IconData icon;
  final void Function()? onPressed;

  const SettingsCard({
    super.key,
    required this.title,
    required this.icon,
    this.onPressed,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: EdgeInsets.symmetric(horizontal: 16.w, vertical: 16.h),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(24.r),
        border: Border.all(width: 1, color: ColorsManager.lightGray),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Row(
            children: [
              Container(
                alignment: Alignment.center,
                width: 45.w,
                height: 45.h,
                decoration: BoxDecoration(shape: BoxShape.circle),
                child: Icon(icon, size: 25.w, color: ColorsManager.darkGray),
              ),
              HorizontalSpace(width: 12),
              Text(
                title,
                style: TextStyles.font17.copyWith(fontWeight: FontWeight.w500),
              ),
            ],
          ),
          IconButton(
            onPressed: onPressed,
            icon: Icon(
              Icons.arrow_forward_ios_rounded,
              color: ColorsManager.darkGray,
            ),
          ),
        ],
      ),
    );
  }
}
