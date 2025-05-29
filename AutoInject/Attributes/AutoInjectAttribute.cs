namespace AutoInject.Attributes
{
    /// <summary>
    /// Attribute used to mark classes that should have automatic dependency injection
    /// Replaces the need to inherit from FactoryBase
    /// All dependencies (including logger) must be marked with [Injectable]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoInjectAttribute : Attribute
    {
    }
}
