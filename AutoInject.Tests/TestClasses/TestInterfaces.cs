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

    // New interfaces for InjectBase testing
    public interface IUsuarioLogado
    {
        string GetUsuarioId();
        string GetUsuarioNome();
    }

    public interface ITestService
    {
        void DoSomething();
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

    // New implementations for InjectBase testing
    public class UsuarioLogadoImplementation : IUsuarioLogado
    {
        public string GetUsuarioId()
        {
            return "123";
        }

        public string GetUsuarioNome()
        {
            return "Test User";
        }
    }

    public class TestServiceImplementation : ITestService
    {
        public void DoSomething()
        {
            // Test implementation
        }
    }
} 