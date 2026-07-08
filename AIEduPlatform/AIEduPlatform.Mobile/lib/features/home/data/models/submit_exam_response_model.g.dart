// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'submit_exam_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

SubmitExamResponseModel _$SubmitExamResponseModelFromJson(
  Map<String, dynamic> json,
) => SubmitExamResponseModel(
  data: json['data'] == null
      ? null
      : SubmitExamDataModel.fromJson(json['data'] as Map<String, dynamic>),
);

Map<String, dynamic> _$SubmitExamResponseModelToJson(
  SubmitExamResponseModel instance,
) => <String, dynamic>{'data': instance.data};

SubmitExamDataModel _$SubmitExamDataModelFromJson(Map<String, dynamic> json) =>
    SubmitExamDataModel(submissionId: json['submissionId'] as String?);

Map<String, dynamic> _$SubmitExamDataModelToJson(
  SubmitExamDataModel instance,
) => <String, dynamic>{'submissionId': instance.submissionId};

SubmissionDetailResponseModel _$SubmissionDetailResponseModelFromJson(
  Map<String, dynamic> json,
) => SubmissionDetailResponseModel(
  data: json['data'] == null
      ? null
      : SubmissionDetailDataModel.fromJson(
          json['data'] as Map<String, dynamic>,
        ),
);

Map<String, dynamic> _$SubmissionDetailResponseModelToJson(
  SubmissionDetailResponseModel instance,
) => <String, dynamic>{'data': instance.data};

SubmissionDetailDataModel _$SubmissionDetailDataModelFromJson(
  Map<String, dynamic> json,
) => SubmissionDetailDataModel(
  id: json['id'] as String?,
  examId: json['examId'] as String?,
  studentId: json['studentId'] as String?,
  examTitle: json['examTitle'] as String?,
  courseName: json['courseName'] as String?,
  studentName: json['studentName'] as String?,
  answers: (json['answers'] as List<dynamic>?)
      ?.map((e) => SubmissionAnswerModel.fromJson(e as Map<String, dynamic>))
      .toList(),
  submittedAt: json['submittedAt'] == null
      ? null
      : DateTime.parse(json['submittedAt'] as String),
  grade: json['grade'] == null
      ? null
      : GradeModel.fromJson(json['grade'] as Map<String, dynamic>),
);

Map<String, dynamic> _$SubmissionDetailDataModelToJson(
  SubmissionDetailDataModel instance,
) => <String, dynamic>{
  'id': instance.id,
  'examId': instance.examId,
  'studentId': instance.studentId,
  'examTitle': instance.examTitle,
  'courseName': instance.courseName,
  'studentName': instance.studentName,
  'answers': instance.answers,
  'submittedAt': instance.submittedAt?.toIso8601String(),
  'grade': instance.grade,
};

SubmissionAnswerModel _$SubmissionAnswerModelFromJson(
  Map<String, dynamic> json,
) => SubmissionAnswerModel(
  questionId: json['questionId'] as String?,
  questionText: json['questionText'] as String?,
  questionType: json['questionType'] as String?,
  answer: json['answer'] as String?,
  correctAnswer: json['correctAnswer'] as String?,
  options: const OptionsConverter().fromJson(json['options']),
  points: (json['points'] as num?)?.toInt(),
  order: (json['order'] as num?)?.toInt(),
);

Map<String, dynamic> _$SubmissionAnswerModelToJson(
  SubmissionAnswerModel instance,
) => <String, dynamic>{
  'questionId': instance.questionId,
  'questionText': instance.questionText,
  'questionType': instance.questionType,
  'answer': instance.answer,
  'correctAnswer': instance.correctAnswer,
  'options': const OptionsConverter().toJson(instance.options),
  'points': instance.points,
  'order': instance.order,
};

GradeModel _$GradeModelFromJson(Map<String, dynamic> json) => GradeModel(
  id: json['id'] as String?,
  submissionId: json['submissionId'] as String?,
  score: (json['score'] as num?)?.toDouble(),
  feedback: json['feedback'] as String?,
  isAiGraded: json['isAiGraded'] as bool?,
  isApproved: json['isApproved'] as bool?,
  examTitle: json['examTitle'] as String?,
  courseTitle: json['courseTitle'] as String?,
);

Map<String, dynamic> _$GradeModelToJson(GradeModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'submissionId': instance.submissionId,
      'score': instance.score,
      'feedback': instance.feedback,
      'isAiGraded': instance.isAiGraded,
      'isApproved': instance.isApproved,
      'examTitle': instance.examTitle,
      'courseTitle': instance.courseTitle,
    };
