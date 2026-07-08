import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/space_helper.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/core/theming/styles.dart';
import 'package:graduation_app/features/ai/data/models/flash_cards_response_model.dart';

class BackFlashCardWidget extends StatelessWidget {
  final FlashCardModel flashCardModel;
  const BackFlashCardWidget({super.key, required this.flashCardModel});

  @override
  Widget build(BuildContext context) {
    return Container(
      alignment: Alignment.center,
      padding: EdgeInsets.symmetric(horizontal: 16.w),
      width: double.infinity,
      height: 370.h,
      decoration: BoxDecoration(
        color: ColorsManager.red,
        borderRadius: BorderRadius.circular(24.r),
      ),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Text(
            flashCardModel.backText ?? '',
            style: TextStyles.font20.copyWith(color: ColorsManager.white),
            textAlign: TextAlign.center,
            maxLines: 11,
            overflow: TextOverflow.ellipsis,
          ),
          VerticalSpace(height: 20.h),
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            spacing: 7.w,
            children: [
              Icon(Icons.touch_app_outlined, color: Colors.white),
              Text(
                'Tap to Flip',
                style: TextStyles.font15.copyWith(
                  fontWeight: FontWeight.w500,
                  color: ColorsManager.white,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
