import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/core/helpers/app_regex.dart';
import 'package:graduation_app/features/register/logic/register_cubit.dart';
import '../../../../core/helpers/space_helper.dart';
import '../../../../core/theming/styles.dart';
import '../../../../core/widgets/custom_text_field.dart';

class RegisterForm extends StatefulWidget {
  const RegisterForm({super.key});

  @override
  State<RegisterForm> createState() => _RegisterFormState();
}

class _RegisterFormState extends State<RegisterForm> {
  bool isObscureText = false;
  bool confirmIsObscureText = false;

  late TextEditingController passwordController;

  @override
  void initState() {
    super.initState();
    passwordController = context.read<RegisterCubit>().registerPassword;
  }

  @override
  Widget build(BuildContext context) {
    return Form(
      key: context.read<RegisterCubit>().registerFormKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Full Name',style: TextStyles.font16.copyWith(fontWeight: FontWeight.w500),),
          VerticalSpace(height: 10),
          CustomTextField(
              hintText: 'alex scholar',
              controller: context.read<RegisterCubit>().registerFullName,
              validator: (v){
            if(v == null || v.isEmpty){
              return 'Please enter a valid name.';
            }
          }),
          VerticalSpace(height: 12),
          Text('Username',style: TextStyles.font16.copyWith(fontWeight: FontWeight.w500),),
          VerticalSpace(height: 10),
          CustomTextField(
              hintText: 'alex89',
              controller: context.read<RegisterCubit>().registerUserName,
              validator: (v){
            if(v == null || v.isEmpty){
              return 'Please enter a valid name.';
            }
          }),
          VerticalSpace(height: 12),
          Text('Email',style: TextStyles.font16.copyWith(fontWeight: FontWeight.w500),),
          VerticalSpace(height: 10),
          CustomTextField(
              hintText: 'alex.smith@university.edu',
              controller: context.read<RegisterCubit>().registerEmail,
              validator: (v){
                if(v == null || v.isEmpty || !AppRegex.isEmailValid(v)){
                  return 'Please enter a valid email.';
                }
              }
          ),
          VerticalSpace(height: 12),
          Text('Password',style: TextStyles.font16.copyWith(fontWeight: FontWeight.w500),),
          VerticalSpace(height: 10),
          CustomTextField(
              hintText: 'password123',
              isObscureText: isObscureText,
              controller: context.read<RegisterCubit>().registerPassword,
              suffixIcon: IconButton(
                onPressed: (){
                  setState(() {
                    isObscureText = !isObscureText;
                  });
                }, icon: !isObscureText ? Icon(Icons.visibility) : Icon(Icons.visibility_off) ,
              ),
              validator: (v){
                if(v == null || v.isEmpty){
                  return 'Please enter a valid password.';
                }
              }
          ),
          VerticalSpace(height: 12),
          Text('Confirm Password',style: TextStyles.font16.copyWith(fontWeight: FontWeight.w500),),
          VerticalSpace(height: 10),
          CustomTextField(
              hintText: 'password123',
              isObscureText: confirmIsObscureText,
              controller: context.read<RegisterCubit>().registerConfirmPassword,
              suffixIcon: IconButton(
                onPressed: (){
                  setState(() {
                    confirmIsObscureText = !confirmIsObscureText;
                  });
                }, icon: !confirmIsObscureText ? Icon(Icons.visibility) : Icon(Icons.visibility_off) ,
              ),
              validator: (v){
                if(v == null || v.isEmpty){
                  return 'Please enter a valid password.';
                }
                else if (v != passwordController.text){
                  return 'Password doesn\'t match.';
                }
              }
          ),

        ],
      ),
    );

  }
  @override
  void dispose() {
    passwordController.dispose();
    super.dispose();
  }
}

