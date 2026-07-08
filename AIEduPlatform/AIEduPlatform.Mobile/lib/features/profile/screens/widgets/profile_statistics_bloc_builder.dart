import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/features/profile/logic/profile_cubit.dart';
import 'package:graduation_app/features/profile/logic/profile_state.dart';
import 'package:graduation_app/features/profile/screens/widgets/profile_my_progress.dart';

class ProfileStatisticsBlocBuilder extends StatelessWidget {
  const ProfileStatisticsBlocBuilder({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<ProfileCubit,ProfileState>(
        buildWhen: (previous,current)=>  current is SuccessGetUserStatistics || current is FailureGetUserStatistics || current is LoadingGetUserStatistics,
        builder: (context, state) {
          if (state is FailureGetUserStatistics){
            return Center(child: Text(state.message ?? 'error'),);
          }
          else if (state is SuccessGetUserStatistics){

            return ProfileMyProgress(userStatisticsData: state.userStatistics);
          }
          else if (state is LoadingGetUserStatistics){

            return CircularProgressIndicator();
          }


          else{
            return Center(child: const SizedBox.shrink());
          }
        }

    );
  }
}
