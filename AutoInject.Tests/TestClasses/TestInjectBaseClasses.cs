using AutoInject.Attributes;

namespace AutoInject.Tests.TestClasses
{
    /// <summary>
    /// Test class that inherits from InjectBase and has Injectable properties
    /// This simulates your Repository<T> : InjectBase scenario
    /// </summary>
    public class TestRepositoryBase : InjectBase
    {
        [Injectable]
        protected readonly IUsuarioLogado? _usuarioLogado;

        [Injectable]
        protected readonly ITestService? _testService;

        // Public property to access the injected dependency for testing
        public IUsuarioLogado? UsuarioLogado => _usuarioLogado;
        public ITestService? TestService => _testService;

        public virtual string GetData()
        {
            return $"Data from user: {_usuarioLogado?.GetUsuarioNome() ?? "Unknown"}";
        }
    }

    /// <summary>
    /// Another test class that inherits from InjectBase
    /// </summary>
    public class TestServiceBase : InjectBase
    {
        [Injectable]
        private readonly IUsuarioLogado? _usuarioLogado;

        [Injectable]
        private readonly ITestRepository? _repository;

        public IUsuarioLogado? UsuarioLogado => _usuarioLogado;
        public ITestRepository? Repository => _repository;

        public virtual void ProcessData()
        {
            var userId = _usuarioLogado?.GetUsuarioId();
            // Process data logic here
        }
    }

    /// <summary>
    /// Test class that inherits from InjectBase but has no Injectable properties
    /// </summary>
    public class TestEmptyBase : InjectBase
    {
        public virtual void DoNothing()
        {
            // Empty implementation
        }
    }

    /// <summary>
    /// Test class that inherits from InjectBase and implements an interface
    /// </summary>
    public class TestRepositoryWithInterface : InjectBase, ITestRepository
    {
        [Injectable]
        protected readonly IUsuarioLogado? _usuarioLogado;

        public IUsuarioLogado? UsuarioLogado => _usuarioLogado;

        public Task<string> GetDataAsync()
        {
            var userName = _usuarioLogado?.GetUsuarioNome() ?? "Unknown";
            return Task.FromResult($"Async data from user: {userName}");
        }
    }
}