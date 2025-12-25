using FluentAssertions;
using SNOA.Core;
using SNOA.Core.Axioms;
using SNOA.Core.CaseStudies;
using System.Collections.Generic;
using Xunit;

namespace SNOA.Tests
{
    /// <summary>
    /// Additional Case Study Tests for New Operators
    /// 
    /// Tests for operators that were not in original implementation:
    /// - L2: RemoveEdgeLeftOperator
    /// - L3: SplitNodeLeftOperator
    /// - R2: CommitStateRightOperator
    /// - R3: UpdateNeighborsRightOperator
    /// </summary>
    public class CaseStudyAdditionalTests
    {
        #region L2: RemoveEdgeLeftOperator Tests

        /// <summary>
        /// Test RemoveEdgeLeftOperator removes neighbor correctly
        /// </summary>
        [Fact]
        public void RemoveEdgeLeftOperator_RemovesNeighbor()
        {
            // Arrange: Create node with neighbors
            var graphData = new GraphData(1, 10.0);
            var properties = new Dictionary<string, object> { ["degree"] = 3 };
            var state = new NodeState
            {
                Neighbors = new List<int> { 2, 3, 4 },
                Level = 0,
                Visited = false
            };
            var node = new SNOAObject<GraphData, NodeState>(graphData, properties, state);

            // Act: Remove edge to node 3
            var removeEdgeOp = new RemoveEdgeLeftOperator(3);
            var result = removeEdgeOp.Apply(node);

            // Assert: Neighbor 3 should be removed
            result.State.Neighbors.Should().NotContain(3);
            result.State.Neighbors.Should().Contain(2);
            result.State.Neighbors.Should().Contain(4);
            result.State.Neighbors.Count.Should().Be(2);
            result.Properties["degree"].Should().Be(2);
            result.Properties.Should().ContainKey("last_modified");

            // Validate A1: Closure
            AxiomValidator.ValidateA1_Closure_Left(removeEdgeOp, node).Should().BeTrue();
        }

        /// <summary>
        /// Test RemoveEdgeLeftOperator handles non-existent neighbor (idempotent)
        /// </summary>
        [Fact]
        public void RemoveEdgeLeftOperator_NonExistentNeighbor_IsIdempotent()
        {
            // Arrange: Create node without neighbor 5
            var graphData = new GraphData(1, 10.0);
            var properties = new Dictionary<string, object> { ["degree"] = 2 };
            var state = new NodeState
            {
                Neighbors = new List<int> { 2, 3 },
                Level = 0,
                Visited = false
            };
            var node = new SNOAObject<GraphData, NodeState>(graphData, properties, state);

            // Act: Try to remove non-existent neighbor 5
            var removeEdgeOp = new RemoveEdgeLeftOperator(5);
            var result = removeEdgeOp.Apply(node);

            // Assert: State unchanged (idempotent operation)
            result.State.Neighbors.Should().BeEquivalentTo(new[] { 2, 3 });
            result.State.Neighbors.Count.Should().Be(2);
            result.Properties["degree"].Should().Be(2);
        }

        #endregion

        #region L3: SplitNodeLeftOperator Tests

        /// <summary>
        /// Test SplitNodeLeftOperator splits neighbors correctly
        /// </summary>
        [Fact]
        public void SplitNodeLeftOperator_SplitsNeighbors()
        {
            // Arrange: Create node with 4 neighbors
            var graphData = new GraphData(1, 10.0);
            var properties = new Dictionary<string, object> { ["degree"] = 4 };
            var state = new NodeState
            {
                Neighbors = new List<int> { 2, 3, 4, 5 },
                Level = 0,
                Visited = false
            };
            var node = new SNOAObject<GraphData, NodeState>(graphData, properties, state);

            // Act: Split node with 50% ratio (newNodeId = 10)
            var splitOp = new SplitNodeLeftOperator(10, 0.5);
            var result = splitOp.Apply(node);

            // Assert: Half neighbors kept (maintains connectivity - at least 1)
            result.State.Neighbors.Count.Should().Be(2); // 50% of 4 = 2
            result.Properties["degree"].Should().Be(2);
            result.Properties["split_node_id"].Should().Be(10);
            result.Properties["split_ratio"].Should().Be(0.5);
            result.Properties["redistributed_neighbors"].Should().Be(2);
            result.Properties.Should().ContainKey("last_modified");

            // Validate A1: Closure
            AxiomValidator.ValidateA1_Closure_Left(splitOp, node).Should().BeTrue();
        }

