
import 'package:graduation_app/core/networking/api_error_handler.dart';
import 'package:graduation_app/core/networking/api_result.dart';
import 'package:graduation_app/core/networking/api_service.dart';
import 'package:graduation_app/features/register/data/models/register_request_body_model.dart';
import 'package:graduation_app/features/register/data/models/register_response_model.dart';

class RegisterRepo{
  final ApiService apiService;

  RegisterRepo({required this.apiService});

  Future<ApiResult<RegisterResponseModel>> register(RegisterRequestBodyModel registerRequestModel)async{
    try {
      final response = await apiService.register(registerRequestModel);
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

}