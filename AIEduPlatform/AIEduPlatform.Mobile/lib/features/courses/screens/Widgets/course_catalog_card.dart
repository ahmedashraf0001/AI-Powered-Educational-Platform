import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/networking/api_constants.dart';
import 'package:graduation_app/core/services/navigation/navigation_service.dart';
import 'package:graduation_app/core/widgets/custom_button.dart';
import 'package:graduation_app/features/courses/data/models/get_all_courses_response_model.dart';
import 'package:graduation_app/features/courses/logic/courses_cubit.dart';
import 'package:graduation_app/features/courses/screens/course_details_screen.dart';
import 'package:graduation_app/features/courses/screens/course_learning_screen.dart';

class CourseCatalogCard extends StatelessWidget {
  final AllCoursesItemModel courseModel;

  const CourseCatalogCard({super.key, required this.courseModel});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colors = theme.colorScheme;

    return Material(
      color: Colors.transparent,
      child: InkWell(
        borderRadius: BorderRadius.circular(20.r),
        onTap: () {
          NavigationService.instance.navigateTo(
            CourseDetailsScreen(courseModel: courseModel),
          );
        },
        child: Container(
          decoration: BoxDecoration(
            color: colors.surface,
            borderRadius: BorderRadius.circular(20.r),
            border: Border.all(color: theme.dividerColor.withOpacity(.12)),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withOpacity(
                  theme.brightness == Brightness.dark ? .20 : .05,
                ),
                blurRadius: 18,
                offset: const Offset(0, 8),
              ),
            ],
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              /// IMAGE
              ClipRRect(
                borderRadius: BorderRadius.vertical(top: Radius.circular(20.r)),
                child: SizedBox(
                  height: 180.h,
                  width: double.infinity,
                  child: Stack(
                    fit: StackFit.expand,
                    children: [
                      Hero(
                        tag: courseModel.courseId ?? courseModel.title ?? '',
                        child: CachedNetworkImage(
                          imageUrl:
                              '${ApiConstants.baseImageUrl}${courseModel.thumbnailUrl}',
                          fit: BoxFit.cover,
                          placeholder: (_, __) => Container(
                            color: colors.surfaceContainerHighest,
                            child: const Center(
                              child: CircularProgressIndicator(),
                            ),
                          ),
                          errorWidget: (_, __, ___) => Container(
                            color: colors.surfaceContainerHighest,
                            child: Icon(
                              Icons.broken_image_outlined,
                              size: 50,
                              color: colors.outline,
                            ),
                          ),
                        ),
                      ),

                      /// Gradient
                      Container(
                        decoration: BoxDecoration(
                          gradient: LinearGradient(
                            begin: Alignment.topCenter,
                            end: Alignment.bottomCenter,
                            colors: [
                              Colors.transparent,
                              Colors.black.withOpacity(.55),
                            ],
                          ),
                        ),
                      ),

                      /// Title on image
                      Positioned(
                        left: 16,
                        right: 16,
                        bottom: 16,
                        child: Text(
                          courseModel.title ?? "",
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: 19.sp,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),

                      /// Badge
                      Positioned(
                        top: 14,
                        right: 14,
                        child: Container(
                          padding: EdgeInsets.symmetric(
                            horizontal: 12.w,
                            vertical: 6.h,
                          ),
                          decoration: BoxDecoration(
                            color: courseModel.isEnrolled == true
                                ? Colors.green
                                : colors.primary,
                            borderRadius: BorderRadius.circular(30.r),
                          ),
                          child: Text(
                            courseModel.isEnrolled == true
                                ? "Enrolled"
                                : "Available",
                            style: TextStyle(
                              color: Colors.white,
                              fontSize: 12.sp,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ),

              Padding(
                padding: EdgeInsets.all(16.r),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    /// Teacher
                    Text(
                      "by ${courseModel.teacherName ?? "Unknown"}",
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: colors.onSurfaceVariant,
                      ),
                    ),

                    SizedBox(height: 12.h),

                    /// Stats
                    Row(
                      children: [
                        Icon(
                          Icons.people_alt_outlined,
                          size: 18.sp,
                          color: colors.primary,
                        ),
                        SizedBox(width: 6.w),
                        Text(
                          "${courseModel.enrollmentCount ?? 0} Students",
                          style: theme.textTheme.bodyMedium,
                        ),
                      ],
                    ),

                    SizedBox(height: 20.h),

                    Row(
                      children: [
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                "Price",
                                style: theme.textTheme.bodySmall?.copyWith(
                                  color: colors.onSurfaceVariant,
                                ),
                              ),
                              SizedBox(height: 4.h),
                              Text(
                                "\$${courseModel.price}",
                                style: theme.textTheme.headlineSmall?.copyWith(
                                  fontWeight: FontWeight.bold,
                                  color: colors.primary,
                                ),
                              ),
                            ],
                          ),
                        ),

                        SizedBox(
                          width: 150.w,
                          child: CustomButton(
                            title: courseModel.isEnrolled == true
                                ? "Continue"
                                : "Add To Cart",
                            height: 46.h,
                            borderRadius: BorderRadius.circular(30.r),
                            color: courseModel.isEnrolled == true
                                ? colors.secondary
                                : colors.primary,
                            onPressed: () {
                              if (courseModel.isEnrolled == true) {
                                NavigationService.instance.navigateTo(
                                  CourseLearningScreen(
                                    courseId: courseModel.courseId,
                                    courseTitle: courseModel.title,
                                  ),
                                );
                              } else {
                                context.read<CoursesCubit>().addCourseToCart(
                                  courseModel.courseId ?? '',
                                );
                              }
                            },
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
