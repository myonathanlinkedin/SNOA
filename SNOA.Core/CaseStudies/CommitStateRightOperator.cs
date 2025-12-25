using System;
using System.Collections.Generic;

namespace SNOA.Core.CaseStudies
{
    /// <summary>
    /// CommitStateRightOperator: R2 operator for Dynamic Graph
    /// Right operator because it manages state (commits changes, updates statistics)
    ///   - V' = R_V(V, P, σ) = V (unchanged, graph data remains same)
    ///   - P' = R_P(V, P, σ) = P with committed state flag and global statistics
    ///   - σ' = R_σ(V, P, σ) = σ with committed state (may reset transient fields)
    /// - State commit: Make temporary state changes permanent
    /// - Global statistics: Update graph-level metrics (total nodes, total edges, etc.)
    /// - This is similar to database commit: make changes visible to other operations
    /// - After commit, state is considered "stable" and ready for next operations
    /// - A1 (Closure): Returns SNOAObject<GraphData, NodeState> (same type)
    /// - A2 (Structural Stability): V, P, σ all remain present (structure preserved)
    /// - A3 (State Mutability): State changes (committed flag set, transient fields may reset)
    /// - A4 (Property Mutability): Properties change (commit metadata, statistics added)
    /// - A5 (Noncommutativity): CommitState ∘ AddEdge ≠ AddEdge ∘ CommitState
    /// </summary>
    public class CommitStateRightOperator : IRightOperator<GraphData, NodeState>
    {
        /// <summary>
        /// Constructor for CommitStateRightOperator
        /// The commit operation is based on the current state of the object.
        /// </summary>
        public CommitStateRightOperator()
        {
            // No parameters needed - commit is based on current object state
        }

        /// <summary>
        /// Apply CommitState operator
        /// 1. Extract current graph data (V), properties (P), and state (σ)
        /// 2. Mark state as committed (set committed flag in properties)
        /// 3. Calculate and update global graph statistics:
        ///    - Total nodes in graph (estimated from node ID or properties)
        ///    - Total edges (estimated from degree or properties)
        ///    - Graph density (if calculable)
        /// 4. Reset transient state fields if needed (Level, Visited may reset after commit)
        /// 5. Update properties with commit metadata and statistics
        /// 6. Return new SNOAObject with committed state
        /// - V' = V (GraphData unchanged - node ID and value remain same)
        /// - P' = P ∪ {committed: true, commit_time: DateTime.Now, global_stats: {...}}
        ///   where global_stats contains graph-level statistics
        /// - σ' = (Neighbors, Level_reset, Visited_reset)
        ///   where Level and Visited may reset after commit (transient fields)
        /// - Committed state means changes are "finalized" and ready for next operations
        /// - Similar to database transaction commit: make changes permanent
        /// - After commit, transient traversal state (Level, Visited) may reset
        /// - Global statistics are updated to reflect committed state
        /// - Since this is a single-node operation, global stats are estimated or tracked per-node
        /// - In a full graph implementation, this would query global graph state
        /// - For now, we track commit count and node-level statistics
        /// - If state already committed: re-commit updates timestamp
        /// - If no neighbors: statistics reflect isolated node
        /// - If properties missing: initialize with default values
        /// </summary>
        /// <param name="obj">Input SNOA object X = (V, P, σ) where V=GraphData, σ=NodeState</param>
        /// <returns>Result SNOA object X' = (V', P', σ') with state committed</returns>
        public SNOAObject<GraphData, NodeState> Apply(SNOAObject<GraphData, NodeState> obj)
        {
            // Extract components from input object X = (V, P, σ)
            // V: GraphData (node ID, value) - remains unchanged
            var graphData = obj.Value;
            
            // P: Properties (metadata) - will be updated with commit metadata and statistics
            var properties = obj.Properties;
            
            // σ: NodeState (neighbors, level, visited) - transient fields may reset after commit
            var state = obj.State;

            // Calculate global graph statistics
            // Note: In a full graph implementation, this would query global graph state
            // For single-node operation, we estimate or track per-node statistics
            var degree = state.Neighbors.Count;
            var nodeId = graphData.NodeId;
            
            // Estimate graph statistics (in real implementation, this would query global graph)
            // These are per-node estimates based on current state
            var estimatedTotalNodes = Math.Max(1, nodeId); // Estimate: at least nodeId nodes exist
            var estimatedTotalEdges = degree; // Estimate: this node contributes 'degree' edges
            
            // Calculate graph density estimate (edges / (nodes * (nodes-1) / 2) for directed)
            // This is a simplified estimate for single-node perspective
            var densityEstimate = estimatedTotalNodes > 1 
                ? (double)estimatedTotalEdges / (estimatedTotalNodes * (estimatedTotalNodes - 1) / 2.0)
                : 0.0;

            // Create new state σ' with committed state
            // After commit, transient traversal fields (Level, Visited) may reset
            // Neighbors remain (structural state is committed, not reset)
            var newState = new NodeState
            {
                Neighbors = state.Neighbors,  // Unchanged: structural state committed as-is
                Level = 0,                     // Reset: transient traversal state cleared after commit
                Visited = false                // Reset: transient traversal state cleared after commit
            };

            // Update properties P' = P ∪ {commit metadata, global statistics, last_modified}
            // Commit metadata tracks that state has been committed
            // Global statistics provide graph-level metrics (estimated from node perspective)
            var newProperties = new Dictionary<string, object>(properties)
            {
                // Commit metadata
                ["committed"] = true,                          // New: state is committed
                ["commit_time"] = DateTime.Now,                // New: when state was committed
                ["commit_count"] = ((int)(properties.GetValueOrDefault("commit_count", 0))) + 1, // Track commit frequency
                
                // Global graph statistics (estimated from node perspective)
                ["estimated_total_nodes"] = estimatedTotalNodes,  // New: estimated total nodes in graph
                ["estimated_total_edges"] = estimatedTotalEdges,  // New: estimated total edges in graph
                ["graph_density_estimate"] = densityEstimate,     // New: estimated graph density
                
                // Node-level statistics
                ["degree"] = degree,                            // Updated: ensure degree is current
                ["last_modified"] = DateTime.Now                // Updated: track modification time
            };

            // Return new SNOAObject X' = (V', P', σ')
            // V' = V (unchanged), P' = updated properties with commit metadata, σ' = committed state
            // This satisfies Axiom A1 (Closure): returns SNOAObject<GraphData, NodeState>
            // State is now "committed" and ready for next operations
            return new SNOAObject<GraphData, NodeState>(graphData, newProperties, newState);
        }
    }
}




