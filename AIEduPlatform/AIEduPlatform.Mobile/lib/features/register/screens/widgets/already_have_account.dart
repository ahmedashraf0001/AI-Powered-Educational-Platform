import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import '../../../../core/services/navigation/navigation_service.dart';
import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';
import '../../../login/screens/login_screen.dart';

class AlreadyHaveAccount extends StatelessWidget {
  const AlreadyHaveAccount({
    super.key,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      spacing: 3.w,
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Text('Already have an account? ',style: TextStyles.font14,),
        GestureDetector(
          onTap: (){
            NavigationService.instance.navigateTo(LoginScreen());
          },
          child: Text('Log In',style: TextStyles.font14.copyWith(fontWeight: FontWeight.bold,color: ColorsManager.mainBlue),),
        ),
      ],
    );
  }
}