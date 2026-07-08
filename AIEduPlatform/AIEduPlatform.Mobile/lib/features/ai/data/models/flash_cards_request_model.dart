import 'package:json_annotation/json_annotation.dart';
part 'flash_cards_request_model.g.dart';

@JsonSerializable()
class FlashCardsRequestModel {
  final String? topic;
  final int? numberOfCards;

  FlashCardsRequestModel({required this.topic, required this.numberOfCards});

  Map<String, dynamic> toJson() => _$FlashCardsRequestModelToJson(this);
}
