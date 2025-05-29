using AutoInject.Attributes;
using AutoInject.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AutoInject.Extensions
{

    /// <summary>
    /// Extension to configure the Factory with the IServiceProvider
    /// </summary>
    public static class FactoryConfigurationExtension
    {
        /// <summary>
        /// Configures the Factory to use the ASP.NET Core IServiceProvider
        /// and adds the middleware to manage scopes per request
        /// </summary>
        /// <param name="app">The WebApplication or IApplicationBuilder</param>
        /// <returns>The same app for chaining</returns>
        public static IApplicationBuilder UseFactoryDependencyInjection(this IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices;
            Factory.Configure(serviceProvider);

            // Adds middleware to dispose scopes at the end of requests
            app.UseMiddleware<FactoryMiddleware>();

            return app;
        }

        /// <summary>
        /// Registers the services required for the Factory to work correctly
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The same collection for chaining</returns>
        public static IServiceCollection AddFactoryServices(this IServiceCollection services)
        {
            // For now, no additional registration is needed
            // The Factory uses Thread ID to identify requests

            return services;
        }

        /// <summary>
        /// Automatically registers all interfaces from a specific namespace
        /// and their corresponding implementations in the DI container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="namespaceName">The base namespace to look for interfaces (e.g: "ISManager.Application.UseCases")</param>
        /// <param name="assemblies">The assemblies to look for interfaces and implementations. If not provided, uses the calling assembly</param>
        public static void AddScopedInjection(this IServiceCollection services, string namespaceName, params Assembly[] assemblies)
        {
            // If no assembly is provided, use the calling assembly
            if (assemblies == null || assemblies.Length == 0)
            {
                assemblies = new[] { Assembly.GetCallingAssembly() };
            }

            // Search for all interfaces in the specified namespace
            var interfacesInNamespace = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsInterface &&
                              !string.IsNullOrEmpty(type.Namespace) &&
                              type.Namespace.StartsWith(namespaceName))
                .ToList();

            foreach (var interfaceType in interfacesInNamespace)
            {
                // Look for implementations of the interface in all assemblies
                var implementationType = assemblies
                    .SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(type => type.IsClass &&
                                          !type.IsAbstract &&
                                          interfaceType.IsAssignableFrom(type));

                if (implementationType != null)
                {
                    // Check if the implementation has the [AutoInject] attribute
                    var hasAutoInjectAttribute = implementationType.GetCustomAttribute<AutoInjectAttribute>() != null;

                    // If it doesn't have [AutoInject], register normally
                    // If it has [AutoInject], it will be registered by AddAutoInjectClasses
                    if (!hasAutoInjectAttribute)
                    {
                        services.AddScoped(interfaceType, implementationType);
                    }
                }
            }
        }

    }
}
