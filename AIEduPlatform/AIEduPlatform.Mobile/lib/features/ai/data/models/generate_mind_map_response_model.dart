import 'dart:convert';

import 'package:json_annotation/json_annotation.dart';

part 'generate_mind_map_response_model.g.dart';

@JsonSerializable(explicitToJson: true)
class CreateMindMapResponseModel {
  final bool? success;
  final MindMapDataModel? data;
  final String? message;

  CreateMindMapResponseModel({this.success, this.data, this.message});

  factory CreateMindMapResponseModel.fromJson(Map<String, dynamic> json) =>
      _$CreateMindMapResponseModelFromJson(json);

  Map<String, dynamic> toJson() => _$CreateMindMapResponseModelToJson(this);
}

@JsonSerializable(explicitToJson: true)
class MindMapDataModel {
  final String? id;
  final String? topic;

  @MindMapNodeConverter()
  final MindMapNodeModel? nodes;

  @ConnectionsConverter()
  final List<dynamic>? connections;

  final DateTime? createdAt;

  MindMapDataModel({
    this.id,
    this.topic,
    this.nodes,
    this.connections,
    this.createdAt,
  });

  factory MindMapDataModel.fromJson(Map<String, dynamic> json) =>
      _$MindMapDataModelFromJson(json);

  Map<String, dynamic> toJson() => _$MindMapDataModelToJson(this);
}

@JsonSerializable(explicitToJson: true)
class MindMapNodeModel {
  final String? id;
  final String? label;
  final String? description;
  final String? sourceTitle;
  final String? sourceLocation;
  final List<MindMapNodeModel>? children;

  MindMapNodeModel({
    this.id,
    this.label,
    this.description,
    this.sourceTitle,
    this.sourceLocation,
    this.children,
  });

  factory MindMapNodeModel.fromJson(Map<String, dynamic> json) =>
      _$MindMapNodeModelFromJson(json);

  Map<String, dynamic> toJson() => _$MindMapNodeModelToJson(this);
}

/// `nodes` arrives as a JSON-encoded string representing a single
/// root node object (not an array), so this decodes a String -> Map
/// -> MindMapNodeModel (recursively, via children's own fromJson).
class MindMapNodeConverter
    implements JsonConverter<MindMapNodeModel?, dynamic> {
  const MindMapNodeConverter();

  @override
  MindMapNodeModel? fromJson(dynamic json) {
    if (json == null) return null;

    if (json is String) {
      if (json.trim().isEmpty) return null;
      final decoded = jsonDecode(json) as Map<String, dynamic>;
      return MindMapNodeModel.fromJson(decoded);
    }

    if (json is Map<String, dynamic>) {
      return MindMapNodeModel.fromJson(json);
    }

    return null;
  }

  @override
  dynamic toJson(MindMapNodeModel? object) {
    if (object == null) return null;
    return jsonEncode(object.toJson());
  }
}

/// `connections` arrives as a JSON-encoded string representing a list.
/// Shape of items is unknown from the sample ("[]"); kept as dynamic
/// until a non-empty example clarifies the structure.
class ConnectionsConverter implements JsonConverter<List<dynamic>?, dynamic> {
  const ConnectionsConverter();

  @override
  List<dynamic>? fromJson(dynamic json) {
    if (json == null) return null;

    if (json is String) {
      if (json.trim().isEmpty) return [];
      return jsonDecode(json) as List<dynamic>;
    }

    if (json is List) {
      return json;
    }

    return null;
  }

  @override
  dynamic toJson(List<dynamic>? object) {
    if (object == null) return null;
    return jsonEncode(object);
  }
}
