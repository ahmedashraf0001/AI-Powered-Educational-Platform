import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/di/dependency_injection.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/core/services/navigation/navigation_service.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/core/theming/styles.dart';
import 'package:graduation_app/core/widgets/custom_button.dart';
import 'package:graduation_app/features/ai/logic/ai_services_cubit/cubit/ai_services_cubit.dart';
import 'package:graduation_app/features/ai/logic/ai_services_cubit/cubit/ai_services_state.dart';
import 'package:graduation_app/features/ai/screens/quiz_result_screen.dart';
import 'package:graduation_app/features/ai/screens/widgets/topic_input_field.dart';

class QuizScreen extends StatefulWidget {
  final String sessionId;
  final String courseName;
  const QuizScreen({
    super.key,
    required this.sessionId,
    required this.courseName,
  });

  @override
  State<QuizScreen> createState() => _QuizScreenState();
}

class _QuizScreenState extends State<QuizScreen> {
  final TextEditingController _quizController = TextEditingController();

  int currentQuestionIndex = 0;
  int? selectedAnswerIndex;

  Map<String, String>? studentAnswers = {};

  @override
  void initState() {
    _quizController.text = widget.courseName;
    super.initState();
  }

  @override
  void dispose() {
    _quizController.dispose();
    super.dispose();
  }

  void _nextQuestion(int totalQuestions) {
    if (currentQuestionIndex < totalQuestions - 1) {
      setState(() {
        currentQuestionIndex++;
        selectedAnswerIndex = null;
      });
    }
  }

