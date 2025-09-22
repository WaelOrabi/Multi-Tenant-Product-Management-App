using Xunit;

// Disable xUnit test parallelization to prevent cross-test data interference in shared MongoDB database
[assembly: CollectionBehavior(DisableTestParallelization = true)]
