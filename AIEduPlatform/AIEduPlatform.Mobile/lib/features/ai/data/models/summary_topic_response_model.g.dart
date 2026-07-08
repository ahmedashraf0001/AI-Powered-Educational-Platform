// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'summary_topic_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

SummaryTopicResponseModel _$SummaryTopicResponseModelFromJson(
  Map<String, dynamic> json,
) => SummaryTopicResponseModel(
  summaryData: json['data'] == null
      ? null
      : SummaryDataModel.fromJson(json['data'] as Map<String, dynamic>),
);

Map<String, dynamic> _$SummaryTopicResponseModelToJson(
  SummaryTopicResponseModel instance,
) => <String, dynamic>{'data': instance.summaryData};

SummaryDataModel _$SummaryDataModelFromJson(Map<String, dynamic> json) =>
    SummaryDataModel(
      summary: json['summary'] as String?,
      keyPoints: (json['keyPoints'] as List<dynamic>?)
          ?.map((e) => e as String)
          .toList(),
      keyTerms: (json['keyTerms'] as Map<String, dynamic>?)?.map(
        (k, e) => MapEntry(k, e as String),
      ),
      sourceTitle: json['sourceTitle'] as String?,
      originalLength: json['originalLength'] as String?,
      summaryLength: json['summaryLength'] as String?,
    );

Map<String, dynamic> _$SummaryDataModelToJson(SummaryDataModel instance) =>
    <String, dynamic>{
      'summary': instance.summary,
      'keyPoints': instance.keyPoints,
      'keyTerms': instance.keyTerms,
      'sourceTitle': instance.sourceTitle,
      'originalLength': instance.originalLength,
      'summaryLength': instance.summaryLength,
    };
