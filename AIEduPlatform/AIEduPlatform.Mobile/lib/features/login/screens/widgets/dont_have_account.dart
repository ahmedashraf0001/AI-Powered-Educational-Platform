import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../../../../core/services/navigation/navigation_service.dart';
import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';
import '../../../register/screens/register_screen.dart';

class DontHaveAccountText extends StatelessWidget {
  const DontHaveAccountText({
    super.key,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      spacing: 3.w,
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Text('Don\'t have an account?',style: TextStyles.font14,),
        GestureDetector(
          onTap: (){
            NavigationService.instance.navigateTo(RegisterScreen());
          },
          child: Text('Sign Up',style: TextStyles.font14.copyWith(fontWeight: FontWeight.bold,color: ColorsManager.mainBlue),),
        ),
      ],
    );
  }
}