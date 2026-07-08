// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'register_request_body_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

RegisterRequestBodyModel _$RegisterRequestBodyModelFromJson(
  Map<String, dynamic> json,
) => RegisterRequestBodyModel(
  email: json['email'] as String,
  userName: json['userName'] as String,
  fullName: json['fullName'] as String,
  password: json['password'] as String,
  confirmPassword: json['confirmPassword'] as String,
);

Map<String, dynamic> _$RegisterRequestBodyModelToJson(
  RegisterRequestBodyModel instance,
) => <String, dynamic>{
  'email': instance.email,
  'userName': instance.userName,
  'fullName': instance.fullName,
  'password': instance.password,
  'confirmPassword': instance.confirmPassword,
};
