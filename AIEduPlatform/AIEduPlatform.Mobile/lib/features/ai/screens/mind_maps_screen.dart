import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:graduation_app/core/helpers/space_helper.dart';
import 'package:graduation_app/core/theming/colors.dart';
import 'package:graduation_app/core/theming/styles.dart';
import 'package:graduation_app/features/ai/data/models/generate_mind_map_response_model.dart';
import 'package:graduation_app/features/ai/logic/ai_services_cubit/cubit/ai_services_cubit.dart';
import 'package:graduation_app/features/ai/logic/ai_services_cubit/cubit/ai_services_state.dart';
import 'package:graduation_app/features/ai/screens/widgets/topic_input_field.dart';
import 'package:graphview/GraphView.dart';

class MindMapScreen extends StatefulWidget {
  final String? sessionId;
  final String courseName;

  const MindMapScreen({super.key, this.sessionId, required this.courseName});

  @override
  State<MindMapScreen> createState() => _MindMapScreenState();
}

class _MindMapScreenState extends State<MindMapScreen> {
  final Graph _graph = Graph()..isTree = true;

  final TransformationController _transformController =
      TransformationController();
  final GlobalKey _graphKey = GlobalKey();

  final TextEditingController _mindMapController = TextEditingController();

  final BuchheimWalkerConfiguration _config = BuchheimWalkerConfiguration()
    ..siblingSeparation = 80
    ..levelSeparation = 100
    ..subtreeSeparation = 80
    ..orientation = BuchheimWalkerConfiguration.ORIENTATION_TOP_BOTTOM;

  // ─── Graph data caches ────────────────────────────────────────────────────
  final Map<String, Node> _nodeMap = {};
  final Map<String, MindMapNodeModel> _dataMap = {};
  final Map<String, int> _depthMap = {};
  String? _builtForId; // guards against rebuilding the same graph repeatedly

  @override
  void dispose() {
    _transformController.dispose();
    _mindMapController.dispose();
    super.dispose();
  }

  // ─── Build graph from recursive node tree ─────────────────────────────────

  void _buildGraphFromTree(MindMapNodeModel root, String dataId) {
    if (_builtForId == dataId) return; // already built for this dataset

    _graph.nodes.clear();
    _graph.edges.clear();
    _nodeMap.clear();
    _dataMap.clear();
    _depthMap.clear();

    void visit(MindMapNodeModel node, String? parentId, int depth) {
      final id = node.id ?? UniqueKey().toString();
      final graphNode = Node.Id(id);

      _nodeMap[id] = graphNode;
      _dataMap[id] = node;
      _depthMap[id] = depth;
      _graph.addNode(graphNode);

      if (parentId != null && _nodeMap[parentId] != null) {
        _graph.addEdge(_nodeMap[parentId]!, graphNode);
      }

      for (final child in node.children ?? const <MindMapNodeModel>[]) {
        visit(child, id, depth + 1);
      }
    }

    visit(root, null, 0);
    _builtForId = dataId;

    WidgetsBinding.instance.addPostFrameCallback((_) {
      Future.delayed(const Duration(milliseconds: 100), _centerGraph);
    });
  }

  // ─── Center the graph on screen ───────────────────────────────────────────

  void _centerGraph() {
    final renderBox =
        _graphKey.currentContext?.findRenderObject() as RenderBox?;
    if (renderBox == null) return;

    final graphSize = renderBox.size;
    final screenSize = MediaQuery.of(context).size;

    final offsetX = (screenSize.width - graphSize.width) / 2;
    final offsetY = (screenSize.height - graphSize.height) / 2;

    _transformController.value = Matrix4.identity()
      ..translate(offsetX, offsetY);
  }

  // ─── Zoom helpers ─────────────────────────────────────────────────────────

  void _zoomIn() {
    final current = _transformController.value.getMaxScaleOnAxis();
    if (current >= 2.5) return;
    _transformController.value = Matrix4.copy(_transformController.value)
      ..scale(1.2);
  }

  void _zoomOut() {
    final current = _transformController.value.getMaxScaleOnAxis();
    if (current <= 0.3) return;
    _transformController.value = Matrix4.copy(_transformController.value)
      ..scale(0.8);
  }

  void _resetZoom() => _centerGraph();

  // ─── Build ────────────────────────────────────────────────────────────────

