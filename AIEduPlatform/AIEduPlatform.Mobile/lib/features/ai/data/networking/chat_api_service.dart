import 'dart:convert';
import 'package:dio/dio.dart';
import 'package:graduation_app/core/networking/dio_factory.dart';
import 'package:graduation_app/features/ai/data/models/chat_message_response_model.dart';
import 'package:graduation_app/features/ai/data/models/send_chat_message_request_model.dart';

class ChatApiService {
  final Dio _dio = DioFactory.getDio();

  Stream<ChatMessageResponseModel> sendChatMessage(
    String sessionId,
    SendChatMessageRequestModel sendChatMessageRequestModel,
  ) async* {
    final response = await _dio.post<ResponseBody>(
      'study-sessions/$sessionId/chat',
      data: sendChatMessageRequestModel.toJson(),
      options: Options(
        responseType: ResponseType.stream,
        extra: {'skipLogging': true},
      ),
    );

    // Using explicit streams ensures packet splits don't break string values
    final stream = response.data!.stream
        .cast<List<int>>()
        .transform(utf8.decoder)
        .transform(const LineSplitter());

    await for (final line in stream) {
      final trimmed = line.trim();
      if (trimmed.isEmpty) continue;

      // 💡 Crucial SSE Check: Skip keep-alive comments sent by servers
      if (trimmed.startsWith(':')) continue;

      String jsonString = trimmed;

      // 💡 Safe Prefix Stripping: If backend uses SSE protocol formatting, clean it up!
      if (trimmed.startsWith('data:')) {
        jsonString = trimmed.substring(5).trim();
      }

      if (jsonString.isEmpty) continue;

      try {
        final json = jsonDecode(jsonString);

        final model = ChatMessageResponseModel.fromJson(json);

        yield model;
      } catch (e) {
        continue;
      }
    }
  }
}
