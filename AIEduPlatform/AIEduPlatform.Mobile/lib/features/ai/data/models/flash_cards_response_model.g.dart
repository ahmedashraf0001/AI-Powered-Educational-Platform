// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'flash_cards_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

FlashCardsResponseModel _$FlashCardsResponseModelFromJson(
  Map<String, dynamic> json,
) => FlashCardsResponseModel(
  dataList: (json['data'] as List<dynamic>)
      .map((e) => FlashCardModel.fromJson(e as Map<String, dynamic>))
      .toList(),
);

Map<String, dynamic> _$FlashCardsResponseModelToJson(
  FlashCardsResponseModel instance,
) => <String, dynamic>{'data': instance.dataList};

FlashCardModel _$FlashCardModelFromJson(Map<String, dynamic> json) =>
    FlashCardModel(
      id: json['id'] as String?,
      topic: json['topic'] as String?,
      frontText: json['frontText'] as String?,
      backText: json['backText'] as String?,
    );

Map<String, dynamic> _$FlashCardModelToJson(FlashCardModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'topic': instance.topic,
      'frontText': instance.frontText,
      'backText': instance.backText,
    };
