using NUnit.Framework;

[SetUpFixture]
public class GlobalSetup
{
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        // Global test setup
        Console.WriteLine("Starting Booking Application Test Suite");
        
        // Setup test environment variables if needed
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
    }

    [OneTimeTearDown]
    public void RunAfterAllTests()
    {
        // Global test cleanup
        Console.WriteLine("Completed Booking Application Test Suite");
    }
}