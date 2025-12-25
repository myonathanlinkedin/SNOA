using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit.Abstractions;

namespace SNOA.Tests
{
    /// <summary>
    /// Test Result Logger - Logs test results to JSON file in result folder
    /// Results can be used to update MD documentation files
    /// </summary>
    public class TestResultLogger
    {
        private readonly ITestOutputHelper _output;
        private readonly List<TestResult> _results = new List<TestResult>();

        public TestResultLogger(ITestOutputHelper output)
        {
            _output = output;
        }

        public void LogTest(string testName, bool passed, string? message = null, Dictionary<string, object>? data = null)
        {
            var result = new TestResult
            {
                TestName = testName,
                Passed = passed,
                Timestamp = DateTime.UtcNow,
                Message = message,
                Data = data ?? new Dictionary<string, object>()
            };

            _results.Add(result);
            _output.WriteLine($"[{result.Timestamp:yyyy-MM-dd HH:mm:ss}] {testName}: {(passed ? "PASS" : "FAIL")} - {message ?? ""}");
        }

        public void SaveResults(string fileName = "test_results.json")
        {
            var resultDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "result");
            Directory.CreateDirectory(resultDir);

            var filePath = Path.Combine(resultDir, fileName);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var summary = new TestSummary
            {
                Timestamp = DateTime.UtcNow,
                TotalTests = _results.Count,
                PassedTests = _results.Count(r => r.Passed),
                FailedTests = _results.Count(r => !r.Passed),
                Results = _results
            };

            var json = JsonSerializer.Serialize(summary, options);
            File.WriteAllText(filePath, json);

            _output.WriteLine($"Results saved to: {filePath}");
            _output.WriteLine($"Total: {summary.TotalTests}, Passed: {summary.PassedTests}, Failed: {summary.FailedTests}");
        }

        private class TestResult
        {
            public string TestName { get; set; } = string.Empty;
            public bool Passed { get; set; }
            public DateTime Timestamp { get; set; }
            public string? Message { get; set; }
            public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        }

        private class TestSummary
        {
            public DateTime Timestamp { get; set; }
            public int TotalTests { get; set; }
            public int PassedTests { get; set; }
            public int FailedTests { get; set; }
            public List<TestResult> Results { get; set; } = new List<TestResult>();
        }
    }
}




