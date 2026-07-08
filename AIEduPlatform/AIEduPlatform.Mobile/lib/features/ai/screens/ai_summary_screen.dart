import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/core/helpers/space_helper.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/core/theming/styles.dart';
import 'package:graduation_app/features/ai/logic/ai_services_cubit/cubit/ai_services_cubit.dart';
import 'package:graduation_app/features/ai/logic/ai_services_cubit/cubit/ai_services_state.dart';
import 'package:graduation_app/features/ai/screens/widgets/topic_input_field.dart';

class AiSummaryScreen extends StatefulWidget {
  final String sessionId;
  final String courseName;
  const AiSummaryScreen({
    super.key,
    required this.sessionId,
    required this.courseName,
  });

  @override
  State<AiSummaryScreen> createState() => _AiSummaryScreenState();
}

class _AiSummaryScreenState extends State<AiSummaryScreen> {
  final TextEditingController _summaryController = TextEditingController();

  @override
  void initState() {
    _summaryController.text = widget.courseName;
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        centerTitle: true,
        title: Text(
          'Summary',
          style: TextStyles.font20.copyWith(color: ColorsManager.mainBlue),
        ),
      ),

      body: Padding(
        padding: EdgeInsets.symmetric(horizontal: 16.w).copyWith(bottom: 16.h),
        child: Column(
          children: [
            TopicInputField(
              hintText: 'Enter topic to Summarize',
              buttonText: 'Generate Summary',
              onPressed: () {
                if (_summaryController.text.isEmpty) return;
                context.read<AiServicesCubit>().summaryTopic(
                  _summaryController.text.trim(),
                  widget.sessionId,
                );
              },
              controller: _summaryController,
            ),

            VerticalSpace(height: 24),
            Flexible(child: SummaryBlocBuilder()),
          ],
        ),
      ),
    );
  }
}

class SummaryBlocBuilder extends StatelessWidget {
  const SummaryBlocBuilder({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<AiServicesCubit, AiServicesState>(
      buildWhen: (previous, current) =>
          current is SuccessSummaryTopic ||
          current is FailureSummaryTopic ||
          current is LoadingSummaryTopic,
      builder: (context, state) {
        if (state is LoadingSummaryTopic) {
          return Center(child: CircularProgressIndicator());
        } else if (state is SuccessSummaryTopic) {
          return Container(
            padding: EdgeInsets.symmetric(horizontal: 24.w, vertical: 24.h),
            decoration: BoxDecoration(
              border: Border.all(width: 1, color: ColorsManager.lightGray),
              borderRadius: BorderRadius.circular(16.r),
            ),
            child: SingleChildScrollView(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Summary: ${state.dataModel.sourceTitle ?? ''}',
                    style: TextStyles.font20,
                  ),
                  VerticalSpace(height: 16),
                  Text(
                    state.dataModel.summary ?? 'empty',
                    style: TextStyles.font16.copyWith(
                      fontWeight: FontWeight.bold,
                      color: context.colors.onSurface.withValues(alpha: 0.8),
                    ),
                  ),
                  VerticalSpace(height: 16),
                  Text(
                    'Key Points',
                    style: TextStyles.font15.copyWith(
                      fontWeight: FontWeight.bold,
                      color: ColorsManager.mainBlue,
                    ),
                  ),
                  VerticalSpace(height: 12),

                  ListView.builder(
                    physics: NeverScrollableScrollPhysics(),
                    shrinkWrap: true,
                    itemBuilder: (context, index) {
                      return Padding(
                        padding: EdgeInsets.only(bottom: 8.h),
                        child: KeyPointsItem(
                          keyPoint: state.dataModel.keyPoints![index],
                        ),
                      );
                    },
                    itemCount: state.dataModel.keyPoints!.length,
                  ),
                ],
              ),
            ),
          );
        } else if (state is FailureSummaryTopic) {
          return Center(child: Text(state.message ?? 'error'));
        } else {
          return SizedBox.shrink();
        }
      },
    );
  }
}

class KeyPointsItem extends StatelessWidget {
  final String? keyPoint;
  const KeyPointsItem({super.key, this.keyPoint});

  @override
  Widget build(BuildContext context) {
    return Row(
      spacing: 8.w,
      children: [
        Icon(Icons.check_circle, size: 24.h, color: ColorsManager.mainBlue),
        Flexible(
          child: Text(
            keyPoint ?? 'empty',
            maxLines: 4,
            style: TextStyles.font14.copyWith(
              fontWeight: FontWeight.w500,
              color: context.colors.onSurface.withValues(alpha: 0.8),
              overflow: TextOverflow.ellipsis,
            ),
          ),
        ),
      ],
    );
  }
}
