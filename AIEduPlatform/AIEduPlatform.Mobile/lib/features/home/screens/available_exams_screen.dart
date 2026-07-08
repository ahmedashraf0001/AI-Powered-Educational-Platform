import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/core/theming/styles.dart';
import 'package:graduation_app/features/home/data/models/get_availble_exams_response_model.dart';
import 'package:graduation_app/features/home/screens/widgets/home_available_exams_card.dart';

class AvailableExamsScreen extends StatelessWidget {
  final List<AvailableExamsItemModel>? examsItemsList;
  const AvailableExamsScreen({super.key, this.examsItemsList});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        centerTitle: true,
        title: Text(
          'Available Exams',
          style: TextStyles.font20.copyWith(color: ColorsManager.mainBlue),
        ),
      ),
      body: Padding(
        padding: EdgeInsets.symmetric(horizontal: 16.w, vertical: 16.h),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Flexible(
              child: examsItemsList!.isEmpty
                  ? SizedBox(
                      child: Center(
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Icon(
                              Icons.quiz_outlined,
                              size: 50.sp,
                              color: Colors.grey,
                            ),
                            SizedBox(height: 12.h),
                            Text(
                              'No Exams yet',
                              style: TextStyle(
                                fontSize: 18.sp,
                                fontWeight: FontWeight.w600,
                                color: Colors.grey[700],
                              ),
                            ),
                            SizedBox(height: 6.h),
                            Text(
                              'When exams are added, they will appear here automatically.',
                              textAlign: TextAlign.center,
                              style: TextStyle(
                                fontSize: 15.sp,
                                color: Colors.grey,
                              ),
                            ),
                          ],
                        ),
                      ),
                    )
                  : ListView.builder(
                      itemBuilder: (context, index) {
                        return Padding(
                          padding: EdgeInsets.only(bottom: 16.h),
                          child: HomeAvailableExamCard(
                            examModel: examsItemsList![index],
                          ),
                        );
                      },
                      itemCount: examsItemsList!.length,
                    ),
            ),
          ],
        ),
      ),
    );
  }
}
