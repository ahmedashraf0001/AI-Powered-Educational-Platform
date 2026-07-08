// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'start_study_session_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

StartSessionResponseModel _$StartSessionResponseModelFromJson(
  Map<String, dynamic> json,
) => StartSessionResponseModel(
  message: json['message'] as String,
  sessionData: SessionData.fromJson(json['data'] as Map<String, dynamic>),
);

Map<String, dynamic> _$StartSessionResponseModelToJson(
  StartSessionResponseModel instance,
) => <String, dynamic>{
  'message': instance.message,
  'data': instance.sessionData,
};

SessionData _$SessionDataFromJson(Map<String, dynamic> json) =>
    SessionData(sessionId: json['sessionId'] as String);

Map<String, dynamic> _$SessionDataToJson(SessionData instance) =>
    <String, dynamic>{'sessionId': instance.sessionId};
