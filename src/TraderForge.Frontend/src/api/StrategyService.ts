import type { Edge } from '@xyflow/react';
import { MarkerType } from '@xyflow/react';
import type { Strategy, StrategyNode, StrategyStatus } from '@models/Strategy';
import type { BotNodeData, BotNodeKind } from '@models/BotFlow';
import { Result } from '@utils/Result';
import { httpClient } from './httpClient';

/**
 * Bridges the trader-facing strategy model (React Flow nodes/edges, 3-state
 * status) with the backend graph API (Trigger/Condition/Action nodes, IsActive
 * + IsEngineActive). The backend `config` is an opaque JSON string, so the full
 * frontend node data travels inside it and round-trips losslessly; `type` is a
 * coarse mapping kept only to satisfy the enum — `config.kind` is authoritative.
 */

// Backend BotNodeType enum: Trigger=0, Condition=1, Action=2.
const KIND_TO_TYPE: Record<BotNodeKind, number> = {
  analysisBot: 1,
  notificationBot: 0,
  actionBot: 2,
};
const TYPE_TO_KIND: Record<number, BotNodeKind> = {
  0: 'notificationBot',
  1: 'analysisBot',
  2: 'actionBot',
};

const EDGE_STYLE = {
  animated: true,
  style: { stroke: '#10b981', strokeWidth: 2 },
  markerEnd: { type: MarkerType.ArrowClosed, color: '#10b981' },
} as const;

interface BackendListItem {
  id: string;
  name: string;
  isActive: boolean;
  isEngineActive: boolean;
  createdAt: string;
}

interface BackendNode {
  id: string;
  type: number;
  name: string;
  config: string;
  positionX: number;
  positionY: number;
  isActive: boolean;
}

interface BackendEdge {
  id: string;
  sourceNodeId: string;
  sourcePort: number;
  targetNodeId: string;
}

interface BackendGraph {
  id: string;
  name: string;
  isActive: boolean;
  isEngineActive: boolean;
  nodes: BackendNode[];
  edges: BackendEdge[];
}

interface NodePayload {
  type: number;
  name: string;
  config: string;
  positionX: number;
  positionY: number;
}

function toNodePayload(node: StrategyNode): NodePayload {
  return {
    type: KIND_TO_TYPE[node.data.kind],
    name: node.data.label,
    config: JSON.stringify({ kind: node.data.kind, config: node.data.config }),
    positionX: node.position.x,
    positionY: node.position.y,
  };
}

function nodeChanged(current: StrategyNode, baseline: StrategyNode): boolean {
  const a = toNodePayload(current);
  const b = toNodePayload(baseline);
  return (
    a.name !== b.name ||
    a.config !== b.config ||
    a.positionX !== b.positionX ||
    a.positionY !== b.positionY
  );
}

function toStrategyNode(bn: BackendNode): StrategyNode {
  let kind: BotNodeKind = TYPE_TO_KIND[bn.type] ?? 'analysisBot';
  let config: unknown;
  try {
    const parsed = JSON.parse(bn.config);
    if (parsed && typeof parsed === 'object' && 'kind' in parsed) {
      kind = parsed.kind as BotNodeKind;
      config = parsed.config;
    }
  } catch {
    // Malformed config — fall back to the coarse type mapping with empty data.
  }
  return {
    id: bn.id,
    type: kind,
    position: { x: bn.positionX, y: bn.positionY },
    data: { kind, label: bn.name, config } as BotNodeData,
  };
}

function toStrategyEdge(be: BackendEdge): Edge {
  return { id: be.id, source: be.sourceNodeId, target: be.targetNodeId, ...EDGE_STYLE };
}

function deriveStatus(nodeCount: number, isEngineActive: boolean): StrategyStatus {
  if (isEngineActive) return 'active';
  return nodeCount === 0 ? 'draft' : 'paused';
}

