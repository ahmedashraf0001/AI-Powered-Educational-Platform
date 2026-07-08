import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_markdown_plus/flutter_markdown_plus.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/core/helpers/space_helper.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/features/ai/logic/chat_cubit/chat_cubit.dart';
import 'package:graduation_app/features/ai/logic/chat_cubit/chat_state.dart';
import '../../../core/theming/styles.dart';
import 'package:intl/intl.dart';

class AiChatScreen extends StatefulWidget {
  final String sessionId;
  const AiChatScreen({super.key, required this.sessionId});

  @override
  State<AiChatScreen> createState() => _AiChatScreenState();
}

class _AiChatScreenState extends State<AiChatScreen> {
  final TextEditingController _controller = TextEditingController();
  final ScrollController _scrollController = ScrollController();

  final List<ChatMessage> _chatMessages = [];
  String _currentAiResponse = '';
  bool _isAiThinking = false;

  void _scrollToBottom() {
    if (_scrollController.hasClients) {
      _scrollController.animateTo(
        _scrollController.position.maxScrollExtent,
        duration: const Duration(milliseconds: 250),
        curve: Curves.easeOut,
      );
    }
  }

  void _handleSendMessage() {
    final text = _controller.text.trim();
    if (text.isEmpty || _isAiThinking) return;

    final now = DateTime.now();
    final timeString = DateFormat('HH:mm').format(DateTime.now());

    setState(() {
      _chatMessages.add(
        ChatMessage(text: text, isUser: true, time: timeString),
      );
      _isAiThinking = true;
      _currentAiResponse = ''; // Reset streaming frame buffer
    });
    _controller.clear();

    // Smooth post frame jump ensures viewport layout tracks updated entry item
    WidgetsBinding.instance.addPostFrameCallback((_) => _scrollToBottom());
    context.read<ChatCubit>().sendChatMessage(widget.sessionId, text);
  }

