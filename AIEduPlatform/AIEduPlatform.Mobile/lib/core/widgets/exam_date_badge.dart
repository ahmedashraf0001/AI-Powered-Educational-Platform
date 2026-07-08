import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../theming/colors.dart';
import '../theming/styles.dart';

class ExamDateBadge extends StatelessWidget {
  final String month;
  final String day;
  const ExamDateBadge({
    super.key, required this.month, required this.day,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      alignment: Alignment.center,
      padding: EdgeInsets.symmetric(vertical:7.h),
      width: 48.w,
      decoration: BoxDecoration(
        color: ColorsManager.mainBlue.withValues(alpha: 0.35),
        borderRadius: BorderRadius.circular(16.r),
      ),
      child: Column(
        children: [
          Text(month,style: TextStyles.font14.copyWith(fontWeight: FontWeight.bold,color: ColorsManager.mainBlue),),
          Text(day,style: TextStyles.font14.copyWith(fontWeight: FontWeight.bold,color: ColorsManager.mainBlue),),
        ],
      ),
    );
  }
}
