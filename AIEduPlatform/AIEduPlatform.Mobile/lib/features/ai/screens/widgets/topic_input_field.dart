import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_svg/svg.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/core/helpers/space_helper.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/core/theming/styles.dart';
import 'package:graduation_app/core/widgets/custom_button.dart';

class TopicInputField extends StatelessWidget {
  final String hintText;
  final String buttonText;
  final VoidCallback onPressed;
  final TextEditingController controller;

  const TopicInputField({
    super.key,
    required this.hintText,
    required this.buttonText,
    required this.onPressed,
    required this.controller,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      color: context.colors.surface,
      padding: EdgeInsets.symmetric(vertical: 8.h),
      child: Column(
        children: [
          Container(
            width: double.infinity,
            height: 57.h,
            padding: EdgeInsets.symmetric(horizontal: 18.w, vertical: 8.h),
            decoration: BoxDecoration(
              color: context.colors.surface,
              borderRadius: BorderRadius.circular(16.r),
              border: Border.all(width: 1, color: ColorsManager.lightGray),
            ),
            child: TextFormField(
              controller: controller,
              decoration: InputDecoration(
                hintText: hintText,
                border: InputBorder.none,
              ),
            ),
          ),

          VerticalSpace(height: 24),

          CustomButton(
            title: '',
            body: Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                SvgPicture.asset('assets/svgs/stars.svg'),
                SizedBox(width: 8.w),
                Text(
                  buttonText,
                  style: TextStyles.font17.copyWith(
                    fontWeight: FontWeight.bold,
                    color: ColorsManager.white,
                  ),
                ),
              ],
            ),
            onPressed: onPressed,
          ),
        ],
      ),
    );
  }
}