  void _previousQuestion() {
    if (currentQuestionIndex > 0) {
      setState(() {
        currentQuestionIndex--;
        selectedAnswerIndex = null;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        centerTitle: true,
        title: Text(
          'Practice Quiz',
          style: TextStyles.font20.copyWith(color: ColorsManager.mainBlue),
        ),
      ),
      body: SafeArea(
        child: Padding(
          padding: EdgeInsets.all(16.w),
          child: Column(
            children: [
              TopicInputField(
                hintText: 'Enter topic to generate quiz',
                buttonText: 'Generate Quiz',
                controller: _quizController,
                onPressed: () {
                  if (_quizController.text.trim().isEmpty) {
                    return;
                  }

                  setState(() {
                    currentQuestionIndex = 0;
                    selectedAnswerIndex = null;
                  });

                  context.read<AiServicesCubit>().generateQuiz(
                    _quizController.text.trim(),
                    5,
                    'medium',
                    widget.sessionId,
                  );
                },
              ),
              SizedBox(height: 20.h),
              Expanded(
                child: BlocBuilder<AiServicesCubit, AiServicesState>(
                  buildWhen: (previous, current) =>
                      current is LoadingGenerateQuiz ||
                      current is SuccessGenerateQuiz ||
                      current is FailureGenerateQuiz,
                  builder: (context, state) {
                    if (state is LoadingGenerateQuiz) {
                      return const Center(child: CircularProgressIndicator());
                    }

                    if (state is FailureGenerateQuiz) {
                      return Center(
                        child: Text(state.message ?? 'Something went wrong'),
                      );
                    }

                    if (state is SuccessGenerateQuiz) {
                      if (state.quizData.items == null ||
                          state.quizData.items!.isEmpty) {
                        return const Center(
                          child: Text("No questions available"),
                        );
                      }
                      final quiz = state.quizData.items?.first;

                      final questions = quiz?.questions ?? [];

                      if (questions.isEmpty) {
                        return const Center(child: Text('No questions found'));
                      }

                      final currentQuestion = questions[currentQuestionIndex];

                      final isLastQuestion =
                          currentQuestionIndex == questions.length - 1;

                      return Column(
                        children: [
                          QuizProgressCard(
                            currentQuestion: currentQuestionIndex + 1,
                            totalQuestions: questions.length,
                          ),

                          SizedBox(height: 16.h),

                          Expanded(
                            child: SingleChildScrollView(
                              child: QuizQuestionCard(
                                question: currentQuestion.questionText ?? '',
                                answers: currentQuestion.options ?? [],
                                selectedIndex: selectedAnswerIndex,
                                onAnswerSelected: (index) {
                                  setState(() {
                                    selectedAnswerIndex = index;
                                  });
                                },
                              ),
                            ),
                          ),

                          SizedBox(height: 16.h),

                          QuizNavigationButtons(
                            isFirstQuestion: currentQuestionIndex == 0,
                            isLastQuestion: isLastQuestion,
                            isAnswerSelected: selectedAnswerIndex != null,
                            onPrevious: _previousQuestion,
                            onSubmit: () {
                              if (selectedAnswerIndex == null) {
                                return;
                              }

                              studentAnswers?.addAll({
                                '$currentQuestionIndex': currentQuestion
                                    .options![selectedAnswerIndex!],
                              });

                              if (isLastQuestion) {
                                NavigationService.instance.navigateTo(
                                  BlocProvider(
                                    create: (context) =>
                                        getIt<AiServicesCubit>()..submitQuiz(
                                          quiz?.id ?? '',
                                          widget.sessionId,
                                          studentAnswers ?? {},
                                        ),
                                    child: QuizResultScreen(
                                      studentAnswers: studentAnswers,
                                    ),
                                  ),
                                );
                              } else {
                                _nextQuestion(questions.length);
                              }
                            },
                          ),
                        ],
                      );
                    }

                    return const SizedBox.shrink();
                  },
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class QuizProgressCard extends StatelessWidget {
  final int currentQuestion;
  final int totalQuestions;

  const QuizProgressCard({
    super.key,
    required this.currentQuestion,
    required this.totalQuestions,
  });

  @override
  Widget build(BuildContext context) {
    final progress = currentQuestion / totalQuestions;

    return Container(
      padding: EdgeInsets.all(16.w),
      decoration: BoxDecoration(
        color: context.colors.surface,
        borderRadius: BorderRadius.circular(16.r),
        border: Border.all(color: ColorsManager.lightGray, width: 1),
      ),
      child: Column(
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Current Progress',
                style: TextStyles.font16.copyWith(fontWeight: FontWeight.bold),
              ),
              Container(
                padding: EdgeInsets.symmetric(horizontal: 12.w, vertical: 6.h),
                decoration: BoxDecoration(
                  color: context.colors.surface,
                  borderRadius: BorderRadius.circular(20.r),
                  border: Border.all(width: 1, color: ColorsManager.lightGray),
                ),
                child: Text(
                  '$currentQuestion / $totalQuestions',
                  style: TextStyles.font14.copyWith(
                    color: ColorsManager.mainBlue,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            ],
          ),
          SizedBox(height: 12.h),
          LinearProgressIndicator(
            value: progress,
            borderRadius: BorderRadius.circular(10.r),
            backgroundColor: ColorsManager.lighterGray,
            color: ColorsManager.mainBlue,
          ),
        ],
      ),
    );
  }
}

class QuizQuestionCard extends StatelessWidget {
  final String question;
  final List<String> answers;
  final int? selectedIndex;
  final ValueChanged<int> onAnswerSelected;

  const QuizQuestionCard({
    super.key,
    required this.question,
    required this.answers,
    required this.selectedIndex,
    required this.onAnswerSelected,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: EdgeInsets.all(20.w),
      decoration: BoxDecoration(
        color: context.colors.surface,
        borderRadius: BorderRadius.circular(20.r),
        border: Border.all(color: ColorsManager.lightGray, width: 1),
        boxShadow: [
          BoxShadow(
            blurRadius: 10,
            offset: const Offset(0, 4),
            color: Colors.black.withValues(alpha: 0.05),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            question,
            style: TextStyles.font20.copyWith(fontWeight: FontWeight.bold),
          ),
          SizedBox(height: 20.h),
          ListView.separated(
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            itemCount: answers.length,
            separatorBuilder: (_, __) => SizedBox(height: 12.h),
            itemBuilder: (context, index) {
              return QuizAnswerCard(
                answer: answers[index],
                isSelected: selectedIndex == index,
                onTap: () => onAnswerSelected(index),
              );
            },
          ),
        ],
      ),
    );
  }
}

class QuizAnswerCard extends StatelessWidget {
  final String answer;
  final bool isSelected;
  final VoidCallback? onTap;

  const QuizAnswerCard({
    super.key,
    required this.answer,
    required this.isSelected,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      borderRadius: BorderRadius.circular(14.r),
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 200),
        padding: EdgeInsets.all(16.w),
        decoration: BoxDecoration(
          color: isSelected
              ? context.colors.secondary.withValues(alpha: 0.2)
              : context.colors.surface,
          borderRadius: BorderRadius.circular(14.r),
          border: Border.all(
            color: isSelected
                ? ColorsManager.mainBlue
                : ColorsManager.lightGray,
            width: 2,
          ),
        ),
        child: Row(
          children: [
            Icon(
              isSelected ? Icons.radio_button_checked : Icons.radio_button_off,
              color: ColorsManager.mainBlue,
            ),
            SizedBox(width: 12.w),
            Expanded(
              child: Text(
                answer,
                style: TextStyles.font16.copyWith(fontWeight: FontWeight.w500),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class QuizNavigationButtons extends StatelessWidget {
  final VoidCallback onPrevious;
  final VoidCallback onSubmit;
  final bool isAnswerSelected;
  final bool isFirstQuestion;
  final bool isLastQuestion;

  const QuizNavigationButtons({
    super.key,
    required this.onPrevious,
    required this.onSubmit,
    required this.isAnswerSelected,
    required this.isFirstQuestion,
    required this.isLastQuestion,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: CustomButton(
            onPressed: isFirstQuestion ? null : onPrevious,
            title: 'Previous',
            color: ColorsManager.gray,
            textColor: ColorsManager.darkGray,
            height: 52.h,
            borderRadius: BorderRadius.circular(26.r),
          ),
        ),
        SizedBox(width: 12.w),
        Expanded(
          child: CustomButton(
            onPressed: isAnswerSelected ? onSubmit : null,
            title: isLastQuestion ? 'Finish' : 'Next',
            color: ColorsManager.mainBlue,
            textColor: Colors.white,
            height: 52.h,
            borderRadius: BorderRadius.circular(26.r),
          ),
        ),
      ],
    );
  }
}
