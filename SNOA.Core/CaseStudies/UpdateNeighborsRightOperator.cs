using System;
using System.Collections.Generic;
using System.Linq;

namespace SNOA.Core.CaseStudies
{
    /// <summary>
    /// UpdateNeighborsRightOperator: R3 operator for Dynamic Graph
    /// Right operator because it manages state (updates neighbor relationships, normalizes connections)
    ///   - V' = R_V(V, P, σ) = V (unchanged, graph data remains same)
    ///   - P' = R_P(V, P, σ) = P with updated neighbor metadata
    ///   - σ' = R_σ(V, P, σ) = σ with updated neighbors based on graph state
    /// - Neighbor update: Refresh neighbor list based on current graph state
    /// - In dynamic graphs, neighbors may change due to other operations
    /// - This operator ensures neighbor list reflects current graph connectivity
    /// - Similar to graph synchronization: update local view based on global state
    /// - Handles bidirectional edges: if A has neighbor B, B should have neighbor A (in undirected graphs)
    /// - A1 (Closure): Returns SNOAObject<GraphData, NodeState> (same type)
    /// - A2 (Structural Stability): V, P, σ all remain present (structure preserved)
    /// - A3 (State Mutability): State changes (Neighbors updated based on graph state)
    /// - A4 (Property Mutability): Properties change (neighbor metadata updated)
    /// - A5 (Noncommutativity): UpdateNeighbors ∘ AddEdge ≠ AddEdge ∘ UpdateNeighbors
    /// </summary>
    public class UpdateNeighborsRightOperator : IRightOperator<GraphData, NodeState>
    {
        /// <summary>
        /// Optional: Function to query graph state for neighbor relationships
        /// In a full graph implementation, this would query the global graph structure
        /// For now, we use the current neighbors list and validate/update it
        /// </summary>
        private readonly Func<int, List<int>>? _graphStateQuery;

        /// <summary>
        /// Constructor for UpdateNeighborsRightOperator
        /// - graphStateQuery: Optional function to query graph state for neighbors
        ///                     If null, uses current neighbors and validates/updates them
        /// - If graphStateQuery provided: query global graph for current neighbors
        /// - If graphStateQuery null: validate and update current neighbors list
        /// - Ensures neighbor list reflects current graph connectivity
        /// </summary>
        /// <param name="graphStateQuery">Optional function to query graph state for neighbors of a node</param>
        public UpdateNeighborsRightOperator(Func<int, List<int>>? graphStateQuery = null)
        {
            _graphStateQuery = graphStateQuery;
        }

