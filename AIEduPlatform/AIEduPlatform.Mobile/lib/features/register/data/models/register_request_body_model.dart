import 'package:freezed_annotation/freezed_annotation.dart';
part 'register_request_body_model.g.dart';

@JsonSerializable()
class RegisterRequestBodyModel {
  final String email;
  final String userName;
  final String fullName;
  final String password;
  final String confirmPassword;

  RegisterRequestBodyModel({
    required this.email,
    required this.userName,
    required this.fullName,
    required this.password,
    required this.confirmPassword,
  });

  Map<String, dynamic> toJson() => _$RegisterRequestBodyModelToJson(this);
}
