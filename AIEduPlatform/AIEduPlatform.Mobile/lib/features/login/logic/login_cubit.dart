import 'package:flutter/cupertino.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/core/helpers/secure_storage_helper.dart';
import 'package:graduation_app/core/networking/api_result.dart';
import 'package:graduation_app/features/login/data/models/login_request_body_model.dart';
import '../../../core/di/dependency_injection.dart';
import '../data/repo/login_repo.dart';
import 'login_state.dart';

class LoginCubit extends Cubit<LoginState> {
  final LoginRepo loginRepo;
  LoginCubit(this.loginRepo) : super(LoginState.initial());


   TextEditingController loginEmailController =TextEditingController() ;
   TextEditingController loginPasswordController =TextEditingController() ;
   final loginFormKey = GlobalKey<FormState>();


  Future<void> login() async {
    emit(LoginState.loading());

    final data = await loginRepo.login(
      LoginRequestBodyModel(
        email: loginEmailController.text,
        password: loginPasswordController.text,
      ),
    );

    data.when(
      success: (success) async {
        final accessToken = success.data?.accessToken;
        final refreshToken = success.data?.refreshToken;

        final storage = getIt<SecureStorageHelper>();

        if (accessToken == null || refreshToken == null) {
          emit(LoginState.failure(message: "Missing tokens from server"));
          return;
        }

        await Future.wait([
          storage.saveToken(accessToken),
          storage.saveRefreshToken(refreshToken),
        ]);

        emit(LoginState.success(success.data));
      },
      failure: (failure) {
        emit(LoginState.failure(message: failure.apiErrorModel.message));
      },
    );
  }
}
