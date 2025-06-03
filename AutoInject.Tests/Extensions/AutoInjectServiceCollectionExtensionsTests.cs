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
         //   _services.AddLogging();
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

        [Fact]
        public void AddInjectBaseClasses_WithAssembly_ShouldRegisterInjectBaseClassesAndDependencies()
        {
            // Arrange
            var assembly = Assembly.GetAssembly(typeof(TestRepositoryBase))!;

            // Act
            _services.AddInjectBaseClasses(assembly);
            _serviceProvider = _services.BuildServiceProvider();

            // Assert
            // Verify that InjectBase classes are registered
            var testRepository = _serviceProvider.GetService<TestRepositoryBase>();
            Assert.NotNull(testRepository);

            // Verify that Injectable dependencies are automatically registered
            var usuarioLogado = _serviceProvider.GetService<IUsuarioLogado>();
            Assert.NotNull(usuarioLogado);

            // Verify that the Injectable property is properly injected
            Assert.NotNull(testRepository.UsuarioLogado);
        }

        [Fact]
        public void AddInjectBaseClasses_WithCallingAssembly_ShouldRegisterInjectBaseClasses()
        {
            // Act
            _services.AddInjectBaseClasses();
            _serviceProvider = _services.BuildServiceProvider();

            // Assert
            var testRepository = _serviceProvider.GetService<TestRepositoryBase>();
            Assert.NotNull(testRepository);
        }

        [Fact]
        public void AddInjectBaseClasses_ShouldNotRegisterDuplicateServices()
        {
            // Arrange
            var assembly = Assembly.GetAssembly(typeof(TestRepositoryBase))!;
            
            // Pre-register a service
            _services.AddScoped<IUsuarioLogado, UsuarioLogadoImplementation>();

            // Act
            _services.AddInjectBaseClasses(assembly);
            _serviceProvider = _services.BuildServiceProvider();

            // Assert
            var usuarioLogadoServices = _services.Where(s => s.ServiceType == typeof(IUsuarioLogado)).ToList();
            Assert.Single(usuarioLogadoServices); // Should only have one registration
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }
} 