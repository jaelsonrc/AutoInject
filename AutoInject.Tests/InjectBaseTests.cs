using AutoInject.Tests.TestClasses;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AutoInject.Tests
{
    public class InjectBaseTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public InjectBaseTests()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddScoped<ITestRepository, TestRepository>();
            services.AddScoped<ITestEmailService, TestEmailService>();

            _serviceProvider = services.BuildServiceProvider();
            Factory.Configure(_serviceProvider);
        }

        [Fact]
        public void Constructor_ShouldInjectDependenciesAndLogger()
        {
            // Act
            var testService = new TestServiceWithInjectBase();

            // Assert
            Assert.NotNull(testService.Repository);
            Assert.NotNull(testService.EmailService);
            Assert.NotNull(testService.Logger);
        }

        [Fact]
        public async Task ProcessDataAsync_ShouldUseInjectedDependencies()
        {
            // Arrange
            var testService = new TestServiceWithInjectBase();

            // Act
            var result = await testService.ProcessDataAsync();

            // Assert
            Assert.Equal("Test Data", result);
            Assert.NotNull(testService.Repository);
            Assert.NotNull(testService.EmailService);
            Assert.NotNull(testService.Logger);
        }

        [Fact]
        public void Logger_ShouldBeOfCorrectType()
        {
            // Act
            var testService = new TestServiceWithInjectBase();

            // Assert
            Assert.NotNull(testService.Logger);
            // Logger should be ILogger (non-generic)
            Assert.IsAssignableFrom<Microsoft.Extensions.Logging.ILogger>(testService.Logger);
        }

        public void Dispose()
        {
            Factory.DisposeAllScopes();
            _serviceProvider?.Dispose();
        }
    }
} 