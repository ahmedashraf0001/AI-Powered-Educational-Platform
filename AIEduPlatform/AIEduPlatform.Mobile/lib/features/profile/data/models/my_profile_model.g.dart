// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'my_profile_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

MyProfileModel _$MyProfileModelFromJson(Map<String, dynamic> json) =>
    MyProfileModel(
      profileData: MyProfileData.fromJson(json['data'] as Map<String, dynamic>),
    );

Map<String, dynamic> _$MyProfileModelToJson(MyProfileModel instance) =>
    <String, dynamic>{'data': instance.profileData};

MyProfileData _$MyProfileDataFromJson(Map<String, dynamic> json) =>
    MyProfileData(
      json['bio'] as String?,
      id: json['id'] as String?,
      email: json['email'] as String?,
      userName: json['userName'] as String?,
      firstName: json['firstName'] as String?,
      lastName: json['lastName'] as String?,
      avatarUrl: json['avatarUrl'] as String?,
    );

Map<String, dynamic> _$MyProfileDataToJson(MyProfileData instance) =>
    <String, dynamic>{
      'id': instance.id,
      'email': instance.email,
      'userName': instance.userName,
      'firstName': instance.firstName,
      'lastName': instance.lastName,
      'avatarUrl': instance.avatarUrl,
      'bio': instance.bio,
    };
