import 'package:freezed_annotation/freezed_annotation.dart';
part 'chat_message_response_model.g.dart';

@JsonSerializable()
class ChatMessageResponseModel {
  final String? content;
  final bool? done;

  ChatMessageResponseModel({required this.content, required this.done});

  factory ChatMessageResponseModel.fromJson(Map<String, dynamic> json) =>
      _$ChatMessageResponseModelFromJson(json);
}