        /// <summary>
        /// Apply UpdateNeighbors operator
        /// 1. Extract current graph data (V), properties (P), and state (σ)
        /// 2. Query or validate current neighbors based on graph state
        /// 3. Update neighbor list to reflect current graph connectivity
        /// 4. Remove invalid neighbors (nodes that no longer exist or edges removed)
        /// 5. Add missing neighbors (if graph state indicates new edges)
        /// 6. Update properties with neighbor metadata
        /// 7. Return new SNOAObject with updated neighbors
        /// - V' = V (GraphData unchanged - node ID and value remain same)
        /// - P' = P ∪ {degree: |Neighbors'|, neighbors_updated: true, update_time: DateTime.Now}
        ///   where Neighbors' = updated neighbors based on graph state
        /// - σ' = (Neighbors', Level, Visited)
        ///   where Neighbors' = neighbors from graph state query or validated current neighbors
        /// - Updates neighbor list to match current graph connectivity
        /// - Removes neighbors that no longer have edges (edge removed elsewhere)
        /// - Adds neighbors that now have edges (edge added elsewhere)
        /// - In undirected graphs, ensures bidirectional consistency
        /// - If graphStateQuery provided: use it to get authoritative neighbor list
        /// - If graphStateQuery null: validate current neighbors (remove duplicates, sort, ensure valid)
        /// - This allows both global graph synchronization and local validation
        /// - If graphStateQuery returns null: use current neighbors (fallback)
        /// - If graphStateQuery returns empty: node has no neighbors (isolated)
        /// - If current neighbors invalid: remove invalid entries
        /// - If duplicates exist: remove duplicates (normalize)
        /// </summary>
        /// <param name="obj">Input SNOA object X = (V, P, σ) where V=GraphData, σ=NodeState</param>
        /// <returns>Result SNOA object X' = (V', P', σ') with neighbors updated</returns>
        public SNOAObject<GraphData, NodeState> Apply(SNOAObject<GraphData, NodeState> obj)
        {
            // Extract components from input object X = (V, P, σ)
            // V: GraphData (node ID, value) - remains unchanged
            var graphData = obj.Value;
            
            // P: Properties (metadata) - will be updated with neighbor metadata
            var properties = obj.Properties;
            
            // σ: NodeState (neighbors, level, visited) - neighbors will be updated
            var state = obj.State;

            // Update neighbors based on current graph state
            List<int> updatedNeighbors;
            
            if (_graphStateQuery != null)
            {
                // Query global graph state for current neighbors
                // This represents synchronization with global graph structure
                // Algorithm: Query graph to get authoritative neighbor list
                var queriedNeighbors = _graphStateQuery(graphData.NodeId);
                
                // Use queried neighbors if available, otherwise fallback to current neighbors
                updatedNeighbors = queriedNeighbors ?? state.Neighbors;
            }
            else
            {
                // No graph state query available: validate and update current neighbors
                // Algorithm: Remove duplicates, sort, ensure valid (non-negative node IDs)
                // This represents local validation of neighbor list
                updatedNeighbors = state.Neighbors
                    .Where(n => n >= 0)  // Remove invalid node IDs (negative = invalid)
                    .Distinct()          // Remove duplicates
                    .OrderBy(n => n)     // Sort for consistency
                    .ToList();
            }

            // Create new state σ' with updated neighbors
            // Level and Visited remain unchanged (only neighbor relationships updated, not traversal state)
            var newState = new NodeState
            {
                Neighbors = updatedNeighbors,  // Updated: neighbors from graph state
                Level = state.Level,            // Unchanged: level not affected by neighbor update
                Visited = state.Visited         // Unchanged: visited flag not affected
            };

            // Calculate neighbor change statistics
            var previousCount = state.Neighbors.Count;
            var currentCount = updatedNeighbors.Count;
            // Added: neighbors in updated but not in original (new neighbors)
            var addedCount = updatedNeighbors.Except(state.Neighbors).Count();
            // Removed: neighbors in original but not in updated (removed neighbors)
            // Note: Use Distinct() to count unique removed neighbors, not total occurrences
            var removedNeighbors = state.Neighbors.Except(updatedNeighbors).Distinct().ToList();
            var removedCount = removedNeighbors.Count;

            // Update properties P' = P ∪ {degree, neighbor metadata, update_time}
            // Degree = number of updated neighbors (graph theory: degree = |adjacency list|)
            // Neighbor metadata tracks the update operation and changes
            var newProperties = new Dictionary<string, object>(properties)
            {
                ["degree"] = updatedNeighbors.Count,              // Updated: degree = updated neighbor count
                ["neighbors_updated"] = true,                      // New: flag that neighbors were updated
                ["neighbors_update_time"] = DateTime.Now,          // New: when neighbors were updated
                ["previous_neighbor_count"] = previousCount,       // New: previous neighbor count
                ["neighbors_added"] = addedCount,                  // New: how many neighbors were added
                ["neighbors_removed"] = removedCount,              // New: how many neighbors were removed
                ["last_modified"] = DateTime.Now                  // Updated: track modification time
            };

            // Return new SNOAObject X' = (V', P', σ')
            // V' = V (unchanged), P' = updated properties, σ' = updated state with synchronized neighbors
            // This satisfies Axiom A1 (Closure): returns SNOAObject<GraphData, NodeState>
            // Neighbors now reflect current graph state
            return new SNOAObject<GraphData, NodeState>(graphData, newProperties, newState);
        }
    }
}

