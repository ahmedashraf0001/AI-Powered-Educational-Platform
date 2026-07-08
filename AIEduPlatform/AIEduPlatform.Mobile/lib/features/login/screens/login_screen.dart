import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/space_helper.dart';
import 'package:graduation_app/features/login/logic/login_cubit.dart';
import 'package:graduation_app/features/login/screens/widgets/dont_have_account.dart';
import 'package:graduation_app/features/login/screens/widgets/login_bloc_listener.dart';
import 'package:graduation_app/features/login/screens/widgets/login_form.dart';
import 'package:graduation_app/features/login/screens/widgets/login_welcome_back.dart';

import '../../../core/di/dependency_injection.dart';
import '../../../core/theming/colors.dart';
import '../../../core/theming/styles.dart';
import '../../../core/widgets/custom_button.dart';

class LoginScreen extends StatelessWidget {
  const LoginScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (_) => getIt.get<LoginCubit>(),
      child: Builder(
        builder: (context) {
          return Scaffold(
            appBar: AppBar(
              title: Text('Login', style: TextStyles.font18),
              centerTitle: true,
            ),
            body: Padding(
              padding: EdgeInsetsGeometry.symmetric(
                horizontal: 16.w,
                vertical: 15.h,
              ),
              child: SingleChildScrollView(
                child: Column(
                  children: [
                    Center(child: LoginWelcomeBack()),
                    VerticalSpace(height: 32),
                    LoginForm(),
                    VerticalSpace(height: 20),
                    GestureDetector(
                      onTap: () {},
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.end,
                        children: [
                          Text(
                            'ForgotPassword?',
                            style: TextStyles.font14.copyWith(
                              fontWeight: FontWeight.bold,
                              color: ColorsManager.mainBlue,
                            ),
                          ),
                        ],
                      ),
                    ),
                    VerticalSpace(height: 30),
                    CustomButton(
                      title: 'Login',
                      onPressed: () {
                        if (context
                            .read<LoginCubit>()
                            .loginFormKey
                            .currentState!
                            .validate()) {
                          context.read<LoginCubit>().login();
                        }
                      },
                    ),
                    VerticalSpace(height: 30),
                    Center(
                      child: Text(
                        'OR JOIN OUR COMMUNITY',
                        style: TextStyles.font12.copyWith(
                          fontWeight: FontWeight.w500,
                          color: ColorsManager.lightGray,
                        ),
                        textAlign: TextAlign.center,
                      ),
                    ),
                    VerticalSpace(height: 30),
                    DontHaveAccountText(),
                    LoginBlocListener(),
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
