import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/di/dependency_injection.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/core/helpers/space_helper.dart';
import 'package:graduation_app/core/widgets/custom_button.dart';
import 'package:graduation_app/features/courses/logic/courses_cubit.dart';
import 'package:graduation_app/features/courses/screens/Widgets/add_course_to_cart_bloc_listener.dart';
import 'package:graduation_app/features/courses/screens/Widgets/details_screen_lectures_bloc_builder.dart';
import '../../../core/networking/api_constants.dart';
import '../../../core/services/navigation/navigation_service.dart';
import '../../../core/theming/styles.dart';
import '../data/models/get_all_courses_response_model.dart';
import 'course_learning_screen.dart';

class CourseDetailsScreen extends StatelessWidget {
  final AllCoursesItemModel courseModel;

  const CourseDetailsScreen({super.key, required this.courseModel});

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return BlocProvider(
      create: (context) =>
          getIt<CoursesCubit>()..getCourseLectures(courseModel.courseId ?? ""),
      child: Builder(
        builder: (context) {
          return Scaffold(
            backgroundColor: colorScheme.surface,
            body: Padding(
              padding: EdgeInsets.only(bottom: 16.h),
              child: Column(
                children: [
                  // --- Hero image ---
                  Stack(
                    children: [
                      SizedBox(
                        height: 220.h,
                        width: double.infinity,
                        child: ClipRRect(
                          borderRadius: BorderRadius.only(
                            bottomRight: Radius.circular(24.r),
                            bottomLeft: Radius.circular(24.r),
                          ),
                          child: CachedNetworkImage(
                            imageUrl:
                                '${ApiConstants.baseImageUrl}${courseModel.thumbnailUrl}',
                            fit: BoxFit.cover,
                            width: double.infinity,
                            height: double.infinity,
                            placeholder: (context, url) => Container(
                              color: colorScheme.surfaceContainerHighest,
                              child: Center(
                                child: CircularProgressIndicator(
                                  color: colorScheme.primary,
                                ),
                              ),
                            ),
                            errorWidget: (context, url, error) => Container(
                              color: colorScheme.surfaceContainerHighest,
                              child: Icon(
                                Icons.broken_image_outlined,
                                color: colorScheme.error,
                                size: 32.sp,
                              ),
                            ),
                          ),
                        ),
                      ),
                      // Subtle gradient so a back button / status bar text
                      // stays readable on any image, light or dark theme.
                      Positioned.fill(
                        child: ClipRRect(
                          borderRadius: BorderRadius.only(
                            bottomRight: Radius.circular(24.r),
                            bottomLeft: Radius.circular(24.r),
                          ),
                          child: DecoratedBox(
                            decoration: BoxDecoration(
                              gradient: LinearGradient(
                                begin: Alignment.topCenter,
                                end: Alignment.bottomCenter,
                                colors: [
                                  Colors.black.withValues(alpha: 0.25),
                                  Colors.transparent,
                                ],
                                stops: const [0.0, 0.4],
                              ),
                            ),
                          ),
                        ),
                      ),
                    ],
                  ),

                  Expanded(
                    child: SingleChildScrollView(
                      child: Container(
                        margin: EdgeInsets.only(bottom: 20.h),
                        padding: EdgeInsets.symmetric(
                          horizontal: 16.w,
                          vertical: 16.h,
                        ),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              courseModel.title ?? '',
                              style: TextStyles.font24.copyWith(
                                color: colorScheme.onSurface,
                                fontWeight: FontWeight.w700,
                              ),
                            ),
                            VerticalSpace(height: 16),
                            Text(
                              'By ${courseModel.teacherName ?? ''} ',
                              style: TextStyles.font15.copyWith(
                                fontWeight: FontWeight.w700,
                                color: colorScheme.onSurfaceVariant,
                              ),
                            ),
                            VerticalSpace(height: 20),
                            Text(
                              'About Course',
                              style: TextStyles.font18.copyWith(
                                color: colorScheme.onSurface,
                              ),
                            ),
                            VerticalSpace(height: 8),
                            Text(
                              courseModel.description ?? '',
                              style: TextStyles.font14.copyWith(
                                fontWeight: FontWeight.w400,
                                color: colorScheme.onSurfaceVariant,
                                height: 1.5,
                              ),
                            ),
                            VerticalSpace(height: 20),

                            SingleChildScrollView(
                              scrollDirection: Axis.horizontal,
                              child: Row(
                                spacing: 8.w,
                                children: [
                                  CourseContentCard(
                                    icon: Icons.play_circle_outline,
                                    title: 'Lectures',
                                    number: courseModel.lectureCount ?? 0,
                                  ),
                                  CourseContentCard(
                                    icon: Icons.star_border_rounded,
                                    title: 'Reviews',
                                    number: courseModel.reviewCount ?? 0,
                                  ),
                                  CourseContentCard(
                                    icon: Icons.people_outline,
                                    title: 'Enrollments',
                                    number: courseModel.enrollmentCount ?? 0,
                                  ),
                                ],
                              ),
                            ),
                            VerticalSpace(height: 20.h),
                            Text(
                              'Course content',
                              style: TextStyles.font18.copyWith(
                                color: colorScheme.onSurface,
                              ),
                            ),
                            VerticalSpace(height: 16.h),
                            DetailsScreenLecturesBlocBuilder(),
                            VerticalSpace(height: 20),
                            Divider(
                              height: 1,
                              color: colorScheme.outlineVariant,
                            ),
                            VerticalSpace(height: 8),
                          ],
                        ),
                      ),
                    ),
                  ),

                  // --- Bottom action bar ---
                  Container(
                    padding: EdgeInsets.symmetric(
                      horizontal: 16.w,
                      vertical: 12.h,
                    ),
                    decoration: BoxDecoration(
                      color: colorScheme.surface,
                      boxShadow: [
                        BoxShadow(
                          color: colorScheme.shadow.withValues(alpha: 0.08),
                          blurRadius: 12,
                          offset: const Offset(0, -2),
                        ),
                      ],
                    ),
                    child: courseModel.isEnrolled == false
                        ? Row(
                            spacing: 16.w,
                            mainAxisAlignment: MainAxisAlignment.start,
                            children: [
                              Column(
                                spacing: 4.h,
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    'Price',
                                    style: TextStyles.font14.copyWith(
                                      fontWeight: FontWeight.w500,
                                      color: colorScheme.onSurfaceVariant,
                                    ),
                                  ),
                                  Text(
                                    '\$${courseModel.price ?? 0}',
                                    style: TextStyles.font18.copyWith(
                                      color: colorScheme.primary,
                                      fontWeight: FontWeight.w700,
                                    ),
                                  ),
                                ],
                              ),
                              Expanded(
                                child: CustomButton(
                                  title: 'Add To Cart',
                                  borderRadius: BorderRadius.circular(24.r),
                                  onPressed: () async {
                                    await context
                                        .read<CoursesCubit>()
                                        .addCourseToCart(
                                          courseModel.courseId ?? '',
                                        );
                                  },
                                ),
                              ),
                            ],
                          )
                        : SizedBox(
                            width: double.infinity,
                            child: CustomButton(
                              title: 'Continue',
                              borderRadius: BorderRadius.circular(24.r),
                              color: context.colors.secondary,
                              onPressed: () {
                                NavigationService.instance.navigateTo(
                                  CourseLearningScreen(
                                    courseId: courseModel.courseId,
                                    courseTitle: courseModel.title,
                                  ),
                                );
                              },
                            ),
                          ),
                  ),
                  AddCourseToCartBlocListener(),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}

class CourseContentCard extends StatelessWidget {
  final IconData icon;
  final String title;
  final int number;

  const CourseContentCard({
    super.key,
    required this.icon,
    required this.title,
    required this.number,
  });

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return Container(
      constraints: BoxConstraints(minWidth: 96.w),
      padding: EdgeInsets.symmetric(horizontal: 16.w, vertical: 12.h),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(14.r),
        color: colorScheme.surfaceContainerHigh,
        border: Border.all(color: colorScheme.outlineVariant, width: 1),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        spacing: 6.h,
        children: [
          Icon(icon, size: 20.sp, color: colorScheme.primary),
          Text(
            '$number',
            style: TextStyles.font16.copyWith(
              fontWeight: FontWeight.w700,
              color: colorScheme.onSurface,
            ),
          ),
          Text(
            title,
            style: TextStyles.font14.copyWith(
              color: colorScheme.onSurfaceVariant,
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }
}
