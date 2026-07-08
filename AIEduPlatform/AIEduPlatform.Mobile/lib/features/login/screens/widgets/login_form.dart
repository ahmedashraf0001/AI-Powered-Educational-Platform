import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/core/helpers/app_regex.dart';
import 'package:graduation_app/features/login/logic/login_cubit.dart';

import '../../../../core/helpers/space_helper.dart';
import '../../../../core/theming/styles.dart';
import '../../../../core/widgets/custom_text_field.dart';

class LoginForm extends StatefulWidget {
  const LoginForm({super.key});

  @override
  State<LoginForm> createState() => _LoginFormState();
}

class _LoginFormState extends State<LoginForm> {
  bool isObscureText = false;

  late TextEditingController passwordController;

  @override
  void initState() {
    super.initState();
    passwordController = context.read<LoginCubit>().loginPasswordController;
  }

  @override
  Widget build(BuildContext context) {
    return Form(
      key: context.read<LoginCubit>().loginFormKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Email',
            style: TextStyles.font16.copyWith(fontWeight: FontWeight.w500),
          ),
          VerticalSpace(height: 10),
          CustomTextField(
            hintText: 'alex.smith@university.edu',
            controller: context.read<LoginCubit>().loginEmailController,
            validator: (v) {
              if (v == null || v.isEmpty || !AppRegex.isEmailValid(v)) {
                return 'Please enter a valid email.';
              }
            },
          ),
          VerticalSpace(height: 25),
          Text(
            'Password',
            style: TextStyles.font16.copyWith(fontWeight: FontWeight.w500),
          ),
          VerticalSpace(height: 10),
          CustomTextField(
            hintText: 'password123',
            controller: context.read<LoginCubit>().loginPasswordController,
            isObscureText: isObscureText,
            suffixIcon: IconButton(
              onPressed: () {
                setState(() {
                  isObscureText = !isObscureText;
                });
              },
              icon: !isObscureText
                  ? Icon(Icons.visibility)
                  : Icon(Icons.visibility_off),
            ),
            validator: (v) {
              if (v == null || v.isEmpty) {
                return 'Please enter a valid password.';
              }
            },
          ),
        ],
      ),
    );
  }

  @override
  dispose() {
    passwordController.dispose();
    super.dispose();
  }
}
