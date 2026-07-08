import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:graduation_app/features/ai/data/models/chat_message_response_model.dart';

part 'chat_state.freezed.dart';

@freezed
class ChatState<T> with _$ChatState<T> {
  const factory ChatState.initial() = _Initial;
  const factory ChatState.loadingSendChatMessage() = LoadingSendChatMessage;
  const factory ChatState.successSendChatMessage(
    ChatMessageResponseModel response,
    String fullMessage,
  ) = SuccessSendChatMessage;
  const factory ChatState.failureSendChatMessage({String? message}) =
      FailureSendChatMessage;
}
