using System;
using System.Collections.Generic;
using System.Linq;

namespace SNOA.Core.CaseStudies
{
    /// <summary>
    /// SplitNodeLeftOperator: L3 operator for Dynamic Graph
    /// Left operator because it modifies graph structure (splits node, redistributes edges)
    ///   - V' = L_V(V, P, σ) = V with new node ID (split creates new node)
    ///   - P' = L_P(V, P, σ) = P with updated degree (redistributed neighbors)
    ///   - σ' = L_σ(V, P, σ) = σ with redistributed neighbors (split between original and new node)
    /// - Node splitting: Divide a node into two nodes while preserving graph structure
    /// - Common algorithm: Distribute neighbors between original and new node
    /// - Connectivity: Ensure graph remains connected (no isolated nodes)
    /// - This operator represents splitting from the perspective of the original node
    /// - The "new node" is created conceptually, but this operator modifies the original node
    /// - A1 (Closure): Returns SNOAObject<GraphData, NodeState> (same type)
    /// - A2 (Structural Stability): V, P, σ all remain present (structure preserved)
    /// - A3 (State Mutability): State changes (Neighbors redistributed)
    /// - A4 (Property Mutability): Properties change (degree updated, split metadata added)
    /// - A5 (Noncommutativity): SplitNode ∘ NormalizeGraph ≠ NormalizeGraph ∘ SplitNode
    /// </summary>
    public class SplitNodeLeftOperator : ILeftOperator<GraphData, NodeState>
    {
        private readonly int _newNodeId;
        private readonly double _splitRatio;

        /// <summary>
        /// Constructor for SplitNodeLeftOperator
        /// - newNodeId: The ID for the new node created by splitting
        /// - splitRatio: Ratio of neighbors to keep in original node (0.0 to 1.0)
        ///               0.5 means 50% neighbors stay, 50% go to new node
        /// - Split node by redistributing neighbors between original and new node
        /// - splitRatio determines how many neighbors stay vs. go to new node
        /// - Ensures graph connectivity by keeping at least one neighbor in original node
        /// </summary>
        /// <param name="newNodeId">ID for the new node created by splitting</param>
        /// <param name="splitRatio">Ratio of neighbors to keep in original node (0.0-1.0, default 0.5)</param>
        public SplitNodeLeftOperator(int newNodeId, double splitRatio = 0.5)
        {
            if (splitRatio < 0.0 || splitRatio > 1.0)
                throw new ArgumentOutOfRangeException(nameof(splitRatio), "Split ratio must be between 0.0 and 1.0");

            _newNodeId = newNodeId;
            _splitRatio = splitRatio;
        }

        /// <summary>
        /// Apply SplitNode operator
        /// 1. Extract current graph data (V), properties (P), and state (σ)
        /// 2. Calculate how many neighbors to keep vs. redistribute
        /// 3. Split neighbors list: keep first N neighbors, redistribute rest to new node
        /// 4. Ensure connectivity: keep at least 1 neighbor in original node
        /// 5. Update properties: degree = kept neighbors count, split metadata added
        /// 6. Return new SNOAObject with updated V, P', σ'
        /// - V' = V (GraphData unchanged - node ID and value remain same for original node)
        ///   Note: New node is created conceptually but this operator modifies original node
        /// - P' = P ∪ {degree: |Neighbors_kept|, split_node_id: newNodeId, split_ratio: ratio, last_modified: DateTime.Now}
        ///   where Neighbors_kept = first N neighbors based on splitRatio
        /// - σ' = (Neighbors_kept, Level, Visited)
        ///   where Neighbors_kept = Neighbors.Take(keepCount)
        /// - Ensures at least 1 neighbor remains in original node (maintains connectivity)
        /// - If splitRatio would result in 0 neighbors, adjust to keep at least 1
        /// - This follows graph theory: isolated nodes break connectivity
        /// - If Neighbors is empty: no split possible, return unchanged
        /// - If Neighbors has 1 element: keep it (maintain connectivity)
        /// - If splitRatio = 0.0: keep minimum 1 neighbor (maintain connectivity)
        /// - If splitRatio = 1.0: keep all neighbors (no redistribution)
        /// </summary>
        /// <param name="obj">Input SNOA object X = (V, P, σ) where V=GraphData, σ=NodeState</param>
        /// <returns>Result SNOA object X' = (V', P', σ') with node split (neighbors redistributed)</returns>
        public SNOAObject<GraphData, NodeState> Apply(SNOAObject<GraphData, NodeState> obj)
        {
            // Extract components from input object X = (V, P, σ)
            // V: GraphData (node ID, value) - remains unchanged (original node)
            var graphData = obj.Value;
            
            // P: Properties (metadata) - will be updated with new degree and split metadata
            var properties = obj.Properties;
            
            // σ: NodeState (neighbors, level, visited) - neighbors will be redistributed
            var state = obj.State;

            // Handle edge case: if no neighbors, cannot split (maintain connectivity requirement)
            if (state.Neighbors.Count == 0)
            {
                // Return unchanged object (cannot split node with no neighbors)
                return new SNOAObject<GraphData, NodeState>(graphData, properties, state);
            }

            // Calculate how many neighbors to keep in original node
            // Algorithm: keepCount = floor(splitRatio * totalNeighbors)
            // But ensure at least 1 neighbor remains (maintain graph connectivity)
            var totalNeighbors = state.Neighbors.Count;
            var keepCount = Math.Max(1, (int)Math.Floor(_splitRatio * totalNeighbors));
            
            // Ensure we don't keep more than total neighbors
            keepCount = Math.Min(keepCount, totalNeighbors);

            // Split neighbors: keep first N neighbors, redistribute rest to new node
            // Algorithm: Take first keepCount neighbors for original node
            // The remaining neighbors conceptually go to new node (but this operator only modifies original)
            // This follows graph theory: splitting redistributes adjacency list
            var keptNeighbors = state.Neighbors.Take(keepCount).ToList();
            
            // Calculate how many neighbors were redistributed to new node
            var redistributedCount = totalNeighbors - keepCount;

            // Create new state σ' with kept neighbors
            // Level and Visited remain unchanged (only structural change, not traversal state)
            var newState = new NodeState
            {
                Neighbors = keptNeighbors,  // Updated: only kept neighbors remain
                Level = state.Level,        // Unchanged: level not affected by split
                Visited = state.Visited    // Unchanged: visited flag not affected
            };

            // Update properties P' = P ∪ {degree, split metadata, last_modified}
            // Degree = number of kept neighbors (graph theory: degree = |adjacency list|)
            // Split metadata tracks the split operation for graph consistency
            var newProperties = new Dictionary<string, object>(properties)
            {
                ["degree"] = keptNeighbors.Count,           // Updated: degree = kept neighbors count
                ["split_node_id"] = _newNodeId,              // New: track which node was created by split
                ["split_ratio"] = _splitRatio,               // New: track split ratio used
                ["redistributed_neighbors"] = redistributedCount, // New: how many neighbors went to new node
                ["last_modified"] = DateTime.Now             // Updated: track modification time
            };

            // Return new SNOAObject X' = (V', P', σ')
            // V' = V (unchanged - original node), P' = updated properties, σ' = updated state
            // This satisfies Axiom A1 (Closure): returns SNOAObject<GraphData, NodeState>
            // Note: The "new node" is conceptually created but this operator only modifies the original node
            // The new node would need to be created separately in the graph structure
            return new SNOAObject<GraphData, NodeState>(graphData, newProperties, newState);
        }
    }
}




