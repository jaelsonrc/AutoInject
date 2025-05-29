using AutoInject.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AutoInject.Extensions
{
    /// <summary>
    /// Extensions to automatically register classes marked with [AutoInject]
    /// </summary>
    public static class AutoInjectServiceCollectionExtensions
    {
        /// <summary>
        /// Automatically registers all classes marked with [AutoInject] from an assembly
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="assembly">The assembly to scan</param>
        /// <param name="serviceLifetime">The service lifetime (default: Scoped)</param>
        /// <returns>The same collection for chaining</returns>
        public static IServiceCollection AddAutoInjectClasses(this IServiceCollection services, Assembly assembly, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var autoInjectTypes = assembly.GetTypes()
                .Where(type => type.IsClass &&
                              !type.IsAbstract &&
                              type.GetCustomAttribute<AutoInjectAttribute>() != null)
                .ToList();

            foreach (var type in autoInjectTypes)
            {
                // Register the concrete class
                services.Add(new ServiceDescriptor(type, CreateAutoInjectFactory(type), serviceLifetime));

                // Also register the interfaces that the class implements
                var interfaces = type.GetInterfaces()
                    .Where(i => !i.IsGenericTypeDefinition && i != typeof(IDisposable))
                    .ToList();

                foreach (var interfaceType in interfaces)
                {
                    services.Add(new ServiceDescriptor(interfaceType, provider => provider.GetRequiredService(type), serviceLifetime));
                }
            }

            return services;
        }

        /// <summary>
        /// Automatically registers all classes marked with [AutoInject] from multiple assemblies
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="assemblies">The assemblies to scan</param>
        /// <param name="serviceLifetime">The service lifetime (default: Scoped)</param>
        /// <returns>The same collection for chaining</returns>
        public static IServiceCollection AddAutoInjectClasses(this IServiceCollection services, IEnumerable<Assembly> assemblies, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            foreach (var assembly in assemblies)
            {
                services.AddAutoInjectClasses(assembly, serviceLifetime);
            }
            return services;
        }

        /// <summary>
        /// Automatically registers all classes marked with [AutoInject] from the calling assembly
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="serviceLifetime">The service lifetime (default: Scoped)</param>
        /// <returns>The same collection for chaining</returns>
        public static IServiceCollection AddAutoInjectClasses(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            return services.AddAutoInjectClasses(callingAssembly, serviceLifetime);
        }

        /// <summary>
        /// Creates a factory that automatically processes dependency injection
        /// </summary>
        private static Func<IServiceProvider, object> CreateAutoInjectFactory(Type type)
        {
            return serviceProvider =>
            {
                // Create the instance using the default constructor
                var instance = Activator.CreateInstance(type);

                if (instance == null)
                    throw new InvalidOperationException($"Could not create an instance of type {type.Name}");

                // Configure the Factory if not already configured
                if (!Factory.IsConfigured)
                {
                    Factory.Configure(serviceProvider);
                }

                // Process auto-injection
                AutoInjectInterceptor.ProcessIfMarked(instance);

                return instance;
            };
        }
    }
}
