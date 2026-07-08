
import 'package:graduation_app/core/networking/api_error_handler.dart';
import 'package:graduation_app/core/networking/api_result.dart';
import 'package:graduation_app/core/networking/api_service.dart';
import 'package:graduation_app/features/login/data/models/login_request_body_model.dart';
import 'package:graduation_app/features/login/data/models/login_response_model.dart';

class LoginRepo{
  final ApiService apiService;

  LoginRepo({required this.apiService});

  Future<ApiResult<LoginResponseModel>> login(LoginRequestBodyModel loginRequestModel)async{
    try {
      final response = await apiService.login(loginRequestModel);
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

}