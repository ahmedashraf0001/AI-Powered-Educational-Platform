import 'package:graduation_app/features/ai/data/models/chat_message_response_model.dart';
import 'package:graduation_app/features/ai/data/models/send_chat_message_request_model.dart';
import 'package:graduation_app/features/ai/data/networking/chat_api_service.dart';

class ChatRepo {
  final ChatApiService chatApiService;
  ChatRepo({required this.chatApiService});

  Stream<ChatMessageResponseModel> sendChatMessage(
    String sessionId,
    String message,
  ) {
    return chatApiService.sendChatMessage(
      sessionId,
      SendChatMessageRequestModel(message: message),
    );
  }
}
