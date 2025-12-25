using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit.Abstractions;

namespace SNOA.Tests
{
    /// <summary>
    /// Axiom Validation Results Logger
    /// Logs detailed axiom validation results for MD documentation updates
    /// Uses thread-safe singleton pattern to share results across all tests
    /// </summary>
    public class AxiomValidationResults
    {
        // Shared static instance for all tests in the collection
        private static readonly object _lock = new object();
        private static AxiomValidationResults? _sharedInstance;
        private static readonly Dictionary<string, AxiomResult> _sharedResults = new Dictionary<string, AxiomResult>();
        
        private readonly ITestOutputHelper? _output;

        public AxiomValidationResults(ITestOutputHelper? output = null)
        {
            _output = output;
        }

        /// <summary>
        /// Get or create shared instance for all tests
        /// </summary>
        public static AxiomValidationResults GetSharedInstance(ITestOutputHelper? output = null)
        {
            if (_sharedInstance == null)
            {
                lock (_lock)
                {
                    if (_sharedInstance == null)
                    {
                        _sharedInstance = new AxiomValidationResults(output);
                    }
                }
            }
            return _sharedInstance;
        }

        /// <summary>
        /// Get shared results dictionary (thread-safe)
        /// </summary>
        private Dictionary<string, AxiomResult> GetResults()
        {
            return _sharedResults;
        }

        public void RecordAxiom(string axiomName, bool validated, string? details = null, Dictionary<string, object>? metrics = null)
        {
            var results = GetResults();
            
            lock (_lock)
            {
                var result = new AxiomResult
                {
                    AxiomName = axiomName,
                    Validated = validated,
                    Timestamp = DateTime.UtcNow,
                    Details = details,
                    Metrics = metrics ?? new Dictionary<string, object>()
                };

                results[axiomName] = result;
            }

            _output?.WriteLine($"Axiom {axiomName}: {(validated ? "VALIDATED" : "FAILED")} - {details ?? ""}");
        }

        public void SaveToFile(string fileName = "axiom_validation_results.json")
        {
            var resultDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "result");
            Directory.CreateDirectory(resultDir);

            var filePath = Path.Combine(resultDir, fileName);

            var results = GetResults();
            Dictionary<string, AxiomResult> resultsCopy;
            
            lock (_lock)
            {
                // Create a copy to avoid locking during serialization
                resultsCopy = new Dictionary<string, AxiomResult>(results);
            }

            var summary = new AxiomValidationSummary
            {
                Timestamp = DateTime.UtcNow,
                TotalAxioms = resultsCopy.Count,
                ValidatedAxioms = resultsCopy.Values.Count(r => r.Validated),
                FailedAxioms = resultsCopy.Values.Count(r => !r.Validated),
                Axioms = resultsCopy
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(summary, options);
            File.WriteAllText(filePath, json);

            _output?.WriteLine($"Axiom validation results saved to: {filePath}");
            _output?.WriteLine($"Total: {summary.TotalAxioms}, Validated: {summary.ValidatedAxioms}, Failed: {summary.FailedAxioms}");
        }

        private class AxiomResult
        {
            public string AxiomName { get; set; } = string.Empty;
            public bool Validated { get; set; }
            public DateTime Timestamp { get; set; }
            public string? Details { get; set; }
            public Dictionary<string, object> Metrics { get; set; } = new Dictionary<string, object>();
        }

        private class AxiomValidationSummary
        {
            public DateTime Timestamp { get; set; }
            public int TotalAxioms { get; set; }
            public int ValidatedAxioms { get; set; }
            public int FailedAxioms { get; set; }
            public Dictionary<string, AxiomResult> Axioms { get; set; } = new Dictionary<string, AxiomResult>();
        }
    }
}




