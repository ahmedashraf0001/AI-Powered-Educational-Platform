import 'package:json_annotation/json_annotation.dart';

part 'summary_topic_request_model.g.dart';

@JsonSerializable()
class SummaryTopicRequestModel {
  final String? topic;
  final int? summaryLength;
  final bool? includeKeyPoints;

  SummaryTopicRequestModel({
    required this.topic,
    required this.summaryLength,
    required this.includeKeyPoints,
  });
  Map<String, dynamic> toJson() => _$SummaryTopicRequestModelToJson(this);
}
