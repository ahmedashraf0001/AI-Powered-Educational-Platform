import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_svg/svg.dart';

import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';

class StudyToolsCard extends StatelessWidget {
  final String title;
  final String subTitle;
  final String image;
  final Color color;

  const StudyToolsCard({
    super.key, required this.title, required this.subTitle, required this.image, required this.color,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: EdgeInsets.symmetric(vertical: 18.h,horizontal: 12.w),
      decoration: BoxDecoration(
        border: Border.all(width: 1,color: ColorsManager.gray),
        borderRadius: BorderRadius.circular(24.r),
      ),
      child: Row(
        spacing: 16.w,
        children: [
          Container(
            alignment: Alignment.center,
            padding: EdgeInsets.symmetric(vertical:7.h),
            width: 48.w,
            height: 48.h,
            decoration: BoxDecoration(
              color: color,
              borderRadius: BorderRadius.circular(16.r),
            ),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                SvgPicture.asset(image,height: 20.h,width: 19.w,),
              ],
            ),
          ),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(title,style: TextStyles.font15.copyWith(fontWeight: FontWeight.bold),),
              Text(subTitle,style: TextStyles.font14.copyWith(fontWeight: FontWeight.w500,color: ColorsManager.darkGray),),
            ],
          ),
          const Spacer(),
          IconButton(
            padding: EdgeInsets.zero,
            alignment: Alignment.centerRight,
            onPressed: (){},
            icon: Icon(Icons.arrow_forward_ios_rounded,size: 17.w,color: ColorsManager.darkGray,),
          ),
        ],
      ),
    );
  }
}
