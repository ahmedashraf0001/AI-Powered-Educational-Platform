// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'generate_mind_map_response_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CreateMindMapResponseModel _$CreateMindMapResponseModelFromJson(
  Map<String, dynamic> json,
) => CreateMindMapResponseModel(
  success: json['success'] as bool?,
  data: json['data'] == null
      ? null
      : MindMapDataModel.fromJson(json['data'] as Map<String, dynamic>),
  message: json['message'] as String?,
);

Map<String, dynamic> _$CreateMindMapResponseModelToJson(
  CreateMindMapResponseModel instance,
) => <String, dynamic>{
  'success': instance.success,
  'data': instance.data?.toJson(),
  'message': instance.message,
};

MindMapDataModel _$MindMapDataModelFromJson(Map<String, dynamic> json) =>
    MindMapDataModel(
      id: json['id'] as String?,
      topic: json['topic'] as String?,
      nodes: const MindMapNodeConverter().fromJson(json['nodes']),
      connections: const ConnectionsConverter().fromJson(json['connections']),
      createdAt: json['createdAt'] == null
          ? null
          : DateTime.parse(json['createdAt'] as String),
    );

Map<String, dynamic> _$MindMapDataModelToJson(MindMapDataModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'topic': instance.topic,
      'nodes': const MindMapNodeConverter().toJson(instance.nodes),
      'connections': const ConnectionsConverter().toJson(instance.connections),
      'createdAt': instance.createdAt?.toIso8601String(),
    };

MindMapNodeModel _$MindMapNodeModelFromJson(Map<String, dynamic> json) =>
    MindMapNodeModel(
      id: json['id'] as String?,
      label: json['label'] as String?,
      description: json['description'] as String?,
      sourceTitle: json['sourceTitle'] as String?,
      sourceLocation: json['sourceLocation'] as String?,
      children: (json['children'] as List<dynamic>?)
          ?.map((e) => MindMapNodeModel.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$MindMapNodeModelToJson(MindMapNodeModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'label': instance.label,
      'description': instance.description,
      'sourceTitle': instance.sourceTitle,
      'sourceLocation': instance.sourceLocation,
      'children': instance.children?.map((e) => e.toJson()).toList(),
    };
