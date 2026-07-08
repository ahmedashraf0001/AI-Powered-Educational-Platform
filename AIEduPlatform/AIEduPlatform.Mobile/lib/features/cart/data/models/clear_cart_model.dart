
import 'package:freezed_annotation/freezed_annotation.dart';
part 'clear_cart_model.g.dart';

@JsonSerializable()
class ClearMyCartModel{
  final String? message;

  ClearMyCartModel({required this.message});

  factory ClearMyCartModel.fromJson(Map<String,dynamic>json)=>_$ClearMyCartModelFromJson(json);
}