namespace AutoInject.Tests.TestClasses
{
    // Test interfaces
    public interface ITestRepository
    {
        Task<string> GetDataAsync();
    }

    public interface ITestEmailService
    {
        Task SendEmailAsync(string email);
    }

    public interface ITestNotificationService
    {
        void SendNotification(string message);
    }

    // Test implementations
    public class TestRepository : ITestRepository
    {
        public Task<string> GetDataAsync()
        {
            return Task.FromResult("Test Data");
        }
    }

    public class TestEmailService : ITestEmailService
    {
        public Task SendEmailAsync(string email)
        {
            return Task.CompletedTask;
        }
    }

    public class TestNotificationService : ITestNotificationService
    {
        public void SendNotification(string message)
        {
            // Test implementation
        }
    }
} 