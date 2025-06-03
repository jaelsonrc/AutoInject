# 🔧 Troubleshooting AddInjectBaseClasses

## 🎯 Problema Comum: "Não consegue localizar a classe que deve carregar"

### 📋 Checklist de Diagnóstico

#### 1. **Verificar se as classes herdam de InjectBase**
```csharp
// ✅ Correto
public class MinhaClasse : InjectBase
{
    [Injectable]
    public IMinhaInterface MinhaPropriedade { get; set; }
}

// ❌ Incorreto - não herda de InjectBase
public class MinhaClasse
{
    [Injectable]
    public IMinhaInterface MinhaPropriedade { get; set; }
}
```

#### 2. **Verificar se as propriedades estão marcadas com [Injectable]**
```csharp
public class MinhaClasse : InjectBase
{
    // ✅ Correto
    [Injectable]
    public IEmailService EmailService { get; set; }
    
    // ❌ Incorreto - sem atributo
    public IEmailService EmailService { get; set; }
}
```

#### 3. **Verificar se as implementações existem no mesmo assembly**
```csharp
// Interface
public interface IEmailService
{
    void EnviarEmail(string para, string assunto, string corpo);
}

// ✅ Implementação deve existir
public class EmailService : IEmailService
{
    public void EnviarEmail(string para, string assunto, string corpo)
    {
        // implementação
    }
}
```

### 🔍 Métodos de Debug

#### Opção 1: Usar versão com Debug
```csharp
// Em Program.cs ou Startup.cs
services.AddInjectBaseClassesDebug(); // Mostra logs detalhados
```

#### Opção 2: Usar versão avançada (escaneia todos os assemblies)
```csharp
// Escaneia todos os assemblies carregados
services.AddInjectBaseClassesAdvanced(Assembly.GetExecutingAssembly());
```

#### Opção 3: Especificar múltiplos assemblies
```csharp
var assemblies = new[]
{
    Assembly.GetExecutingAssembly(),
    Assembly.GetAssembly(typeof(MinhaClasse)),
    // outros assemblies onde estão as implementações
};

services.AddInjectBaseClasses(assemblies);
```

### 🚨 Problemas Comuns e Soluções

#### **Problema 1: Implementações em assembly diferente**
```csharp
// ❌ Problema: Interface no Assembly A, Implementação no Assembly B
// Solução: Especificar ambos os assemblies
services.AddInjectBaseClasses(new[] { assemblyA, assemblyB });

// Ou usar a versão avançada
services.AddInjectBaseClassesAdvanced(Assembly.GetExecutingAssembly());
```

#### **Problema 2: Assembly não carregado**
```csharp
// ✅ Forçar carregamento do assembly
Assembly.LoadFrom("CaminhoParaAssembly.dll");
services.AddInjectBaseClasses();
```

#### **Problema 3: Namespace incorreto**
```csharp
// Verificar se os using estão corretos
using AutoInject;
using AutoInject.Attributes;
using AutoInject.Extensions;
```

### 📝 Exemplo Completo Funcionando

#### 1. Interface
```csharp
namespace MeuProjeto.Interfaces
{
    public interface IUsuarioService
    {
        string ObterNomeUsuario();
    }
}
```

#### 2. Implementação
```csharp
namespace MeuProjeto.Services
{
    public class UsuarioService : IUsuarioService
    {
        public string ObterNomeUsuario()
        {
            return "João Silva";
        }
    }
}
```

#### 3. Classe que usa InjectBase
```csharp
using AutoInject;
using AutoInject.Attributes;

namespace MeuProjeto.Controllers
{
    public class UsuarioController : InjectBase
    {
        [Injectable]
        public IUsuarioService UsuarioService { get; set; }

        public string ObterUsuario()
        {
            return UsuarioService.ObterNomeUsuario();
        }
    }
}
```

#### 4. Configuração no Program.cs
```csharp
using AutoInject.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ✅ Registrar automaticamente
builder.Services.AddInjectBaseClasses();

// Ou com debug para ver o que está acontecendo
// builder.Services.AddInjectBaseClassesDebug();

var app = builder.Build();
```

### 🔧 Comandos de Debug

#### Para ver o que está sendo registrado:
```csharp
// Adicione este código após AddInjectBaseClasses()
foreach (var service in builder.Services)
{
    Console.WriteLine($"Registrado: {service.ServiceType.Name} -> {service.ImplementationType?.Name}");
}
```

### 📞 Se ainda não funcionar:

1. **Use a versão debug**: `AddInjectBaseClassesDebug()`
2. **Verifique os logs** no console
3. **Confirme que**:
   - As classes herdam de `InjectBase`
   - As propriedades têm `[Injectable]`
   - As implementações existem no assembly
   - Os namespaces estão corretos

### 🎯 Dica Final

Se você comentou os registros manuais (como `services.AddScoped<IEmailService, EmailService>()`), certifique-se de que:

1. **Todas as interfaces** que você registrava manualmente têm implementações no assembly
2. **As classes que usam essas interfaces** herdam de `InjectBase`
3. **As propriedades** estão marcadas com `[Injectable]`

O `AddInjectBaseClasses()` só registra automaticamente as dependências que encontra nas classes `InjectBase`. Se uma interface não é usada em nenhuma classe `InjectBase`, ela não será registrada automaticamente.