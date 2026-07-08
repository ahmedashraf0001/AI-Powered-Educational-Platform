import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/space_helper.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/core/theming/styles.dart';
import 'package:graduation_app/features/courses/data/models/get_course_lectures_response_model.dart';
import 'package:graduation_app/features/courses/screens/Widgets/pdf_preview_widget.dart';

class LessonMaterialItemCard extends StatelessWidget {
  final CourseLectureMaterials lessonMaterial;
  const LessonMaterialItemCard({super.key, required this.lessonMaterial});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: () {
        showModalBottomSheet(
          context: context,
          isScrollControlled: true,
          useSafeArea: true,
          builder: (modalContext) {
            return SizedBox(
              height: MediaQuery.of(context).size.height * 0.85,
              child: PdfPreviewWidget(pdfUrl: lessonMaterial.streamUrl ?? ''),
            );
          },
        );
      },
      child: Container(
        padding: EdgeInsets.symmetric(horizontal: 12.w, vertical: 20.h),
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(24.r),
          border: Border.all(width: 1, color: ColorsManager.gray),
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
                  lessonMaterial.title ?? 'empty',
                  style: TextStyles.font14.copyWith(
                    fontWeight: FontWeight.w600,
                  ),
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
            Text(
              'Open',
              style: TextStyles.font14.copyWith(
                fontWeight: FontWeight.w600,
                color: ColorsManager.mainBlue,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
