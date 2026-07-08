import 'package:freezed_annotation/freezed_annotation.dart';
part 'start_study_session_request_model.g.dart';

@JsonSerializable()
class StartStudySessionRequestModel {
  final String courseId;

  StartStudySessionRequestModel({required this.courseId});

  Map<String, dynamic> toJson() => _$StartStudySessionRequestModelToJson(this);
}
