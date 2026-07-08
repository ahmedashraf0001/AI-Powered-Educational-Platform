import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';

import 'available_exam_card.dart';

class AvailableExamsListView extends StatelessWidget {
  const AvailableExamsListView({
    super.key,
  });

  @override
  Widget build(BuildContext context) {
    return ListView.builder(itemBuilder: (context,index){
      return Padding(
        padding: EdgeInsets.only(bottom: 12.h),
        child: AvailableExamCard(),
      );
    },itemCount: 5,);
  }
}
