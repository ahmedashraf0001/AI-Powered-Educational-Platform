import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/di/dependency_injection.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/core/helpers/space_helper.dart';
import 'package:graduation_app/core/services/navigation/navigation_service.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/features/ai/logic/chat_cubit/chat_cubit.dart';
import 'package:graduation_app/features/ai/logic/ai_services_cubit/cubit/ai_services_cubit.dart';
import 'package:graduation_app/features/ai/screens/ai_chat_screen.dart';
import 'package:graduation_app/features/ai/screens/ai_summary_screen.dart';
import 'package:graduation_app/features/ai/screens/flash_cards_screen.dart';
import 'package:graduation_app/features/ai/screens/mind_maps_screen.dart';
import 'package:graduation_app/features/ai/screens/quiz_screen.dart';
import 'package:graduation_app/features/courses/logic/courses_cubit.dart';
import 'package:graduation_app/features/courses/logic/courses_state.dart';
import 'package:graduation_app/features/courses/screens/Widgets/course_learning_custom_button.dart';
import 'package:graduation_app/features/courses/screens/Widgets/lesson_materials_bloc_builder.dart';
import 'package:youtube_player_flutter/youtube_player_flutter.dart';

class CourseLearningScreenBody extends StatefulWidget {
  final String? courseId;
  final String? courseTitle;
  const CourseLearningScreenBody({super.key, this.courseId, this.courseTitle});

  @override
  State<CourseLearningScreenBody> createState() =>
      _CourseLearningScreenBodyState();
}

class _CourseLearningScreenBodyState extends State<CourseLearningScreenBody> {
  late YoutubePlayerController controller;

  bool _isFullScreen = false;

  @override
  void initState() {
    super.initState();
    context.read<CoursesCubit>().getCourseLectures(widget.courseId ?? '');

    final videoId = YoutubePlayer.convertUrlToId(
      'https://youtu.be/6Jubl1UnJTE?si=EJcLPFrxis7hBlKs',
    );
    controller =
        YoutubePlayerController(
          initialVideoId: videoId!,
          flags: const YoutubePlayerFlags(
            autoPlay: false,
            mute: false,
            enableCaption: true,
          ),
        )..addListener(() {
          if (controller.value.isFullScreen) {
            controller.toggleFullScreenMode();
          }
        });
  }

  @override
  void dispose() {
    controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: EdgeInsets.symmetric(horizontal: 16.w, vertical: 16.h),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          ClipRRect(
            borderRadius: _isFullScreen
                ? BorderRadius.zero
                : BorderRadius.circular(24.r),
            child: SizedBox(
              width: double.infinity,
              child: YoutubePlayer(
                controller: controller,
                showVideoProgressIndicator: true,
                progressIndicatorColor: Colors.red,
                bottomActions: [
                  CurrentPosition(),
                  ProgressBar(isExpanded: true),
                  RemainingDuration(),
                ],
              ),
            ),
          ),

          VerticalSpace(height: 24),
          BlocListener<CoursesCubit, CoursesState>(
            listener: (context, state) {
              state.whenOrNull(
                successStartSession: (response) {
                  showAiToolsBottomSheet(
                    context,
                    onChat: () {
                      NavigationService.instance.navigateTo(
                        BlocProvider(
                          create: (context) => getIt<ChatCubit>(),
                          child: AiChatScreen(
                            sessionId: response.sessionData.sessionId,
                          ),
                        ),
                      );
                    },
                    onSummary: () {
                      NavigationService.instance.navigateTo(
                        BlocProvider(
                          create: (context) => getIt<AiServicesCubit>(),
                          child: AiSummaryScreen(
                            sessionId: response.sessionData.sessionId,
                            courseName: widget.courseTitle ?? '',
                          ),
                        ),
                      );
                    },
                    onFlashcards: () {
                      NavigationService.instance.navigateTo(
                        BlocProvider(
                          create: (context) => getIt<AiServicesCubit>(),
                          child: FlashCardsScreen(
                            sessionId: response.sessionData.sessionId,
                            courseName: widget.courseTitle ?? '',
                          ),
                        ),
                      );
                    },
                    onQuiz: () {
                      NavigationService.instance.navigateTo(
                        BlocProvider(
                          create: (context) => getIt<AiServicesCubit>(),
                          child: QuizScreen(
                            sessionId: response.sessionData.sessionId,
                            courseName: widget.courseTitle ?? '',
                          ),
                        ),
                      );
                    },
                    onMindMap: () {
                      NavigationService.instance.navigateTo(
                        BlocProvider(
                          create: (context) => getIt<AiServicesCubit>(),
                          child: MindMapScreen(
                            sessionId: response.sessionData.sessionId,
                            courseName: widget.courseTitle ?? '',
                          ),
                        ),
                      );
                    },
                  );
                },
              );
            },
            child: Row(
              spacing: 12.w,
              children: [
                Expanded(
                  child: CourseLearningCustomButton(
                    onTap: () async {
                      await context.read<CoursesCubit>().startStudySession(
                        widget.courseId ?? '',
                      );
                    },
                    iconName: 'assets/svgs/stars.svg',
                    title: 'Study With Ai',
                    color: ColorsManager.lightBlue,
                    textColor: ColorsManager.mainBlue,
                    iconColor: ColorsManager.mainBlue,
                  ),
                ),
                /* Expanded(
                  child: CourseLearningCustomButton(
                    iconName: 'assets/svgs/course.svg',
                    title: 'Take Exam',
                    color: ColorsManager.mainBlue,
                    iconColor: ColorsManager.white,
                  ),
                ),*/
              ],
            ),
          ),
          VerticalSpace(height: 35),
          Expanded(child: LessonMaterialsBlocBuilder()),
        ],
      ),
    );
  }
}

