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
        /// Automatically registers all classes that inherit from InjectBase and their Injectable dependencies
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="assembly">The assembly to scan</param>
        /// <param name="serviceLifetime">The service lifetime (default: Scoped)</param>
        /// <returns>The same collection for chaining</returns>
        [Obsolete("This method is obsolete. Use AddAutoInject() instead for Smart Auto-Registration that automatically discovers and registers dependencies on-demand. This provides better performance and eliminates the need for manual registration.", false)]
        public static IServiceCollection AddInjectBaseClasses(this IServiceCollection services, Assembly assembly, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            // Find all classes that inherit from InjectBase
            var injectBaseTypes = assembly.GetTypes()
                .Where(type => type.IsClass &&
                              !type.IsAbstract &&
                              typeof(InjectBase).IsAssignableFrom(type))
                .ToList();

            // Collect all Injectable properties from InjectBase classes
            var injectableInterfaces = new HashSet<Type>();

            foreach (var type in injectBaseTypes)
            {
                // Get all properties and fields marked with [Injectable]
                var injectableProperties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(prop => prop.GetCustomAttribute<InjectableAttribute>() != null)
                    .Select(prop => prop.PropertyType);

                var injectableFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(field => field.GetCustomAttribute<InjectableAttribute>() != null)
                    .Select(field => field.FieldType);

                foreach (var interfaceType in injectableProperties.Concat(injectableFields))
                {
                    if (interfaceType.IsInterface)
                    {
                        injectableInterfaces.Add(interfaceType);
                    }
                }
            }

            // For each Injectable interface, find the first implementation and register it
            foreach (var interfaceType in injectableInterfaces)
            {
                var implementation = assembly.GetTypes()
                    .FirstOrDefault(type => type.IsClass &&
                                          !type.IsAbstract &&
                                          interfaceType.IsAssignableFrom(type));

                if (implementation != null)
                {
                    // Check if the service is already registered
                    if (!services.Any(s => s.ServiceType == interfaceType))
                    {
                        services.Add(new ServiceDescriptor(interfaceType, implementation, serviceLifetime));
                    }
                }
            }

            // Register the InjectBase classes themselves
            foreach (var type in injectBaseTypes)
            {
                // Register the concrete class
                services.Add(new ServiceDescriptor(type, type, serviceLifetime));

                // Also register the interfaces that the class implements
                var interfaces = type.GetInterfaces()
                    .Where(i => !i.IsGenericTypeDefinition && i != typeof(IDisposable))
                    .ToList();

                foreach (var interfaceType in interfaces)
                {
                    if (!services.Any(s => s.ServiceType == interfaceType))
                    {
                        services.Add(new ServiceDescriptor(interfaceType, provider => provider.GetRequiredService(type), serviceLifetime));
                    }
                }
            }

            return services;
        }

        /// <summary>
        /// Automatically registers all classes that inherit from InjectBase and their Injectable dependencies from multiple assemblies
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="assemblies">The assemblies to scan</param>
        /// <param name="serviceLifetime">The service lifetime (default: Scoped)</param>
        /// <returns>The same collection for chaining</returns>
        [Obsolete("This method is obsolete. Use AddAutoInject() instead for Smart Auto-Registration that automatically discovers and registers dependencies on-demand. This provides better performance and eliminates the need for manual registration.", false)]
        public static IServiceCollection AddInjectBaseClasses(this IServiceCollection services, IEnumerable<Assembly> assemblies, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            foreach (var assembly in assemblies)
            {
                services.AddInjectBaseClasses(assembly, serviceLifetime);
            }
            return services;
        }

        /// <summary>
        /// Automatically registers all classes that inherit from InjectBase and their Injectable dependencies from the calling assembly
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="serviceLifetime">The service lifetime (default: Scoped)</param>
        /// <returns>The same collection for chaining</returns>
        [Obsolete("This method is obsolete. Use AddAutoInject() instead for Smart Auto-Registration that automatically discovers and registers dependencies on-demand. This provides better performance and eliminates the need for manual registration.", false)]
        public static IServiceCollection AddInjectBaseClasses(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            return services.AddInjectBaseClasses(callingAssembly, serviceLifetime);
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