export class StrategyService {
  /** All strategies of the active portfolio, with their graph (newest first). */
  async list(): Promise<Result<Strategy[]>> {
    try {
      const { data } = await httpClient.get<BackendListItem[]>('/api/portfolio/strategies');
      const strategies = await Promise.all(
        data.map(async (item): Promise<Strategy> => {
          const graph = await this.fetchGraph(item.id);
          const nodes = graph?.nodes.map(toStrategyNode) ?? [];
          const edges = graph?.edges.map(toStrategyEdge) ?? [];
          return {
            id: item.id,
            name: item.name,
            status: deriveStatus(nodes.length, item.isEngineActive),
            nodes,
            edges,
            createdAt: item.createdAt,
            updatedAt: item.createdAt,
          };
        }),
      );
      strategies.sort((a, b) => b.createdAt.localeCompare(a.createdAt));
      return Result.ok(strategies);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'No se pudieron cargar las estrategias.'));
    }
  }

  /** A single strategy with its full graph, for the builder. */
  async get(id: string): Promise<Result<Strategy>> {
    try {
      const graph = await this.fetchGraph(id);
      if (!graph) return Result.fail('Estrategia no encontrada.');
      const nodes = graph.nodes.map(toStrategyNode);
      return Result.ok({
        id: graph.id,
        name: graph.name,
        status: deriveStatus(nodes.length, graph.isEngineActive),
        nodes,
        edges: graph.edges.map(toStrategyEdge),
        createdAt: '',
        updatedAt: '',
      });
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'No se pudo cargar la estrategia.'));
    }
  }

  async create(name: string): Promise<Result<Strategy>> {
    try {
      const { data } = await httpClient.post<{ id: string }>('/api/portfolio/strategies', {
        name: name.trim() || 'Nueva estrategia',
      });
      const now = new Date().toISOString();
      return Result.ok({
        id: data.id,
        name: name.trim() || 'Nueva estrategia',
        status: 'draft',
        nodes: [],
        edges: [],
        createdAt: now,
        updatedAt: now,
      });
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'No se pudo crear la estrategia.'));
    }
  }

  async remove(id: string): Promise<Result<void>> {
    try {
      await httpClient.delete(`/api/strategies/${id}`);
      return Result.ok(undefined);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'No se pudo eliminar la estrategia.'));
    }
  }

  async startEngine(id: string): Promise<Result<void>> {
    try {
      await httpClient.post(`/api/strategies/${id}/engine/start`);
      return Result.ok(undefined);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'No se pudo activar la estrategia.'));
    }
  }

  async stopEngine(id: string): Promise<Result<void>> {
    try {
      await httpClient.post(`/api/strategies/${id}/engine/stop`);
      return Result.ok(undefined);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'No se pudo pausar la estrategia.'));
    }
  }

  /** Copies a strategy by creating a new one and replaying its graph. */
  async duplicate(sourceId: string, name: string): Promise<Result<Strategy>> {
    const source = await this.get(sourceId);
    if (!source.isSuccess) return Result.fail(source.errorMessage!);

    const created = await this.create(name);
    if (!created.isSuccess) return created;

    const synced = await this.saveGraph(created.value!.id, source.value!.nodes, source.value!.edges, [], []);
    if (!synced.isSuccess) return Result.fail(synced.errorMessage!);
    return Result.ok(synced.value!);
  }

  /**
   * Persists the canvas by diffing it against the baseline (the graph as last
   * loaded/saved) and issuing only the needed node/edge mutations. Returns the
   * fresh graph so the caller can adopt the backend-assigned ids.
   */
  async saveGraph(
    strategyId: string,
    nodes: StrategyNode[],
    edges: Edge[],
    baselineNodes: StrategyNode[],
    baselineEdges: Edge[],
  ): Promise<Result<Strategy>> {
    try {
      const baseNodeById = new Map(baselineNodes.map((n) => [n.id, n]));
      const baseEdgeIds = new Set(baselineEdges.map((e) => e.id));
      const currentNodeIds = new Set(nodes.map((n) => n.id));
      const currentEdgeIds = new Set(edges.map((e) => e.id));
      const idMap = new Map<string, string>();

      // 1. Add new nodes / update changed ones.
      for (const node of nodes) {
        const baseline = baseNodeById.get(node.id);
        if (!baseline) {
          const { data } = await httpClient.post<{ nodeId: string }>(
            `/api/strategies/${strategyId}/nodes`,
            toNodePayload(node),
          );
          idMap.set(node.id, data.nodeId);
        } else {
          idMap.set(node.id, node.id);
          if (nodeChanged(node, baseline)) {
            const { type: _t, ...update } = toNodePayload(node);
            void _t;
            await httpClient.put(`/api/strategies/${strategyId}/nodes/${node.id}`, update);
          }
        }
      }

      // 2. Add new edges, remapping any temp source/target ids to real ones.
      for (const edge of edges) {
        if (baseEdgeIds.has(edge.id)) continue;
        const sourceNodeId = idMap.get(edge.source) ?? edge.source;
        const targetNodeId = idMap.get(edge.target) ?? edge.target;
        await httpClient.post(`/api/strategies/${strategyId}/edges`, {
          sourceNodeId,
          sourcePort: 0,
          targetNodeId,
        });
      }

      // 3. Remove deleted edges (before nodes, to avoid cascade conflicts).
      for (const edge of baselineEdges) {
        if (!currentEdgeIds.has(edge.id)) {
          await httpClient.delete(`/api/strategies/${strategyId}/edges/${edge.id}`);
        }
      }

      // 4. Remove deleted nodes.
      for (const node of baselineNodes) {
        if (!currentNodeIds.has(node.id)) {
          await httpClient.delete(`/api/strategies/${strategyId}/nodes/${node.id}`);
        }
      }

      return this.get(strategyId);
    } catch (error) {
      return Result.fail(extractErrorMessage(error, 'No se pudo guardar la estrategia.'));
    }
  }

  private async fetchGraph(id: string): Promise<BackendGraph | null> {
    try {
      const { data } = await httpClient.get<BackendGraph>(`/api/strategies/${id}/graph`);
      return data;
    } catch {
      return null;
    }
  }
}

function extractErrorMessage(error: unknown, fallback: string): string {
  const e = error as { response?: { status?: number; data?: { error?: string } }; code?: string };
  if (e?.response?.status === 403 || e?.response?.status === 401) return 'UNAUTHORIZED';
  if (e?.response?.data?.error) return e.response.data.error;
  if (e?.code === 'ERR_NETWORK' || !e?.response) return 'No se puede conectar al servidor.';
  return fallback;
}
