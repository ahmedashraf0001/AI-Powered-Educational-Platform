// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'update_profile_request_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

UpdateProfileRequestModel _$UpdateProfileRequestModelFromJson(
  Map<String, dynamic> json,
) => UpdateProfileRequestModel(
  json['firstName'] as String?,
  json['lastName'] as String?,
  json['userName'] as String?,
  json['bio'] as String?,
);

Map<String, dynamic> _$UpdateProfileRequestModelToJson(
  UpdateProfileRequestModel instance,
) => <String, dynamic>{
  'firstName': instance.firstName,
  'lastName': instance.lastName,
  'userName': instance.userName,
  'bio': instance.bio,
};
