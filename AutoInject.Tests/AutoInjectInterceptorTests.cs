using AutoInject.Tests.TestClasses;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AutoInject.Tests
{
    public class AutoInjectInterceptorTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public AutoInjectInterceptorTests()
        {
            var services = new ServiceCollection();
          //  services.AddLogging();
            services.AddScoped<ITestRepository, TestRepository>();
            services.AddScoped<ITestEmailService, TestEmailService>();
            services.AddScoped<ITestNotificationService, TestNotificationService>();

            _serviceProvider = services.BuildServiceProvider();
            Factory.Configure(_serviceProvider);
        }

        [Fact]
        public void ProcessIfMarked_WithAutoInjectAttribute_ShouldReturnTrueAndInjectDependencies()
        {
            // Arrange
            var testService = new TestServiceWithAutoInject();

            // Act
            var result = AutoInjectInterceptor.ProcessIfMarked(testService);

            // Assert
            Assert.True(result);
            Assert.NotNull(testService.Repository);
            Assert.NotNull(testService.EmailService);
            Assert.NotNull(testService.Logger);
        }

        [Fact]
        public void ProcessIfMarked_WithoutAutoInjectAttribute_ShouldReturnFalseAndNotInject()
        {
            // Arrange
            var testService = new TestServiceWithoutAutoInject();

            // Act
            var result = AutoInjectInterceptor.ProcessIfMarked(testService);

            // Assert
            Assert.False(result);
            Assert.Null(testService.Repository);
            Assert.Null(testService.EmailService);
        }

        [Fact]
        public void ProcessIfMarked_WithNullInstance_ShouldReturnFalse()
        {
            // Act
            var result = AutoInjectInterceptor.ProcessIfMarked(null!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ProcessIfMarked_WithFieldsAutoInject_ShouldInjectFields()
        {
            // Arrange
            var testService = new TestServiceWithFields();

            // Act
            var result = AutoInjectInterceptor.ProcessIfMarked(testService);

            // Assert
            Assert.True(result);
            Assert.NotNull(testService.NotificationService);
            Assert.NotNull(testService.Logger);
        }

        [Fact]
        public void ProcessIfMarked_ShouldCacheTypeInformation()
        {
            // Arrange - Ensure Factory is configured
            if (!Factory.IsConfigured)
            {
                Factory.Configure(_serviceProvider);
            }
            
            var testService1 = new TestServiceWithAutoInject();
            var testService2 = new TestServiceWithAutoInject();

            // Act
            var result1 = AutoInjectInterceptor.ProcessIfMarked(testService1);
            var result2 = AutoInjectInterceptor.ProcessIfMarked(testService2);

            // Assert
            Assert.True(result1);
            Assert.True(result2);
            // Both instances should be processed successfully (testing cache)
            Assert.NotNull(testService1.Repository);
            Assert.NotNull(testService2.Repository);
        }

        public void Dispose()
        {
            Factory.DisposeAllScopes();
            _serviceProvider?.Dispose();
        }
    }
} 