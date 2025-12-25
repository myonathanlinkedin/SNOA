using System;
using System.Collections.Generic;
using System.Linq;

namespace SNOA.Core.CaseStudies
{
    /// <summary>
    /// RemoveEdgeLeftOperator: L2 operator for Dynamic Graph
    /// Left operator because it modifies graph structure (removes edge/neighbor relationship)
    ///   - V' = L_V(V, P, σ) = V (unchanged, graph data remains same)
    ///   - P' = L_P(V, P, σ) = P with updated degree (degree decreases by 1)
    ///   - σ' = L_σ(V, P, σ) = σ with neighbor removed from Neighbors list
    /// - Removing an edge means removing a neighbor from the adjacency list
    /// - In undirected graphs, edge (u,v) means u has neighbor v and v has neighbor u
    /// - This operator removes neighbor from current node's perspective
    /// - Degree decreases when neighbor is removed
    /// - A1 (Closure): Returns SNOAObject<GraphData, NodeState> (same type)
    /// - A2 (Structural Stability): V, P, σ all remain present (structure preserved)
    /// - A3 (State Mutability): State changes (Neighbors list modified)
    /// - A4 (Property Mutability): Properties change (degree updated)
    /// - A5 (Noncommutativity): RemoveEdge ∘ NormalizeGraph ≠ NormalizeGraph ∘ RemoveEdge
    /// </summary>
    public class RemoveEdgeLeftOperator : ILeftOperator<GraphData, NodeState>
    {
        private readonly int _targetNodeId;

        /// <summary>
        /// Constructor for RemoveEdgeLeftOperator
        /// - targetNodeId: The neighbor node ID to remove from the current node's neighbor list
        /// </summary>
        /// <param name="targetNodeId">Target node ID to remove edge from (neighbor to remove)</param>
        public RemoveEdgeLeftOperator(int targetNodeId)
        {
            _targetNodeId = targetNodeId;
        }

        /// <summary>
        /// Apply RemoveEdge operator
        /// 1. Extract current graph data (V), properties (P), and state (σ)
        /// 2. Remove targetNodeId from Neighbors list (if exists)
        /// 3. Create new state with updated Neighbors list
        /// 4. Update properties: degree = new neighbor count, last_modified = current time
        /// 5. Return new SNOAObject with same V, updated P', updated σ'
        /// - V' = V (GraphData unchanged - node ID and value remain same)
        /// - P' = P ∪ {degree: |Neighbors'|, last_modified: DateTime.Now}
        ///   where Neighbors' = Neighbors \ {targetNodeId}
        /// - σ' = (Neighbors', Level, Visited)
        ///   where Neighbors' = Neighbors.Remove(targetNodeId)
        /// - If targetNodeId not in Neighbors: operation is idempotent (no change)
        /// - If Neighbors is empty: degree becomes 0
        /// - If targetNodeId appears multiple times: all occurrences removed (shouldn't happen if graph normalized)
        /// </summary>
        /// <param name="obj">Input SNOA object X = (V, P, σ) where V=GraphData, σ=NodeState</param>
        /// <returns>Result SNOA object X' = (V', P', σ') with edge removed</returns>
        public SNOAObject<GraphData, NodeState> Apply(SNOAObject<GraphData, NodeState> obj)
        {
            // Extract components from input object X = (V, P, σ)
            // V: GraphData (node ID, value) - remains unchanged
            var graphData = obj.Value;
            
            // P: Properties (metadata) - will be updated with new degree
            var properties = obj.Properties;
            
            // σ: NodeState (neighbors, level, visited) - neighbors will be modified
            var state = obj.State;

            // Remove neighbor (structural change)
            // Algorithm: Create new list without targetNodeId
            // This follows graph theory: removing edge means removing neighbor from adjacency list
            // Using LINQ Where to filter out targetNodeId (handles multiple occurrences if any)
            var newNeighbors = state.Neighbors.Where(n => n != _targetNodeId).ToList();
            
            // Create new state σ' with updated neighbors
            // Level and Visited remain unchanged (only structural change, not traversal state)
            var newState = new NodeState
            {
                Neighbors = newNeighbors,  // Updated: neighbor removed
                Level = state.Level,       // Unchanged: level not affected by edge removal
                Visited = state.Visited   // Unchanged: visited flag not affected
            };

            // Update properties P' = P ∪ {degree: |Neighbors'|, last_modified: DateTime.Now}
            // Degree = number of neighbors (graph theory: degree = |adjacency list|)
            // This follows MD requirement: "removes neighbor and updates degree"
            var newProperties = new Dictionary<string, object>(properties)
            {
                ["degree"] = newNeighbors.Count,  // Updated: degree = new neighbor count
                ["last_modified"] = DateTime.Now  // Updated: track modification time
            };

            // Return new SNOAObject X' = (V', P', σ')
            // V' = V (unchanged), P' = updated properties, σ' = updated state
            // This satisfies Axiom A1 (Closure): returns SNOAObject<GraphData, NodeState>
            return new SNOAObject<GraphData, NodeState>(graphData, newProperties, newState);
        }
    }
}

