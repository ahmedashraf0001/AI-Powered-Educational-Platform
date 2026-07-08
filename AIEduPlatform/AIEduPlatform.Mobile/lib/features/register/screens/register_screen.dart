import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/di/dependency_injection.dart';
import 'package:graduation_app/core/helpers/space_helper.dart';
import 'package:graduation_app/features/register/logic/register_cubit.dart';
import 'package:graduation_app/features/register/screens/widgets/already_have_account.dart';
import 'package:graduation_app/features/register/screens/widgets/register_bloc_listener.dart';
import 'package:graduation_app/features/register/screens/widgets/register_form.dart';
import '../../../core/theming/colors.dart';
import '../../../core/theming/styles.dart';
import '../../../core/widgets/custom_button.dart';

class RegisterScreen extends StatelessWidget {
  const RegisterScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => getIt.get<RegisterCubit>(),
      child: Builder(
        builder: (context) {
          return Scaffold(
            appBar: AppBar(
              title: Text('Sign Up', style: TextStyles.font18),
              centerTitle: true,
            ),
            body: Padding(
              padding: EdgeInsetsGeometry.symmetric(
                horizontal: 16.w,
                vertical: 15.h,
              ),
              child: SingleChildScrollView(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('Create your account', style: TextStyles.font24),
                    Text(
                      'Join thousands of students learning today.',
                      style: TextStyles.font14.copyWith(
                        color: ColorsManager.darkGray,
                      ),
                    ),
                    VerticalSpace(height: 24),
                    RegisterForm(),
                    VerticalSpace(height: 40),
                    CustomButton(
                      title: 'Create Account',
                      onPressed: () {
                        if (context
                            .read<RegisterCubit>()
                            .registerFormKey
                            .currentState!
                            .validate()) {
                          context.read<RegisterCubit>().register();
                        }
                      },
                    ),
                    VerticalSpace(height: 30),
                    AlreadyHaveAccount(),
                    VerticalSpace(height: 24),
                    Center(
                      child: Text(
                        'By creating an account, you agree to our Terms of Service and\nPrivacy Policy.',
                        style: TextStyles.font10.copyWith(
                          color: ColorsManager.gray,
                        ),
                        textAlign: TextAlign.center,
                      ),
                    ),
                    RegisterBlocListener(),
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
