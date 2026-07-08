// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'summary_topic_request_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

SummaryTopicRequestModel _$SummaryTopicRequestModelFromJson(
  Map<String, dynamic> json,
) => SummaryTopicRequestModel(
  topic: json['topic'] as String?,
  summaryLength: (json['summaryLength'] as num?)?.toInt(),
  includeKeyPoints: json['includeKeyPoints'] as bool?,
);

Map<String, dynamic> _$SummaryTopicRequestModelToJson(
  SummaryTopicRequestModel instance,
) => <String, dynamic>{
  'topic': instance.topic,
  'summaryLength': instance.summaryLength,
  'includeKeyPoints': instance.includeKeyPoints,
};
