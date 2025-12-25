using System;
using System.Collections.Generic;
using System.Linq;

namespace SNOA.Core.CaseStudies
{
    /// <summary>
    /// NormalizeGraphRightOperator: R1 operator for Dynamic Graph
    /// Right operator because it manages state (normalization, state cleanup)
    ///   - V' = R_V(V, P, σ) = V (unchanged, graph data remains same)
    ///   - P' = R_P(V, P, σ) = P with normalized flag and updated degree
    ///   - σ' = R_σ(V, P, σ) = σ with normalized neighbors (distinct, sorted), transient fields reset
    /// - Normalization: Clean up graph structure by removing duplicates and sorting
    /// - Duplicate neighbors can occur from multiple AddEdge operations
    /// - Sorting ensures consistent neighbor order (important for algorithms like BFS/DFS)
    /// - Resetting Level and Visited clears transient traversal state
    /// - This is similar to graph cleanup: ensure data structure integrity
    /// - A1 (Closure): Returns SNOAObject<GraphData, NodeState> (same type)
    /// - A2 (Structural Stability): V, P, σ all remain present (structure preserved)
    /// - A3 (State Mutability): State changes (Neighbors normalized, transient fields reset)
    /// - A4 (Property Mutability): Properties change (normalized flag, degree updated)
    /// - A5 (Noncommutativity): NormalizeGraph ∘ AddEdge ≠ AddEdge ∘ NormalizeGraph
    /// </summary>
    public class NormalizeGraphRightOperator : IRightOperator<GraphData, NodeState>
    {
        /// <summary>
        /// Constructor for NormalizeGraphRightOperator
        /// The normalization operation is based on the current state of the object.
        /// </summary>
        public NormalizeGraphRightOperator()
        {
            // No parameters needed - normalization is based on current object state
        }

        /// <summary>
        /// Apply NormalizeGraph operator
        /// 1. Extract current graph data (V), properties (P), and state (σ)
        /// 2. Normalize neighbors: remove duplicates, sort ascending
        /// 3. Reset transient state fields: Level = 0, Visited = false
        /// 4. Update properties: degree = normalized neighbor count, normalized flag, timestamp
        /// 5. Return new SNOAObject with normalized state
        /// - V' = V (GraphData unchanged - node ID and value remain same)
        /// - P' = P ∪ {degree: |Neighbors_normalized|, normalized: true, normalization_time: DateTime.Now}
        ///   where Neighbors_normalized = Distinct(Neighbors).Sort()
        /// - σ' = (Neighbors_normalized, Level=0, Visited=false)
        ///   where Neighbors_normalized = Distinct(Neighbors).OrderBy(n => n)
        /// - Remove duplicates: ensures each neighbor appears only once
        /// - Sort ascending: ensures consistent neighbor order (important for graph algorithms)
        /// - Reset Level: clears traversal depth (ready for new traversal)
        /// - Reset Visited: clears traversal flag (ready for new traversal)
        /// - Duplicate neighbors can occur from multiple AddEdge operations
        /// - Unsorted neighbors make graph algorithms unpredictable
        /// - Transient fields (Level, Visited) should reset after normalization
        /// - Normalized state is "clean" and ready for graph operations
        /// - If Neighbors is empty: normalization results in empty list (valid)
        /// - If Neighbors has duplicates: all duplicates removed
        /// - If Neighbors is unsorted: sorted ascending
        /// </summary>
        /// <param name="obj">Input SNOA object X = (V, P, σ) where V=GraphData, σ=NodeState</param>
        /// <returns>Result SNOA object X' = (V', P', σ') with normalized state</returns>
        public SNOAObject<GraphData, NodeState> Apply(SNOAObject<GraphData, NodeState> obj)
        {
            // Extract components from input object X = (V, P, σ)
            // V: GraphData (node ID, value) - remains unchanged
            var graphData = obj.Value;
            
            // P: Properties (metadata) - will be updated with normalized flag
            var properties = obj.Properties;
            
            // σ: NodeState (neighbors, level, visited) - will be normalized
            var state = obj.State;

            // Normalize: remove duplicates, sort neighbors
            // Algorithm: Distinct() removes duplicates, OrderBy() sorts ascending
            // This follows graph theory: normalized adjacency list has unique, sorted neighbors
            // Why distinct: duplicate neighbors can occur from multiple AddEdge operations
            // Why sort: consistent order is important for graph algorithms (BFS, DFS, etc.)
            var normalizedNeighbors = state.Neighbors.Distinct().OrderBy(n => n).ToList();
            
            // Create new state σ' with normalized neighbors and reset transient fields
            // Neighbors: normalized (distinct, sorted)
            // Level: reset to 0 (clear traversal depth)
            // Visited: reset to false (clear traversal flag)
            var newState = new NodeState
            {
                Neighbors = normalizedNeighbors,  // Updated: normalized (distinct, sorted)
                Level = 0,                        // Reset: clear traversal depth
                Visited = false                   // Reset: clear traversal flag
            };

            // Update properties P' = P ∪ {degree, normalized flag, normalization_time}
            // Degree = number of normalized neighbors (graph theory: degree = |adjacency list|)
            // Normalized flag indicates state has been normalized
            // Normalization time tracks when normalization occurred
            var newProperties = new Dictionary<string, object>(properties)
            {
                ["degree"] = normalizedNeighbors.Count,  // Updated: degree = normalized neighbor count
                ["normalized"] = true,                    // New: flag that state is normalized
                ["normalization_time"] = DateTime.Now     // New: when normalization occurred
            };

            // Return new SNOAObject X' = (V', P', σ')
            // V' = V (unchanged), P' = updated properties, σ' = normalized state
            // This satisfies Axiom A1 (Closure): returns SNOAObject<GraphData, NodeState>
            // State is now "normalized" and ready for graph operations
            return new SNOAObject<GraphData, NodeState>(graphData, newProperties, newState);
        }
    }
}

