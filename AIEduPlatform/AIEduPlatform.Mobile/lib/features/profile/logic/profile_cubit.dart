import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/core/helpers/secure_storage_helper.dart';
import 'package:graduation_app/core/networking/api_constants.dart';
import 'package:graduation_app/core/networking/api_result.dart';
import 'package:graduation_app/features/profile/data/repo/profile_repo.dart';
import 'package:graduation_app/features/profile/logic/profile_state.dart';

import '../../../core/di/dependency_injection.dart';
import '../../../core/networking/api_error_handler.dart';

class ProfileCubit extends Cubit<ProfileState> {
  final ProfileRepo profileRepo;
  ProfileCubit(this.profileRepo) : super(ProfileState.initial());

  String? userId = '';

  getMyProfile() async {
    emit(LoadingMyProfile());
    final data = await profileRepo.getMyProfile();
    return data.when(
      success: (profile) async {
        userId = profile.id;
        emit(SuccessMyProfile(profile));
        await getUserStatistics();
      },
      failure: (failure) {
        emit(
          FailureMyProfile(message: failure.apiErrorModel.message ?? 'error'),
        );
      },
    );
  }

  getUserStatistics() async {
    emit(LoadingGetUserStatistics());
    final data = await profileRepo.getUserStatistics(userId ?? '');
    return data.when(
      success: (userStatistics) {
        emit(SuccessGetUserStatistics(userStatistics));
      },
      failure: (failure) {
        emit(FailureGetUserStatistics(message: failure.apiErrorModel.message));
      },
    );
  }

  Future<void> logout() async {
    emit(LoadingLogout());

    try {
      final refreshToken = await getIt<SecureStorageHelper>().getToken(
        key: ApiKeys.refreshToken,
      );

      if (refreshToken == null) {
        emit(FailureLogout(message: 'Refresh token not found'));
        return;
      }

      await profileRepo.logout(refreshToken);

      await getIt<SecureStorageHelper>().clearAllTokens();

      emit(SuccessLogout('Logout done'));
    } catch (error) {
      final apiError = ErrorHandler.handle(error);

      emit(FailureLogout(message: apiError.apiErrorModel.message ?? 'error'));
    }
  }

  Future updateMyProfile(
    String? firstName,
    String? lastName,
    String? userName,
    String? bio,
  ) async {
    emit(LoadingUpdateProfile());
    final data = await profileRepo.updateMyProfile(
      firstName,
      lastName,
      userName,
      bio,
    );
    return data.when(
      success: (response) async {
        await getMyProfile();
        emit(SuccesUpdateProfile(response.message ?? 'profile updated.'));
      },
      failure: (error) {
        emit(FailureUpdateProfile(message: error.apiErrorModel.message));
      },
    );
  }
}
