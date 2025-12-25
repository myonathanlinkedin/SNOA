using System.Collections.Generic;

namespace SNOA.Core.CaseStudies
{
    /// <summary>
    /// GraphNode: SNOA Object for Dynamic Graph case study
    /// Where:
    /// - V: GraphData - node ID, value, etc.
    /// - P: NodeProperties - metadata (degree, centrality, etc.)
    /// - σ: NodeState - neighbors, level, etc.
    /// </summary>
    public class GraphNode : SNOAObject<GraphData, NodeState>
    {
        /// <summary>
        /// Constructor for GraphNode
        /// 1. Initialize base SNOAObject with GraphData (V), properties (P), and NodeState (σ)
        /// 2. GraphNode inherits from SNOAObject<GraphData, NodeState>
        /// - V: GraphData (node ID, value) - main value component
        /// - P: Dictionary<string, object> (properties/metadata) - degree, centrality, etc.
        /// - σ: NodeState (neighbors, level, visited) - internal state component
        /// <param name="graphData">V component: GraphData (node ID, value)</param>
        /// <param name="properties">P component: Properties/metadata dictionary</param>
        /// <param name="state">σ component: NodeState (neighbors, level, visited)</param>
        public GraphNode(GraphData graphData, Dictionary<string, object> properties, NodeState state)
            : base(graphData, properties, state)
        {
        }

        /// <summary>
        /// Helper property: Degree from properties
        /// </summary>
        public int Degree => (int)(Properties.GetValueOrDefault("degree", 0));

        /// <summary>
        /// Helper property: Centrality from properties
        /// </summary>
        public double Centrality => (double)(Properties.GetValueOrDefault("centrality", 0.0));
    }
}

