using Microsoft.Extensions.Logging;

namespace AutoInject
{
    public abstract class InjectBase
    {
      protected readonly ILogger _logger;

        protected InjectBase()
        {
            Factory.InjectDependencies(this);

            _logger = Factory.GetLogger(this.GetType());
        }
    }
}
