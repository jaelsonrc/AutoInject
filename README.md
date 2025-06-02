# 🚀 AutoInject

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/badge/NuGet-Coming%20Soon-orange.svg)](#)

> **Stop writing constructor injection boilerplate! 🛑**  
> AutoInject is a lightweight library that brings **attribute-based dependency injection** to .NET, eliminating the need for constructor injection hell.

## 🎯 Why AutoInject?

Tired of this? 😤

```csharp
public class OrderService
{
    private readonly IRepository _repository;
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderService> _logger;
    private readonly IPaymentService _paymentService;
    private readonly INotificationService _notificationService;

    public OrderService(
        IRepository repository,
        IEmailService emailService,
        ILogger<OrderService> logger,
        IPaymentService paymentService,
        INotificationService notificationService)
    {
        _repository = repository;
        _emailService = emailService;
        _logger = logger;
        _paymentService = paymentService;
        _notificationService = notificationService;
    }
}
```

**With AutoInject, write this instead:** ✨

```csharp
[AutoInject]
public class OrderService
{
    [Injectable] private readonly IRepository _repository;
    [Injectable] private readonly IEmailService _emailService;
    [Injectable] private readonly ILogger<OrderService> _logger;
    [Injectable] private readonly IPaymentService _paymentService;
    [Injectable] private readonly INotificationService _notificationService;

    // Clean constructor - no dependencies!
    public OrderService() { }
}
```

## 🌟 Key Features

- ✅ **Zero Boilerplate**: No more constructor injection bloat
- ✅ **Attribute-Based**: Simple `[AutoInject]` and `[Injectable]` attributes
- ✅ **ASP.NET Core Integration**: Seamless integration with built-in DI container
- ✅ **Automatic Registration**: Auto-scan and register your classes
- ✅ **Scoped Management**: Automatic scope management for web requests
- ✅ **Logger Support**: Built-in logger injection support
- ✅ **Performance Optimized**: Type caching for reflection operations
- ✅ **Thread-Safe**: Safe for concurrent operations

## 📦 Installation

```bash
# Coming soon to NuGet
Install-Package JZen.AutoInject
```

Or add the project reference:

```xml
<ProjectReference Include="path/to/AutoInject/AutoInject.csproj" />
```

## 🚀 Quick Start

### 1. Configure your services in `Program.cs`

```csharp
using AutoInject.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register your services
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IEmailService, EmailService>();

// 🔥 Auto-register all classes marked with [AutoInject]
builder.Services.AddAutoInjectClasses(Assembly.GetExecutingAssembly());

var app = builder.Build();

// 🔥 Enable AutoInject
app.UseFactoryDependencyInjection();

app.Run();
```

### 2. Mark your classes with `[AutoInject]`

```csharp
using AutoInject.Attributes;

[AutoInject]
public class OrderService
{
    [Injectable] private readonly IRepository _repository;
    [Injectable] private readonly IEmailService _emailService;
    [Injectable] private readonly ILogger<OrderService> _logger;

    public async Task ProcessOrderAsync(Order order)
    {
        _logger.LogInformation("Processing order {OrderId}", order.Id);
        
        await _repository.SaveAsync(order);
        await _emailService.SendConfirmationAsync(order.CustomerEmail);
        
        _logger.LogInformation("Order {OrderId} processed successfully", order.Id);
    }
}
```

### 3. Use your services normally

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] Order order)
    {
        await _orderService.ProcessOrderAsync(order);
        return Ok();
    }
}
```

## 📚 Advanced Usage

### Inherit from `InjectBase` (Alternative Approach)

```csharp
using AutoInject;

public class OrderService : InjectBase
{
    [Injectable] private readonly IRepository _repository;
    [Injectable] private readonly IEmailService _emailService;
    
    // Logger is automatically available as _logger
    public void ProcessOrder()
    {
        _logger.LogInformation("Processing order...");
        // Your logic here
    }
}
```

### Automatic Interface Registration

```csharp
// Register all interfaces from a specific namespace
builder.Services.AddScopedInjection("MyApp.Application.UseCases");
```

### Multiple Assembly Registration

```csharp
var assemblies = new[] 
{ 
    Assembly.GetExecutingAssembly(),
    typeof(SomeOtherClass).Assembly 
};

builder.Services.AddAutoInjectClasses(assemblies);
```

### Custom Service Lifetimes

```csharp
// Register as Singleton
builder.Services.AddAutoInjectClasses(
    Assembly.GetExecutingAssembly(), 
    ServiceLifetime.Singleton
);
```

## 🔧 How It Works

AutoInject uses a combination of:

1. **Reflection**: To discover types and properties marked with attributes
2. **Custom Factory**: Creates instances and injects dependencies
3. **Interceptors**: Automatically process classes during instantiation
4. **Middleware**: Manages scoped services lifecycle in web applications
5. **Caching**: Optimizes reflection operations for better performance

### Architecture Overview

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   [AutoInject]  │───▶│     Factory     │───▶│  DI Container   │
│     Classes     │    │   + Interceptor │    │   (ASP.NET)     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                        │                        │
         ▼                        ▼                        ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   [Injectable]  │    │   Auto-Factory  │    │   Scope Mgmt    │
│   Properties    │    │   Registration  │    │   Middleware    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🎯 Best Practices

### ✅ DO

```csharp
[AutoInject]
public class UserService
{
    [Injectable] private readonly IUserRepository _userRepository;
    [Injectable] private readonly ILogger<UserService> _logger;
    
    // Use private readonly fields
    // Mark with [Injectable]
    // Keep constructors clean
}
```

### ❌ DON'T

```csharp
[AutoInject]
public class UserService
{
    [Injectable] public IUserRepository UserRepository; // Don't use public fields
    [Injectable] private ILogger<UserService> _logger;  // Don't use mutable fields
    
    // Don't mix constructor injection with AutoInject
    public UserService(ISomeService service) { }
}
```

## 🔍 Comparison

| Feature | Constructor Injection | AutoInject |
|---------|---------------------|------------|
| **Boilerplate Code** | High 📈 | Minimal ✨ |
| **Readability** | Cluttered 😵 | Clean 🧹 |
| **Refactoring** | Painful 😤 | Easy 🎯 |
| **Performance** | Faster 🏃‍♂️ | Slightly Slower 🚶‍♂️ |
| **Learning Curve** | Standard 📚 | Minimal 🎓 |

## 🤝 Contributing

We welcome contributions! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙋‍♂️ Support

- 📧 Email: [jaelsonrc@hotmail.com]
- 🐛 Issues: [GitHub Issues](https://github.com/jaelsonrc/AutoInject/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/jaesonrc/AutoInject/discussions)

---

**Made with ❤️ by developers who hate constructor injection boilerplate!**

> "Life's too short for constructor injection hell" - Anonymous Developer 