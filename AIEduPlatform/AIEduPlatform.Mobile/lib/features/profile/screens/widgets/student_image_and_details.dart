import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/features/profile/data/models/my_profile_model.dart';

import '../../../../core/helpers/space_helper.dart';
import '../../../../core/theming/colors.dart';
import '../../../../core/theming/styles.dart';
class StudentImageAndDetails extends StatelessWidget {
  final MyProfileData profileData;
  const StudentImageAndDetails({
    super.key, required this.profileData,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      alignment: Alignment.center,
      width: 375.w,
      child: Column(
        children: [
          Stack(
            children: [
              CircleAvatar(
                radius: 55.r,
                backgroundColor: ColorsManager.mainBlue,
                //backgroundImage: AssetImage('assets/images/avatar.png'),
                child: Text('${profileData.firstName!.toUpperCase().substring(0,1)}${profileData.lastName!.toUpperCase().substring(0,1)}',style: TextStyles.font24,),
              ),
              // Positioned(
              //   bottom: 6,
              //   right: 0,
              //   child: CircleAvatar(
              //       radius: 20.r,
              //       backgroundColor: ColorsManager.mainBlue,
              //       child: IconButton(onPressed: (){}, icon: Icon(Icons.edit,color: ColorsManager.white,size: 25.w,))
              //   ),
              // )
            ],
          ),
          VerticalSpace(height: 16),
          Text('${profileData.firstName?? ''} ${profileData.lastName ??''}',style: TextStyles.font24,),
          Text(profileData.bio ?? 'Computer Science Student',style: TextStyles.font16.copyWith(fontWeight: FontWeight.w500,color: ColorsManager.darkGray),),
          Text('University of Technology',style: TextStyles.font14.copyWith(fontWeight: FontWeight.w400,color: ColorsManager.darkGray),),
        ],
      ),

    );
  }
}
