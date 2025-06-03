# 🚀 AddInjectBaseClasses - Registro Automático de Dependências

## 📋 Visão Geral

O método `AddInjectBaseClasses` automatiza o registro de classes que herdam de `InjectBase` e suas dependências marcadas com `[Injectable]`.

## 🎯 Como Funciona

1. **Procura todas as classes que herdam de `InjectBase`**
2. **Identifica propriedades/campos marcados com `[Injectable]`**
3. **Encontra a primeira implementação de cada interface**
4. **Registra automaticamente no DI container**

## 💡 Exemplo de Uso

### 1. Definindo suas Classes Base

```csharp
// Interface que será injetada
public interface IUsuarioLogado
{
    string GetUsuarioId();
    string GetUsuarioNome();
}

// Implementação da interface
public class UsuarioLogadoService : IUsuarioLogado
{
    public string GetUsuarioId() => "123";
    public string GetUsuarioNome() => "João Silva";
}

// Sua classe Repository que herda de InjectBase
public class UsuarioRepository : InjectBase
{
    [Injectable]
    protected readonly IUsuarioLogado _usuarioLogado;

    [Injectable]
    protected readonly ILogger<UsuarioRepository> _logger;

    public string GetCurrentUserData()
    {
        return $"Dados do usuário: {_usuarioLogado.GetUsuarioNome()}";
    }
}
```

### 2. Registrando no DI Container

```csharp
// No seu Program.cs ou Startup.cs
var builder = WebApplication.CreateBuilder(args);

// ✨ Uma única linha registra tudo automaticamente!
builder.Services.AddInjectBaseClasses();

// Ou especificando um assembly específico
builder.Services.AddInjectBaseClasses(typeof(UsuarioRepository).Assembly);

// Ou especificando o lifetime
builder.Services.AddInjectBaseClasses(ServiceLifetime.Singleton);

var app = builder.Build();
```

### 3. O que Acontece Automaticamente

O método `AddInjectBaseClasses` fará:

```csharp
// ✅ Registra automaticamente:
services.AddScoped<IUsuarioLogado, UsuarioLogadoService>();
services.AddScoped<UsuarioRepository>();

// ✅ Se UsuarioRepository implementar IUsuarioRepository:
services.AddScoped<IUsuarioRepository>(provider => provider.GetRequiredService<UsuarioRepository>());
```

## 🔄 Comparação: Antes vs Depois

### ❌ Antes (Manual)
```csharp
// Você tinha que registrar manualmente cada dependência
services.AddScoped<IUsuarioLogado, UsuarioLogadoService>();
services.AddScoped<IEmailService, EmailService>();
services.AddScoped<INotificationService, NotificationService>();
services.AddScoped<IPaymentService, PaymentService>();
services.AddScoped<UsuarioRepository>();
services.AddScoped<ProdutoRepository>();
services.AddScoped<PedidoRepository>();
// ... e assim por diante para cada classe
```

### ✅ Depois (Automático)
```csharp
// Uma única linha faz tudo!
services.AddInjectBaseClasses();
```

## 🎨 Cenários Avançados

### Múltiplos Assemblies
```csharp
var assemblies = new[]
{
    typeof(UsuarioRepository).Assembly,
    typeof(ProdutoService).Assembly,
    typeof(EmailService).Assembly
};

services.AddInjectBaseClasses(assemblies);
```

### Diferentes Lifetimes
```csharp
// Registra como Singleton
services.AddInjectBaseClasses(ServiceLifetime.Singleton);

// Registra como Transient
services.AddInjectBaseClasses(ServiceLifetime.Transient);
```

### Evitando Duplicatas
```csharp
// Se você já registrou manualmente algum serviço, 
// o AddInjectBaseClasses não irá sobrescrever
services.AddScoped<IUsuarioLogado, CustomUsuarioLogadoService>();
services.AddInjectBaseClasses(); // Não irá registrar IUsuarioLogado novamente
```

## 🧪 Exemplo Completo

```csharp
// Interfaces
public interface IUsuarioLogado
{
    string GetUsuarioId();
}

public interface IEmailService
{
    Task SendEmailAsync(string email, string message);
}

// Implementações
public class UsuarioLogadoService : IUsuarioLogado
{
    public string GetUsuarioId() => "user123";
}

public class EmailService : IEmailService
{
    public Task SendEmailAsync(string email, string message)
    {
        // Implementação do envio de email
        return Task.CompletedTask;
    }
}

// Repositórios que herdam de InjectBase
public class UsuarioRepository : InjectBase
{
    [Injectable]
    protected readonly IUsuarioLogado _usuarioLogado;

    public string GetCurrentUser()
    {
        return _usuarioLogado.GetUsuarioId();
    }
}

public class NotificationRepository : InjectBase
{
    [Injectable]
    protected readonly IUsuarioLogado _usuarioLogado;

    [Injectable]
    protected readonly IEmailService _emailService;

    public async Task SendWelcomeEmail()
    {
        var userId = _usuarioLogado.GetUsuarioId();
        await _emailService.SendEmailAsync($"{userId}@example.com", "Bem-vindo!");
    }
}

// Configuração no Program.cs
var builder = WebApplication.CreateBuilder(args);

// 🎯 Registra automaticamente:
// - IUsuarioLogado -> UsuarioLogadoService
// - IEmailService -> EmailService  
// - UsuarioRepository
// - NotificationRepository
builder.Services.AddInjectBaseClasses();

var app = builder.Build();
```

## ✨ Benefícios

- **🚀 Produtividade**: Elimina código boilerplate
- **🔧 Manutenibilidade**: Menos código para manter
- **🎯 Convenção**: Segue padrões consistentes
- **🛡️ Segurança**: Evita duplicatas automáticamente
- **📦 Flexibilidade**: Funciona com múltiplos assemblies

## 🤝 Combinando com AddAutoInjectClasses

Você pode usar ambos os métodos juntos:

```csharp
services.AddAutoInjectClasses();    // Para classes com [AutoInject]
services.AddInjectBaseClasses();    // Para classes que herdam de InjectBase
```

Agora você tem o melhor dos dois mundos! 🎉