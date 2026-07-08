import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import '../../../../core/helpers/space_helper.dart';
import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';
import '../../../../core/widgets/custom_button.dart';

class StartAiChat extends StatelessWidget {
  const StartAiChat({
    super.key,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: EdgeInsets.only(bottom: 32.h),
      padding: EdgeInsets.symmetric(horizontal: 24.w,vertical: 24.h),
      width: 327.w,
      height: 183.h,
      decoration: BoxDecoration(
        color: ColorsManager.mainBlue,
        borderRadius: BorderRadius.circular(24.r),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Start AI Chat',style: TextStyles.font20.copyWith(color: ColorsManager.white),),
          VerticalSpace(height: 4),
          Flexible(
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text('Instant answers for your\nhomework',style: TextStyles.font14.copyWith(color: ColorsManager.white),),
                CircleAvatar(
                  radius: 30.r,
                  backgroundColor: ColorsManager.white.withValues(alpha: 0.20),
                  child: Icon(Icons.chat_rounded,size: 26.w,color: ColorsManager.white,),
                ),
              ],
            ),
          ),
          CustomButton(title: 'Chat Now',color: ColorsManager.white,textColor: ColorsManager.mainBlue,width: 123.w,height: 37.h,),


        ],
      ),
    );
  }
}
