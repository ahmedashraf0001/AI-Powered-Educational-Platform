import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../../../core/helpers/space_helper.dart';
import '../../../core/theming/colors.dart';
import '../../../core/theming/styles.dart';
import '../../../core/widgets/exam_date_badge.dart';

class UpComingExamCard extends StatelessWidget {
  const UpComingExamCard({
    super.key,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: EdgeInsets.symmetric(horizontal: 16.w,vertical: 16.h),
      width: 280.w,
      height: 205.h,
      decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(24.r),
          border: Border.all(width: 1,color: ColorsManager.gray)
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          ExamDateBadge(month: 'OCT', day: '15'),
          VerticalSpace(height: 12),
          Text('Organic Chemistry Midterm',style: TextStyles.font18,),
          VerticalSpace(height: 4),
          Text('Science & Chemistry',style: TextStyles.font14.copyWith(fontWeight: FontWeight.w500,color: ColorsManager.darkGray),),
          VerticalSpace(height: 4),
          Divider(),
          VerticalSpace(height: 4),
          Text('Online • 02:30 PM',style: TextStyles.font14.copyWith(fontWeight: FontWeight.w500,color: ColorsManager.darkGray),),
        ],
      ),
    );
  }
}
