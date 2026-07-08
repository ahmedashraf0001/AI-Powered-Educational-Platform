import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

class AiStudyRobotImage extends StatelessWidget {
  const AiStudyRobotImage({
    super.key,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: EdgeInsets.only(top: 16.h,bottom: 32.h),
      width: 327.w,
      height: 183.h,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(24.r),
        image: DecorationImage(image: AssetImage('assets/images/ai_robot.png')),
      ),
    );
  }
}
