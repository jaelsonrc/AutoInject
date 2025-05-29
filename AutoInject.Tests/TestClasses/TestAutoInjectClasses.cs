using AutoInject.Attributes;
using Microsoft.Extensions.Logging;

namespace AutoInject.Tests.TestClasses
{
    [AutoInject]
    public class TestServiceWithAutoInject
    {
        [Injectable] private readonly ITestRepository? _repository;
        [Injectable] private readonly ITestEmailService? _emailService;
        [Injectable] private readonly ILogger<TestServiceWithAutoInject>? _logger;

        public ITestRepository? Repository => _repository;
        public ITestEmailService? EmailService => _emailService;
        public ILogger<TestServiceWithAutoInject>? Logger => _logger;

        public async Task<string> ProcessDataAsync()
        {
            _logger?.LogInformation("Processing data...");
            return await _repository!.GetDataAsync();
        }
    }

    [AutoInject]
    public class TestServiceWithFields
    {
        [Injectable] private ITestNotificationService? _notificationService;
        [Injectable] private ILogger<TestServiceWithFields>? _logger;

        public ITestNotificationService? NotificationService => _notificationService;
        public ILogger<TestServiceWithFields>? Logger => _logger;

        public void SendNotification(string message)
        {
            _logger?.LogInformation("Sending notification: {Message}", message);
            _notificationService?.SendNotification(message);
        }
    }

    public class TestServiceWithInjectBase : InjectBase
    {
        [Injectable] private readonly ITestRepository? _repository;
        [Injectable] private readonly ITestEmailService? _emailService;

        public ITestRepository? Repository => _repository;
        public ITestEmailService? EmailService => _emailService;
        public ILogger Logger => _logger;

        public async Task<string> ProcessDataAsync()
        {
            _logger?.LogInformation("Processing data with InjectBase...");
            return await _repository!.GetDataAsync();
        }
    }

    public class TestServiceWithoutAutoInject
    {
        [Injectable] private readonly ITestRepository? _repository;
        [Injectable] private readonly ITestEmailService? _emailService;

        public ITestRepository? Repository => _repository;
        public ITestEmailService? EmailService => _emailService;
    }

    [AutoInject]
    public class TestServiceWithNonInjectableProperties
    {
        [Injectable] private readonly ITestRepository? _repository;
        private readonly ITestEmailService? _emailService; // Not marked with [Injectable]

        public ITestRepository? Repository => _repository;
        public ITestEmailService? EmailService => _emailService;
    }
} 