import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/extensions.dart';
import 'package:graduation_app/core/helpers/space_helper.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/core/theming/styles.dart';
import 'package:graduation_app/core/widgets/custom_button.dart';
import 'package:graduation_app/features/ai/logic/ai_services_cubit/cubit/ai_services_cubit.dart';
import 'package:graduation_app/features/ai/logic/ai_services_cubit/cubit/ai_services_state.dart';
import 'package:graduation_app/features/ai/screens/widgets/back_flash_card_widget.dart';
import 'package:graduation_app/features/ai/screens/widgets/front_flash_card_widget.dart';
import 'package:graduation_app/features/ai/screens/widgets/topic_input_field.dart';
import 'package:skeletonizer/skeletonizer.dart';

class FlashCardsScreen extends StatefulWidget {
  final String sessionId;
  final String courseName;
  const FlashCardsScreen({
    super.key,
    required this.sessionId,
    required this.courseName,
  });

  @override
  State<FlashCardsScreen> createState() => _FlashCardsScreenState();
}

class _FlashCardsScreenState extends State<FlashCardsScreen>
    with TickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _animation;
  bool _isFront = true;

  int index = 0;
  final TextEditingController _topicController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _topicController.text = widget.courseName;
    _controller = AnimationController(
      duration: Duration(milliseconds: 600),
      vsync: this,
    );

    _animation = Tween<double>(
      begin: 0,
      end: 1,
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.easeInOut));
  }

  void _toggleCard() {
    if (_isFront) {
      _controller.forward();
    } else {
      _controller.reverse();
    }
    setState(() {
      _isFront = !_isFront;
    });
  }

  @override
  void dispose() {
    _controller.dispose();
    _topicController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(
          'AI Flashcards',
          style: TextStyles.font20.copyWith(color: ColorsManager.mainBlue),
        ),
        centerTitle: true,
      ),
      body: Padding(
        padding: EdgeInsets.symmetric(horizontal: 16.w).copyWith(bottom: 20.h),
        child: Column(
          children: [
            TopicInputField(
              hintText: 'Enter topic for flashcards',
              buttonText: 'Generate Flashcards',
              onPressed: () {
                if (_topicController.text.trim().isEmpty) {
                  return;
                }

                index = 0;

                context.read<AiServicesCubit>().generateFlashCards(
                  _topicController.text.trim(),
                  widget.sessionId,
                );
              },
              controller: _topicController,
            ),

            VerticalSpace(height: 16),

            Expanded(
              child: BlocBuilder<AiServicesCubit, AiServicesState>(
                buildWhen: (previous, current) =>
                    current is SuccessFlashCards ||
                    current is LoadingFlashCards ||
                    current is FailureFlashCards,
                builder: (context, state) {
                  if (state is LoadingFlashCards) {
                    return Skeletonizer(
                      enabled: true,
                      enableSwitchAnimation: true,
                      child: Container(
                        alignment: Alignment.center,
                        padding: EdgeInsets.symmetric(horizontal: 16.w),
                        width: double.infinity,
                        height: 370.h,
                        decoration: BoxDecoration(
                          color: context.colors.surface,
                          borderRadius: BorderRadius.circular(24.r),
                        ),
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Text(
                              'loading loading loading loading loading',
                              style: TextStyles.font20.copyWith(
                                color: Colors.grey.shade400,
                              ),
                              textAlign: TextAlign.center,
                              maxLines: 11,
                              overflow: TextOverflow.ellipsis,
                            ),
                            VerticalSpace(height: 20.h),
                            Row(
                              mainAxisAlignment: MainAxisAlignment.center,
                              spacing: 7.w,
                              children: [
                                Icon(
                                  Icons.touch_app_outlined,
                                  color: Colors.grey.shade400,
                                ),
                                Text(
                                  'loading loading loading loading loading',
                                  style: TextStyles.font15.copyWith(
                                    fontWeight: FontWeight.w500,
                                    color: Colors.grey.shade400,
                                  ),
                                ),
                              ],
                            ),
                          ],
                        ),
                      ),
                    );
                  } else if (state is SuccessFlashCards) {
                    return SingleChildScrollView(
                      physics: BouncingScrollPhysics(),
                      child: Column(
                        children: [
                          Container(
                            padding: EdgeInsets.symmetric(
                              horizontal: 24.w,
                            ).copyWith(bottom: 16.h),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              spacing: 8.h,
                              children: [
                                Text(
                                  'Current Session',
                                  style: TextStyles.font13.copyWith(
                                    fontWeight: FontWeight.bold,
                                    color: ColorsManager.darkGray,
                                  ),
                                ),
                                Row(
                                  mainAxisAlignment:
                                      MainAxisAlignment.spaceBetween,
                                  children: [
                                    Text(
                                      state.dataList.first.topic ?? '',
                                      style: TextStyles.font16.copyWith(
                                        fontWeight: FontWeight.w500,
                                      ),
                                    ),
                                    Container(
                                      height: 28.h,
                                      width: 70.w,
                                      alignment: Alignment.center,
                                      decoration: BoxDecoration(
                                        color: context.colors.surface,
                                        border: Border.all(
                                          width: 1,
                                          color: ColorsManager.lightGray,
                                        ),
                                        borderRadius: BorderRadius.circular(
                                          7.r,
                                        ),
                                      ),
                                      child: Text(
                                        '${index + 1} of ${state.dataList.length}',
                                        style: TextStyles.font14.copyWith(
                                          fontWeight: FontWeight.bold,
                                          color: ColorsManager.mainBlue,
                                        ),
                                      ),
                                    ),
                                  ],
                                ),
                              ],
                            ),
                          ),
                          GestureDetector(
                            onTap: _toggleCard,
                            child: AnimatedBuilder(
                              animation: _animation,
                              builder: (context, child) {
                                return Transform(
                                  transform: Matrix4.rotationY(
                                    _animation.value * 3.14159,
                                  ),
                                  alignment: Alignment.center,
                                  child: _animation.value < 0.5
                                      ? FrontFlashCardWidget(
                                          flashCardModel: state.dataList[index],
                                        )
                                      : Transform.scale(
                                          scaleX: -1,
                                          scaleY: 1,
                                          child: BackFlashCardWidget(
                                            flashCardModel:
                                                state.dataList[index],
                                          ),
                                        ),
                                );
                              },
                            ),
                          ),
                          VerticalSpace(height: 30),
                          Row(
                            spacing: 16.w,
                            children: [
                              Expanded(
                                child: CustomButton(
                                  onPressed: () {
                                    setState(() {
                                      if (index == 0) return;
                                      index--;
                                      _controller.reset();
                                      _isFront = true;
                                    });
                                  },
                                  title: '',

                                  body: Column(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    spacing: 4.h,
                                    children: [
                                      Icon(
                                        Icons.arrow_back_ios_rounded,
                                        color: ColorsManager.darkGray,
                                      ),
                                      Text(
                                        'Previous Card',
                                        style: TextStyles.font17.copyWith(
                                          fontWeight: FontWeight.bold,
                                          color: ColorsManager.darkGray,
                                        ),
                                      ),
                                    ],
                                  ),
                                  color: ColorsManager.gray,
                                  textColor: ColorsManager.darkGray,
                                  height: 66.h,
                                  borderRadius: BorderRadius.circular(24.r),
                                ),
                              ),
                              Expanded(
                                child: CustomButton(
                                  title: '',
                                  body: Column(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    spacing: 4.h,
                                    children: [
                                      Icon(
                                        Icons.arrow_forward_ios_rounded,
                                        color: ColorsManager.white,
                                      ),
                                      Text(
                                        'Next Card',
                                        style: TextStyles.font17.copyWith(
                                          fontWeight: FontWeight.bold,
                                          color: ColorsManager.white,
                                        ),
                                      ),
                                    ],
                                  ),
                                  color: ColorsManager.mainBlue,
                                  textColor: ColorsManager.white,
                                  height: 66.h,
                                  borderRadius: BorderRadius.circular(24.r),
                                  onPressed: () {
                                    setState(() {
                                      if (index >= state.dataList.length - 1)
                                        return;
                                      index++;
                                      _controller.reset();
                                      _isFront = true;
                                    });
                                  },
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                    );
                  } else if (state is FailureFlashCards) {
                    return Center(child: Text(state.message ?? 'error'));
                  } else {
                    // Idle state: fake placeholder card shown before first generation
                    return Container(
                      alignment: Alignment.center,
                      padding: EdgeInsets.symmetric(horizontal: 24.w),
                      width: double.infinity,
                      height: 370.h,
                      decoration: BoxDecoration(
                        color: context.colors.surface,
                        borderRadius: BorderRadius.circular(24.r),
                        border: Border.all(
                          color: ColorsManager.lightGray,
                          width: 1,
                          style: BorderStyle.solid,
                        ),
                      ),
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(
                            Icons.style_outlined,
                            size: 56.sp,
                            color: ColorsManager.mainBlue.withOpacity(0.4),
                          ),
                          VerticalSpace(height: 16.h),
                          Text(
                            'Your flashcards will show up here',
                            style: TextStyles.font18.copyWith(
                              fontWeight: FontWeight.w600,
                              color: ColorsManager.darkGray,
                            ),
                            textAlign: TextAlign.center,
                          ),
                          VerticalSpace(height: 8.h),
                          Text(
                            'Enter a topic above and tap "Generate Flashcards" to get started',
                            style: TextStyles.font14.copyWith(
                              color: Colors.grey.shade500,
                            ),
                            textAlign: TextAlign.center,
                          ),
                          VerticalSpace(height: 20.h),
                          Row(
                            mainAxisAlignment: MainAxisAlignment.center,
                            spacing: 7.w,
                            children: [
                              Icon(
                                Icons.touch_app_outlined,
                                color: Colors.grey.shade400,
                                size: 20.sp,
                              ),
                              Text(
                                'Tap Generate to begin',
                                style: TextStyles.font15.copyWith(
                                  fontWeight: FontWeight.w500,
                                  color: Colors.grey.shade400,
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                    );
                  }
                },
              ),
            ),
          ],
        ),
      ),
    );
  }
}
