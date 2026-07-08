import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/space_helper.dart';
import 'package:graduation_app/core/services/navigation/navigation_service.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/core/theming/styles.dart';
import 'package:graduation_app/core/widgets/custom_button.dart';
import 'package:graduation_app/core/widgets/custom_text_field.dart';
import 'package:graduation_app/features/profile/data/models/my_profile_model.dart';
import 'package:graduation_app/features/profile/logic/profile_cubit.dart';
import 'package:graduation_app/features/profile/logic/profile_state.dart';

class EditProifleScreen extends StatefulWidget {
  final MyProfileData profileData;
  const EditProifleScreen({super.key, required this.profileData});

  @override
  State<EditProifleScreen> createState() => _EditProifleScreenState();
}

class _EditProifleScreenState extends State<EditProifleScreen> {
  late final TextEditingController _updateUserName = TextEditingController();
  late final TextEditingController _updateFirstName = TextEditingController();
  late final TextEditingController _updateLastName = TextEditingController();
  late final TextEditingController _updateBio = TextEditingController();

  final _updateFormKey = GlobalKey<FormState>();

  @override
  void initState() {
    _updateUserName.text = widget.profileData.userName ?? '';
    _updateFirstName.text = widget.profileData.firstName ?? '';
    _updateLastName.text = widget.profileData.lastName ?? '';
    _updateBio.text = widget.profileData.bio ?? '';
    super.initState();
  }

