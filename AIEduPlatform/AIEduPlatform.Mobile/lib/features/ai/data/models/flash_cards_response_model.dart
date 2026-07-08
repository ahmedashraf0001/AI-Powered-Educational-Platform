import 'package:json_annotation/json_annotation.dart';

part 'flash_cards_response_model.g.dart';

@JsonSerializable()
class FlashCardsResponseModel {
  @JsonKey(name: 'data')
  final List<FlashCardModel> dataList;

  FlashCardsResponseModel({required this.dataList});
  factory FlashCardsResponseModel.fromJson(Map<String, dynamic> json) =>
      _$FlashCardsResponseModelFromJson(json);
}

@JsonSerializable()
class FlashCardModel {
  final String? id;
  final String? topic;
  final String? frontText;
  final String? backText;

  FlashCardModel({
    required this.id,
    required this.topic,
    required this.frontText,
    required this.backText,
  });
  factory FlashCardModel.fromJson(Map<String, dynamic> json) =>
      _$FlashCardModelFromJson(json);
}
