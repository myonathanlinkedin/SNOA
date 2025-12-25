using BenchmarkDotNet.Running;

namespace SNOA.Tests.Benchmarks
{
    /// <summary>
    /// Benchmark Runner for SNOA Operators
    /// Run this to execute all benchmarks
    /// </summary>
    public static class BenchmarkRunnerProgram
    {
        public static void RunBenchmarks(string[] args)
        {
            // Run all benchmarks
            var summary1 = BenchmarkRunner.Run<SNOAOperatorBenchmark>();
            var summary2 = BenchmarkRunner.Run<SNOAScalabilityBenchmark>();
        }
    }
}

