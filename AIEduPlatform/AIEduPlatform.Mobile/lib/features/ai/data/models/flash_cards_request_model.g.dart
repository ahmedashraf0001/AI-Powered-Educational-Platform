// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'flash_cards_request_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

FlashCardsRequestModel _$FlashCardsRequestModelFromJson(
  Map<String, dynamic> json,
) => FlashCardsRequestModel(
  topic: json['topic'] as String?,
  numberOfCards: (json['numberOfCards'] as num?)?.toInt(),
);

Map<String, dynamic> _$FlashCardsRequestModelToJson(
  FlashCardsRequestModel instance,
) => <String, dynamic>{
  'topic': instance.topic,
  'numberOfCards': instance.numberOfCards,
};
