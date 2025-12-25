namespace SNOA.Core.CaseStudies
{
    /// <summary>
    /// GraphData: V component for Dynamic Graph case study
    /// - V: GraphData represents the main value component in SNOAObject<GraphData, NodeState>
    /// - Part of SNOA structure: X = (V, P, σ) where V = GraphData
    /// - NodeId: Unique identifier for the graph node
    /// - Value: Associated value/weight with the node (e.g., node weight, centrality score)
    /// - Used in graph algorithms: BFS, DFS, shortest path, etc.
    /// </summary>
    public class GraphData
    {
        /// <summary>
        /// NodeId: Unique identifier for the graph node
        /// - Each node in a graph has a unique identifier
        /// - Used for graph operations: edge creation, neighbor lookup, etc.
        /// - Typically non-negative integer (0, 1, 2, ...)
        /// </summary>
        public int NodeId { get; set; }

        /// <summary>
        /// Value: Associated value/weight with the node
        /// - Node value can represent weight, centrality, importance, etc.
        /// - Used in weighted graph algorithms
        /// - Can be used for graph metrics and analysis
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Constructor for GraphData
        /// 1. Initialize NodeId with provided nodeId
        /// 2. Initialize Value with provided value
        /// - nodeId: Unique identifier for the graph node (typically >= 0)
        /// - value: Associated value/weight with the node
        /// </summary>
        /// <param name="nodeId">Unique identifier for the graph node</param>
        /// <param name="value">Associated value/weight with the node</param>
        public GraphData(int nodeId, double value)
        {
            NodeId = nodeId;
            Value = value;
        }

        /// <summary>
        /// Object Equality: GraphData equality based on NodeId and Value
        /// 1. Check if obj is GraphData type
        /// 2. Compare NodeId for equality
        /// 3. Compare Value for equality (double comparison)
        /// 4. Return true if all components equal
        /// <param name="obj">Object to compare</param>
        /// <returns>True if NodeId and Value are equal</returns>
        public override bool Equals(object? obj)
        {
            return obj is GraphData other &&
                   NodeId == other.NodeId &&
                   Value == other.Value;
        }

        /// <summary>
        /// GetHashCode: Hash code based on NodeId and Value
        /// 1. Combine NodeId and Value using HashCode.Combine
        /// 2. Return combined hash code
        /// </summary>
        /// <returns>Hash code for GraphData</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(NodeId, Value);
        }
    }
}

