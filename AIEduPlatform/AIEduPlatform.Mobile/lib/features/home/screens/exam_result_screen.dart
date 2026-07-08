import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/core/helpers/space_helper.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/core/theming/styles.dart';
import 'package:graduation_app/core/widgets/custom_button.dart';
import 'package:graduation_app/features/home/logic/home_cubit.dart';
import 'package:graduation_app/features/home/logic/home_state.dart';
import 'package:graduation_app/features/main/screens/main_screen.dart';

class ExamResultScreen extends StatelessWidget {
  const ExamResultScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        centerTitle: true,
        title: Text(
          "Exam Result",
          style: TextStyles.font20.copyWith(
            color: ColorsManager.mainBlue,
            fontWeight: FontWeight.bold,
          ),
        ),
      ),
      body: SafeArea(
        child: Padding(
          padding: EdgeInsets.all(16.w),
          child: BlocBuilder<HomeCubit, HomeState>(
            buildWhen: (previous, current) =>
                current is LoadingSubmitExam ||
                current is FailureSubmitExam ||
                current is LoadingSubmissionDetails ||
                current is SuccessSubmissionDetails ||
                current is FailureSubmissionDetails,

            builder: (context, state) {
              if (state is LoadingSubmitExam ||
                  state is LoadingSubmissionDetails) {
                return Center(child: CircularProgressIndicator());
              } else if (state is SuccessSubmissionDetails) {
                final submission = state.success.data;

                if (submission == null) {
                  return const Center(child: Text('No submission data'));
                }

                final answers = submission.answers ?? [];

                // Objective-question correctness is computed locally since
                // it's known instantly; final approved score may lag behind
                // (written questions need teacher/AI grading).
                final correctCount = answers
                    .where(
                      (a) =>
                          (a.correctAnswer ?? '').trim().isNotEmpty &&
                          (a.correctAnswer ?? '').trim() ==
                              (a.answer ?? '').trim(),
                    )
                    .length;
                final inCorrectCount = answers.length - correctCount;

                final grade = submission.grade;
                final isGraded = grade != null;

                return SingleChildScrollView(
                  child: Column(
                    children: [
                      /// SCORE CIRCLE (or pending-grading placeholder)
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
                          child: isGraded
                              ? Column(
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  children: [
                                    Text(
                                      "$correctCount/${answers.length}",
                                      style: TextStyles.font32.copyWith(
                                        fontWeight: FontWeight.bold,
                                        color: ColorsManager.mainBlue,
                                      ),
                                    ),
                                    Text(
                                      "${grade.score?.toInt() ?? 0}%",
                                      style: TextStyles.font16.copyWith(
                                        color: ColorsManager.darkGray,
                                        fontWeight: FontWeight.bold,
                                      ),
                                    ),
                                  ],
                                )
                              : Column(
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  children: [
                                    Icon(
                                      Icons.hourglass_top_rounded,
                                      color: ColorsManager.mainBlue,
                                      size: 32.sp,
                                    ),
                                    SizedBox(height: 6.h),
                                    Text(
                                      "Pending",
                                      style: TextStyles.font14.copyWith(
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
                        isGraded
                            ? "Great job! Keep it up!"
                            : "Submitted successfully!",
                        style: TextStyles.font20.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                      ),

                      Text(
                        isGraded
                            ? "You've completed the exam."
                            : "Your exam is awaiting grading.",
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
                                    '$correctCount',
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
                                    isGraded
                                        ? (grade.feedback?.isNotEmpty == true
                                              ? grade.feedback!
                                              : "Nice work! You've successfully completed the exam. "
                                                    "Keep practicing regularly to strengthen your understanding "
                                                    "and improve your results even further.")
                                        : "Your written answers are being reviewed by your instructor. "
                                              "You'll be notified once your final grade is approved.",
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
                                              "${answers.length} Questions Reviewed",
                                              style: TextStyles.font14.copyWith(
                                                color: ColorsManager.darkGray,
                                              ),
                                            ),

                                            SizedBox(height: 20.h),

                                            Expanded(
                                              child: ListView.separated(
                                                controller: scrollController,
                                                itemCount: answers.length,
                                                separatorBuilder: (_, __) =>
                                                    Divider(
                                                      color: context
                                                          .colors
                                                          .surface,
                                                    ),
                                                itemBuilder: (context, index) {
                                                  final result = answers[index];

                                                  final hasCorrectAnswer =
                                                      (result.correctAnswer ??
                                                              '')
                                                          .trim()
                                                          .isNotEmpty;

                                                  final isCorrect =
                                                      hasCorrectAnswer &&
                                                      (result.correctAnswer ??
                                                                  '')
                                                              .trim() ==
                                                          (result.answer ?? '')
                                                              .trim();

                                                  final questionNumber =
                                                      (result.order ?? index) +
                                                      1;

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
                                                                  "$questionNumber",
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
                                                                "Question $questionNumber",
                                                                style: TextStyles
                                                                    .font16
                                                                    .copyWith(
                                                                      fontWeight:
                                                                          FontWeight
                                                                              .bold,
                                                                    ),
                                                              ),
                                                            ),

                                                            // Hide the badge for written
                                                            // questions with no fixed
                                                            // correct answer to grade against.
                                                            if (hasCorrectAnswer)
                                                              Container(
                                                                padding:
                                                                    EdgeInsets.symmetric(
                                                                      horizontal:
                                                                          10.w,
                                                                      vertical:
                                                                          6.h,
                                                                    ),
                                                                decoration: BoxDecoration(
                                                                  color:
                                                                      isCorrect
                                                                      ? Colors.green.withValues(
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

                                                        if (hasCorrectAnswer) ...[
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
                                                              color: Colors
                                                                  .green
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
                                                                      alpha:
                                                                          0.25,
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

                                                          SizedBox(
                                                            height: 14.h,
                                                          ),
                                                        ],

                                                        Text(
                                                          "Your Answer",
                                                          style: TextStyles.font14.copyWith(
                                                            fontWeight:
                                                                FontWeight.w600,
                                                            color:
                                                                !hasCorrectAnswer
                                                                ? ColorsManager
                                                                      .mainBlue
                                                                : (isCorrect
                                                                      ? ColorsManager
                                                                            .mainBlue
                                                                      : Colors
                                                                            .red),
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
                                                            color:
                                                                (!hasCorrectAnswer ||
                                                                    isCorrect)
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
                                                              color:
                                                                  (!hasCorrectAnswer ||
                                                                      isCorrect)
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
                                                            result.answer ?? '',
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
                              Navigator.of(context).pushAndRemoveUntil(
                                MaterialPageRoute(
                                  builder: (_) => const MainScreen(),
                                ),
                                (route) => false,
                              );
                            },
                            title: "Back to home",
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
              } else if (state is FailureSubmitExam) {
                return Center(child: Text('error : ${state.message} ?? error'));
              } else if (state is FailureSubmissionDetails) {
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
