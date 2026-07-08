import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/features/home/data/models/get_availble_exams_response_model.dart';

import '../../../core/theming/colors.dart';
import '../../../core/theming/styles.dart';
import '../../../core/widgets/custom_button.dart';

class AvailableExamCard extends StatelessWidget {
  final AvailableExamsItemModel? examModel;
  const AvailableExamCard({super.key, this.examModel});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: EdgeInsets.symmetric(vertical: 16.h, horizontal: 8.w),
      decoration: BoxDecoration(
        border: Border.all(width: 1, color: ColorsManager.lightGray),
        borderRadius: BorderRadius.circular(24.r),
      ),
      child: Row(
        spacing: 16.w,
        children: [
          CircleAvatar(
            backgroundColor: ColorsManager.customGreen,
            radius: 25.r,
            child: Icon(
              Icons.play_circle_outlined,
              size: 25.w,
              color: Colors.green,
            ),
          ),
          Flexible(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  examModel!.title ?? '',
                  style: TextStyles.font15.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                  overflow: TextOverflow.ellipsis,
                ),
                Text(
                  examModel!.startTime ?? '',
                  style: TextStyles.font14.copyWith(
                    fontWeight: FontWeight.w500,
                    color: ColorsManager.darkGray,
                  ),
                ),
              ],
            ),
          ),
          CustomButton(
            title: 'Start Exam',
            width: 97.w,
            height: 35.h,
            borderRadius: BorderRadius.circular(14.r),
          ),
        ],
      ),
    );
  }
}