  @override
  void dispose() {
    _updateUserName.dispose();
    _updateFirstName.dispose();
    _updateLastName.dispose();
    _updateBio.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;
    final isDark = theme.brightness == Brightness.dark;

    // Adaptive surfaces — fall back gracefully if ColorsManager doesn't
    // define dark variants.
    final scaffoldBg = colorScheme.surface;
    final cardBg = isDark ? colorScheme.surfaceContainerHigh : Colors.white;
    final labelColor = colorScheme.onSurface;
    final subtleColor = colorScheme.onSurfaceVariant;
    final accent = isDark ? colorScheme.primary : ColorsManager.mainBlue;

    String initials() {
      final f = widget.profileData.firstName?.trim() ?? '';
      final l = widget.profileData.lastName?.trim() ?? '';
      final letters = '${f.isNotEmpty ? f[0] : ''}${l.isNotEmpty ? l[0] : ''}';
      return letters.isEmpty ? '?' : letters.toUpperCase();
    }

    return Scaffold(
      backgroundColor: scaffoldBg,
      appBar: AppBar(
        backgroundColor: scaffoldBg,
        elevation: 0,
        scrolledUnderElevation: 0,
        centerTitle: true,
        title: Text(
          'Edit Profile',
          style: TextStyles.font20.copyWith(
            color: labelColor,
            fontWeight: FontWeight.w700,
          ),
        ),
      ),
      body: SafeArea(
        child: BlocConsumer<ProfileCubit, ProfileState>(
          listener: (context, state) {
            if (state is SuccesUpdateProfile) {
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  behavior: SnackBarBehavior.floating,
                  backgroundColor: Colors.green.shade600,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12.r),
                  ),
                  content: const Text('Profile updated successfully'),
                ),
              );
              NavigationService.instance.goBack();
            }

            if (state is FailureUpdateProfile) {
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  behavior: SnackBarBehavior.floating,
                  backgroundColor: Colors.red.shade600,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12.r),
                  ),
                  content: Text(state.message ?? 'Something went wrong'),
                ),
              );
            }
          },
          builder: (context, state) {
            final isLoading = state is LoadingUpdateProfile;

            return SingleChildScrollView(
              padding: EdgeInsets.fromLTRB(16.w, 8.h, 16.w, 24.h),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.center,
                children: [
                  // Avatar header
                  Stack(
                    clipBehavior: Clip.none,
                    children: [
                      Container(
                        width: 96.w,
                        height: 96.w,
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          gradient: LinearGradient(
                            begin: Alignment.topLeft,
                            end: Alignment.bottomRight,
                            colors: [accent, accent.withValues(alpha: 0.6)],
                          ),
                          border: Border.all(color: cardBg, width: 3.w),
                          boxShadow: [
                            BoxShadow(
                              color: accent.withValues(
                                alpha: isDark ? 0.25 : 0.18,
                              ),
                              blurRadius: 16,
                              offset: const Offset(0, 6),
                            ),
                          ],
                        ),
                        alignment: Alignment.center,
                        child: Text(
                          initials(),
                          style: TextStyles.font20.copyWith(
                            color: Colors.white,
                            fontWeight: FontWeight.w700,
                            fontSize: 28.sp,
                          ),
                        ),
                      ),
                    ],
                  ),

                  VerticalSpace(height: 24),

                  // Form card
                  Container(
                    width: double.infinity,
                    padding: EdgeInsets.all(16.w),
                    decoration: BoxDecoration(
                      color: cardBg,
                      borderRadius: BorderRadius.circular(20.r),
                      border: Border.all(
                        color: colorScheme.outlineVariant.withValues(
                          alpha: isDark ? 0.3 : 0.6,
                        ),
                      ),
                      boxShadow: isDark
                          ? []
                          : [
                              BoxShadow(
                                color: Colors.black.withValues(alpha: 0.04),
                                blurRadius: 12,
                                offset: const Offset(0, 4),
                              ),
                            ],
                    ),
                    child: Form(
                      key: _updateFormKey,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          _FieldLabel(text: 'First Name', color: labelColor),
                          VerticalSpace(height: 8),
                          CustomTextField(
                            hintText: 'alex scholar',
                            controller: _updateFirstName,
                            validator: (_) => null,
                          ),

                          VerticalSpace(height: 18),

                          _FieldLabel(text: 'Last Name', color: labelColor),
                          VerticalSpace(height: 8),
                          CustomTextField(
                            hintText: 'alex scholar',
                            controller: _updateLastName,
                            validator: (_) => null,
                          ),

                          VerticalSpace(height: 18),

                          _FieldLabel(text: 'Username', color: labelColor),
                          VerticalSpace(height: 8),
                          CustomTextField(
                            hintText: 'alex89',
                            controller: _updateUserName,
                            validator: (_) => null,
                          ),

                          VerticalSpace(height: 18),

                          _FieldLabel(text: 'Bio', color: labelColor),
                          VerticalSpace(height: 8),
                          CustomTextField(
                            hintText: 'Tell people a little about yourself',
                            controller: _updateBio,
                            validator: (_) => null,
                          ),
                          VerticalSpace(height: 6),
                          Text(
                            'A short bio helps others get to know you.',
                            style: TextStyles.font14.copyWith(
                              color: subtleColor,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),

                  VerticalSpace(height: 28),

                  SizedBox(
                    width: double.infinity,
                    child: CustomButton(
                      title: isLoading ? 'Updating...' : 'Save Changes',
                      onPressed: isLoading
                          ? null
                          : () {
                              if (_updateFormKey.currentState!.validate()) {
                                context.read<ProfileCubit>().updateMyProfile(
                                  _updateFirstName.text.trim().isEmpty
                                      ? null
                                      : _updateFirstName.text.trim(),
                                  _updateLastName.text.trim().isEmpty
                                      ? null
                                      : _updateLastName.text.trim(),
                                  _updateUserName.text.trim().isEmpty
                                      ? null
                                      : _updateUserName.text.trim(),
                                  _updateBio.text.trim().isEmpty
                                      ? null
                                      : _updateBio.text.trim(),
                                );
                              }
                            },
                    ),
                  ),

                  VerticalSpace(height: 12),

                  TextButton(
                    onPressed: isLoading
                        ? null
                        : () => NavigationService.instance.goBack(),
                    child: Text(
                      'Cancel',
                      style: TextStyles.font14.copyWith(
                        color: subtleColor,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ),
                ],
              ),
            );
          },
        ),
      ),
    );
  }
}

class _FieldLabel extends StatelessWidget {
  final String text;
  final Color color;
  const _FieldLabel({required this.text, required this.color});

  @override
  Widget build(BuildContext context) {
    return Text(
      text,
      style: TextStyles.font16.copyWith(
        fontWeight: FontWeight.w600,
        color: color,
      ),
    );
  }
}
