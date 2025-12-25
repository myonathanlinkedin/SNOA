using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using SNOA.Core;
using SNOA.Core.CaseStudies;
using SNOA.Core.Examples;

namespace SNOA.Tests.Benchmarks
{
    /// <summary>
    /// Performance Benchmarks for SNOA Operators
    /// Uses BenchmarkDotNet (available since .NET Core 1.0, fully supported in .NET 10)
    /// 
    /// This addresses limitation #2: No Performance Benchmarks
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob] // Uses default runtime (.NET 10)
    public class SNOAOperatorBenchmark
    {
        private SNOAObject<int, int> _testObject = null!;
        private TransformLeftOperator _leftOp = null!;
        private NormalizeRightOperator _rightOp = null!;

        [GlobalSetup]
        public void Setup()
        {
            _testObject = new SNOAObject<int, int>(5, new Dictionary<string, object>(), 2);
            _leftOp = new TransformLeftOperator();
            _rightOp = new NormalizeRightOperator();
        }

        [Benchmark]
        public SNOAObject<int, int> LeftOperator_Transform()
        {
            return _leftOp.Apply(_testObject);
        }

        [Benchmark]
        public SNOAObject<int, int> RightOperator_Normalize()
        {
            return _rightOp.Apply(_testObject);
        }

        [Benchmark]
        public SNOAObject<int, int> Composition_LeftThenRight()
        {
            return _rightOp.Apply(_leftOp.Apply(_testObject));
        }

        [Benchmark]
        public SNOAObject<int, int> Composition_RightThenLeft()
        {
            return _leftOp.Apply(_rightOp.Apply(_testObject));
        }
    }

    /// <summary>
    /// Scalability Benchmarks for Dynamic Graph Case Study
    /// Uses BenchmarkDotNet parameterized benchmarks
    /// 
    /// This addresses limitation #6: No Scalability Analysis
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob] // Uses default runtime (.NET 10)
    public class SNOAScalabilityBenchmark
    {
        [Params(10, 100, 1000, 10000)]
        public int NodeCount { get; set; }

        private List<SNOAObject<GraphData, NodeState>> _nodes = new();
        private AddEdgeLeftOperator _addEdgeOp = null!;
        private NormalizeGraphRightOperator _normalizeOp = null!;

        [GlobalSetup]
        public void Setup()
        {
            _nodes = new List<SNOAObject<GraphData, NodeState>>();
            for (int i = 0; i < NodeCount; i++)
            {
                var graphData = new GraphData(i, i * 1.0);
                var properties = new Dictionary<string, object> { ["degree"] = 0 };
                var state = new NodeState { Neighbors = new List<int>() };
                _nodes.Add(new SNOAObject<GraphData, NodeState>(graphData, properties, state));
            }

            _addEdgeOp = new AddEdgeLeftOperator(999);
            _normalizeOp = new NormalizeGraphRightOperator();
        }

        [Benchmark]
        public void AddEdge_ToAllNodes()
        {
            foreach (var node in _nodes)
            {
                _addEdgeOp.Apply(node);
            }
        }

        [Benchmark]
        public void Normalize_AllNodes()
        {
            foreach (var node in _nodes)
            {
                _normalizeOp.Apply(node);
            }
        }

        [Benchmark]
        public void AddEdgeThenNormalize_AllNodes()
        {
            foreach (var node in _nodes)
            {
                var afterAdd = _addEdgeOp.Apply(node);
                _normalizeOp.Apply(afterAdd);
            }
        }
    }
}

