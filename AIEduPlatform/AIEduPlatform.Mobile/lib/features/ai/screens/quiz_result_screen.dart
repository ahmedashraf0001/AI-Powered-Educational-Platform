import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/core/helpers/space_helper.dart';
import 'package:graduation_app/core/services/navigation/navigation_service.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/core/theming/styles.dart';
import 'package:graduation_app/core/widgets/custom_button.dart';
import 'package:graduation_app/features/ai/logic/ai_services_cubit/cubit/ai_services_cubit.dart';
import 'package:graduation_app/features/ai/logic/ai_services_cubit/cubit/ai_services_state.dart';

class QuizResultScreen extends StatelessWidget {
  final Map<String, String>? studentAnswers;
  const QuizResultScreen({super.key, this.studentAnswers});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        centerTitle: true,
        title: Text(
          "Quiz Result",
          style: TextStyles.font20.copyWith(
            color: ColorsManager.mainBlue,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      body: SafeArea(
        child: Padding(
          padding: EdgeInsets.all(16.w),
          child: BlocBuilder<AiServicesCubit, AiServicesState>(
            buildWhen: (previous, current) =>
                current is SuccessSubmitQuiz ||
                current is LoadingSubmitQuiz ||
                current is FailureSubmitQuiz,

            builder: (context, state) {
              if (state is LoadingSubmitQuiz) {
                return Center(child: CircularProgressIndicator());
              } else if (state is SuccessSubmitQuiz) {
                final quizData = state.submitQuizData;
                final inCorrectCount =
                    (quizData.totalQuestions!) - (quizData.correctCount!);
                final resultsList = quizData.resultsList;
                return SingleChildScrollView(
                  child: Column(
                    children: [
                      /// SCORE CIRCLE
                      Container(
                        width: 170.w,
                        height: 170.w,
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          border: Border.all(
                            color: ColorsManager.mainBlue.withValues(
                              alpha: 0.1,
                            ),
                            width: 10,
                          ),
                        ),
                        child: Center(
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Text(
                                "${quizData.correctCount}/${quizData.totalQuestions}",
                                style: TextStyles.font32.copyWith(
                                  fontWeight: FontWeight.bold,
                                  color: ColorsManager.mainBlue,
                                ),
                              ),
                              Text(
                                "${quizData.score!.toInt()}%",
                                style: TextStyles.font16.copyWith(
                                  color: ColorsManager.darkGray,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),

                      SizedBox(height: 20.h),

                      Text(
                        "Great job! Keep it up!",
                        style: TextStyles.font20.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                      ),

                      Text(
                        "You've completed the quiz.",
                        style: TextStyles.font14.copyWith(
                          color: ColorsManager.darkGray,
                        ),
                      ),

                      SizedBox(height: 24.h),

                      /// STATS ROW
                      Row(
                        children: [
                          Expanded(
                            child: Container(
                              padding: EdgeInsets.all(16.w),
                              decoration: BoxDecoration(
                                color: ColorsManager.mainBlue.withValues(
                                  alpha: 0.2,
                                ),
                                borderRadius: BorderRadius.circular(16.r),
                              ),
                              child: Column(
                                children: [
                                  Icon(
                                    Icons.check_circle,
                                    color: ColorsManager.mainBlue,
                                  ),
                                  SizedBox(height: 8.h),
                                  Text("Correct", style: TextStyles.font14),
                                  Text(
                                    '${quizData.correctCount ?? 0}',
                                    style: TextStyles.font20.copyWith(
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ),
                          SizedBox(width: 12.w),
                          Expanded(
                            child: Container(
                              padding: EdgeInsets.all(16.w),
                              decoration: BoxDecoration(
                                color: Colors.red.withValues(alpha: 0.1),
                                borderRadius: BorderRadius.circular(16.r),
                              ),
                              child: Column(
                                children: [
                                  Icon(Icons.cancel, color: Colors.red),
                                  SizedBox(height: 8.h),
                                  Text("Incorrect", style: TextStyles.font14),
                                  Text(
                                    '$inCorrectCount',
                                    style: TextStyles.font20.copyWith(
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ),
                        ],
                      ),

                      SizedBox(height: 16.h),

                      /// AI INSIGHT
                      /// PERFORMANCE SUMMARY
                      Container(
                        width: double.infinity,
                        padding: EdgeInsets.all(16.w),
                        decoration: BoxDecoration(
                          color: ColorsManager.mainBlue.withValues(alpha: 0.08),
                          borderRadius: BorderRadius.circular(16.r),
                        ),
                        child: Row(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Icon(
                              Icons.emoji_events_rounded,
                              color: ColorsManager.mainBlue,
                              size: 28.sp,
                            ),
                            SizedBox(width: 10.w),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    "Performance Summary",
                                    style: TextStyles.font16.copyWith(
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                  SizedBox(height: 6.h),
                                  Text(
                                    "Nice work! You've successfully completed the quiz. "
                                    "Keep practicing regularly to strengthen your understanding "
                                    "and improve your results even further.",
                                    style: TextStyles.font14.copyWith(
                                      color: ColorsManager.darkGray,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ),
                      ),

                      VerticalSpace(height: 20),

                      /// BUTTONS
                      Column(
                        spacing: 16.h,
                        children: [
                          CustomButton(
                            onPressed: () {
                              showModalBottomSheet(
                                context: context,
                                isScrollControlled: true,
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.vertical(
                                    top: Radius.circular(24.r),
                                  ),
                                ),
                                builder: (_) {
                                  return DraggableScrollableSheet(
                                    expand: false,
                                    initialChildSize: 0.75,
                                    maxChildSize: 0.9,
                                    minChildSize: 0.5,
                                    builder: (context, scrollController) {
                                      return Padding(
                                        padding: EdgeInsets.all(16.w),
                                        child: Column(
                                          children: [
                                            Container(
                                              width: 50.w,
                                              height: 5.h,
                                              decoration: BoxDecoration(
                                                borderRadius:
                                                    BorderRadius.circular(20.r),
                                              ),
                                            ),

                                            SizedBox(height: 20.h),

                                            Icon(
                                              Icons.fact_check_rounded,
                                              size: 40.sp,
                                              color: ColorsManager.mainBlue,
                                            ),

                                            SizedBox(height: 10.h),

                                            Text(
                                              "Answer Review",
                                              style: TextStyles.font20.copyWith(
                                                fontWeight: FontWeight.bold,
                                              ),
                                            ),
                                            Text(
                                              "${resultsList?.length ?? 0} Questions Reviewed",
                                              style: TextStyles.font14.copyWith(
                                                color: ColorsManager.darkGray,
                                              ),
                                            ),

                                            SizedBox(height: 20.h),

                                            Expanded(
                                              child: ListView.separated(
                                                controller: scrollController,
                                                itemCount:
                                                    resultsList?.length ?? 0,
                                                separatorBuilder: (_, __) =>
                                                    Divider(
                                                      color: context
                                                          .colors
                                                          .surface,
                                                    ),
                                                itemBuilder: (context, index) {
                                                  final result =
                                                      resultsList![index];
                                                  final yourAnswer =
                                                      studentAnswers!.entries
                                                          .elementAt(index);

                                                  final isCorrect =
                                                      result.correctAnswer
                                                          ?.trim() ==
                                                      yourAnswer.value.trim();

                                                  return Container(
                                                    padding: EdgeInsets.all(
                                                      16.w,
                                                    ),
                                                    decoration: BoxDecoration(
                                                      borderRadius:
                                                          BorderRadius.circular(
                                                            20.r,
                                                          ),
                                                      border: Border.all(
                                                        width: 1,
                                                        color: ColorsManager
                                                            .lightGray,
                                                      ),
                                                      boxShadow: [
                                                        BoxShadow(
                                                          blurRadius: 10,
                                                          offset: const Offset(
                                                            0,
                                                            4,
                                                          ),
                                                          color: Colors.black
                                                              .withValues(
                                                                alpha: 0.05,
                                                              ),
                                                        ),
                                                      ],
                                                    ),
                                                    child: Column(
                                                      crossAxisAlignment:
                                                          CrossAxisAlignment
                                                              .start,
                                                      children: [
                                                        Row(
                                                          children: [
                                                            Container(
                                                              width: 38.w,
                                                              height: 38.w,
                                                              decoration: BoxDecoration(
                                                                color: ColorsManager
                                                                    .mainBlue
                                                                    .withValues(
                                                                      alpha:
                                                                          0.1,
                                                                    ),
                                                                shape: BoxShape
                                                                    .circle,
                                                              ),
                                                              child: Center(
                                                                child: Text(
                                                                  "${result.questionIndex! + 1}",
                                                                  style: TextStyles.font14.copyWith(
                                                                    color: ColorsManager
                                                                        .mainBlue,
                                                                    fontWeight:
                                                                        FontWeight
                                                                            .bold,
                                                                  ),
                                                                ),
                                                              ),
                                                            ),

                                                            SizedBox(
                                                              width: 12.w,
                                                            ),

                                                            Expanded(
                                                              child: Text(
                                                                "Question ${result.questionIndex! + 1}",
                                                                style: TextStyles
                                                                    .font16
                                                                    .copyWith(
                                                                      fontWeight:
                                                                          FontWeight
                                                                              .bold,
                                                                    ),
                                                              ),
                                                            ),

                                                            Container(
                                                              padding:
                                                                  EdgeInsets.symmetric(
                                                                    horizontal:
                                                                        10.w,
                                                                    vertical:
                                                                        6.h,
                                                                  ),
                                                              decoration: BoxDecoration(
                                                                color: isCorrect
                                                                    ? Colors
                                                                          .green
                                                                          .withValues(
                                                                            alpha:
                                                                                0.15,
                                                                          )
                                                                    : Colors.red.withValues(
                                                                        alpha:
                                                                            0.15,
                                                                      ),
                                                                borderRadius:
                                                                    BorderRadius.circular(
                                                                      20.r,
                                                                    ),
                                                              ),
                                                              child: Text(
                                                                isCorrect
                                                                    ? "Correct"
                                                                    : "Wrong",
                                                                style: TextStyles.font12.copyWith(
                                                                  color:
                                                                      isCorrect
                                                                      ? Colors
                                                                            .green
                                                                      : Colors
                                                                            .red,
                                                                  fontWeight:
                                                                      FontWeight
                                                                          .bold,
                                                                ),
                                                              ),
                                                            ),
                                                          ],
                                                        ),

                                                        SizedBox(height: 18.h),

                                                        Text(
                                                          "Correct Answer",
                                                          style: TextStyles
                                                              .font14
                                                              .copyWith(
                                                                fontWeight:
                                                                    FontWeight
                                                                        .w600,
                                                                color: Colors
                                                                    .green,
                                                              ),
                                                        ),

                                                        SizedBox(height: 6.h),

                                                        Container(
                                                          width:
                                                              double.infinity,
                                                          padding:
                                                              EdgeInsets.all(
                                                                14.w,
                                                              ),
                                                          decoration: BoxDecoration(
                                                            color: Colors.green
                                                                .withValues(
                                                                  alpha: 0.08,
                                                                ),
                                                            borderRadius:
                                                                BorderRadius.circular(
                                                                  14.r,
                                                                ),
                                                            border: Border.all(
                                                              color: Colors
                                                                  .green
                                                                  .withValues(
                                                                    alpha: 0.25,
                                                                  ),
                                                            ),
                                                          ),
                                                          child: Text(
                                                            result.correctAnswer ??
                                                                '',
                                                            style: TextStyles
                                                                .font14,
                                                          ),
                                                        ),

                                                        SizedBox(height: 14.h),

                                                        Text(
                                                          "Your Answer",
                                                          style: TextStyles
                                                              .font14
                                                              .copyWith(
                                                                fontWeight:
                                                                    FontWeight
                                                                        .w600,
                                                                color: isCorrect
                                                                    ? ColorsManager
                                                                          .mainBlue
                                                                    : Colors
                                                                          .red,
                                                              ),
                                                        ),

                                                        SizedBox(height: 6.h),

                                                        Container(
                                                          width:
                                                              double.infinity,
                                                          padding:
                                                              EdgeInsets.all(
                                                                14.w,
                                                              ),
                                                          decoration: BoxDecoration(
                                                            color: isCorrect
                                                                ? ColorsManager
                                                                      .mainBlue
                                                                      .withValues(
                                                                        alpha:
                                                                            0.08,
                                                                      )
                                                                : Colors.red
                                                                      .withValues(
                                                                        alpha:
                                                                            0.08,
                                                                      ),
                                                            borderRadius:
                                                                BorderRadius.circular(
                                                                  14.r,
                                                                ),
                                                            border: Border.all(
                                                              color: isCorrect
                                                                  ? ColorsManager
                                                                        .mainBlue
                                                                        .withValues(
                                                                          alpha:
                                                                              0.2,
                                                                        )
                                                                  : Colors.red
                                                                        .withValues(
                                                                          alpha:
                                                                              0.2,
                                                                        ),
                                                            ),
                                                          ),
                                                          child: Text(
                                                            yourAnswer.value,
                                                            style: TextStyles
                                                                .font14,
                                                          ),
                                                        ),
                                                      ],
                                                    ),
                                                  );
                                                },
                                              ),
                                            ),
                                          ],
                                        ),
                                      );
                                    },
                                  );
                                },
                              );
                            },
                            title: "Review Answers",
                            color: ColorsManager.mainBlue,
                            textColor: Colors.white,
                            height: 52.h,
                            borderRadius: BorderRadius.circular(30.r),
                          ),

                          CustomButton(
                            onPressed: () {
                              NavigationService.instance.goBack();
                            },
                            title: "Back to quiz",
                            color: Colors.white,
                            textColor: ColorsManager.mainBlue,
                            height: 52.h,
                            borderRadius: BorderRadius.circular(30.r),
                          ),
                        ],
                      ),
                    ],
                  ),
                );
              } else if (state is FailureSubmitQuiz) {
                return Center(child: Text('error : ${state.message} ?? error'));
              } else {
                return SizedBox.shrink();
              }
            },
          ),
        ),
      ),
    );
  }
}
