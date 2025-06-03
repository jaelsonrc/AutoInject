# 🧠 Smart Auto-Registration

O AutoInject agora possui um sistema **inteligente de auto-registro** que elimina a necessidade de registrar manualmente todas as dependências no container DI.

## 🎯 Como Funciona

Quando você herda de `InjectBase` e marca propriedades com `[Injectable]`, o sistema automaticamente:

1. **Procura** por dependências não registradas
2. **Encontra** a primeira implementação disponível
3. **Registra** automaticamente como Scoped
4. **Injeta** a dependência

## 🚀 Exemplo Prático

### Antes (Modo Tradicional)
```csharp
// Program.cs - Tinha que registrar tudo manualmente
services.AddScoped<IUsuarioRepository, UsuarioRepository>();
services.AddScoped<IEmailService, EmailService>();
services.AddScoped<INotificationService, NotificationService>();
services.AddScoped<IPaymentService, PaymentService>();
// ... dezenas de registros manuais

// UsuarioService.cs
public class UsuarioService
{
    private readonly IUsuarioRepository _repository;
    private readonly IEmailService _emailService;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(
        IUsuarioRepository repository,
        IEmailService emailService,
        ILogger<UsuarioService> logger)
    {
        _repository = repository;
        _emailService = emailService;
        _logger = logger;
    }
}
```

### Agora (Modo Inteligente) ✨
```csharp
// Program.cs - Só registra o básico
services.AddLogging();
services.AddAutoInject(); // Configura o Factory

// UsuarioService.cs - Zero boilerplate!
public class UsuarioService : InjectBase
{
    [Injectable] private readonly IUsuarioRepository? _repository;
    [Injectable] private readonly IEmailService? _emailService;
    
    public async Task<Usuario> CriarUsuarioAsync(string nome)
    {
        _logger.LogInformation("Criando usuário {Nome}", nome);
        
        var usuario = new Usuario { Nome = nome };
        await _repository!.SalvarAsync(usuario);
        
        _emailService!.EnviarEmail("Bem-vindo!", $"Olá {nome}!");
        
        return usuario;
    }
}
```

## 🔍 O que Acontece Automaticamente

1. **Primeira vez** que `UsuarioService` é instanciado:
   - Factory procura por `IUsuarioRepository` no container DI
   - Não encontra? Busca automaticamente por `UsuarioRepository` em todos os assemblies
   - Encontra e registra: `services.AddScoped<IUsuarioRepository, UsuarioRepository>()`
   - Faz o mesmo para `IEmailService` → `EmailService`
   - Injeta as dependências nas propriedades

2. **Próximas vezes**:
   - Usa o cache de mapeamentos já descobertos
   - Performance otimizada

## 🎛️ Configuração Simples

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Registra apenas os serviços essenciais
builder.Services.AddLogging();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configura o Factory (uma única vez)
Factory.Configure(app.Services);

app.Run();
```

## 🔧 Funcionalidades Avançadas

### Mistura com Registros Manuais
```csharp
// Se você quiser controle sobre alguns serviços
services.AddScoped<IUsuarioRepository, UsuarioRepositoryCustomizado>();

// O sistema usará o registro manual e auto-registrará o resto
public class UsuarioService : InjectBase
{
    [Injectable] private readonly IUsuarioRepository? _repository; // Usa o manual
    [Injectable] private readonly IEmailService? _emailService;    // Auto-registra
}
```

### Injeção Manual (sem herdar InjectBase)
```csharp
public class MinhaClasse
{
    [Injectable] private readonly IUsuarioRepository? _repository;
    
    public MinhaClasse()
    {
        // Chama manualmente a injeção
        Factory.InjectDependencies(this);
    }
}
```

### Scoped por Request (Web Apps)
```csharp
// Automaticamente gerencia scopes por request HTTP
public class UsuarioController : ControllerBase
{
    private readonly UsuarioService _usuarioService;
    
    public UsuarioController()
    {
        _usuarioService = new UsuarioService(); // Auto-injeta tudo
    }
    
    [HttpPost]
    public async Task<IActionResult> CriarUsuario(string nome)
    {
        var usuario = await _usuarioService.CriarUsuarioAsync(nome);
        return Ok(usuario);
    }
}
```

## 🎯 Vantagens

### ✅ Elimina Boilerplate
- Zero construtores gigantes
- Zero registros manuais repetitivos
- Código mais limpo e focado na lógica de negócio

### ✅ Descoberta Automática
- Encontra implementações automaticamente
- Funciona com qualquer assembly carregado
- Cache inteligente para performance

### ✅ Flexibilidade Total
- Mistura registros manuais com auto-registro
- Funciona com e sem herança de `InjectBase`
- Compatível com ASP.NET Core DI

### ✅ Gerenciamento de Lifecycle
- Scoped por request HTTP automaticamente
- Dispose automático de recursos
- Thread-safe

## 🔍 Como o Sistema Encontra Implementações

1. **Busca em Assemblies**: Varre todos os assemblies carregados (exceto System/Microsoft)
2. **Filtros Inteligentes**: Procura por classes concretas que implementam a interface
3. **Primeira Implementação**: Usa a primeira implementação encontrada
4. **Cache**: Armazena o mapeamento para uso futuro

## 🚨 Considerações

### Quando Usar
- ✅ Aplicações com muitas dependências
- ✅ Prototipagem rápida
- ✅ Quando você quer focar na lógica de negócio
- ✅ Projetos onde convenção > configuração

### Quando NÃO Usar
- ❌ Quando você precisa de controle total sobre o DI
- ❌ Múltiplas implementações da mesma interface
- ❌ Configurações complexas de lifetime
- ❌ Aplicações com requisitos de performance extrema

## 🔄 Migração Gradual

Você pode migrar gradualmente:

```csharp
// Mantenha registros existentes
services.AddScoped<IServicoImportante, ServicoImportanteCustomizado>();

// Novos serviços usam auto-registro
public class NovoServico : InjectBase
{
    [Injectable] private readonly IServicoImportante? _servico; // Usa o manual
    [Injectable] private readonly INovoRepository? _repository;  // Auto-registra
}
```

## 🎉 Resultado Final

Com o sistema inteligente, você escreve **80% menos código** de configuração e foca no que realmente importa: **sua lógica de negócio**!

```csharp
// Antes: 50 linhas de configuração DI + construtores gigantes
// Agora: 3 linhas de configuração + propriedades simples

public class MeuServico : InjectBase
{
    [Injectable] private readonly IRepository? _repo;
    [Injectable] private readonly IEmailService? _email;
    
    public async Task FazerAlgo() => await _repo!.SalvarAsync(dados);
}
```

**É isso! Simples, inteligente e poderoso.** 🚀