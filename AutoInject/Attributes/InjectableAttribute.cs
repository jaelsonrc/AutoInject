namespace AutoInject.Attributes
{
    /// <summary>
    /// Attribute used to mark properties that should receive dependency injection
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class InjectableAttribute : Attribute
    {
    }
}
