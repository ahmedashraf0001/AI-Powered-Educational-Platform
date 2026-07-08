import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/features/login/screens/login_screen.dart';
import 'package:graduation_app/features/profile/logic/profile_cubit.dart';
import 'package:graduation_app/features/profile/logic/profile_state.dart';
import '../../../../core/services/navigation/navigation_service.dart';
import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';


class LogoutBlocListener extends StatelessWidget {
  const LogoutBlocListener({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocListener<ProfileCubit, ProfileState>(
      listenWhen: (previous,current)=> current is LoadingLogout || current is SuccessLogout || current is FailureLogout,
      listener: (context, state) {
        state.whenOrNull(
            loadingLogout: (){
              showDialog(
                context: context,
                builder: (context)=>Center(child: CircularProgressIndicator(color: ColorsManager.mainBlue,)),
              );
            },
            successLogout: (loginResponse){
              NavigationService.instance.goBack();
              NavigationService.instance.navigateToAndRemoveUntil(LoginScreen());
            },
            failureLogout: (error){
              setupErrorState(context, error!);
            }
        );
      },
      child: const SizedBox.shrink(),
    );
  }
}


void setupErrorState(BuildContext context, String error) {
  NavigationService.instance.goBack();
  showDialog(
    context: context,
    builder: (context) => AlertDialog(
      icon: const Icon(
        Icons.error,
        color: Colors.red,
        size: 32,
      ),
      content: Text(
        error,
        style: TextStyles.font15.copyWith(color: ColorsManager.darkBlue,fontWeight: FontWeight.w500),
      ),
      actions: [
        TextButton(
          onPressed: () {
            NavigationService.instance.goBack();
          },
          child: Text(
            'Got it',
            style: TextStyles.font14.copyWith(color: ColorsManager.darkBlue,fontWeight: FontWeight.w600),
          ),
        ),
      ],
    ),
  );
}