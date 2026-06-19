import { useState, useCallback } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { studySessionsApi } from '@/api/studySessions.api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Spinner } from '@/components/ui/Spinner';
import { toast } from 'sonner';
import ReactFlow, {
  MiniMap,
  Controls,
  Background,
  useNodesState,
  useEdgesState,
  MarkerType,
  Position,
  type Node,
  type Edge,
} from 'reactflow';
import 'reactflow/dist/style.css';
import dagre from 'dagre';

interface MindMapViewProps {
  sessionId: string;
  lectureIds: string[];
  materialIds: string[];
}

// Helper to generate a consistent node ID
function generateNodeId(node: any, parentId: string | null, index: number): string {
  if (node.id) return String(node.id);
  const label = node.label || node.name || node.topic || `node-${index}`;
  return parentId ? `${parentId}-${label}-${index}` : `root-${label}`;
}

// Build nodes and return an ID mapping for edge resolution
function buildNodesWithMapping(
  tree: any,
  x = 0,
  y = 0,
  level = 0,
  parentId: string | null = null,
  index = 0
): { nodes: Node[]; idMap: Map<string, string> } {
  const nodes: Node[] = [];
  const idMap = new Map<string, string>();

  const nodeId = generateNodeId(tree, parentId, index);

  // Map all possible identifiers to this node's ID
  if (tree.id) idMap.set(String(tree.id), nodeId);
  if (tree.label) idMap.set(tree.label, nodeId);
  if (tree.name) idMap.set(tree.name, nodeId);
  if (tree.topic) idMap.set(tree.topic, nodeId);
  // Also map the generated ID to itself
  idMap.set(nodeId, nodeId);

  nodes.push({
    id: nodeId,
    position: { x: x * 250, y: y * 100 },
    data: { label: tree.label || tree.name || tree.topic || '' },
    style: {
      background: level === 0 ? '#6366f1' : level === 1 ? '#8b5cf6' : '#e2e8f0',
      color: level < 2 ? '#fff' : '#000',
      borderRadius: '8px',
      padding: '8px 16px',
      fontSize: level === 0 ? '14px' : '12px',
      fontWeight: level === 0 ? 700 : 400,
    },
  });

  if (tree.children) {
    tree.children.forEach((child: any, idx: number) => {
      const childResult = buildNodesWithMapping(
        child,
        x + idx - Math.floor(tree.children.length / 2),
        y + 1,
        level + 1,
        nodeId,
        idx
      );
      nodes.push(...childResult.nodes);
      childResult.idMap.forEach((v, k) => idMap.set(k, v));
    });
  }

  return { nodes, idMap };
}

// Build edges using the ID mapping to resolve source/target
function buildEdgesFromTree(tree: any, idMap: Map<string, string>): Edge[] {
  const edges: Edge[] = [];
  const parentKey = tree.id || tree.label || tree.name || tree.topic || '';
  const parentId = idMap.get(String(parentKey)) || String(parentKey);

  if (tree.children) {
    tree.children.forEach((child: any, idx: number) => {
      const childKey = child.id || child.label || child.name || child.topic || '';
      const childId = idMap.get(String(childKey)) || String(childKey);

      if (parentId && childId && parentId !== childId) {
        edges.push({
          id: `edge-${parentId}-${childId}-${idx}`,
          source: parentId,
          target: childId,
          type: 'smoothstep',
          animated: true,
          markerEnd: { type: MarkerType.ArrowClosed, width: 20, height: 20, color: '#6366f1' },
          style: { strokeWidth: 2, stroke: '#6366f1' },
        });
      }
      edges.push(...buildEdgesFromTree(child, idMap));
    });
  }
  return edges;
}

// Build edges from API connections data using the ID mapping
function buildEdgesFromConnections(connections: any[], idMap: Map<string, string>): Edge[] {
  return connections.map((c: any, idx: number) => {
    const sourceKey = String(c.source || c.from || '');
    const targetKey = String(c.target || c.to || '');

    // Look up actual node IDs using the mapping
    const sourceId = idMap.get(sourceKey) || sourceKey;
    const targetId = idMap.get(targetKey) || targetKey;

    return {
      id: `edge-conn-${idx}`,
      source: sourceId,
      target: targetId,
      type: 'smoothstep',
      animated: true,
      markerEnd: { type: MarkerType.ArrowClosed, width: 20, height: 20, color: '#6366f1' },
      style: { strokeWidth: 2, stroke: '#6366f1' },
    };
  });
}

const dagreGraph = new dagre.graphlib.Graph();
dagreGraph.setDefaultEdgeLabel(() => ({}));

const nodeWidth = 200;
const nodeHeight = 50;

function getLayoutedElements(nodes: Node[], edges: Edge[], direction = 'TB') {
  dagreGraph.setGraph({ rankdir: direction });

  nodes.forEach((node) => {
    dagreGraph.setNode(node.id, { width: nodeWidth, height: nodeHeight });
  });

  edges.forEach((edge) => {
    dagreGraph.setEdge(edge.source, edge.target);
  });

  dagre.layout(dagreGraph);

  nodes.forEach((node) => {
    const nodeWithPosition = dagreGraph.node(node.id);
    node.targetPosition = direction === 'TB' ? Position.Top : Position.Left;
    node.sourcePosition = direction === 'TB' ? Position.Bottom : Position.Right;

    // We are shifting the dagre node position (anchor=center center) to the top left
    // so it matches the React Flow node anchor point (top left).
    node.position = {
      x: nodeWithPosition.x - nodeWidth / 2,
      y: nodeWithPosition.y - nodeHeight / 2,
    };

    return node;
  });

  return { layoutedNodes: nodes, layoutedEdges: edges };
}

