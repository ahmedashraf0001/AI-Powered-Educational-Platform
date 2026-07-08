import 'package:freezed_annotation/freezed_annotation.dart';
part 'generate_mind_map_request_model.g.dart';

@JsonSerializable()
class GenerateMindMapRequestModel {
  final String? centralTopic;
  final int? maxDepth;
  final String? sessionId;

  GenerateMindMapRequestModel({
    required this.centralTopic,
    required this.maxDepth,
    required this.sessionId,
  });

  Map<String, dynamic> toJson() => _$GenerateMindMapRequestModelToJson(this);
}
