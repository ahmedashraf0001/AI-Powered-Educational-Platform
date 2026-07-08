import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/services/navigation/navigation_service.dart';
import 'package:graduation_app/features/profile/logic/profile_cubit.dart';
import 'package:graduation_app/features/profile/screens/edit_proifle_screen.dart';

import '../../../../core/helpers/space_helper.dart';
import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';
import '../../data/models/my_profile_model.dart';

class ProfilePersonalInfo extends StatelessWidget {
  final MyProfileData profileData;
  const ProfilePersonalInfo({super.key, required this.profileData});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: () async {
        final message = await NavigationService.instance.navigateTo(
          BlocProvider.value(
            value: context.read<ProfileCubit>(),
            child: EditProifleScreen(profileData: profileData),
          ),
        );
        if (message != null && context.mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text(message), backgroundColor: Colors.green),
          );
        }
      },
      child: Container(
        padding: EdgeInsets.symmetric(horizontal: 16.w, vertical: 24.h),
        width: double.infinity,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Personal Info', style: TextStyles.font18),
            VerticalSpace(height: 16),
            PersonalInformationCard(
              title: 'Email Address',
              contentText: profileData.email ?? '',
              icon: Icons.email_outlined,
            ),

            VerticalSpace(height: 12),
            PersonalInformationCard(
              title: 'User Name',
              contentText: profileData.userName ?? '',
              icon: Icons.person_pin_outlined,
            ),
          ],
        ),
      ),
    );
  }
}

class PersonalInformationCard extends StatelessWidget {
  final String title;
  final String contentText;
  final IconData icon;
  final void Function()? onPressed;

  const PersonalInformationCard({
    super.key,
    required this.title,
    required this.contentText,
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
          Expanded(
            child: Row(
              children: [
                Container(
                  alignment: Alignment.center,
                  height: 45.h,
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    color: ColorsManager.mainBlue.withValues(alpha: 0.25),
                  ),
                  child: Icon(icon, size: 22.w, color: ColorsManager.mainBlue),
                ),
                HorizontalSpace(width: 12),
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      title,
                      style: TextStyles.font13.copyWith(
                        color: ColorsManager.darkGray,
                      ),
                    ),
                    Text(
                      contentText,
                      style: TextStyles.font14.copyWith(
                        fontWeight: FontWeight.w500,
                      ),
                      overflow: TextOverflow.ellipsis,
                    ),
                  ],
                ),
              ],
            ),
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
