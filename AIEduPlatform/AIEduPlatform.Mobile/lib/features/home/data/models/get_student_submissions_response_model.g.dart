// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'get_student_submissions_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

GetStudentSubmissionsResponseModel _$GetStudentSubmissionsResponseModelFromJson(
  Map<String, dynamic> json,
) => GetStudentSubmissionsResponseModel(
  data: json['data'] == null
      ? null
      : PagedSubmissionsDataModel.fromJson(
          json['data'] as Map<String, dynamic>,
        ),
);

Map<String, dynamic> _$GetStudentSubmissionsResponseModelToJson(
  GetStudentSubmissionsResponseModel instance,
) => <String, dynamic>{'data': instance.data};

PagedSubmissionsDataModel _$PagedSubmissionsDataModelFromJson(
  Map<String, dynamic> json,
) => PagedSubmissionsDataModel(
  items: (json['items'] as List<dynamic>?)
      ?.map((e) => SubmissionDataModel.fromJson(e as Map<String, dynamic>))
      .toList(),
  page: (json['page'] as num?)?.toInt(),
  pageSize: (json['pageSize'] as num?)?.toInt(),
  totalCount: (json['totalCount'] as num?)?.toInt(),
  totalPages: (json['totalPages'] as num?)?.toInt(),
  hasPrevious: json['hasPrevious'] as bool?,
  hasNext: json['hasNext'] as bool?,
);

Map<String, dynamic> _$PagedSubmissionsDataModelToJson(
  PagedSubmissionsDataModel instance,
) => <String, dynamic>{
  'items': instance.items,
  'page': instance.page,
  'pageSize': instance.pageSize,
  'totalCount': instance.totalCount,
  'totalPages': instance.totalPages,
  'hasPrevious': instance.hasPrevious,
  'hasNext': instance.hasNext,
};

SubmissionDataModel _$SubmissionDataModelFromJson(Map<String, dynamic> json) =>
    SubmissionDataModel(
      id: json['id'] as String?,
      examId: json['examId'] as String?,
      studentId: json['studentId'] as String?,
      examTitle: json['examTitle'] as String?,
      courseName: json['courseName'] as String?,
      studentName: json['studentName'] as String?,
      submittedAt: json['submittedAt'] == null
          ? null
          : DateTime.parse(json['submittedAt'] as String),
      isGraded: json['isGraded'] as bool?,
      score: (json['score'] as num?)?.toDouble(),
    );

Map<String, dynamic> _$SubmissionDataModelToJson(
  SubmissionDataModel instance,
) => <String, dynamic>{
  'id': instance.id,
  'examId': instance.examId,
  'studentId': instance.studentId,
  'examTitle': instance.examTitle,
  'courseName': instance.courseName,
  'studentName': instance.studentName,
  'submittedAt': instance.submittedAt?.toIso8601String(),
  'isGraded': instance.isGraded,
  'score': instance.score,
};