        /// <summary>
        /// Test SplitNodeLeftOperator maintains connectivity (keeps at least 1 neighbor)
        /// </summary>
        [Fact]
        public void SplitNodeLeftOperator_MaintainsConnectivity()
        {
            // Arrange: Create node with 1 neighbor
            var graphData = new GraphData(1, 10.0);
            var properties = new Dictionary<string, object> { ["degree"] = 1 };
            var state = new NodeState
            {
                Neighbors = new List<int> { 2 },
                Level = 0,
                Visited = false
            };
            var node = new SNOAObject<GraphData, NodeState>(graphData, properties, state);

            // Act: Try to split with 0% ratio (should keep at least 1 for connectivity)
            var splitOp = new SplitNodeLeftOperator(10, 0.0);
            var result = splitOp.Apply(node);

            // Assert: At least 1 neighbor kept (maintains connectivity)
            result.State.Neighbors.Count.Should().Be(1);
            result.Properties["degree"].Should().Be(1);
        }

        /// <summary>
        /// Test SplitNodeLeftOperator handles empty neighbors (cannot split)
        /// </summary>
        [Fact]
        public void SplitNodeLeftOperator_EmptyNeighbors_ReturnsUnchanged()
        {
            // Arrange: Create node with no neighbors
            var graphData = new GraphData(1, 10.0);
            var properties = new Dictionary<string, object> { ["degree"] = 0 };
            var state = new NodeState
            {
                Neighbors = new List<int>(),
                Level = 0,
                Visited = false
            };
            var node = new SNOAObject<GraphData, NodeState>(graphData, properties, state);

            // Act: Try to split node with no neighbors
            var splitOp = new SplitNodeLeftOperator(10, 0.5);
            var result = splitOp.Apply(node);

            // Assert: Unchanged (cannot split node with no neighbors)
            result.State.Neighbors.Should().BeEmpty();
            result.Properties["degree"].Should().Be(0);
        }

        #endregion

        #region R2: CommitStateRightOperator Tests

        /// <summary>
        /// Test CommitStateRightOperator commits state correctly
        /// </summary>
        [Fact]
        public void CommitStateRightOperator_CommitsState()
        {
            // Arrange: Create node with state
            var graphData = new GraphData(1, 10.0);
            var properties = new Dictionary<string, object> { ["degree"] = 3 };
            var state = new NodeState
            {
                Neighbors = new List<int> { 2, 3, 4 },
                Level = 5,
                Visited = true
            };
            var node = new SNOAObject<GraphData, NodeState>(graphData, properties, state);

            // Act: Commit state
            var commitOp = new CommitStateRightOperator();
            var result = commitOp.Apply(node);

            // Assert: State committed, transient fields reset
            result.Properties["committed"].Should().Be(true);
            result.Properties.Should().ContainKey("commit_time");
            result.Properties.Should().ContainKey("commit_count");
            result.Properties.Should().ContainKey("estimated_total_nodes");
            result.Properties.Should().ContainKey("estimated_total_edges");
            result.Properties.Should().ContainKey("graph_density_estimate");
            
            // Transient fields reset
            result.State.Level.Should().Be(0);
            result.State.Visited.Should().BeFalse();
            
            // Structural state preserved
            result.State.Neighbors.Should().BeEquivalentTo(new[] { 2, 3, 4 });

            // Validate A1: Closure
            AxiomValidator.ValidateA1_Closure_Right(commitOp, node).Should().BeTrue();
        }

        /// <summary>
        /// Test CommitStateRightOperator updates commit count
        /// </summary>
        [Fact]
        public void CommitStateRightOperator_UpdatesCommitCount()
        {
            // Arrange: Create node with existing commit count
            var graphData = new GraphData(1, 10.0);
            var properties = new Dictionary<string, object> 
            { 
                ["degree"] = 2,
                ["commit_count"] = 3
            };
            var state = new NodeState
            {
                Neighbors = new List<int> { 2, 3 },
                Level = 0,
                Visited = false
            };
            var node = new SNOAObject<GraphData, NodeState>(graphData, properties, state);

            // Act: Commit state again
            var commitOp = new CommitStateRightOperator();
            var result = commitOp.Apply(node);

            // Assert: Commit count incremented
            result.Properties["commit_count"].Should().Be(4);
        }

        #endregion

        #region R3: UpdateNeighborsRightOperator Tests

        /// <summary>
        /// Test UpdateNeighborsRightOperator updates neighbors without query
        /// </summary>
        [Fact]
        public void UpdateNeighborsRightOperator_UpdatesNeighbors_WithoutQuery()
        {
            // Arrange: Create node with duplicates and unsorted neighbors
            var graphData = new GraphData(1, 10.0);
            var properties = new Dictionary<string, object> { ["degree"] = 5 };
            var state = new NodeState
            {
                Neighbors = new List<int> { 3, 1, 3, 2, -1 }, // Duplicates and invalid
                Level = 0,
                Visited = false
            };
            var node = new SNOAObject<GraphData, NodeState>(graphData, properties, state);

            // Act: Update neighbors (no query function - validates current neighbors)
            var updateOp = new UpdateNeighborsRightOperator();
            var result = updateOp.Apply(node);

            // Assert: Neighbors normalized (duplicates removed, invalid removed, sorted)
            result.State.Neighbors.Should().BeEquivalentTo(new[] { 1, 2, 3 });
            result.State.Neighbors.Should().BeInAscendingOrder();
            result.Properties["degree"].Should().Be(3);
            result.Properties["neighbors_updated"].Should().Be(true);
            result.Properties.Should().ContainKey("neighbors_update_time");
            result.Properties["neighbors_added"].Should().Be(0);
            // Removed: invalid -1 is removed (3, 1, 2 remain in result, so only -1 is counted as removed)
            // Note: removedCount counts unique neighbors in original but not in updated
            // Since 3, 1, 2 appear in both original and updated, only -1 is removed
            result.Properties["neighbors_removed"].Should().Be(1); // Only invalid -1 removed

            // Validate A1: Closure
            AxiomValidator.ValidateA1_Closure_Right(updateOp, node).Should().BeTrue();
        }

