// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'chat_message_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

ChatMessageResponseModel _$ChatMessageResponseModelFromJson(
  Map<String, dynamic> json,
) => ChatMessageResponseModel(
  content: json['content'] as String?,
  done: json['done'] as bool?,
);

Map<String, dynamic> _$ChatMessageResponseModelToJson(
  ChatMessageResponseModel instance,
) => <String, dynamic>{'content': instance.content, 'done': instance.done};