void showAiToolsBottomSheet(
  BuildContext context, {
  required VoidCallback onChat,
  required VoidCallback onSummary,
  required VoidCallback onFlashcards,
  required VoidCallback onQuiz,
  required VoidCallback onMindMap,
}) {
  showModalBottomSheet(
    context: context,
    isScrollControlled: true,
    backgroundColor: context.colors.surface,
    builder: (_) {
      return DraggableScrollableSheet(
        initialChildSize: 0.55,
        minChildSize: 0.4,
        maxChildSize: 0.85,
        expand: false,
        builder: (context, scrollController) {
          return Container(
            decoration: BoxDecoration(
              color: context.colors.surface,
              borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
            ),
            child: Column(
              children: [
                const SizedBox(height: 12),

                Container(
                  width: 50,
                  height: 5,
                  decoration: BoxDecoration(
                    color: context.colors.surface,
                    borderRadius: BorderRadius.circular(20),
                  ),
                ),

                const SizedBox(height: 20),

                const Text(
                  "AI Study Tools",
                  style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
                ),

                const SizedBox(height: 6),

                Text(
                  "Choose how you'd like to study",
                  style: TextStyle(color: Colors.grey.shade600),
                ),

                const SizedBox(height: 20),

                Expanded(
                  child: ListView(
                    controller: scrollController,
                    padding: const EdgeInsets.symmetric(horizontal: 20),
                    children: [
                      _AiToolTile(
                        icon: Icons.chat_bubble_outline,
                        title: "AI Tutor Chat",
                        subtitle: "Ask questions about this lesson",
                        onTap: () {
                          Navigator.pop(context);
                          onChat();
                        },
                      ),

                      _AiToolTile(
                        icon: Icons.summarize_outlined,
                        title: "Lesson Summary",
                        subtitle: "Generate key points instantly",
                        onTap: () {
                          Navigator.pop(context);
                          onSummary();
                        },
                      ),

                      _AiToolTile(
                        icon: Icons.style_outlined,
                        title: "Smart Flashcards",
                        subtitle: "Generate study cards",
                        onTap: () {
                          Navigator.pop(context);
                          onFlashcards();
                        },
                      ),

                      _AiToolTile(
                        icon: Icons.quiz_outlined,
                        title: "Practice Quiz",
                        subtitle: "Test your understanding",
                        onTap: () {
                          Navigator.pop(context);
                          onQuiz();
                        },
                      ),

                      _AiToolTile(
                        icon: Icons.account_tree_outlined,
                        title: "Mind Map",
                        subtitle: "Visualize concepts",
                        onTap: () {
                          Navigator.pop(context);
                          onMindMap();
                        },
                      ),
                    ],
                  ),
                ),
              ],
            ),
          );
        },
      );
    },
  );
}

class _AiToolTile extends StatelessWidget {
  final IconData icon;
  final String title;
  final String subtitle;
  final VoidCallback onTap;

  const _AiToolTile({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 14),
      child: Material(
        color: context.colors.surface,
        borderRadius: BorderRadius.circular(18),
        child: InkWell(
          borderRadius: BorderRadius.circular(18),
          onTap: onTap,
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Row(
              children: [
                CircleAvatar(
                  radius: 24,
                  backgroundColor: context.colors.onSurface,
                  child: Icon(icon, color: context.colors.surface),
                ),

                const SizedBox(width: 14),

                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        title,
                        style: const TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.w600,
                        ),
                      ),

                      const SizedBox(height: 4),

                      Text(
                        subtitle,
                        style: TextStyle(color: Colors.grey.shade600),
                      ),
                    ],
                  ),
                ),

                const Icon(Icons.arrow_forward_ios, size: 16),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
