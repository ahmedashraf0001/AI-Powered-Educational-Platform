import 'package:json_annotation/json_annotation.dart';
part 'update_profile_request_model.g.dart';

@JsonSerializable()
class UpdateProfileRequestModel {
  final String? firstName;
  final String? lastName;
  final String? userName;
  final String? bio;

  UpdateProfileRequestModel(
    this.firstName,
    this.lastName,
    this.userName,
    this.bio,
  );

  Map<String, dynamic> toJson() => _$UpdateProfileRequestModelToJson(this);
}
