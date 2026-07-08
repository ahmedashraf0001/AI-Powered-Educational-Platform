import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/features/courses/data/models/get_all_courses_response_model.dart';

import 'course_catalog_card.dart';

class CourseCatalogListView extends StatelessWidget {
  final List<AllCoursesItemModel> coursesList;
  const CourseCatalogListView({super.key, required this.coursesList});

  @override
  Widget build(BuildContext context) {
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
                    'no available courses',
                    textAlign: TextAlign.center,
                    style: TextStyle(fontSize: 13.sp, color: Colors.grey),
                  ),
                ],
              ),
            ),
          )
        : ListView.builder(
            physics: const AlwaysScrollableScrollPhysics(),
            itemCount: coursesList.length,
            padding: EdgeInsets.zero,
            itemBuilder: (context, index) {
              return Padding(
                padding: EdgeInsets.only(bottom: 16.h),
                child: CourseCatalogCard(courseModel: coursesList[index]),
              );
            },
          );
  }
}
