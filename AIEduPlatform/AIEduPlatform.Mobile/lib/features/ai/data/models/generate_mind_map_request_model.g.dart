// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'generate_mind_map_request_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

GenerateMindMapRequestModel _$GenerateMindMapRequestModelFromJson(
  Map<String, dynamic> json,
) => GenerateMindMapRequestModel(
  centralTopic: json['centralTopic'] as String?,
  maxDepth: (json['maxDepth'] as num?)?.toInt(),
  sessionId: json['sessionId'] as String?,
);

Map<String, dynamic> _$GenerateMindMapRequestModelToJson(
  GenerateMindMapRequestModel instance,
) => <String, dynamic>{
  'centralTopic': instance.centralTopic,
  'maxDepth': instance.maxDepth,
  'sessionId': instance.sessionId,
};
