import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/core/di/dependency_injection.dart';
import 'package:graduation_app/features/cart/logic/cart_cubit.dart';
import 'package:graduation_app/features/main/screens/main_screen.dart';
import '../../../../core/services/navigation/navigation_service.dart';
import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';
import '../../logic/login_cubit.dart';
import '../../logic/login_state.dart';

class LoginBlocListener extends StatelessWidget {
  const LoginBlocListener({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocListener<LoginCubit, LoginState>(
      listenWhen: (previous, current) =>
          current is Loading || current is Success || current is Failure,
      listener: (context, state) {
        state.whenOrNull(
          loading: () {
            showDialog(
              context: context,
              builder: (context) => Center(
                child: CircularProgressIndicator(color: ColorsManager.mainBlue),
              ),
            );
          },
          success: (loginResponse) {
            NavigationService.instance.goBack();
            NavigationService.instance.navigateTo(
              BlocProvider(
                create: (context) => getIt<CartCubit>(),
                child: MainScreen(),
              ),
            );
          },
          failure: (error) {
            setupErrorState(context, error!);
          },
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
      icon: const Icon(Icons.error, color: Colors.red, size: 32),
      content: Text(
        error,
        style: TextStyles.font15.copyWith(
          color: ColorsManager.darkBlue,
          fontWeight: FontWeight.w500,
        ),
      ),
      actions: [
        TextButton(
          onPressed: () {
            NavigationService.instance.goBack();
          },
          child: Text(
            'Got it',
            style: TextStyles.font14.copyWith(
              color: ColorsManager.darkBlue,
              fontWeight: FontWeight.w600,
            ),
          ),
        ),
      ],
    ),
  );
}
