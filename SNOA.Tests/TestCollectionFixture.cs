using System;
using Xunit;

namespace SNOA.Tests
{
    /// <summary>
    /// Test Collection Fixture to save results after all tests complete
    /// </summary>
    [CollectionDefinition("AxiomTests")]
    public class AxiomTestCollection : ICollectionFixture<TestResultsFixture>
    {
    }

    public class TestResultsFixture : IDisposable
    {
        public TestResultsFixture()
        {
            // Initialize shared logger instance
            AxiomValidationResults.GetSharedInstance();
        }

        public void Dispose()
        {
            // Save results when fixture is disposed (after all tests complete)
            var logger = AxiomValidationResults.GetSharedInstance();
            logger.SaveToFile();
        }
    }
}




