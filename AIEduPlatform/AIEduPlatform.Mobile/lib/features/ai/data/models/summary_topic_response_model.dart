import 'package:json_annotation/json_annotation.dart';
part 'summary_topic_response_model.g.dart';

@JsonSerializable()
class SummaryTopicResponseModel {
  @JsonKey(name: 'data')
  final SummaryDataModel? summaryData;

  SummaryTopicResponseModel({required this.summaryData});
  factory SummaryTopicResponseModel.fromJson(Map<String, dynamic> json) =>
      _$SummaryTopicResponseModelFromJson(json);
}

@JsonSerializable()
class SummaryDataModel {
  final String? summary;
  final List<String>? keyPoints;
  final Map<String, String>? keyTerms;
  final String? sourceTitle;
  final String? originalLength;
  final String? summaryLength;

  SummaryDataModel({
    required this.summary,
    required this.keyPoints,
    required this.keyTerms,
    required this.sourceTitle,
    required this.originalLength,
    required this.summaryLength,
  });

  factory SummaryDataModel.fromJson(Map<String, dynamic> json) =>
      _$SummaryDataModelFromJson(json);
}
