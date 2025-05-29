using AutoInject.Extensions;
using AutoInject.Tests.TestClasses;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Xunit;

namespace AutoInject.Tests.Extensions
{
    public class AutoInjectServiceCollectionExtensionsTests : IDisposable
    {
        private readonly ServiceCollection _services;
        private ServiceProvider? _serviceProvider;

        public AutoInjectServiceCollectionExtensionsTests()
        {
            _services = new ServiceCollection();
            _services.AddLogging();
            _services.AddScoped<ITestRepository, TestRepository>();
            _services.AddScoped<ITestEmailService, TestEmailService>();
            _services.AddScoped<ITestNotificationService, TestNotificationService>();
        }

        [Fact]
        public void AddAutoInjectClasses_WithAssembly_ShouldRegisterAutoInjectClasses()
        {
            // Arrange
            var assembly = Assembly.GetAssembly(typeof(TestServiceWithAutoInject))!;

            // Act
            _services.AddAutoInjectClasses(assembly);
            _serviceProvider = _services.BuildServiceProvider();

            // Assert
            var testService = _serviceProvider.GetService<TestServiceWithAutoInject>();
            Assert.NotNull(testService);
            Assert.NotNull(testService.Repository);
            Assert.NotNull(testService.EmailService);
        }

        [Fact]
        public void AddAutoInjectClasses_WithCallingAssembly_ShouldRegisterAutoInjectClasses()
        {
            // Act
            _services.AddAutoInjectClasses();
            _serviceProvider = _services.BuildServiceProvider();

            // Assert
            var testService = _serviceProvider.GetService<TestServiceWithAutoInject>();
            Assert.NotNull(testService);
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }
} 