  @override
  void initState() {
    super.initState();
    _mindMapController.text = widget.courseName;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        centerTitle: true,
        title: Text(
          'Mind Maps',
          style: TextStyles.font20.copyWith(color: ColorsManager.mainBlue),
        ),
        actions: [
          IconButton(
            icon: Icon(Icons.info_outline, size: 20.sp),
            onPressed: () {
              showDialog(
                context: context,
                builder: (_) => AlertDialog(
                  title: const Text('Graph info'),
                  content: Text(
                    'Nodes: ${_nodeMap.length}\n'
                    'Edges: ${_graph.edges.length}',
                  ),
                  actions: [
                    TextButton(
                      onPressed: () => Navigator.pop(context),
                      child: const Text('Close'),
                    ),
                  ],
                ),
              );
            },
          ),
        ],
      ),
      body: Padding(
        padding: EdgeInsets.symmetric(horizontal: 16.w),
        child: Column(
          children: [
            TopicInputField(
              hintText: 'Enter topic for mindmaps',
              buttonText: 'Generate MindMap',
              onPressed: () {
                if (_mindMapController.text.trim().isEmpty) return;
                context.read<AiServicesCubit>().generateMindMap(
                  widget.sessionId ?? '',
                  _mindMapController.text.trim(),
                  3,
                );
              },
              controller: _mindMapController,
            ),
            VerticalSpace(height: 20.h),
            Flexible(
              child: BlocBuilder<AiServicesCubit, AiServicesState>(
                buildWhen: (previous, current) =>
                    current is LoadingMindMap ||
                    current is SuccessMindMap ||
                    current is FailureMindMap,
                builder: (context, state) {
                  if (state is LoadingMindMap) {
                    return const Center(child: CircularProgressIndicator());
                  } else if (state is SuccessMindMap) {
                    final nodes = state.mindMapData.nodes;
                    if (nodes == null) {
                      return const Center(child: Text('No mind map data'));
                    }

                    _buildGraphFromTree(
                      nodes,
                      state.mindMapData.id ?? nodes.hashCode.toString(),
                    );

                    return Center(
                      child: Stack(
                        children: [
                          // ── Graph canvas ────────────────────────────────
                          InteractiveViewer(
                            constrained: false,
                            minScale: 0.3,
                            maxScale: 2.5,
                            transformationController: _transformController,
                            child: GraphView(
                              key: _graphKey,
                              graph: _graph,
                              algorithm: BuchheimWalkerAlgorithm(
                                _config,
                                TreeEdgeRenderer(_config),
                              ),
                              paint: Paint()
                                ..color = const Color(0x557F77DD)
                                ..strokeWidth = 1.5
                                ..style = PaintingStyle.stroke,
                              builder: (Node node) {
                                final id = node.key!.value as String;
                                final data = _dataMap[id];
                                final depth = _depthMap[id] ?? 0;
                                if (data == null) {
                                  return const SizedBox.shrink();
                                }
                                return MindMapNodeBubble(
                                  node: data,
                                  depth: depth,
                                );
                              },
                            ),
                          ),

                          // ── Zoom buttons ────────────────────────────────
                          Positioned(
                            bottom: 32.h,
                            right: 0.w,
                            child: Column(
                              children: [
                                ZoomButton(
                                  icon: Icons.add,
                                  onTap: _zoomIn,
                                  tooltip: 'Zoom in',
                                ),
                                SizedBox(height: 8.h),
                                ZoomButton(
                                  icon: Icons.remove,
                                  onTap: _zoomOut,
                                  tooltip: 'Zoom out',
                                ),
                                SizedBox(height: 8.h),
                                ZoomButton(
                                  icon: Icons.center_focus_strong,
                                  onTap: _resetZoom,
                                  tooltip: 'Reset',
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
                    );
                  } else if (state is FailureMindMap) {
                    return const Center(child: Text('error'));
                  } else {
                    return const SizedBox.shrink();
                  }
                },
              ),
            ),
          ],
        ),
      ),
    );
  }
}

// ─── Mind map node bubble ────────────────────────────────────────────────────

class MindMapNodeBubble extends StatelessWidget {
  const MindMapNodeBubble({super.key, required this.node, required this.depth});

  final MindMapNodeModel node;
  final int depth;

  @override
  Widget build(BuildContext context) {
    final id = node.id ?? '';
    final label = node.label ?? '';
    final isRoot = depth == 0;
    final isLevel1 = depth == 1;

    late final Color bgColor;
    late final Color textColor;
    late final Color borderColor;
    late final double borderRadius;
    late final double fontSize;
    late final EdgeInsets padding;

    if (isRoot) {
      bgColor = const Color(0xFF534AB7);
      textColor = Colors.white;
      borderColor = const Color(0xFF7F77DD);
      borderRadius = 28.r;
      fontSize = 14.sp;
      padding = EdgeInsets.symmetric(horizontal: 20.w, vertical: 12.h);
    } else if (isLevel1) {
      bgColor = const Color(0xFF0F6E56);
      textColor = Colors.white;
      borderColor = const Color(0xFF1D9E75);
      borderRadius = 16.r;
      fontSize = 12.sp;
      padding = EdgeInsets.symmetric(horizontal: 14.w, vertical: 8.h);
    } else {
      bgColor = const Color(0xFF1A2A3A);
      textColor = Colors.white;
      borderColor = const Color(0xFF378ADD);
      borderRadius = 12.r;
      fontSize = 11.sp;
      padding = EdgeInsets.symmetric(horizontal: 12.w, vertical: 6.h);
    }

    return Container(
      padding: padding,
      decoration: BoxDecoration(
        color: bgColor,
        borderRadius: BorderRadius.circular(borderRadius),
        border: Border.all(color: borderColor, width: isRoot ? 1.5.w : 1.w),
      ),
      child: Text(
        label,
        style: TextStyle(
          color: textColor,
          fontWeight: FontWeight.w500,
          fontSize: fontSize,
        ),
      ),
    );
  }
}

// ─── Zoom button ──────────────────────────────────────────────────────────────

class ZoomButton extends StatelessWidget {
  const ZoomButton({
    super.key,
    required this.icon,
    required this.onTap,
    this.tooltip,
  });

  final IconData icon;
  final VoidCallback onTap;
  final String? tooltip;

  @override
  Widget build(BuildContext context) {
    return Tooltip(
      message: tooltip ?? '',
      child: GestureDetector(
        onTap: onTap,
        child: Container(
          width: 40.w,
          height: 40.h,
          decoration: BoxDecoration(
            color: const Color(0xFF1A1A3E),
            borderRadius: BorderRadius.circular(12.r),
            border: Border.all(color: const Color(0xFF534AB7), width: 0.5.w),
          ),
          child: Icon(icon, color: Colors.white, size: 20.sp),
        ),
      ),
    );
  }
}
