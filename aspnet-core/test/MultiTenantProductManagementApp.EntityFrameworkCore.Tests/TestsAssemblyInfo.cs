using Xunit;

// Disable xUnit test parallelization to prevent cross-test interference during EF Core DB migrations and shared LocalDB usage
[assembly: CollectionBehavior(DisableTestParallelization = true)]
