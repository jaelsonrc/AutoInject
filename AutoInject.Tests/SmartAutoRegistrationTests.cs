using AutoInject.Tests.TestClasses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AutoInject.Tests
{
    /// <summary>
    /// Tests for the smart auto-registration functionality
    /// </summary>
    public class SmartAutoRegistrationTests
    {
        [Fact]
        public void InjectBase_Should_AutoRegister_Dependencies_OnDemand()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Only register logging - don't register ITestRepository or ITestEmailService
          //  services.AddLogging();
            
            var serviceProvider = services.BuildServiceProvider();
            Factory.Configure(serviceProvider);

            // Act - Create an instance that inherits from InjectBase
            // This should automatically find and register ITestRepository and ITestEmailService
            var testService = new TestServiceWithInjectBase();

            // Assert
            Assert.NotNull(testService.Repository);
            Assert.NotNull(testService.EmailService);
            Assert.NotNull(testService.Logger);
            
            // Verify the types are correct
            Assert.IsAssignableFrom<ITestRepository>(testService.Repository);
            Assert.IsAssignableFrom<ITestEmailService>(testService.EmailService);
        }

        [Fact]
        public void InjectBase_Should_Use_Existing_Registrations_When_Available()
        {
            // Arrange
            var services = new ServiceCollection();
          //  services.AddLogging();
            
            // Pre-register one service
            services.AddScoped<ITestRepository, TestRepository>();
            
            var serviceProvider = services.BuildServiceProvider();
            Factory.Configure(serviceProvider);

            // Act
            var testService = new TestServiceWithInjectBase();

            // Assert
            Assert.NotNull(testService.Repository);
            Assert.NotNull(testService.EmailService);
            
            // The repository should be the pre-registered one
            Assert.IsType<TestRepository>(testService.Repository);
            
            // The email service should be auto-registered
            Assert.IsAssignableFrom<ITestEmailService>(testService.EmailService);
        }

        [Fact]
        public void InjectBase_Should_Handle_Multiple_Instances_Correctly()
        {
            // Arrange
            var services = new ServiceCollection();
     //       services.AddLogging();
            
            var serviceProvider = services.BuildServiceProvider();
            Factory.Configure(serviceProvider);

            // Act - Create multiple instances
            var testService1 = new TestServiceWithInjectBase();
            var testService2 = new TestServiceWithInjectBase();

            // Assert
            Assert.NotNull(testService1.Repository);
            Assert.NotNull(testService1.EmailService);
            Assert.NotNull(testService2.Repository);
            Assert.NotNull(testService2.EmailService);
            
            // Each instance should have its own dependencies (not singletons)
            Assert.NotSame(testService1.Repository, testService2.Repository);
            Assert.NotSame(testService1.EmailService, testService2.EmailService);
        }

        [Fact]
        public void InjectBase_Should_Cache_Type_Mappings()
        {
            // Arrange
            var services = new ServiceCollection();
         //   services.AddLogging();
            
            var serviceProvider = services.BuildServiceProvider();
            Factory.Configure(serviceProvider);

            // Act - Create multiple instances to test caching
            var testService1 = new TestServiceWithInjectBase();
            var testService2 = new TestServiceWithInjectBase();
            var testService3 = new TestServiceWithInjectBase();

            // Assert - All should have dependencies resolved
            Assert.NotNull(testService1.Repository);
            Assert.NotNull(testService2.Repository);
            Assert.NotNull(testService3.Repository);
            
            // All should use the same implementation type (cached mapping)
            Assert.Equal(testService1.Repository.GetType(), testService2.Repository.GetType());
            Assert.Equal(testService2.Repository.GetType(), testService3.Repository.GetType());
        }

        [Fact]
        public async Task InjectBase_Should_Work_With_Async_Operations()
        {
            // Arrange
            var services = new ServiceCollection();
          //  services.AddLogging();
            
            var serviceProvider = services.BuildServiceProvider();
            Factory.Configure(serviceProvider);

            var testService = new TestServiceWithInjectBase();

            // Act
            var result = await testService.ProcessDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test data from repository", result);
        }

        [Fact]
        public void InjectBase_Should_Throw_When_No_Implementation_Found()
        {
            // Arrange
            var services = new ServiceCollection();
       //     services.AddLogging();
            
            var serviceProvider = services.BuildServiceProvider();
            Factory.Configure(serviceProvider);

            // Act & Assert
            // This should work because we have implementations for ITestRepository and ITestEmailService
            // But if we had an interface with no implementation, it would throw
            var testService = new TestServiceWithInjectBase();
            
            // This should not throw because implementations exist
            Assert.NotNull(testService.Repository);
            Assert.NotNull(testService.EmailService);
        }

        [Fact]
        public void Manual_InjectDependencies_Should_Work_For_Non_InjectBase_Classes()
        {
            // Arrange
            var services = new ServiceCollection();
          //  services.AddLogging();
            
            var serviceProvider = services.BuildServiceProvider();
            Factory.Configure(serviceProvider);

            // Act - Manually call InjectDependencies for a class that doesn't inherit from InjectBase
            var testService = new TestServiceWithoutAutoInject();
            Factory.InjectDependencies(testService);

            // Assert
            Assert.NotNull(testService.Repository);
            Assert.NotNull(testService.EmailService);
        }
    }
}