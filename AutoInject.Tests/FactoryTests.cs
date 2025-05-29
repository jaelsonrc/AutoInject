using AutoInject.Tests.TestClasses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AutoInject.Tests
{
    public class FactoryTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public FactoryTests()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddScoped<ITestRepository, TestRepository>();
            services.AddScoped<ITestEmailService, TestEmailService>();
            services.AddScoped<ITestNotificationService, TestNotificationService>();

            _serviceProvider = services.BuildServiceProvider();
            Factory.Configure(_serviceProvider);
        }

        [Fact]
        public void Configure_ShouldSetIsConfiguredToTrue()
        {
            // Arrange & Act
            Factory.Configure(_serviceProvider);

            // Assert
            Assert.True(Factory.IsConfigured);
        }

        [Fact]
        public void InjectDependencies_WithValidInstance_ShouldInjectProperties()
        {
            // Arrange
            var testService = new TestServiceWithAutoInject();

            // Act
            Factory.InjectDependencies(testService);

            // Assert
            Assert.NotNull(testService.Repository);
            Assert.NotNull(testService.EmailService);
            Assert.NotNull(testService.Logger);
        }

        [Fact]
        public void InjectDependencies_WithValidInstance_ShouldInjectFields()
        {
            // Arrange
            var testService = new TestServiceWithFields();

            // Act
            Factory.InjectDependencies(testService);

            // Assert
            Assert.NotNull(testService.NotificationService);
            Assert.NotNull(testService.Logger);
        }

        [Fact]
        public void InjectDependencies_WithNullInstance_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => Factory.InjectDependencies(null!));
        }

        [Fact]
        public void InjectDependencies_WithoutConfiguration_ShouldThrowException()
        {
            // Arrange
            Factory.Configure(null!);
            var testService = new TestServiceWithAutoInject();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => Factory.InjectDependencies(testService));
        }

        [Fact]
        public void InjectDependencies_WithNonInjectableProperties_ShouldNotInjectThem()
        {
            // Arrange
            var testService = new TestServiceWithNonInjectableProperties();

            // Act
            Factory.InjectDependencies(testService);

            // Assert
            Assert.NotNull(testService.Repository); // Has [Injectable]
            Assert.Null(testService.EmailService);  // Doesn't have [Injectable]
        }

        [Fact]
        public void GetLogger_Generic_ShouldReturnLogger()
        {
            // Act
            var logger = Factory.GetLogger<TestServiceWithAutoInject>();

            // Assert
            Assert.NotNull(logger);
            Assert.IsType<Logger<TestServiceWithAutoInject>>(logger);
        }

        [Fact]
        public void GetLogger_WithType_ShouldReturnLogger()
        {
            // Act
            var logger = Factory.GetLogger(typeof(TestServiceWithAutoInject));

            // Assert
            Assert.NotNull(logger);
        }

        [Fact]
        public void GetLogger_WithoutConfiguration_ShouldThrowException()
        {
            // Arrange
            Factory.Configure(null!);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => Factory.GetLogger<TestServiceWithAutoInject>());
        }

        [Fact]
        public void DisposeRequestScope_ShouldNotThrowException()
        {
            // Act & Assert (should not throw)
            Factory.DisposeRequestScope();
        }

        [Fact]
        public void DisposeAllScopes_ShouldNotThrowException()
        {
            // Act & Assert (should not throw)
            Factory.DisposeAllScopes();
        }

        public void Dispose()
        {
            Factory.DisposeAllScopes();
            _serviceProvider?.Dispose();
        }
    }
} 