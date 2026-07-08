import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../../../../core/helpers/extensions.dart';
import '../../../../core/helpers/space_helper.dart';
import '../../../../core/services/navigation/navigation_service.dart';
import '../../../../core/theming/styles.dart';
import '../../../../core/widgets/custom_button.dart';
import '../../../cart/data/models/my_courses_response_model.dart';
import '../course_learning_screen.dart';

class MyCoursesItemCard extends StatelessWidget {
  final CoursesProgress coursesProgress;

  const MyCoursesItemCard({super.key, required this.coursesProgress});

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;
    final brightness = Theme.of(context).brightness;

    final progress =
        ((coursesProgress.progressPercentage ?? 0).clamp(0, 100)) / 100;

    return SizedBox(
      height: 380.h,
      child: Card(
        elevation: brightness == Brightness.dark ? 2 : 6,
        shadowColor: Colors.black.withOpacity(
          brightness == Brightness.dark ? .15 : .08,
        ),
        color: colors.surface,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(24.r),
        ),
        child: ClipRRect(
          borderRadius: BorderRadius.circular(24.r),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Header
              Container(
                height: 170.h,
                width: double.infinity,
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    colors: [colors.primary, colors.primary.withOpacity(.75)],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
                ),
                child: Stack(
                  children: [
                    Positioned(
                      top: 16.h,
                      right: 16.w,
                      child: Container(
                        padding: EdgeInsets.symmetric(
                          horizontal: 12.w,
                          vertical: 6.h,
                        ),
                        decoration: BoxDecoration(
                          color: Colors.white.withOpacity(.18),
                          borderRadius: BorderRadius.circular(30.r),
                        ),
                        child: Text(
                          coursesProgress.status ?? '',
                          style: TextStyles.font12.copyWith(
                            color: Colors.white,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ),

              Expanded(
                child: Padding(
                  padding: EdgeInsets.all(18.w),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        coursesProgress.courseTitle ?? '',
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                        style: TextStyles.font18.copyWith(
                          fontWeight: FontWeight.bold,
                          color: colors.onSurface,
                        ),
                      ),

                      VerticalSpace(height: 18),

                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(
                            'Progress',
                            style: TextStyles.font14.copyWith(
                              color: colors.onSurfaceVariant,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                          Text(
                            '${coursesProgress.progressPercentage ?? 0}%',
                            style: TextStyles.font14.copyWith(
                              color: colors.primary,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ],
                      ),

                      VerticalSpace(height: 8),

                      ClipRRect(
                        borderRadius: BorderRadius.circular(100),
                        child: LinearProgressIndicator(
                          value: progress,
                          minHeight: 8.h,
                          backgroundColor: colors.surfaceContainerHighest,
                          valueColor: AlwaysStoppedAnimation(colors.primary),
                        ),
                      ),

                      const Spacer(),

                      CustomButton(
                        title: (coursesProgress.progressPercentage ?? 0) == 0
                            ? 'Start Learning'
                            : 'Continue Learning',
                        height: 50.h,
                        onPressed: () {
                          NavigationService.instance.navigateTo(
                            CourseLearningScreen(
                              courseId: coursesProgress.courseId,
                              courseTitle: coursesProgress.courseTitle,
                            ),
                          );
                        },
                      ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
