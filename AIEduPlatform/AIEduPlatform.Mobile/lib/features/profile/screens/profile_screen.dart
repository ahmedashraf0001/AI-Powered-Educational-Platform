import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/core/widgets/custom_button.dart';
import 'package:graduation_app/features/profile/logic/profile_cubit.dart';
import 'package:graduation_app/features/profile/screens/widgets/logout_bloc_listener.dart';
import 'package:graduation_app/features/profile/screens/widgets/profile_details_bloc_builder.dart';
import 'package:graduation_app/features/profile/screens/widgets/profile_settings.dart';
import 'package:graduation_app/features/profile/screens/widgets/profile_statistics_bloc_builder.dart';

import '../../../core/di/dependency_injection.dart';
import '../../../core/theming/styles.dart';

class ProfileScreen extends StatelessWidget {
  const ProfileScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => getIt<ProfileCubit>()..getMyProfile(),
      child: Builder(
        builder: (context) {
          return Scaffold(
            appBar: AppBar(
              title: Text('My Profile', style: TextStyles.font20),
              centerTitle: true,
              /*actions: [Icon(Icons.settings_outlined, size: 25.w)],
              actionsPadding: EdgeInsets.symmetric(horizontal: 16.w),*/
            ),
            body: Padding(
              padding: EdgeInsets.symmetric(horizontal: 16.w, vertical: 20.h),
              child: SingleChildScrollView(
                physics: BouncingScrollPhysics(),
                child: Column(
                  children: [
                    ProfileDetailsBlocBuilder(),
                    ProfileStatisticsBlocBuilder(),
                    ProfileSettings(),
                    CustomButton(
                      title: 'Logout',
                      color: ColorsManager.lightRed,
                      textColor: Colors.red,
                      onPressed: () async {
                        await context.read<ProfileCubit>().logout();
                      },
                    ),
                    LogoutBlocListener(),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}
