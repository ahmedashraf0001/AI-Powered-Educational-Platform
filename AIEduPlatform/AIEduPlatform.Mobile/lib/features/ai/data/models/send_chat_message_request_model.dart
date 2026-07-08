import 'package:freezed_annotation/freezed_annotation.dart';
part 'send_chat_message_request_model.g.dart';

@JsonSerializable()
class SendChatMessageRequestModel {
  final String? message;

  SendChatMessageRequestModel({required this.message});

  Map<String, dynamic> toJson() => _$SendChatMessageRequestModelToJson(this);
}
