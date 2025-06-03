using AutoInject.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AutoInject
{
    /// <summary>
    /// Factory responsible for performing dependency injection in properties marked with [Injectable]
    /// </summary>
    public static class Factory
    {
        private static IServiceProvider? _provider;
        private static readonly Dictionary<string, IServiceScope> _requestScopes = [];
        private static readonly Dictionary<Type, Type> _autoRegisteredTypes = [];
        private static readonly object _lock = new();

        /// <summary>
        /// Indicates if the Factory has been configured
        /// </summary>
        public static bool IsConfigured => _provider != null;

        /// <summary>
        /// Configures the service provider that will be used to resolve dependencies
        /// </summary>
        /// <param name="provider">The ASP.NET Core IServiceProvider</param>
        public static void Configure(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Injects dependencies into a specific object using the IServiceProvider
        /// First tries to resolve directly, if it fails, it creates a scope
        /// </summary>
        /// <param name="instance">Instance of the object to inject dependencies</param>
        public static void InjectDependencies(object instance)
        {
            if (instance == null || _provider == null)
                throw new InvalidOperationException("Instance is null or Factory is not configured. Call Factory.Configure() first.");

            var type = instance.GetType();

            // Process properties
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var property in properties)
            {
                // Only uses [Injectable]
                var injectableAttr = property.GetCustomAttribute<InjectableAttribute>();

                if (injectableAttr == null)
                    continue;

                if (!property.CanWrite)
                    continue;

                if (property.GetValue(instance) != null)
                    continue;

                // Uses the property type
                var serviceType = property.PropertyType;
                object? dependency = GetService(serviceType, instance);

                if (dependency != null)
                {
                    property.SetValue(instance, dependency);
                }
                else
                {
                    throw new InvalidOperationException($"Could not resolve dependency of type {serviceType.Name} for property {property.Name} in class {type.Name}");
                }
            }

            // Process fields
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                // Only uses [Injectable]
                var injectableAttr = field.GetCustomAttribute<InjectableAttribute>();

                if (injectableAttr == null)
                    continue;

                if (field.GetValue(instance) != null)
                    continue;

                // Uses the field type
                var serviceType = field.FieldType;
                var dependency = GetService(serviceType, instance);

                if (dependency != null)
                {
                    field.SetValue(instance, dependency);
                }
                else
                {
                    throw new InvalidOperationException($"Could not resolve dependency of type {serviceType.Name} for field {field.Name} in class {type.Name}");
                }
            }
        }

        /// <summary>
        /// Gets a service from the DI container.
        /// First tries to resolve directly. If it fails because it is a scoped service,
        /// it creates a scope per HTTP request and keeps it active.
        /// If the service is not registered, it automatically finds and registers the first implementation.
        /// </summary>
        private static object? GetService(Type serviceType, object instance)
        {
            if (serviceType == null || _provider == null)
            {
                throw new InvalidOperationException("Service type is null or Factory is not configured. Call Factory.Configure() first.");
            }

            try
            {
                // First tries to resolve directly from the DI container
                var service = _provider.GetService(serviceType);
                if (service != null)
                    return service;

                // If service is null, try to auto-resolve
                return TryAutoResolve(serviceType);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("scoped service") || ex.Message.Contains("root provider"))
            {
                // If it fails because it is scoped, creates a scope per HTTP request
                var requestId = GetRequestId();

                lock (_lock)
                {
                    if (!_requestScopes.ContainsKey(requestId))
                    {
                        _requestScopes[requestId] = _provider.CreateScope();
                    }
                }

                var service = _requestScopes[requestId].ServiceProvider.GetService(serviceType);
                if (service != null)
                    return service;

                // If still null, try to auto-resolve with scoped behavior
                return TryAutoResolveScoped(serviceType, requestId);
            }
        }

        /// <summary>
        /// Tries to automatically resolve a service by finding and instantiating its first implementation
        /// </summary>
        private static object? TryAutoResolve(Type serviceType)
        {
            if (!serviceType.IsInterface)
                return null;

            lock (_lock)
            {
                // Check if we already found an implementation for this type
                if (_autoRegisteredTypes.TryGetValue(serviceType, out var cachedImplementationType))
                {
                    return CreateInstance(cachedImplementationType);
                }

                // Find the first implementation
                var implementationType = FindFirstImplementation(serviceType);
                if (implementationType == null)
                    return null;

                // Cache the mapping for future use
                _autoRegisteredTypes[serviceType] = implementationType;

                return CreateInstance(implementationType);
            }
        }

        /// <summary>
        /// Tries to automatically resolve a service with scoped behavior
        /// </summary>
        private static object? TryAutoResolveScoped(Type serviceType, string requestId)
        {
            if (!serviceType.IsInterface)
                return null;

            lock (_lock)
            {
                // For scoped services, we need to check if we already have an instance in this scope
                if (!_requestScopes.TryGetValue(requestId, out var scope))
                {
                    scope = _provider.CreateScope();
                    _requestScopes[requestId] = scope;
                }

                // Try to get from scope first
                var existingService = scope.ServiceProvider.GetService(serviceType);
                if (existingService != null)
                    return existingService;

                // Check if we already found an implementation for this type
                if (_autoRegisteredTypes.TryGetValue(serviceType, out var cachedImplementationType))
                {
                    return CreateInstance(cachedImplementationType);
                }

                // Find the first implementation
                var implementationType = FindFirstImplementation(serviceType);
                if (implementationType == null)
                    return null;

                // Cache the mapping for future use
                _autoRegisteredTypes[serviceType] = implementationType;

                return CreateInstance(implementationType);
            }
        }

        /// <summary>
        /// Creates an instance of the specified type, handling dependency injection
        /// </summary>
        private static object? CreateInstance(Type implementationType)
        {
            try
            {
                // Try to create using the DI container first (in case it has dependencies)
                if (_provider != null)
                {
                    try
                    {
                        return ActivatorUtilities.CreateInstance(_provider, implementationType);
                    }
                    catch
                    {
                        // If DI creation fails, fall back to direct instantiation
                    }
                }

                // Fallback: create using default constructor
                var instance = Activator.CreateInstance(implementationType);
                
                // If the instance inherits from InjectBase, dependencies will be injected automatically
                // Otherwise, we need to manually inject dependencies if it has Injectable properties
                if (instance != null && !typeof(InjectBase).IsAssignableFrom(implementationType))
                {
                    InjectDependencies(instance);
                }

                return instance;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Finds the first implementation of an interface in all loaded assemblies
        /// </summary>
        private static Type? FindFirstImplementation(Type interfaceType)
        {
            try
            {
                // Get all loaded assemblies (excluding system assemblies)
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic &&
                               !a.FullName.StartsWith("System") &&
                               !a.FullName.StartsWith("Microsoft") &&
                               !a.FullName.StartsWith("netstandard") &&
                               !a.FullName.StartsWith("mscorlib"))
                    .ToList();

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var implementation = assembly.GetTypes()
                            .FirstOrDefault(type => type.IsClass &&
                                                  !type.IsAbstract &&
                                                  !type.IsInterface &&
                                                  interfaceType.IsAssignableFrom(type));

                        if (implementation != null)
                            return implementation;
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // Skip assemblies that can't be loaded
                        continue;
                    }
                    catch
                    {
                        // Skip any other assembly loading errors
                        continue;
                    }
                }
            }
            catch
            {
                // If any error occurs, return null
            }

            return null;
        }

        /// <summary>
        /// Gets a unique identifier for the current request
        /// Prioritizes HttpContext.TraceIdentifier, then Activity.Current, and finally Thread ID as fallback
        /// </summary>
        private static string GetRequestId()
        {
            try
            {
                // First option: use HttpContext if available (safer for HTTP requests)
                if (_provider != null)
                {
                    var httpContextAccessor = _provider.GetService<IHttpContextAccessor>();
                    var httpContext = httpContextAccessor?.HttpContext;

                    if (httpContext != null)
                    {
                        // TraceIdentifier is unique per HTTP request
                        return $"http-{httpContext.TraceIdentifier}";
                    }
                }

                // Second option: use Activity.Current if available (for distributed tracing)
                var activity = System.Diagnostics.Activity.Current;
                if (activity != null)
                {
                    return $"activity-{activity.Id}";
                }
            }
            catch
            {
                // If there is any error, use fallback
            }

            // Fallback: Thread ID (less safe, but works outside the HTTP context)
            return $"thread-{Environment.CurrentManagedThreadId}";
        }

        /// <summary>
        /// Disposes the scope associated with the current request
        /// </summary>
        public static void DisposeRequestScope()
        {
            var requestId = GetRequestId();

            lock (_lock)
            {
                if (_requestScopes.TryGetValue(requestId, out var scope))
                {
                    scope.Dispose();
                    _requestScopes.Remove(requestId);
                }
            }
        }

        /// <summary>
        /// Disposes all scopes (used for cleanup)
        /// </summary>
        public static void DisposeAllScopes()
        {
            lock (_lock)
            {
                foreach (var scope in _requestScopes.Values)
                {
                    scope.Dispose();
                }
                _requestScopes.Clear();
            }
        }

        /// <summary>
        /// Gets an ILogger instance for the specified type.
        /// </summary>
        /// <typeparam name="T">The type requesting the logger.</typeparam>
        /// <returns>An ILogger instance.</returns>
        public static ILogger<T> GetLogger<T>()
        {
            if (_provider == null)
            {
                throw new InvalidOperationException("Factory not configured. Call Factory.Configure() first.");
            }
            return _provider.GetRequiredService<ILogger<T>>();
        }

        /// <summary>
        /// Gets an ILogger instance for the specified type (using Type).
        /// </summary>
        /// <param name="type">The type requesting the logger.</param>
        /// <returns>An ILogger instance.</returns>
        public static ILogger GetLogger(Type type)
        {
            if (_provider == null)
            {
                throw new InvalidOperationException("Factory not configured. Call Factory.Configure() first.");
            }
            // Dynamically creates the ILogger<> type for the provided type
            var loggerType = typeof(ILogger<>).MakeGenericType(type);
            // Gets the ILogger<T> service from the provider
            var logger = _provider.GetService(loggerType);

            // Fallback to a non-generic logger if the generic one isn't registered
            logger ??= _provider.GetService<ILogger>();

            if (logger == null)
            {
                throw new InvalidOperationException($"Could not get logger service for type {type.FullName}.");
            }

            return (ILogger)logger;
        }
    }
}