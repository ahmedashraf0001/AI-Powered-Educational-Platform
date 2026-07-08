import 'dart:async';

import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:graduation_app/features/ai/data/repo/chat_repo.dart';
import 'package:graduation_app/features/ai/logic/chat_cubit/chat_state.dart';

class ChatCubit extends Cubit<ChatState> {
  final ChatRepo aiRepo;
  ChatCubit(this.aiRepo) : super(ChatState.initial());

  StreamSubscription? _subscription;

  Future<void> sendChatMessage(String sessionId, String message) async {
    String fullMessage = '';

    // cancel previous stream if user sends another message
    await _subscription?.cancel();

    emit(const ChatState.loadingSendChatMessage());

    _subscription = aiRepo
        .sendChatMessage(sessionId, message)
        .listen(
          (chunk) {
            fullMessage += chunk.content ?? '';

            emit(ChatState.successSendChatMessage(chunk, fullMessage));

            // stop stream when backend signals completion
            if (chunk.done == true) {
              _subscription?.cancel();
            }
          },
          onError: (e) {
            emit(ChatState.failureSendChatMessage(message: e.toString()));
          },
          onDone: () {
            _subscription = null;
          },
          cancelOnError: true,
        );
  }
}