        /// <summary>
        /// Test UpdateNeighborsRightOperator updates neighbors with query
        /// </summary>
        [Fact]
        public void UpdateNeighborsRightOperator_UpdatesNeighbors_WithQuery()
        {
            // Arrange: Create node with neighbors
            var graphData = new GraphData(1, 10.0);
            var properties = new Dictionary<string, object> { ["degree"] = 2 };
            var state = new NodeState
            {
                Neighbors = new List<int> { 2, 3 },
                Level = 0,
                Visited = false
            };
            var node = new SNOAObject<GraphData, NodeState>(graphData, properties, state);

            // Act: Update neighbors with query function (simulates global graph state)
            var graphStateQuery = new System.Func<int, List<int>>(nodeId =>
            {
                // Simulate graph state: node 1 now has neighbors 4, 5, 6
                return new List<int> { 4, 5, 6 };
            });
            var updateOp = new UpdateNeighborsRightOperator(graphStateQuery);
            var result = updateOp.Apply(node);

            // Assert: Neighbors updated from query
            result.State.Neighbors.Should().BeEquivalentTo(new[] { 4, 5, 6 });
            result.Properties["degree"].Should().Be(3);
            result.Properties["neighbors_added"].Should().Be(3);
            result.Properties["neighbors_removed"].Should().Be(2);
        }

        #endregion

        #region Noncommutativity Tests for New Operators

        /// <summary>
        /// Test noncommutativity: RemoveEdge ∘ NormalizeGraph ≠ NormalizeGraph ∘ RemoveEdge
        /// Demonstrates A5 (Noncommutativity) for new operators
        /// </summary>
        [Fact]
        public void Noncommutativity_RemoveEdgeAndNormalize()
        {
            // Arrange
            var graphData = new GraphData(1, 10.0);
            var properties = new Dictionary<string, object> { ["degree"] = 3 };
            var state = new NodeState
            {
                Neighbors = new List<int> { 2, 3, 2 }, // Duplicate
                Level = 0,
                Visited = false
            };
            var node = new SNOAObject<GraphData, NodeState>(graphData, properties, state);

            var removeOp = new RemoveEdgeLeftOperator(2);
            var normalizeOp = new NormalizeGraphRightOperator();

            // Act: RemoveEdge(NormalizeGraph(X))
            var result1 = removeOp.Apply(normalizeOp.Apply(node));

            // Act: NormalizeGraph(RemoveEdge(X))
            var result2 = normalizeOp.Apply(removeOp.Apply(node));

            // Assert: Results differ (noncommutative)
            result1.Should().NotBeEquivalentTo(result2);

            // Validate A5: Noncommutativity
            AxiomValidator.ValidateA5_Noncommutativity(removeOp, normalizeOp, node).Should().BeTrue();
        }

        /// <summary>
        /// Test noncommutativity: SplitNode ∘ CommitState ≠ CommitState ∘ SplitNode
        /// </summary>
        [Fact]
        public void Noncommutativity_SplitNodeAndCommitState()
        {
            // Arrange
            var graphData = new GraphData(1, 10.0);
            var properties = new Dictionary<string, object> { ["degree"] = 4 };
            var state = new NodeState
            {
                Neighbors = new List<int> { 2, 3, 4, 5 },
                Level = 3,
                Visited = true
            };
            var node = new SNOAObject<GraphData, NodeState>(graphData, properties, state);

            var splitOp = new SplitNodeLeftOperator(10, 0.5);
            var commitOp = new CommitStateRightOperator();

            // Act: SplitNode(CommitState(X))
            var result1 = splitOp.Apply(commitOp.Apply(node));

            // Act: CommitState(SplitNode(X))
            var result2 = commitOp.Apply(splitOp.Apply(node));

            // Assert: Results differ (noncommutative)
            result1.Should().NotBeEquivalentTo(result2);

            // Validate A5: Noncommutativity
            AxiomValidator.ValidateA5_Noncommutativity(splitOp, commitOp, node).Should().BeTrue();
        }

        #endregion
    }
}