export function MindMapView({ sessionId, lectureIds, materialIds }: MindMapViewProps) {
  const [nodes, setNodes, onNodesChange] = useNodesState([]);
  const [edges, setEdges, onEdgesChange] = useEdgesState([]);
  const [topic, setTopic] = useState('');
  const queryClient = useQueryClient();

  const generateMutation = useMutation({
    mutationFn: () => {
      const promise = studySessionsApi.generateMindMap(sessionId, { centralTopic: topic || 'Main concepts', lectureIds, materialIds })
        .then((res) => {
          queryClient.invalidateQueries({ queryKey: ['mindmaps-history', sessionId] });
          return res;
        });

      toast.promise(promise, {
        loading: 'Generating mind map...',
        success: 'Mind map generated successfully!',
        error: (err: any) => err?.userMessage || 'Failed to generate mind map'
      });

      return promise;
    },
    onSuccess: (res) => {
      const data = res.data.data;
      if (!data) return;
      try {
        const nodesData = typeof data.nodes === 'string' ? JSON.parse(data.nodes) : data.nodes;
        const connectionsData = typeof data.connections === 'string' ? JSON.parse(data.connections) : data.connections;

        // Build nodes with ID mapping
        const { nodes: parsedNodes, idMap } = buildNodesWithMapping(nodesData);

        // Build edges using the ID mapping for proper source/target resolution
        let builtEdges: Edge[] = [];
        if (connectionsData && Array.isArray(connectionsData) && connectionsData.length > 0) {
          builtEdges = buildEdgesFromConnections(connectionsData, idMap);
        } else {
          builtEdges = buildEdgesFromTree(nodesData, idMap);
        }

        const { layoutedNodes, layoutedEdges } = getLayoutedElements(parsedNodes, builtEdges, 'TB');
        setNodes(layoutedNodes);
        setEdges(layoutedEdges);
      } catch {
        // fallback
      }
    },
  });

  const { data: history } = useQuery({
    queryKey: ['mindmaps-history', sessionId],
    queryFn: () => studySessionsApi.getMindMaps(sessionId),
    select: (res) => res.data.data?.items,
  });

  const loadMap = useCallback(
    (map: any) => {
      try {
        const nodesData = typeof map.nodes === 'string' ? JSON.parse(map.nodes) : map.nodes;
        const connectionsData = typeof map.connections === 'string' ? JSON.parse(map.connections) : map.connections;

        // Build nodes with ID mapping
        const { nodes: parsedNodes, idMap } = buildNodesWithMapping(nodesData);

        // Build edges using the ID mapping
        let builtEdges: Edge[] = [];
        if (connectionsData && Array.isArray(connectionsData) && connectionsData.length > 0) {
          builtEdges = buildEdgesFromConnections(connectionsData, idMap);
        } else {
          builtEdges = buildEdgesFromTree(nodesData, idMap);
        }

        const { layoutedNodes, layoutedEdges } = getLayoutedElements(parsedNodes, builtEdges, 'TB');
        setNodes(layoutedNodes);
        setEdges(layoutedEdges);
      } catch {
        /* ignore */
      }
    },
    [setNodes, setEdges]
  );

  return (
    <div className="p-4 space-y-4">
      <div className="flex items-center justify-between gap-2">
        <h3 className="font-bold text-lg">Mind Map</h3>
        <div className="flex gap-2">
          <Input
            placeholder="Central topic (optional)"
            value={topic}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => setTopic(e.target.value)}
            className="w-48"
          />
          <Button
            onClick={() => generateMutation.mutate()}
            loading={generateMutation.isPending}
          >
            Generate
          </Button>
        </div>
      </div>

      {generateMutation.isPending && (
        <div className="flex items-center justify-center py-12">
          <Spinner />
          <span className="ml-2 text-muted-foreground">Generating mind map...</span>
        </div>
      )}

      {nodes.length > 0 && (
        <div className="border rounded-lg overflow-hidden" style={{ height: 500 }}>
          <ReactFlow
            nodes={nodes}
            edges={edges}
            onNodesChange={onNodesChange}
            onEdgesChange={onEdgesChange}
            defaultEdgeOptions={{
              type: 'smoothstep',
              animated: true,
              markerEnd: { type: MarkerType.ArrowClosed, width: 20, height: 20, color: '#6366f1' },
              style: { strokeWidth: 2, stroke: '#6366f1' },
            }}
            fitView
          >
            <Controls />
            <MiniMap />
            <Background gap={16} />
          </ReactFlow>
        </div>
      )}

      {history && Array.isArray(history) && history.length > 0 && (
        <div className="border-t pt-4">
          <h4 className="font-medium mb-2">Previous Mind Maps</h4>
          <div className="flex flex-wrap gap-2">
            {history.map((map: any, idx: number) => (
              <Button key={idx} variant="outline" size="sm" onClick={() => loadMap(map)}>
                Map {idx + 1}
              </Button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
