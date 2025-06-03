using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AutoInject.Extensions
{
    /// <summary>
    /// Extensions for easy configuration of Smart Auto-Registration
    /// </summary>
    public static class SmartAutoInjectExtensions
    {
        /// <summary>
        /// Adds AutoInject with smart auto-registration capabilities
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The same collection for chaining</returns>
        public static IServiceCollection AddAutoInject(this IServiceCollection services)
        {
            // HttpContextAccessor will be registered by the consuming application
            // services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return services;
        }


        /// <summary>
        /// Configures AutoInject Factory with a custom service provider
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureProvider">Optional action to configure the provider before Factory setup</param>
        /// <returns>The configured service provider</returns>
        public static IServiceProvider ConfigureAutoInject(this IServiceCollection services, Action<IServiceProvider>? configureProvider = null)
        {
            var serviceProvider = services.BuildServiceProvider();

            configureProvider?.Invoke(serviceProvider);

            Factory.Configure(serviceProvider);

            return serviceProvider;
        }
    }
}
