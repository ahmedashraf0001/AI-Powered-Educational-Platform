import 'package:flutter/cupertino.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/core/networking/api_result.dart';
import 'package:graduation_app/features/register/data/models/register_request_body_model.dart';
import 'package:graduation_app/features/register/data/repo/register_repo.dart';
import 'package:graduation_app/features/register/logic/register_state.dart';

class RegisterCubit extends Cubit<RegisterState> {
  final RegisterRepo registerRepo;
  RegisterCubit(this.registerRepo) : super(RegisterState.initial());

  TextEditingController registerEmail = TextEditingController();
  TextEditingController registerUserName = TextEditingController();
  TextEditingController registerFullName = TextEditingController();
  TextEditingController registerPassword = TextEditingController();
  TextEditingController registerConfirmPassword = TextEditingController();

  final registerFormKey = GlobalKey<FormState>();

  void register() async {
    emit(RegisterState.loading());
    final data = await registerRepo.register(
      RegisterRequestBodyModel(
        email: registerEmail.text,
        userName: registerUserName.text,
        fullName: registerFullName.text,
        password: registerPassword.text,
        confirmPassword: registerConfirmPassword.text,
      ),
    );
    data.when(
      success: (data) {
        emit(RegisterState.success(data.message ?? 'account created.'));
      },
      failure: (error) {
        emit(
          RegisterState.failure(
            message: error.apiErrorModel.message ?? 'error',
          ),
        );
      },
    );
  }
}
