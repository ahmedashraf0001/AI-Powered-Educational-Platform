import 'package:freezed_annotation/freezed_annotation.dart';
part 'my_courses_response_model.g.dart';

@JsonSerializable()
class MyCoursesResponseModel {
  @JsonKey(name: 'data')
  final MyCoursesData coursesData;

  MyCoursesResponseModel({required this.coursesData});

  factory MyCoursesResponseModel.fromJson(Map<String, dynamic> json) =>
      _$MyCoursesResponseModelFromJson(json);
}

@JsonSerializable()
class MyCoursesData {
  final int? totalEnrolledCourses;
  final int? completedCourses;
  final int? inProgressCourses;
  final int? totalLecturesCompleted;
  final int? totalLectures;
  final int? overallProgressPercentage;
  final int? certificatesEarned;
  final StreakModel? streak;
  @JsonKey(name: 'courseProgress')
  final List<CoursesProgress>? courseProgressList;
  @JsonKey(name: 'engagement')
  final EngagementModel engagementModel;
  @JsonKey(name: 'performance')
  final PerformanceModel performance;

  MyCoursesData({
    required this.totalEnrolledCourses,
    required this.completedCourses,
    required this.inProgressCourses,
    required this.totalLecturesCompleted,
    required this.totalLectures,
    required this.overallProgressPercentage,
    required this.certificatesEarned,
    required this.streak,
    required this.courseProgressList,
    required this.engagementModel,
    required this.performance,
  });
  factory MyCoursesData.fromJson(Map<String, dynamic> json) =>
      _$MyCoursesDataFromJson(json);
}

@JsonSerializable()
class CoursesProgress {
  final String? courseId;
  final String? courseTitle;
  final String? status;
  final int? completedMaterials;
  final int? totalMaterials;
  final int? progressPercentage;

  CoursesProgress({
    required this.courseId,
    required this.courseTitle,
    required this.status,
    required this.completedMaterials,
    required this.totalMaterials,
    required this.progressPercentage,
  });
  factory CoursesProgress.fromJson(Map<String, dynamic> json) =>
      _$CoursesProgressFromJson(json);
}

@JsonSerializable()
class EngagementModel {
  final int? totalStudySessions;
  final int? totalMaterialsViewed;
  final int? totalTimeSpentMinutes;
  final int? totalQuizzesGenerated;
  final int? totalFlashcardsGenerated;
  final int? coursesEnrolled;
  final int? coursesCompleted;

  EngagementModel({
    required this.totalStudySessions,
    required this.totalMaterialsViewed,
    required this.totalTimeSpentMinutes,
    required this.totalQuizzesGenerated,
    required this.totalFlashcardsGenerated,
    required this.coursesEnrolled,
    required this.coursesCompleted,
  });
  factory EngagementModel.fromJson(Map<String, dynamic> json) =>
      _$EngagementModelFromJson(json);
}

@JsonSerializable()
class PerformanceModel {
  final int? examsTaken;
  final int? averageScore;
  final int? highestScore;
  final int? lowestScore;

  PerformanceModel({
    required this.examsTaken,
    required this.averageScore,
    required this.highestScore,
    required this.lowestScore,
  });
  factory PerformanceModel.fromJson(Map<String, dynamic> json) =>
      _$PerformanceModelFromJson(json);
}

@JsonSerializable()
class StreakModel {
  final int? currentStreak;
  final List<bool>? activeDays;

  StreakModel({required this.currentStreak, required this.activeDays});

  factory StreakModel.fromJson(Map<String, dynamic> json) =>
      _$StreakModelFromJson(json);
}
