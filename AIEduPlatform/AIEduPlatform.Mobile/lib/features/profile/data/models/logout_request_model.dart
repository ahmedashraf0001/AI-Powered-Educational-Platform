import 'package:json_annotation/json_annotation.dart';

part 'logout_request_model.g.dart';

@JsonSerializable()
class LogoutRequestModel {
  final String refreshToken;

  LogoutRequestModel({required this.refreshToken});
  Map<String, dynamic> toJson() => _$LogoutRequestModelToJson(this);
}
