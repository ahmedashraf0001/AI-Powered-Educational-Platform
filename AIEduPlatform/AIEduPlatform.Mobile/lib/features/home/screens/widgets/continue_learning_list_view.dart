import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/features/cart/data/models/my_courses_response_model.dart';

import 'learning_course_card.dart';

class ContinueLearningListView extends StatelessWidget {
  final List<CoursesProgress> coursesList;
  const ContinueLearningListView({super.key, required this.coursesList});

  @override
  Widget build(BuildContext context) {
    const cardWidth = 240.0;
    const rightPadding = 16.0;
    final itemExtent = (cardWidth + rightPadding).w;
    return coursesList.isEmpty
        ? SizedBox(
            height: 225.h,
            child: Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(Icons.school_outlined, size: 48.sp, color: Colors.grey),
                  SizedBox(height: 12.h),
                  Text(
                    'No courses yet',
                    style: TextStyle(
                      fontSize: 16.sp,
                      fontWeight: FontWeight.w600,
                      color: Colors.grey[700],
                    ),
                  ),
                  SizedBox(height: 6.h),
                  Text(
                    'Start learning now and explore available courses',
                    textAlign: TextAlign.center,
                    style: TextStyle(fontSize: 13.sp, color: Colors.grey),
                  ),
                ],
              ),
            ),
          )
        : SizedBox(
            height: 235.h,
            child: ListView.builder(
              scrollDirection: Axis.horizontal,
              itemExtent: itemExtent,
              itemBuilder: (context, index) {
                return Padding(
                  padding: EdgeInsets.only(right: 16.w),
                  child: LearningCourseCard(courseModel: coursesList[index]),
                );
              },
              itemCount: coursesList.length,
            ),
          );
  }
}
