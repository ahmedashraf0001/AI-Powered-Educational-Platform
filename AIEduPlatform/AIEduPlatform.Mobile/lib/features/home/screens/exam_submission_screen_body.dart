import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/features/home/logic/home_cubit.dart';
import 'package:graduation_app/features/home/logic/home_state.dart';
import 'package:intl/intl.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/core/services/navigation/navigation_service.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/core/theming/styles.dart';
import 'package:graduation_app/core/widgets/custom_button.dart';
import 'package:graduation_app/features/home/screens/exam_result_screen.dart';

class ExamSubmissionsScreenBody extends StatefulWidget {
  const ExamSubmissionsScreenBody({super.key});

  @override
  State<ExamSubmissionsScreenBody> createState() =>
      _ExamSubmissionsScreenState();
}

class _ExamSubmissionsScreenState extends State<ExamSubmissionsScreenBody> {
  int currentPage = 1;
  static const int pageSize = 20;

  void _goToPage(int page) {
    setState(() => currentPage = page);
    context.read<HomeCubit>().getStudentSubmissions(
      page: currentPage,
      pageSize: pageSize,
    );
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDarkMode = theme.brightness == Brightness.dark;

    return Padding(
      padding: EdgeInsets.all(16.w),
      child: BlocBuilder<HomeCubit, HomeState>(
        buildWhen: (previous, current) =>
            current is LoadingStudentSubmissions ||
            current is SuccessStudentSubmissions ||
            current is FailureStudentSubmissions,
        builder: (context, state) {
          if (state is LoadingStudentSubmissions) {
            return const Center(child: CircularProgressIndicator());
          }

          if (state is FailureStudentSubmissions) {
            return Center(
              child: Text(
                state.message ?? 'Something went wrong',
                style: TextStyle(color: theme.colorScheme.error),
              ),
            );
          }

          if (state is SuccessStudentSubmissions) {
            final paged = state.success.data;
            final submissions = paged?.items ?? [];

            if (submissions.isEmpty) {
              return Center(
                child: Text(
                  'No submissions yet',
                  style: TextStyle(color: theme.colorScheme.onSurfaceVariant),
                ),
              );
            }

            return RefreshIndicator(
              onRefresh: () async {
                await context.read<HomeCubit>().getStudentSubmissions(
                  page: 1,
                  pageSize: 20,
                );
              },

              child: Column(
                children: [
                  Expanded(
                    child: ListView.separated(
                      physics: AlwaysScrollableScrollPhysics(),
                      itemCount: submissions.length,
                      separatorBuilder: (_, __) => SizedBox(height: 12.h),
                      itemBuilder: (context, index) {
                        final submission = submissions[index];
                        final isGraded = submission.isGraded ?? false;

                        // Dynamic badge coloring logic based on Theme mode
                        final badgeColor = isGraded
                            ? (isDarkMode
                                  ? Colors.greenAccent
                                  : ColorsManager.mainBlue)
                            : Colors.orange;

                        return InkWell(
                          borderRadius: BorderRadius.circular(16.r),
                          onTap: () {
                            final submissionId = submission.id;
                            if (submissionId == null) return;

                            context.read<HomeCubit>().getSubmissionDetails(
                              submissionId,
                            );

                            NavigationService.instance.navigateTo(
                              BlocProvider.value(
                                value: context.read<HomeCubit>(),
                                child: const ExamResultScreen(),
                              ),
                            );
                          },
                          child: Container(
                            padding: EdgeInsets.all(16.w),
                            decoration: BoxDecoration(
                              color: context.colors.surface,
                              borderRadius: BorderRadius.circular(16.r),
                              border: Border.all(
                                color: theme.colorScheme.outlineVariant,
                                width: 1,
                              ),
                              boxShadow: [
                                BoxShadow(
                                  color: Colors.black.withValues(
                                    alpha: isDarkMode ? 0.3 : 0.05,
                                  ),
                                  blurRadius: 8,
                                  offset: const Offset(0, 2),
                                ),
                              ],
                            ),
                            child: Row(
                              children: [
                                Expanded(
                                  child: Column(
                                    crossAxisAlignment:
                                        CrossAxisAlignment.start,
                                    children: [
                                      Text(
                                        submission.examTitle ?? 'Exam',
                                        style: TextStyles.font16.copyWith(
                                          fontWeight: FontWeight.bold,
                                          color: theme.colorScheme.onSurface,
                                        ),
                                      ),
                                      SizedBox(height: 4.h),
                                      Text(
                                        submission.courseName ?? '',
                                        style: TextStyles.font14.copyWith(
                                          color: theme
                                              .colorScheme
                                              .onSurfaceVariant,
                                        ),
                                      ),
                                      SizedBox(height: 6.h),
                                      Text(
                                        submission.submittedAt != null
                                            ? DateFormat(
                                                'MMM d, yyyy • h:mm a',
                                              ).format(submission.submittedAt!)
                                            : '',
                                        style: TextStyles.font12.copyWith(
                                          color: theme
                                              .colorScheme
                                              .onSurfaceVariant
                                              .withValues(alpha: 0.8),
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                                Container(
                                  padding: EdgeInsets.symmetric(
                                    horizontal: 12.w,
                                    vertical: 6.h,
                                  ),
                                  decoration: BoxDecoration(
                                    color: badgeColor.withValues(
                                      alpha: isDarkMode ? 0.2 : 0.1,
                                    ),
                                    borderRadius: BorderRadius.circular(20.r),
                                  ),
                                  child: Text(
                                    isGraded
                                        ? '${submission.score?.toInt() ?? 0}%'
                                        : 'Pending',
                                    style: TextStyles.font12.copyWith(
                                      color: badgeColor,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          ),
                        );
                      },
                    ),
                  ),
                  SizedBox(height: 12.h),
                  Row(
                    children: [
                      Expanded(
                        child: CustomButton(
                          onPressed: (paged?.hasPrevious ?? false)
                              ? () => _goToPage(currentPage - 1)
                              : null,
                          title: 'Previous',
                          color: isDarkMode
                              ? theme.colorScheme.surfaceContainerHighest
                              : ColorsManager.gray,
                          textColor: theme.colorScheme.onSurfaceVariant,
                          height: 48.h,
                          borderRadius: BorderRadius.circular(24.r),
                        ),
                      ),
                      SizedBox(width: 12.w),
                      Expanded(
                        child: CustomButton(
                          onPressed: (paged?.hasNext ?? false)
                              ? () => _goToPage(currentPage + 1)
                              : null,
                          title: 'Next',
                          color: isDarkMode
                              ? theme.colorScheme.primary
                              : ColorsManager.mainBlue,
                          textColor: isDarkMode
                              ? theme.colorScheme.onPrimary
                              : Colors.white,
                          height: 48.h,
                          borderRadius: BorderRadius.circular(24.r),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            );
          }

          return const SizedBox.shrink();
        },
      ),
    );
  }
}
