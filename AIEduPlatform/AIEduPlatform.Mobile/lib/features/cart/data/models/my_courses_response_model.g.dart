// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'my_courses_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

MyCoursesResponseModel _$MyCoursesResponseModelFromJson(
  Map<String, dynamic> json,
) => MyCoursesResponseModel(
  coursesData: MyCoursesData.fromJson(json['data'] as Map<String, dynamic>),
);

Map<String, dynamic> _$MyCoursesResponseModelToJson(
  MyCoursesResponseModel instance,
) => <String, dynamic>{'data': instance.coursesData};

MyCoursesData _$MyCoursesDataFromJson(Map<String, dynamic> json) =>
    MyCoursesData(
      totalEnrolledCourses: (json['totalEnrolledCourses'] as num?)?.toInt(),
      completedCourses: (json['completedCourses'] as num?)?.toInt(),
      inProgressCourses: (json['inProgressCourses'] as num?)?.toInt(),
      totalLecturesCompleted: (json['totalLecturesCompleted'] as num?)?.toInt(),
      totalLectures: (json['totalLectures'] as num?)?.toInt(),
      overallProgressPercentage: (json['overallProgressPercentage'] as num?)
          ?.toInt(),
      certificatesEarned: (json['certificatesEarned'] as num?)?.toInt(),
      streak: json['streak'] == null
          ? null
          : StreakModel.fromJson(json['streak'] as Map<String, dynamic>),
      courseProgressList: (json['courseProgress'] as List<dynamic>?)
          ?.map((e) => CoursesProgress.fromJson(e as Map<String, dynamic>))
          .toList(),
      engagementModel: EngagementModel.fromJson(
        json['engagement'] as Map<String, dynamic>,
      ),
      performance: PerformanceModel.fromJson(
        json['performance'] as Map<String, dynamic>,
      ),
    );

Map<String, dynamic> _$MyCoursesDataToJson(MyCoursesData instance) =>
    <String, dynamic>{
      'totalEnrolledCourses': instance.totalEnrolledCourses,
      'completedCourses': instance.completedCourses,
      'inProgressCourses': instance.inProgressCourses,
      'totalLecturesCompleted': instance.totalLecturesCompleted,
      'totalLectures': instance.totalLectures,
      'overallProgressPercentage': instance.overallProgressPercentage,
      'certificatesEarned': instance.certificatesEarned,
      'streak': instance.streak,
      'courseProgress': instance.courseProgressList,
      'engagement': instance.engagementModel,
      'performance': instance.performance,
    };

CoursesProgress _$CoursesProgressFromJson(Map<String, dynamic> json) =>
    CoursesProgress(
      courseId: json['courseId'] as String?,
      courseTitle: json['courseTitle'] as String?,
      status: json['status'] as String?,
      completedMaterials: (json['completedMaterials'] as num?)?.toInt(),
      totalMaterials: (json['totalMaterials'] as num?)?.toInt(),
      progressPercentage: (json['progressPercentage'] as num?)?.toInt(),
    );

Map<String, dynamic> _$CoursesProgressToJson(CoursesProgress instance) =>
    <String, dynamic>{
      'courseId': instance.courseId,
      'courseTitle': instance.courseTitle,
      'status': instance.status,
      'completedMaterials': instance.completedMaterials,
      'totalMaterials': instance.totalMaterials,
      'progressPercentage': instance.progressPercentage,
    };

EngagementModel _$EngagementModelFromJson(Map<String, dynamic> json) =>
    EngagementModel(
      totalStudySessions: (json['totalStudySessions'] as num?)?.toInt(),
      totalMaterialsViewed: (json['totalMaterialsViewed'] as num?)?.toInt(),
      totalTimeSpentMinutes: (json['totalTimeSpentMinutes'] as num?)?.toInt(),
      totalQuizzesGenerated: (json['totalQuizzesGenerated'] as num?)?.toInt(),
      totalFlashcardsGenerated: (json['totalFlashcardsGenerated'] as num?)
          ?.toInt(),
      coursesEnrolled: (json['coursesEnrolled'] as num?)?.toInt(),
      coursesCompleted: (json['coursesCompleted'] as num?)?.toInt(),
    );

Map<String, dynamic> _$EngagementModelToJson(EngagementModel instance) =>
    <String, dynamic>{
      'totalStudySessions': instance.totalStudySessions,
      'totalMaterialsViewed': instance.totalMaterialsViewed,
      'totalTimeSpentMinutes': instance.totalTimeSpentMinutes,
      'totalQuizzesGenerated': instance.totalQuizzesGenerated,
      'totalFlashcardsGenerated': instance.totalFlashcardsGenerated,
      'coursesEnrolled': instance.coursesEnrolled,
      'coursesCompleted': instance.coursesCompleted,
    };

PerformanceModel _$PerformanceModelFromJson(Map<String, dynamic> json) =>
    PerformanceModel(
      examsTaken: (json['examsTaken'] as num?)?.toInt(),
      averageScore: (json['averageScore'] as num?)?.toInt(),
      highestScore: (json['highestScore'] as num?)?.toInt(),
      lowestScore: (json['lowestScore'] as num?)?.toInt(),
    );

Map<String, dynamic> _$PerformanceModelToJson(PerformanceModel instance) =>
    <String, dynamic>{
      'examsTaken': instance.examsTaken,
      'averageScore': instance.averageScore,
      'highestScore': instance.highestScore,
      'lowestScore': instance.lowestScore,
    };

StreakModel _$StreakModelFromJson(Map<String, dynamic> json) => StreakModel(
  currentStreak: (json['currentStreak'] as num?)?.toInt(),
  activeDays: (json['activeDays'] as List<dynamic>?)
      ?.map((e) => e as bool)
      .toList(),
);

Map<String, dynamic> _$StreakModelToJson(StreakModel instance) =>
    <String, dynamic>{
      'currentStreak': instance.currentStreak,
      'activeDays': instance.activeDays,
    };
