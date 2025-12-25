using System;
using System.Collections.Generic;

namespace SNOA.Core.CaseStudies
{
    /// <summary>
    /// AddEdgeLeftOperator: L1 operator for Dynamic Graph
    /// Left operator because it modifies graph structure (adds edge/neighbor relationship)
    ///   - V' = L_V(V, P, σ) = V (unchanged, graph data remains same)
    ///   - P' = L_P(V, P, σ) = P with updated degree (degree increases by 1)
    ///   - σ' = L_σ(V, P, σ) = σ with new neighbor added to Neighbors list
    /// - Adding an edge means adding a neighbor to the adjacency list
    /// - In undirected graphs, edge (u,v) means u has neighbor v and v has neighbor u
    /// - This operator adds neighbor from current node's perspective
    /// - Degree increases when neighbor is added (graph theory: degree = |adjacency list|)
    /// - A1 (Closure): Returns SNOAObject<GraphData, NodeState> (same type)
    /// - A2 (Structural Stability): V, P, σ all remain present (structure preserved)
    /// - A3 (State Mutability): State changes (Neighbors list modified)
    /// - A4 (Property Mutability): Properties change (degree updated)
    /// - A5 (Noncommutativity): AddEdge ∘ NormalizeGraph ≠ NormalizeGraph ∘ AddEdge
    /// </summary>
    public class AddEdgeLeftOperator : ILeftOperator<GraphData, NodeState>
    {
        private readonly int _targetNodeId;

        /// <summary>
        /// Constructor for AddEdgeLeftOperator
        /// - targetNodeId: The neighbor node ID to add to the current node's neighbor list
        /// </summary>
        /// <param name="targetNodeId">Target node ID to add edge to (neighbor to add)</param>
        public AddEdgeLeftOperator(int targetNodeId)
        {
            _targetNodeId = targetNodeId;
        }

        /// <summary>
        /// Apply AddEdge operator
        /// 1. Extract current graph data (V), properties (P), and state (σ)
        /// 2. Add targetNodeId to Neighbors list (structural change)
        /// 3. Create new state with updated Neighbors list
        /// 4. Update properties: degree = new neighbor count, last_modified = current time
        /// 5. Return new SNOAObject with same V, updated P', updated σ'
        /// - V' = V (GraphData unchanged - node ID and value remain same)
        /// - P' = P ∪ {degree: |Neighbors'|, last_modified: DateTime.Now}
        ///   where Neighbors' = Neighbors ∪ {targetNodeId}
        /// - σ' = (Neighbors', Level, Visited)
        ///   where Neighbors' = Neighbors + targetNodeId
        /// - If targetNodeId already in Neighbors: duplicate added (should be normalized later)
        /// - If targetNodeId equals current node ID: self-loop edge (valid in graph theory)
        /// - If Neighbors is null: initialized to new list with targetNodeId
        /// </summary>
        /// <param name="obj">Input SNOA object X = (V, P, σ) where V=GraphData, σ=NodeState</param>
        /// <returns>Result SNOA object X' = (V', P', σ') with edge added</returns>
        public SNOAObject<GraphData, NodeState> Apply(SNOAObject<GraphData, NodeState> obj)
        {
            // Extract components from input object X = (V, P, σ)
            // V: GraphData (node ID, value) - remains unchanged
            var graphData = obj.Value;
            
            // P: Properties (metadata) - will be updated with new degree
            var properties = obj.Properties;
            
            // σ: NodeState (neighbors, level, visited) - neighbors will be modified
            var state = obj.State;

            // Add neighbor (structural change)
            // Algorithm: Create new list with existing neighbors + targetNodeId
            // This follows graph theory: adding edge means adding neighbor to adjacency list
            // Note: Duplicates are allowed here (will be normalized by NormalizeGraphRightOperator)
            var newNeighbors = new List<int>(state.Neighbors) { _targetNodeId };
            
            // Create new state σ' with updated neighbors
            // Level and Visited remain unchanged (only structural change, not traversal state)
            var newState = new NodeState
            {
                Neighbors = newNeighbors,  // Updated: new neighbor added
                Level = state.Level,       // Unchanged: level not affected by edge addition
                Visited = state.Visited    // Unchanged: visited flag not affected
            };

            // Update properties P' = P ∪ {degree: |Neighbors'|, last_modified: DateTime.Now}
            // Degree = number of neighbors (graph theory: degree = |adjacency list|)
            // This follows MD requirement: "updates degree" when edge is added
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

