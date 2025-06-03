# 🚀 AutoInject

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/JZen.AutoInject.svg)](https://www.nuget.org/packages/JZen.AutoInject/)

> **🎉 NEW v2.0: Smart Auto-Registration!**  
> **Stop writing constructor injection boilerplate! 🛑**  
> AutoInject **automatically discovers and registers** your dependencies using inheritance - zero manual registration needed!

## 🧠 Smart Auto-Registration via InjectBase

**The future of dependency injection is here!** No more manual registration:

```csharp
// Program.cs - Just this! 🎯
var builder = WebApplication.CreateBuilder(args);
// No special setup needed for InjectBase! Factory auto-configures!

var app = builder.Build();
app.UseFactoryDependencyInjection(); // Configure AutoInject Factory
app.Run();

// Your service - Zero configuration needed! ✨
public class UserService : InjectBase
{
    [Injectable] private readonly IUserRepository? _repository;     // Auto-found!
    [Injectable] private readonly IEmailService? _emailService;     // Auto-registered!
    
    public async Task CreateUserAsync(string name)
    {
        var user = new User { Name = name };
        await _repository!.SaveAsync(user);           // Works automatically!
        _emailService!.SendWelcomeEmail(user);        // No setup required!
    }
}
```

**🎯 How Smart Auto-Registration Works:**
1. 🔍 **Inherits from InjectBase** - Automatic dependency discovery
2. 📝 **Auto-registers** implementations as Scoped services on first use
3. 💉 **Injects** dependencies instantly using `[Injectable]` attributes
4. 🚀 **Caches** mappings for blazing-fast performance
5. 🧹 **Manages** scoping and disposal automatically

**🆚 Before vs After:**

| Before (Manual Hell) | After (InjectBase Auto-Registration) |
|----------------------|--------------------------------------|
| 50+ lines of registration | Zero registration needed |
| Manual interface mapping | Automatic discovery |
| Assembly scanning setup | Just inherit InjectBase |
| Startup performance hit | Lazy, on-demand loading |
| Maintenance nightmare | Self-maintaining |

## 📋 Two Ways to Use AutoInject

### 🌟 Method 1: InjectBase (Recommended - Zero Config!)

Simply inherit from `InjectBase` - no registration needed:

```csharp
// Program.cs - Minimal setup
var app = builder.Build();
app.UseFactoryDependencyInjection();
app.Run();

// Your service - Just inherit and use!
public class OrderService : InjectBase
{
    [Injectable] private readonly IOrderRepository? _repository;
    [Injectable] private readonly IEmailService? _emailService;
    
    public async Task ProcessOrderAsync(Order order)
    {
        await _repository!.SaveAsync(order);
        _emailService!.SendConfirmation(order);
    }
}
```

### 🔧 Method 2: [AutoInject] Attribute (For Existing Classes)

For classes you can't modify to inherit from InjectBase:

```csharp
// Program.cs - Register classes with [AutoInject]
builder.Services.AddAutoInjectClasses(); // Scans for [AutoInject] classes
var app = builder.Build();
app.UseFactoryDependencyInjection();
app.Run();

// Your existing class
[AutoInject]
public class LegacyService
{
    [Injectable] private readonly IRepository? _repository;
    
    public void DoSomething()
    {
        // Factory.InjectDependencies(this) called automatically
        _repository!.Save(data);
    }
}
```

## 🚀 Quick Start

### 1. Install the Package
```bash
dotnet add package JZen.AutoInject
```

### 2. Configure (One Line!)

For **InjectBase** classes (recommended):
```csharp
var builder = WebApplication.CreateBuilder(args);
// No services registration needed!

var app = builder.Build();
app.UseFactoryDependencyInjection(); // 🚀 Factory configured!
app.Run();
```

For **[AutoInject]** classes:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAutoInjectClasses(); // 🎯 Register classes with [AutoInject]

var app = builder.Build();
app.UseFactoryDependencyInjection(); // 🚀 Factory configured!
app.Run();
```

### 3. Use in Your Classes

**Option A: InjectBase (Zero Setup)**
```csharp
public class ProductService : InjectBase
{
    [Injectable] private readonly IProductRepository? _repository;
    [Injectable] private readonly ICacheService? _cache;
    
    public async Task<Product> GetProductAsync(int id)
    {
        _logger.LogInformation("Getting product {Id}", id);
        
        var cached = _cache!.Get<Product>($"product_{id}");
        if (cached != null) return cached;
        
        var product = await _repository!.GetByIdAsync(id);
        _cache.Set($"product_{id}", product);
        
        return product;
    }
}
```

**Option B: [AutoInject] Attribute**
```csharp
[AutoInject]
public class ProductService
{
    [Injectable] private readonly IProductRepository? _repository;
    [Injectable] private readonly ICacheService? _cache;
    
    // Dependencies injected automatically when created via DI container
}
```

**That's it! No manual registration needed!** 🎉

## 🎛️ Features

### 🧠 Smart Auto-Registration
- **Zero configuration** - InjectBase automatically finds implementations
- **On-demand discovery** - Only registers what you actually use  
- **Performance optimized** - Caches mappings for speed
- **Cross-assembly support** - Works with any loaded assembly

### 🎯 Two Integration Patterns
- **InjectBase**: Automatic injection on construction (recommended)
- **[AutoInject]**: Factory-based injection for existing classes

### 🪵 Built-in Logging
- Automatic `ILogger` injection via `InjectBase`
- Type-safe logger instances
- Zero configuration

### ✅ Scoped Management
- HTTP request scoping
- Automatic disposal
- Thread-safe operations

### ✅ ASP.NET Core Integration
- Works with existing DI container
- Middleware support
- Controller injection

### ✅ Backward Compatibility
- Mix with manual registrations
- Existing code keeps working
- Gradual migration support

## 📋 Usage Examples

### Basic Service with InjectBase
```csharp
public class EmailService : InjectBase
{
    [Injectable] private readonly IEmailProvider? _provider;
    [Injectable] private readonly ITemplateEngine? _templates;
    
    public async Task SendAsync(string to, string subject, string body)
    {
        _logger.LogInformation("Sending email to {To}", to);
        
        var template = _templates!.Render(body);
        await _provider!.SendAsync(to, subject, template);
    }
}
```

### Controller Integration
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService = new(); // InjectBase auto-injects
    
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request.Name);
        return Ok(user);
    }
}
```

### Background Service
```csharp
public class OrderProcessingService : BackgroundService, InjectBase
{
    [Injectable] private readonly IOrderRepository? _orders;
    [Injectable] private readonly IPaymentService? _payments;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var pendingOrders = await _orders!.GetPendingAsync();
            
            foreach (var order in pendingOrders)
            {
                await _payments!.ProcessAsync(order);
                _logger.LogInformation("Processed order {OrderId}", order.Id);
            }
            
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
```

## 🔧 Advanced Configuration

### Mixed Registration
```csharp
// Program.cs - Mix manual and auto registration
builder.Services.AddScoped<ISpecialService, CustomImplementation>(); // Manual registration
// InjectBase classes will auto-register their dependencies

public class MyService : InjectBase
{
    [Injectable] private readonly ISpecialService? _special;      // Uses manual registration
    [Injectable] private readonly IRegularService? _regular;     // Auto-registered
}
```

### Non-Web Applications
```csharp
// For console apps, background services, etc.
var services = new ServiceCollection();
services.AddLogging();

var serviceProvider = services.BuildServiceProvider();
Factory.Configure(serviceProvider);

// Now you can use InjectBase classes
var myService = new MyService(); // Dependencies auto-injected
```

### Manual Injection
```csharp
public class LegacyClass
{
    [Injectable] private readonly IRepository? _repository;
    
    public LegacyClass()
    {
        Factory.InjectDependencies(this); // Manual injection
    }
}
```

### Custom Assembly Scanning for [AutoInject]
```csharp
// Scan specific assemblies for [AutoInject] classes
builder.Services.AddAutoInjectClasses(Assembly.GetExecutingAssembly());
builder.Services.AddAutoInjectClasses(typeof(SomeExternalClass).Assembly);

// Or scan multiple assemblies
var assemblies = new[] { Assembly.GetExecutingAssembly(), typeof(ExternalService).Assembly };
builder.Services.AddAutoInjectClasses(assemblies);
```

## 🎯 When to Use AutoInject

### ✅ Perfect For:
- **Rapid prototyping** - Get up and running fast
- **Clean architecture** - Focus on business logic  
- **Large applications** - Reduce boilerplate significantly
- **Convention over configuration** - Sensible defaults

### ⚠️ Consider Alternatives When:
- You need multiple implementations of the same interface
- Complex lifetime management requirements (Singleton, custom scopes)
- Performance-critical applications with strict DI requirements
- You prefer explicit registration for all services

## 🔄 Migration Guide

### 🎉 Upgrading to v2.0

v2.0 is **100% backward compatible**. Your existing code continues to work unchanged.

**Recommended new setup:**
```csharp
// Program.cs - v2.0 InjectBase (Zero registration!)
var app = builder.Build();
app.UseFactoryDependencyInjection(); // Just this line!
app.Run();

// Your service - Just inherit InjectBase
public class MyService : InjectBase
{
    [Injectable] private readonly IRepository? _repository; // Auto-discovered!
}
```

**For [AutoInject] classes:**
```csharp
// Program.cs - v2.0 with [AutoInject] support
builder.Services.AddAutoInjectClasses(); // Register [AutoInject] classes
app.UseFactoryDependencyInjection();
```

### ⚠️ Obsolete Methods

The following methods are now obsolete and will be removed in v3.0:

```csharp
// ❌ Obsolete - Don't use these anymore
services.AddInjectBaseClasses(assembly);
services.AddInjectBaseClasses(assemblies);  
services.AddInjectBaseClasses();
```

**Migration is simple:**
```csharp
// Before - Manual registration (obsolete)
services.AddInjectBaseClasses(Assembly.GetExecutingAssembly());

// After - No registration needed for InjectBase!
// Just use: app.UseFactoryDependencyInjection();

// OR for [AutoInject] classes:
services.AddAutoInjectClasses(); // Replaces AddInjectBaseClasses
```

### From Constructor Injection
```csharp
// Before
public class OrderService
{
    private readonly IRepository _repo;
    private readonly IEmailService _email;
    
    public OrderService(IRepository repo, IEmailService email)
    {
        _repo = repo;
        _email = email;
    }
}

// After
public class OrderService : InjectBase
{
    [Injectable] private readonly IRepository? _repo;
    [Injectable] private readonly IEmailService? _email;
}
```

### From Manual DI Registration
```csharp
// Before - Program.cs (50+ lines)
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IEmailService, EmailService>();
services.AddScoped<INotificationService, NotificationService>();
// ... dozens more registrations

// After - Program.cs (1 line)
app.UseFactoryDependencyInjection(); // InjectBase auto-handles everything!
```

## 📊 Performance

- **Startup**: Minimal overhead, lazy discovery
- **Runtime**: Cached type mappings for fast resolution
- **Memory**: Efficient scoping and disposal  
- **Throughput**: Comparable to native DI container

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Inspired by the need for cleaner dependency injection in .NET
- Built on top of Microsoft.Extensions.DependencyInjection
- Community feedback and contributions

---

**Made with ❤️ for the .NET community**

[⭐ Star this repo](https://github.com/jaelsonrc/AutoInject) if you find it useful!