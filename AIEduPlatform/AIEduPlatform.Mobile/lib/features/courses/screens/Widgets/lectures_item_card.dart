import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/features/courses/data/models/get_course_lectures_response_model.dart';

import '../../../../core/helpers/space_helper.dart';
import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';

class LecturesItemCard extends StatelessWidget {
  final CourseLectureMaterials lessonMaterial;
  const LecturesItemCard({super.key, required this.lessonMaterial});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.symmetric(horizontal: 12.w, vertical: 20.h),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(24.r),
        border: Border.all(width: 1, color: ColorsManager.lightGray),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.start,
        children: [
          Icon(Icons.picture_as_pdf, size: 24.w, color: ColorsManager.red),
          HorizontalSpace(width: 16),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisAlignment: MainAxisAlignment.start,
            spacing: 4.h,
            children: [
              Text(
                lessonMaterial.title ?? '',
                style: TextStyles.font14.copyWith(fontWeight: FontWeight.w600),
              ),
              Text(
                lessonMaterial.type ?? '',
                style: TextStyles.font12.copyWith(
                  fontWeight: FontWeight.w400,
                  color: ColorsManager.darkGray,
                ),
              ),
            ],
          ),
          Spacer(),
          Icon(Icons.lock_outline, size: 24.w, color: ColorsManager.mainBlue),
        ],
      ),
    );
  }
}
