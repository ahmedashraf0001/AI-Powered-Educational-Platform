import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:graduation_app/features/profile/data/models/get_user_statistics_model.dart';
import 'package:graduation_app/features/profile/data/models/my_profile_model.dart';

part 'profile_state.freezed.dart';

@freezed
class ProfileState<T> with _$ProfileState<T> {
  const factory ProfileState.initial() = _Initial;

  const factory ProfileState.loadingMyProfile() = LoadingMyProfile;

  const factory ProfileState.successMyProfile(MyProfileData profileData) =
      SuccessMyProfile;

  const factory ProfileState.failureMyProfile({String? message}) =
      FailureMyProfile;

  const factory ProfileState.loadingGetUserStatistics() =
      LoadingGetUserStatistics;
  const factory ProfileState.successGetUserStatistics(
    UserStatisticsData userStatistics,
  ) = SuccessGetUserStatistics;
  const factory ProfileState.failureGetUserStatistics({String? message}) =
      FailureGetUserStatistics;

  const factory ProfileState.loadingLogout() = LoadingLogout;
  const factory ProfileState.successLogout(T data) = SuccessLogout<T>;
  const factory ProfileState.failureLogout({String? message}) = FailureLogout;

  const factory ProfileState.loadingUpdateProfile() = LoadingUpdateProfile;
  const factory ProfileState.successUpdateProfile(String? message) =
      SuccesUpdateProfile;
  const factory ProfileState.failureUpdateProfile({String? message}) =
      FailureUpdateProfile;
}
