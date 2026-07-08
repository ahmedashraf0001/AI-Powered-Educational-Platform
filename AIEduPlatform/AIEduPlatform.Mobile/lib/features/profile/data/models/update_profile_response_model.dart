import 'package:freezed_annotation/freezed_annotation.dart';
part 'update_profile_response_model.g.dart';

@JsonSerializable()
class UpdateProfileResponseModel {
  final String? message;

  UpdateProfileResponseModel({required this.message});

  factory UpdateProfileResponseModel.fromJson(Map<String, dynamic> json) =>
      _$UpdateProfileResponseModelFromJson(json);
}
