using AutoInject.Attributes;
using System.Reflection;

namespace AutoInject
{
    /// <summary>
    /// Interceptor responsible for detecting and processing classes marked with [AutoInject]
    /// </summary>
    public static class AutoInjectInterceptor
    {
        private static readonly Dictionary<Type, bool> _typeCache = [];
        private static readonly object _cacheLock = new();

        /// <summary>
        /// Processes an instance if it is marked with [AutoInject]
        /// </summary>
        /// <param name="instance">Instance to be processed</param>
        /// <returns>True if the instance was processed, False otherwise</returns>
        public static bool ProcessIfMarked(object instance)
        {
            if (instance == null) return false;

            var type = instance.GetType();

            if (!HasAutoInjectAttribute(type))
                return false;

            ProcessAutoInject(instance);
            return true;
        }

        /// <summary>
        /// Checks if a type has the [AutoInject] attribute (with cache)
        /// </summary>
        private static bool HasAutoInjectAttribute(Type type)
        {
            lock (_cacheLock)
            {
                if (_typeCache.TryGetValue(type, out bool cached))
                    return cached;

                bool hasAttribute = type.GetCustomAttribute<AutoInjectAttribute>() != null;
                _typeCache[type] = hasAttribute;
                return hasAttribute;
            }
        }

        /// <summary>
        /// Processes automatic injection on an instance
        /// </summary>
        private static void ProcessAutoInject(object instance)
        {
            var type = instance.GetType();
            var autoInjectAttr = type.GetCustomAttribute<AutoInjectAttribute>();

            if (autoInjectAttr == null) return;

            // Injects all dependencies marked with [Injectable] (including logger)
            Factory.InjectDependencies(instance);
        }


    }
}