  @override
  void dispose() {
    _controller.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: () => FocusScope.of(context).unfocus(),
      child: Scaffold(
        appBar: AppBar(
          title: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            spacing: 4.h, // Adjusted layout padding hierarchy
            children: [
              Row(
                spacing: 7.w,
                children: [
                  Text(
                    'AI Tutor',
                    style: TextStyles.font20.copyWith(
                      color: ColorsManager.mainBlue,
                    ),
                  ),
                  CircleAvatar(
                    radius: 5.r,
                    backgroundColor: ColorsManager.green,
                  ),
                ],
              ),
              Text(
                'LEARNIFY ACTIVE SESSION',
                style: TextStyles.font12.copyWith(
                  fontWeight: FontWeight.w600,
                  color: ColorsManager.customGray,
                ),
              ),
            ],
          ),
        ),
        body: Padding(
          padding: EdgeInsets.symmetric(
            horizontal: 16.w,
          ).copyWith(top: 16.h, bottom: 20.h),
          child: SafeArea(
            child: Column(
              children: [
                const TodayLabel(),
                VerticalSpace(height: 16),
                _chatMessages.isEmpty
                    ? Expanded(
                        child: Center(
                          child: Text(
                            'I\'am always ready, waiting for your signal...',
                            textAlign: TextAlign.center,
                            style: TextStyles.font18.copyWith(
                              color: context.colors.onSurface.withValues(
                                alpha: 0.8,
                              ),
                            ),
                          ),
                        ),
                      )
                    : Expanded(
                        child: BlocConsumer<ChatCubit, ChatState>(
                          listener: (context, state) {
                            state.maybeWhen(
                              successSendChatMessage: (response, fullMessage) {
                                if (response.done == true) {
                                  final now = DateTime.now();
                                  final timeString =
                                      "${now.hour}:${now.minute.toString().padLeft(2, '0')}";

                                  setState(() {
                                    _chatMessages.add(
                                      ChatMessage(
                                        text: fullMessage,
                                        isUser: false,
                                        time: timeString,
                                      ),
                                    );
                                    _currentAiResponse = '';
                                    _isAiThinking = false;
                                  });
                                } else {
                                  setState(() {
                                    _currentAiResponse = fullMessage;
                                  });
                                }
                                _scrollToBottom();
                              },
                              failureSendChatMessage: (errorMessage) {
                                setState(() {
                                  _isAiThinking = false;
                                });
                                ScaffoldMessenger.of(context).showSnackBar(
                                  SnackBar(
                                    content: Text('Error: $errorMessage'),
                                    backgroundColor: Colors.red,
                                  ),
                                );
                              },
                              orElse: () {},
                            );
                          },
                          builder: (context, state) {
                            return ListView.builder(
                              controller: _scrollController,
                              physics: const BouncingScrollPhysics(),
                              itemCount:
                                  _chatMessages.length +
                                  (_isAiThinking ? 1 : 0),
                              itemBuilder: (BuildContext context, int index) {
                                if (index < _chatMessages.length) {
                                  return ChatBubble(
                                    message: _chatMessages[index],
                                  );
                                }

                                // Return the active typewriter streaming preview card row element
                                return ChatBubble(
                                  message: ChatMessage(
                                    text: _currentAiResponse.isEmpty
                                        ? 'Thinking...'
                                        : _currentAiResponse,
                                    isUser: false,
                                    time: 'Now',
                                  ),
                                );
                              },
                            );
                          },
                        ),
                      ),
                ChatInputField(
                  controller: _controller,
                  onSend: _handleSendMessage,
                  isThinking: _isAiThinking,
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class ChatInputField extends StatelessWidget {
  final TextEditingController controller;
  final VoidCallback onSend;
  final bool isThinking;

  const ChatInputField({
    super.key,
    required this.controller,
    required this.onSend,
    required this.isThinking,
  });

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;

    return SafeArea(
      top: false,
      child: Container(
        padding: EdgeInsets.fromLTRB(16.w, 10.h, 16.w, 10.h),
        decoration: BoxDecoration(
          color: colors.surface,
          border: Border(
            top: BorderSide(color: colors.outline.withOpacity(.15)),
          ),
        ),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.end,
          children: [
            Expanded(
              child: AnimatedContainer(
                duration: const Duration(milliseconds: 200),
                decoration: BoxDecoration(
                  color: colors.surfaceContainerHighest.withOpacity(.55),
                  borderRadius: BorderRadius.circular(26.r),
                  border: Border.all(color: colors.outline.withOpacity(.15)),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withOpacity(.03),
                      blurRadius: 10,
                      offset: const Offset(0, 2),
                    ),
                  ],
                ),
                padding: EdgeInsets.symmetric(horizontal: 18.w, vertical: 4.h),
                child: TextFormField(
                  controller: controller,
                  minLines: 1,
                  maxLines: 4,
                  keyboardType: TextInputType.multiline,
                  cursorColor: colors.primary,
                  style: TextStyles.font14.copyWith(color: colors.onSurface),
                  decoration: InputDecoration(
                    hintText: "Ask Learnify AI anything...",
                    hintStyle: TextStyles.font14.copyWith(
                      color: colors.onSurfaceVariant,
                    ),
                    border: InputBorder.none,
                    isDense: true,
                    contentPadding: EdgeInsets.symmetric(vertical: 12.h),
                  ),
                ),
              ),
            ),

            SizedBox(width: 10.w),

            AnimatedContainer(
              duration: const Duration(milliseconds: 200),
              width: 48.w,
              height: 48.w,
              decoration: BoxDecoration(
                color: isThinking
                    ? colors.surfaceContainerHighest
                    : colors.primary,
                shape: BoxShape.circle,
                boxShadow: [
                  if (!isThinking)
                    BoxShadow(
                      color: colors.primary.withOpacity(.35),
                      blurRadius: 14,
                      offset: const Offset(0, 4),
                    ),
                ],
              ),
              child: Material(
                color: Colors.transparent,
                child: InkWell(
                  borderRadius: BorderRadius.circular(100),
                  onTap: isThinking ? null : onSend,
                  child: Center(
                    child: AnimatedSwitcher(
                      duration: const Duration(milliseconds: 250),
                      child: isThinking
                          ? SizedBox(
                              key: const ValueKey("loading"),
                              width: 18.w,
                              height: 18.w,
                              child: LinearProgressIndicator(
                                color: ColorsManager.mainBlue,
                                valueColor: AlwaysStoppedAnimation(
                                  colors.onSurfaceVariant,
                                ),
                              ),
                            )
                          : Icon(
                              Icons.arrow_upward_rounded,
                              key: const ValueKey("send"),
                              color: colors.onPrimary,
                              size: 22.sp,
                            ),
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class TodayLabel extends StatelessWidget {
  const TodayLabel({super.key});

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: double.infinity,
      child: Center(
        child: Text(
          'Today',
          style: TextStyles.font14.copyWith(
            fontWeight: FontWeight.w500,
            color: ColorsManager.darkGray,
          ),
        ),
      ),
    );
  }
}

class ChatBubble extends StatelessWidget {
  final ChatMessage message;
  const ChatBubble({super.key, required this.message});

  @override
  Widget build(BuildContext context) {
    final isUser = message.isUser;

    return Padding(
      padding: EdgeInsets.only(bottom: 10.h),
      child: Column(
        crossAxisAlignment: isUser
            ? CrossAxisAlignment.end
            : CrossAxisAlignment.start,
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            mainAxisAlignment: isUser
                ? MainAxisAlignment.end
                : MainAxisAlignment.start,
            children: [
              if (!isUser) ...[
                CircleAvatar(
                  radius: 16.r,
                  backgroundColor: const Color(0xffEEF3FF),
                  child: Icon(
                    Icons.smart_toy_outlined,
                    color: ColorsManager.mainBlue,
                    size: 18.r,
                  ),
                ),
                HorizontalSpace(width: 8),
              ],
              Flexible(
                child: Container(
                  padding: EdgeInsets.symmetric(
                    horizontal: 16.w,
                    vertical: 12.h,
                  ),
                  decoration: BoxDecoration(
                    color: isUser
                        ? const Color(0xff1F6BFF)
                        : const Color(0xffF1F2F6),
                    borderRadius: BorderRadius.only(
                      topLeft: Radius.circular(18.r),
                      topRight: Radius.circular(18.r),
                      bottomLeft: isUser
                          ? Radius.circular(18.r)
                          : Radius.circular(4.r),
                      bottomRight: isUser
                          ? Radius.circular(4.r)
                          : Radius.circular(18.r),
                    ),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withOpacity(0.02),
                        blurRadius: 6,
                        offset: const Offset(0, 2),
                      ),
                    ],
                  ),
                  child: isUser
                      ? Text(
                          message.text,
                          style: TextStyle(
                            height: 1.4,
                            fontSize: 15.sp,
                            color: isUser
                                ? ColorsManager.white
                                : ColorsManager.black,
                          ),
                        )
                      : MarkdownBody(
                          data: message.text,
                          selectable: true,
                          styleSheet: MarkdownStyleSheet(
                            p: TextStyle(
                              fontSize: 15.sp,
                              height: 1.5,
                              color: Colors.black,
                            ),
                            h1: TextStyle(
                              fontSize: 24.sp,
                              fontWeight: FontWeight.bold,
                            ),
                            h2: TextStyle(
                              fontSize: 20.sp,
                              fontWeight: FontWeight.bold,
                            ),
                            h3: TextStyle(
                              fontSize: 18.sp,
                              fontWeight: FontWeight.bold,
                            ),
                            code: TextStyle(
                              fontFamily: 'monospace',
                              fontSize: 14.sp,
                            ),
                            codeblockDecoration: BoxDecoration(
                              color: Colors.grey.shade200,
                              borderRadius: BorderRadius.circular(8),
                            ),
                            listBullet: TextStyle(
                              fontSize: 15.sp,
                              color: Colors.black,
                            ),
                          ),
                        ),
                ),
              ),
            ],
          ),
          SizedBox(height: 4.h),
          Padding(
            padding: EdgeInsets.only(
              left: isUser ? 0.w : 44.w,
              right: isUser ? 8.w : 0.w,
            ),
            child: Text(
              message.time,
              style: TextStyle(color: Colors.grey.shade500, fontSize: 11.sp),
            ),
          ),
        ],
      ),
    );
  }
}

class ChatMessage {
  final String text;

  final bool isUser;

  final String time;

  ChatMessage({required this.text, required this.isUser, required this.time});
}
