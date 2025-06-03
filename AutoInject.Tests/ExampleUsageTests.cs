using AutoInject.Attributes;
using AutoInject.Tests.TestClasses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AutoInject.Tests
{
    /// <summary>
    /// Practical examples showing how the smart auto-registration works
    /// </summary>
    public class ExampleUsageTests
    {
        [Fact]
        public void Example_OrderService_With_Smart_AutoRegistration()
        {
            // Arrange - Simulate a real application startup
            var services = new ServiceCollection();
            
            // Only register the basic services (logging, etc.)
          //  services.AddLogging();
            
            // Build the service provider WITHOUT manually registering repositories
            var serviceProvider = services.BuildServiceProvider();
            Factory.Configure(serviceProvider);

            // Act - Create a service that needs dependencies
            // The Factory will automatically find and register:
            // - ITestRepository -> TestRepository
            // - ITestEmailService -> TestEmailService
            var orderService = new OrderService();

            // Assert
            Assert.NotNull(orderService.Repository);
            Assert.NotNull(orderService.EmailService);
            Assert.NotNull(orderService.Logger);
        }

        [Fact]
        public void Example_UserService_With_Mixed_Registration()
        {
            // Arrange - Some services are pre-registered, others are auto-discovered
            var services = new ServiceCollection();
            //services.AddLogging();
            
            // Pre-register only one service
            services.AddScoped<ITestEmailService, TestEmailService>();
            
            var serviceProvider = services.BuildServiceProvider();
            Factory.Configure(serviceProvider);

            // Act - Create a service
            var userService = new UserService();

            // Assert
            Assert.NotNull(userService.Repository); // Auto-registered
            Assert.NotNull(userService.EmailService); // Pre-registered
            Assert.NotNull(userService.Logger);
            
            // Verify the pre-registered service is used
            Assert.IsType<TestEmailService>(userService.EmailService);
        }

        [Fact]
        public void Example_PaymentService_With_No_Manual_Registration()
        {
            // Arrange - Zero manual registration except logging
            var services = new ServiceCollection();
           // services.AddLogging();
            
            var serviceProvider = services.BuildServiceProvider();
            Factory.Configure(serviceProvider);

            // Act - Create multiple services that all need different dependencies
            var paymentService = new PaymentService();
            var notificationService = new NotificationService();

            // Assert - All dependencies should be auto-resolved
            Assert.NotNull(paymentService.Repository);
            Assert.NotNull(paymentService.EmailService);
            Assert.NotNull(paymentService.Logger);
            
            Assert.NotNull(notificationService.NotificationServiceField);
            Assert.NotNull(notificationService.Logger);
        }
    }

    // Example services that demonstrate the smart auto-registration
    public class OrderService : InjectBase
    {
        [Injectable] private readonly ITestRepository? _repository;
        [Injectable] private readonly ITestEmailService? _emailService;

        public ITestRepository? Repository => _repository;
        public ITestEmailService? EmailService => _emailService;
        public ILogger Logger => _logger;

        public async Task<bool> ProcessOrderAsync(int orderId)
        {
            _logger.LogInformation("Processing order {OrderId}", orderId);
            
            var data = await _repository!.GetDataAsync();
           await  _emailService!.SendEmailAsync("Order processed"+ $"Order {orderId} has been processed. Data: {data}");
            
            return true;
        }
    }

    public class UserService : InjectBase
    {
        [Injectable] private readonly ITestRepository? _repository;
        [Injectable] private readonly ITestEmailService? _emailService;

        public ITestRepository? Repository => _repository;
        public ITestEmailService? EmailService => _emailService;
        public ILogger Logger => _logger;

        public async Task<string> CreateUserAsync(string username)
        {
            _logger.LogInformation("Creating user {Username}", username);
            
            var userData = await _repository!.GetDataAsync();
           await _emailService!.SendEmailAsync("Welcome" + $"Welcome {username}!");
            
            return $"User {username} created successfully";
        }
    }

    public class PaymentService : InjectBase
    {
        [Injectable] private readonly ITestRepository? _repository;
        [Injectable] private readonly ITestEmailService? _emailService;

        public ITestRepository? Repository => _repository;
        public ITestEmailService? EmailService => _emailService;
        public ILogger Logger => _logger;

        public async Task<bool> ProcessPaymentAsync(decimal amount)
        {
            _logger.LogInformation("Processing payment of {Amount:C}", amount);
            
            var paymentData = await _repository!.GetDataAsync();
           await _emailService!.SendEmailAsync("Payment Confirmation"+ $"Payment of {amount:C} processed");
            
            return true;
        }
    }

    public class NotificationService : InjectBase
    {
        [Injectable] private readonly ITestNotificationService? _notificationServiceField;

        public ITestNotificationService? NotificationServiceField => _notificationServiceField;
        public ILogger Logger => _logger;

        public void SendBulkNotifications(List<string> messages)
        {
            _logger.LogInformation("Sending {Count} notifications", messages.Count);

            foreach (var message in messages)
            {
                _notificationServiceField!.SendNotification(message);
            }
        }
    }
}