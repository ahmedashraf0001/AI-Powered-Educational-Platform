import 'package:graduation_app/core/networking/api_error_handler.dart';
import 'package:graduation_app/core/networking/api_result.dart';
import 'package:graduation_app/core/networking/api_service.dart';
import 'package:graduation_app/features/profile/data/models/get_user_statistics_model.dart';
import 'package:graduation_app/features/profile/data/models/logout_request_model.dart';
import 'package:graduation_app/features/profile/data/models/my_profile_model.dart';
import 'package:graduation_app/features/profile/data/models/update_profile_response_model.dart';

class ProfileRepo {
  final ApiService apiService;

  ProfileRepo({required this.apiService});

  Future<ApiResult<MyProfileData>> getMyProfile() async {
    try {
      final response = await apiService.getMyProfile();
      return ApiResult.success(response.profileData);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

  Future<void> logout(String refreshToken) async {
    await apiService.logout(LogoutRequestModel(refreshToken: refreshToken));
  }

  Future<ApiResult<UserStatisticsData>> getUserStatistics(String userId) async {
    try {
      final response = await apiService.getUserStatistics(userId);
      return ApiResult.success(response.statisticsData);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }

  Future<ApiResult<UpdateProfileResponseModel>> updateMyProfile(
    String? firstName,
    String? lastName,
    String? userName,
    String? bio,
  ) async {
    try {
      final response = await apiService.updateMyProfile(
        firstName,
        lastName,
        userName,
        bio,
      );
      return ApiResult.success(response);
    } catch (error) {
      return ApiResult.failure(ErrorHandler.handle(error));
    }
  }
}
