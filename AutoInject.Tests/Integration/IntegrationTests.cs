using AutoInject.Extensions;
using AutoInject.Tests.TestClasses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Xunit;

namespace AutoInject.Tests.Integration
{
    public class IntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public IntegrationTests()
        {
            var services = new ServiceCollection();
            
            // Setup logging
          //  services.AddLogging();
            
            // Register dependencies
            services.AddScoped<ITestRepository, TestRepository>();
            services.AddScoped<ITestEmailService, TestEmailService>();
            services.AddScoped<ITestNotificationService, TestNotificationService>();
            
            // Add AutoInject classes
            services.AddAutoInjectClasses(Assembly.GetExecutingAssembly());
            
            _serviceProvider = services.BuildServiceProvider();
            
            // Configure Factory
            Factory.Configure(_serviceProvider);
        }

        [Fact]
        public async Task FullWorkflow_WithAutoInjectService_ShouldWorkCorrectly()
        {
            // Arrange
            var testService = _serviceProvider.GetService<TestServiceWithAutoInject>();

            // Act
            var result = await testService!.ProcessDataAsync();

            // Assert
            Assert.NotNull(testService);
            Assert.NotNull(testService.Repository);
            Assert.NotNull(testService.EmailService);
            Assert.NotNull(testService.Logger);
            Assert.Equal("Test Data", result);
        }

        [Fact]
        public void FullWorkflow_WithFieldsService_ShouldWorkCorrectly()
        {
            // Arrange
            var testService = _serviceProvider.GetService<TestServiceWithFields>();

            // Act
            testService!.SendNotification("Test message");

            // Assert
            Assert.NotNull(testService);
            Assert.NotNull(testService.NotificationService);
            Assert.NotNull(testService.Logger);
        }

        [Fact]
        public async Task FullWorkflow_WithInjectBase_ShouldWorkCorrectly()
        {
            // Arrange
            var testService = new TestServiceWithInjectBase();

            // Act
            var result = await testService.ProcessDataAsync();

            // Assert
            Assert.NotNull(testService.Repository);
            Assert.NotNull(testService.EmailService);
            Assert.NotNull(testService.Logger);
            Assert.Equal("Test Data", result);
        }

        [Fact]
        public void MultipleInstances_ShouldAllHaveInjectedDependencies()
        {
            // Act
            var service1 = _serviceProvider.GetService<TestServiceWithAutoInject>();
            var service2 = _serviceProvider.GetService<TestServiceWithAutoInject>();
            var service3 = new TestServiceWithInjectBase();

            // Assert
            Assert.NotNull(service1?.Repository);
            Assert.NotNull(service1?.EmailService);
            Assert.NotNull(service1?.Logger);

            Assert.NotNull(service2?.Repository);
            Assert.NotNull(service2?.EmailService);
            Assert.NotNull(service2?.Logger);

            Assert.NotNull(service3.Repository);
            Assert.NotNull(service3.EmailService);
            Assert.NotNull(service3.Logger);
        }

        [Fact]
        public void ScopedServices_ShouldBehaveProperly()
        {
            // Arrange & Act
            using var scope1 = _serviceProvider.CreateScope();
            using var scope2 = _serviceProvider.CreateScope();

            var service1a = scope1.ServiceProvider.GetService<TestServiceWithAutoInject>();
            var service1b = scope1.ServiceProvider.GetService<TestServiceWithAutoInject>();
            var service2 = scope2.ServiceProvider.GetService<TestServiceWithAutoInject>();

            // Assert
            Assert.NotNull(service1a);
            Assert.NotNull(service1b);
            Assert.NotNull(service2);
            
            // Same instance within scope
            Assert.Same(service1a, service1b);
            
            // Different instances across scopes
            Assert.NotSame(service1a, service2);
        }

        [Fact]
        public void LoggerInjection_ShouldWorkForDifferentTypes()
        {
            // Act
            var service1 = _serviceProvider.GetService<TestServiceWithAutoInject>();
            var service2 = _serviceProvider.GetService<TestServiceWithFields>();

            // Assert
            Assert.NotNull(service1?.Logger);
            Assert.NotNull(service2?.Logger);
            
            // Loggers should be of correct generic types
            Assert.IsType<Logger<TestServiceWithAutoInject>>(service1.Logger);
            Assert.IsType<Logger<TestServiceWithFields>>(service2.Logger);
        }

        public void Dispose()
        {
            Factory.DisposeAllScopes();
            _serviceProvider?.Dispose();
        }
    }
} 