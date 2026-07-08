import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/core/services/navigation/navigation_service.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/core/theming/styles.dart';
import 'package:graduation_app/core/widgets/custom_button.dart';
import 'package:graduation_app/features/home/logic/home_cubit.dart';
import 'package:graduation_app/features/home/logic/home_state.dart';
import 'package:graduation_app/features/home/screens/exam_result_screen.dart';

class ExamScreen extends StatefulWidget {
  final String examId;
  final String examName;
  const ExamScreen({super.key, required this.examId, required this.examName});

  @override
  State<ExamScreen> createState() => _ExamScreenState();
}

class _ExamScreenState extends State<ExamScreen> {
  int currentQuestionIndex = 0;
  int? selectedAnswerIndex;

  /// Keyed by questionId -> chosen answer text (required by submitExam's
  /// Map<String, String> payload, unlike quiz which keyed by question index).
  final Map<String, String> studentAnswers = {};

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
        title: Text(
          widget.examName,
          style: TextStyles.font18.copyWith(color: ColorsManager.mainBlue),
          overflow: TextOverflow.ellipsis,
        ),
      ),
      body: Padding(
        padding: EdgeInsets.all(16.w),
        child: BlocBuilder<HomeCubit, HomeState>(
          buildWhen: (previous, current) =>
              current is LoadingExamQuestions ||
              current is SuccessExamQuestions ||
              current is FailureExamQuestions,
          builder: (context, state) {
            if (state is LoadingExamQuestions) {
              return const Center(child: CircularProgressIndicator());
            }

            if (state is FailureExamQuestions) {
              return Center(
                child: Text(state.message ?? 'Something went wrong'),
              );
            }

            if (state is SuccessExamQuestions) {
              final questions = state.questions;

              if (questions!.isEmpty) {
                return const Center(child: Text('No questions found'));
              }

              final currentQuestion = questions[currentQuestionIndex];
              final isLastQuestion =
                  currentQuestionIndex == questions.length - 1;

              return Column(
                children: [
                  ExamProgressCard(
                    currentQuestion: currentQuestionIndex + 1,
                    totalQuestions: questions.length,
                  ),

                  SizedBox(height: 16.h),

                  Expanded(
                    child: SingleChildScrollView(
                      child: ExamQuestionCard(
                        question: currentQuestion.text ?? '',
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

                  ExamNavigationButtons(
                    isFirstQuestion: currentQuestionIndex == 0,
                    isLastQuestion: isLastQuestion,
                    isAnswerSelected: selectedAnswerIndex != null,
                    onPrevious: _previousQuestion,
                    onSubmit: () {
                      if (selectedAnswerIndex == null) {
                        return;
                      }

                      final options = currentQuestion.options;
                      if (options == null ||
                          selectedAnswerIndex! >= options.length) {
                        return;
                      }

                      final questionId = currentQuestion.id ?? '';
                      final selectedAnswer = options[selectedAnswerIndex!];

                      studentAnswers[questionId] = selectedAnswer;

                      if (isLastQuestion) {
                        // Triggers submitExam, which (per the cubit design)
                        // chains into getSubmissionDetails internally.
                        context.read<HomeCubit>().submitExam(
                          widget.examId,
                          studentAnswers,
                        );

                        context.read<HomeCubit>().getAvailableExams(1, 20);

                        NavigationService.instance.navigateTo(
                          BlocProvider.value(
                            value: context.read<HomeCubit>(),
                            child: const ExamResultScreen(),
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
    );
  }
}

class ExamProgressCard extends StatelessWidget {
  final int currentQuestion;
  final int totalQuestions;

  const ExamProgressCard({
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
            backgroundColor: context.colors.onSurface,
            color: ColorsManager.mainBlue,
          ),
        ],
      ),
    );
  }
}

class ExamQuestionCard extends StatelessWidget {
  final String question;
  final List<String> answers;
  final int? selectedIndex;
  final ValueChanged<int> onAnswerSelected;

  const ExamQuestionCard({
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
              return ExamAnswerCard(
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

class ExamAnswerCard extends StatelessWidget {
  final String answer;
  final bool isSelected;
  final VoidCallback? onTap;

  const ExamAnswerCard({
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

class ExamNavigationButtons extends StatelessWidget {
  final VoidCallback onPrevious;
  final VoidCallback onSubmit;
  final bool isAnswerSelected;
  final bool isFirstQuestion;
  final bool isLastQuestion;

  const ExamNavigationButtons({
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
