import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/features/profile/logic/profile_cubit.dart';
import 'package:graduation_app/features/profile/logic/profile_state.dart';
import 'package:graduation_app/features/profile/screens/widgets/profile_personal_info.dart';
import 'package:graduation_app/features/profile/screens/widgets/student_image_and_details.dart';

class ProfileDetailsBlocBuilder extends StatelessWidget {
  const ProfileDetailsBlocBuilder({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<ProfileCubit,ProfileState>(
      buildWhen: (previous,current)=> current is SuccessMyProfile || current is FailureMyProfile,
        builder: (context, state) {
          if (state is FailureMyProfile){
            return Center(child: Text(state.message ?? 'error'),);
          }
          else if (state is SuccessMyProfile){
            return Column(
              children: [
                StudentImageAndDetails(profileData: state.profileData,),
                ProfilePersonalInfo(profileData: state.profileData,),
              ],
            );
          }


          else{
            return Center(child: const SizedBox.shrink());
          }
        }

    );
  }
}
