using FluentAssertions;
using SNOA.Core;
using SNOA.Core.Axioms;
using SNOA.Core.CaseStudies;
using System.Collections.Generic;
using Xunit;

namespace SNOA.Tests
{
    /// <summary>
    /// Dynamic Graph Case Study Tests
    /// </summary>
    public class CaseStudyTests
    {
        [Fact]
        public void GraphNode_Creation_WellFormed()
        {
            // Create GraphNode
            var graphData = new GraphData(1, 10.5);
            var properties = new Dictionary<string, object> { ["degree"] = 0 };
            var state = new NodeState();

            var node = new GraphNode(graphData, properties, state);

            node.Value.NodeId.Should().Be(1);
            node.Value.Value.Should().Be(10.5);
            node.Properties.Should().ContainKey("degree");
            node.State.Neighbors.Should().BeEmpty();
        }

        [Fact]
        public void AddEdgeLeftOperator_AddsNeighbor()
        {
            // Test AddEdge operator
            var graphData = new GraphData(1, 10.5);
            var properties = new Dictionary<string, object> { ["degree"] = 0 };
            var state = new NodeState();
            var node = new SNOAObject<GraphData, NodeState>(graphData, properties, state);

            var addEdgeOp = new AddEdgeLeftOperator(2);
            var result = addEdgeOp.Apply(node);

            // Neighbor should be added
            result.State.Neighbors.Should().Contain(2);
            result.State.Neighbors.Count.Should().Be(1);

            // Degree should be updated
            result.Properties["degree"].Should().Be(1);
            result.Properties.Should().ContainKey("last_modified");

            // Validate A1: Closure
            AxiomValidator.ValidateA1_Closure_Left(addEdgeOp, node).Should().BeTrue();
        }

        [Fact]
        public void NormalizeGraphRightOperator_NormalizesNeighbors()
        {
            // Test NormalizeGraph operator
            var graphData = new GraphData(1, 10.5);
            var properties = new Dictionary<string, object> { ["degree"] = 3 };
            var state = new NodeState
            {
                Neighbors = new List<int> { 3, 1, 3, 2 }, // Duplicates and unsorted
                Level = 5,
                Visited = true
            };
            var node = new SNOAObject<GraphData, NodeState>(graphData, properties, state);

            var normalizeOp = new NormalizeGraphRightOperator();
            var result = normalizeOp.Apply(node);

            // Neighbors should be normalized (distinct, sorted)
            result.State.Neighbors.Should().BeEquivalentTo(new[] { 1, 2, 3 });
            result.State.Neighbors.Should().BeInAscendingOrder();

            // Level and Visited should be reset
            result.State.Level.Should().Be(0);
            result.State.Visited.Should().BeFalse();

            // Properties should be updated
            result.Properties["degree"].Should().Be(3);
            result.Properties["normalized"].Should().Be(true);
            result.Properties.Should().ContainKey("normalization_time");

            // Validate A1: Closure
            AxiomValidator.ValidateA1_Closure_Right(normalizeOp, node).Should().BeTrue();
        }

        [Fact]
        public void Noncommutativity_DynamicGraph_AddEdgeAndNormalize()
        {
            // Demonstrate noncommutativity (A5) in dynamic graph context
            var graphData = new GraphData(1, 10.5);
            var properties = new Dictionary<string, object> { ["degree"] = 0 };
            var state = new NodeState { Neighbors = new List<int> { 5 } };
            var node = new SNOAObject<GraphData, NodeState>(graphData, properties, state);

            var addEdgeOp = new AddEdgeLeftOperator(3);
            var normalizeOp = new NormalizeGraphRightOperator();

            // AddEdge(Normalize(X))
            var result1 = addEdgeOp.Apply(normalizeOp.Apply(node));

            // Normalize(AddEdge(X))
            var result2 = normalizeOp.Apply(addEdgeOp.Apply(node));

            // Results should differ (noncommutative)
            result1.Should().NotBeEquivalentTo(result2);

            // Validate A5: Noncommutativity
            AxiomValidator.ValidateA5_Noncommutativity(addEdgeOp, normalizeOp, node).Should().BeTrue();
        }

        [Fact]
        public void GraphNode_HelperProperties_Work()
        {
            // Test helper properties from GraphNode
            var graphData = new GraphData(1, 10.5);
            var properties = new Dictionary<string, object>
            {
                ["degree"] = 5,
                ["centrality"] = 0.75
            };
            var state = new NodeState();
            var node = new GraphNode(graphData, properties, state);

            node.Degree.Should().Be(5);
            node.Centrality.Should().Be(0.75);
        }
    }
}




