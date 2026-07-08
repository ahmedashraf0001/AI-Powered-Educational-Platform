import 'package:freezed_annotation/freezed_annotation.dart';
part 'start_study_session_response_model.g.dart';

@JsonSerializable()
class StartSessionResponseModel {
  final String message;
  @JsonKey(name: 'data')
  final SessionData sessionData;

  StartSessionResponseModel({required this.message, required this.sessionData});

  factory StartSessionResponseModel.fromJson(Map<String, dynamic> json) =>
      _$StartSessionResponseModelFromJson(json);
}

@JsonSerializable()
class SessionData {
  final String sessionId;

  SessionData({required this.sessionId});

  factory SessionData.fromJson(Map<String, dynamic> json) =>
      _$SessionDataFromJson(json);
}
