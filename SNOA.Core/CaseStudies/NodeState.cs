using System.Collections.Generic;

namespace SNOA.Core.CaseStudies
{
    /// <summary>
    /// NodeState: σ component for Dynamic Graph case study
    /// - σ: NodeState represents the internal state component in SNOAObject<GraphData, NodeState>
    /// - Part of SNOA structure: X = (V, P, σ) where σ = NodeState
    /// - State mutability: Axiom A3 allows state changes σ' = T(σ)
    /// - Neighbors: Adjacency list (list of neighbor node IDs)
    /// - Level: Traversal depth (used in BFS/DFS algorithms)
    /// - Visited: Traversal flag (used in graph traversal algorithms)
    /// - These are transient fields that change during graph operations
    /// </summary>
    public class NodeState
    {
        /// <summary>
        /// Neighbors: Adjacency list - list of neighbor node IDs
        /// - Represents edges connected to this node
        /// - In undirected graphs: if A has neighbor B, then B has neighbor A
        /// - Degree of node = |Neighbors| (number of neighbors)
        /// - Used for graph traversal: BFS, DFS, shortest path, etc.
        /// - Part of state σ that changes with structural operations (AddEdge, RemoveEdge)
        /// - Modified by Left Operators (L1: AddEdge, L2: RemoveEdge)
        /// - Normalized by Right Operators (R1: NormalizeGraph, R3: UpdateNeighbors)
        /// </summary>
        public List<int> Neighbors { get; set; }

        /// <summary>
        /// Level: Traversal depth - distance from source node in graph traversal
        /// - Used in BFS (Breadth-First Search): level = distance from start node
        /// - Level 0: source/start node
        /// - Level 1: nodes directly connected to source
        /// - Level 2: nodes two hops away from source, etc.
        /// - Transient field: reset after traversal or normalization
        /// - Transient state field (reset by NormalizeGraphRightOperator)
        /// - Modified during graph traversal operations
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Visited: Traversal flag - indicates if node has been visited in graph traversal
        /// - Used in DFS (Depth-First Search) and BFS to avoid revisiting nodes
        /// - Prevents infinite loops in cyclic graphs
        /// - Transient field: reset after traversal or normalization
        /// - Transient state field (reset by NormalizeGraphRightOperator)
        /// - Modified during graph traversal operations
        /// </summary>
        public bool Visited { get; set; }

        /// <summary>
        /// Default constructor for NodeState
        /// 1. Initialize Neighbors to empty list
        /// 2. Initialize Level to 0 (no traversal depth)
        /// 3. Initialize Visited to false (not visited)
        /// </summary>
        public NodeState()
        {
            Neighbors = new List<int>();
            Level = 0;
            Visited = false;
        }

        /// <summary>
        /// Constructor for NodeState with initial values
        /// 1. Initialize Neighbors with provided list (or empty if null)
        /// 2. Initialize Level with provided level
        /// 3. Initialize Visited with provided visited flag
        /// - neighbors: Initial adjacency list (null-safe: defaults to empty list)
        /// - level: Initial traversal depth
        /// - visited: Initial visited flag
        /// </summary>
        /// <param name="neighbors">Initial adjacency list (list of neighbor node IDs)</param>
        /// <param name="level">Initial traversal depth (default: 0)</param>
        /// <param name="visited">Initial visited flag (default: false)</param>
        public NodeState(List<int> neighbors, int level, bool visited)
        {
            Neighbors = neighbors ?? new List<int>();
            Level = level;
            Visited = visited;
        }

        /// <summary>
        /// Object Equality: NodeState equality based on Neighbors, Level, and Visited
        /// 1. Check if obj is NodeState type
        /// 2. Compare Neighbors list length
        /// 3. Compare each neighbor in order (list order matters)
        /// 4. Compare Level for equality
        /// 5. Compare Visited for equality
        /// 6. Return true if all components equal
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if Neighbors, Level, and Visited are all equal</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not NodeState other)
                return false;

            if (Neighbors.Count != other.Neighbors.Count)
                return false;

            for (int i = 0; i < Neighbors.Count; i++)
            {
                if (Neighbors[i] != other.Neighbors[i])
                    return false;
            }

            return Level == other.Level && Visited == other.Visited;
        }

        /// <summary>
        /// GetHashCode: Hash code based on Neighbors, Level, and Visited
        /// 1. Create HashCode builder
        /// 2. Add each neighbor to hash code (order matters)
        /// 3. Add Level to hash code
        /// 4. Add Visited to hash code
        /// 5. Return combined hash code
        /// Note: Hash code includes all neighbors in order
        /// </summary>
        /// <returns>Hash code for NodeState</returns>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            foreach (var neighbor in Neighbors)
            {
                hashCode.Add(neighbor);
            }
            hashCode.Add(Level);
            hashCode.Add(Visited);
            return hashCode.ToHashCode();
        }
    }
}

