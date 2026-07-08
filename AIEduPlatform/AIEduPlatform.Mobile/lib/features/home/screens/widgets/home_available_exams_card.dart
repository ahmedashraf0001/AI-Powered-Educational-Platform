import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/di/dependency_injection.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/core/services/navigation/navigation_service.dart';
import 'package:graduation_app/features/home/data/models/get_availble_exams_response_model.dart';
import 'package:graduation_app/features/home/logic/home_cubit.dart';
import 'package:graduation_app/features/home/screens/exam_screen.dart';

import '../../../../core/theming/styles.dart';
import '../../../../core/widgets/exam_date_badge.dart';

class HomeAvailableExamCard extends StatelessWidget {
  final AvailableExamsItemModel examModel;
  const HomeAvailableExamCard({super.key, required this.examModel});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final isDark = theme.brightness == Brightness.dark;

    return Material(
      color: Colors.transparent,
      child: InkWell(
        borderRadius: BorderRadius.circular(24.r),
        onTap: () {
          NavigationService.instance.navigateTo(
            BlocProvider(
              create: (context) =>
                  getIt<HomeCubit>()..getExamQuestions(examModel.id ?? ''),
              child: ExamScreen(
                examId: examModel.id ?? '',
                examName: examModel.title ?? '',
              ),
            ),
          );
        },
        child: Container(
          width: double.infinity,
          padding: EdgeInsets.symmetric(vertical: 14.h, horizontal: 14.w),
          decoration: BoxDecoration(
            color: colorScheme.surface,
            borderRadius: BorderRadius.circular(24.r),
            border: Border.all(
              width: 1,
              color: isDark
                  ? colorScheme.outline.withOpacity(0.25)
                  : colorScheme.outline.withOpacity(0.4),
            ),
            boxShadow: isDark
                ? null
                : [
                    BoxShadow(
                      color: Colors.black.withOpacity(0.04),
                      blurRadius: 10,
                      offset: const Offset(0, 4),
                    ),
                  ],
          ),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Expanded(
                child: Row(
                  children: [
                    ExamDateBadge(
                      month: examModel.startTime?.examMonth ?? '--',
                      day: examModel.startTime?.examDay ?? '--',
                    ),
                    SizedBox(width: 16.w),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Text(
                            examModel.title ?? 'empty',
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            style: TextStyles.font15.copyWith(
                              fontWeight: FontWeight.bold,
                              color: colorScheme.onSurface,
                            ),
                          ),
                          SizedBox(height: 4.h),
                          Text(
                            examModel.startTime?.examTime ?? '--',
                            style: TextStyles.font14.copyWith(
                              fontWeight: FontWeight.w500,
                              color: colorScheme.onSurfaceVariant,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
              SizedBox(width: 8.w),
              Container(
                padding: EdgeInsets.all(8.w),
                decoration: BoxDecoration(
                  color: colorScheme.primary.withOpacity(isDark ? 0.18 : 0.1),
                  shape: BoxShape.circle,
                ),
                child: Icon(
                  Icons.arrow_forward_ios_rounded,
                  size: 16.w,
                  color: colorScheme.primary,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
