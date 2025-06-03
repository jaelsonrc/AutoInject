# 🎯 Exemplo Completo de Uso

Este exemplo mostra como usar o AutoInject com Smart Auto-Registration em uma aplicação real.

## 📁 Estrutura do Projeto

```
MyApp/
├── Program.cs
├── Controllers/
│   └── UsuarioController.cs
├── Services/
│   ├── UsuarioService.cs
│   └── EmailService.cs
├── Repositories/
│   ├── IUsuarioRepository.cs
│   └── UsuarioRepository.cs
└── Models/
    └── Usuario.cs
```

## 🚀 1. Configuração Inicial (Program.cs)

```csharp
using AutoInject.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configuração básica
builder.Services.AddControllers();
builder.Services.AddLogging();

// 🎯 AutoInject - Uma linha resolve tudo!
builder.Services.AddAutoInject();

var app = builder.Build();

// 🎯 Configura o Factory
app.UseAutoInject();

app.MapControllers();
app.Run();
```

## 📝 2. Modelos (Models/Usuario.cs)

```csharp
namespace MyApp.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
```

## 🗄️ 3. Repository (Repositories/)

```csharp
// IUsuarioRepository.cs
namespace MyApp.Repositories
{
    public interface IUsuarioRepository
    {
        Task<Usuario> CriarAsync(Usuario usuario);
        Task<Usuario?> ObterPorIdAsync(int id);
        Task<List<Usuario>> ListarTodosAsync();
        Task<bool> ExisteEmailAsync(string email);
    }
}

// UsuarioRepository.cs
using MyApp.Models;

namespace MyApp.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        // Simulando um banco de dados em memória
        private static readonly List<Usuario> _usuarios = new();
        private static int _proximoId = 1;

        public async Task<Usuario> CriarAsync(Usuario usuario)
        {
            await Task.Delay(10); // Simula operação async
            
            usuario.Id = _proximoId++;
            usuario.DataCriacao = DateTime.UtcNow;
            _usuarios.Add(usuario);
            
            return usuario;
        }

        public async Task<Usuario?> ObterPorIdAsync(int id)
        {
            await Task.Delay(5);
            return _usuarios.FirstOrDefault(u => u.Id == id);
        }

        public async Task<List<Usuario>> ListarTodosAsync()
        {
            await Task.Delay(10);
            return _usuarios.ToList();
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            await Task.Delay(5);
            return _usuarios.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }
}
```

## 📧 4. Email Service (Services/EmailService.cs)

```csharp
namespace MyApp.Services
{
    public interface IEmailService
    {
        Task EnviarBoasVindasAsync(Usuario usuario);
        Task EnviarNotificacaoAsync(string email, string assunto, string mensagem);
    }

    public class EmailService : IEmailService
    {
        public async Task EnviarBoasVindasAsync(Usuario usuario)
        {
            await Task.Delay(50); // Simula envio de email
            
            Console.WriteLine($"📧 Email de boas-vindas enviado para {usuario.Email}");
            Console.WriteLine($"   Assunto: Bem-vindo, {usuario.Nome}!");
            Console.WriteLine($"   Mensagem: Sua conta foi criada com sucesso.");
        }

        public async Task EnviarNotificacaoAsync(string email, string assunto, string mensagem)
        {
            await Task.Delay(30);
            
            Console.WriteLine($"📧 Email enviado para {email}");
            Console.WriteLine($"   Assunto: {assunto}");
            Console.WriteLine($"   Mensagem: {mensagem}");
        }
    }
}
```

## 🏢 5. Business Service (Services/UsuarioService.cs)

```csharp
using AutoInject;
using AutoInject.Attributes;
using MyApp.Models;
using MyApp.Repositories;

namespace MyApp.Services
{
    // 🎯 Herda de InjectBase para injeção automática
    public class UsuarioService : InjectBase
    {
        // 🎯 Propriedades marcadas com [Injectable] são injetadas automaticamente
        [Injectable] private readonly IUsuarioRepository? _usuarioRepository;
        [Injectable] private readonly IEmailService? _emailService;
        
        // 🎯 Logger é injetado automaticamente via InjectBase
        // Disponível como _logger

        public async Task<Usuario> CriarUsuarioAsync(string nome, string email)
        {
            _logger.LogInformation("Iniciando criação de usuário: {Nome} - {Email}", nome, email);

            // Validações
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome é obrigatório", nameof(nome));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email é obrigatório", nameof(email));

            // Verifica se email já existe
            if (await _usuarioRepository!.ExisteEmailAsync(email))
            {
                _logger.LogWarning("Tentativa de criar usuário com email já existente: {Email}", email);
                throw new InvalidOperationException("Email já está em uso");
            }

            // Cria o usuário
            var usuario = new Usuario
            {
                Nome = nome,
                Email = email
            };

            var usuarioCriado = await _usuarioRepository.CriarAsync(usuario);
            _logger.LogInformation("Usuário criado com sucesso: ID {Id}", usuarioCriado.Id);

            // Envia email de boas-vindas
            try
            {
                await _emailService!.EnviarBoasVindasAsync(usuarioCriado);
                _logger.LogInformation("Email de boas-vindas enviado para {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email de boas-vindas para {Email}", email);
                // Não falha a criação do usuário por causa do email
            }

            return usuarioCriado;
        }

        public async Task<Usuario?> ObterUsuarioAsync(int id)
        {
            _logger.LogInformation("Buscando usuário por ID: {Id}", id);
            
            var usuario = await _usuarioRepository!.ObterPorIdAsync(id);
            
            if (usuario == null)
                _logger.LogWarning("Usuário não encontrado: ID {Id}", id);
            else
                _logger.LogInformation("Usuário encontrado: {Nome} - {Email}", usuario.Nome, usuario.Email);

            return usuario;
        }

        public async Task<List<Usuario>> ListarUsuariosAsync()
        {
            _logger.LogInformation("Listando todos os usuários");
            
            var usuarios = await _usuarioRepository!.ListarTodosAsync();
            
            _logger.LogInformation("Encontrados {Count} usuários", usuarios.Count);
            
            return usuarios;
        }
    }
}
```

## 🎮 6. Controller (Controllers/UsuarioController.cs)

```csharp
using Microsoft.AspNetCore.Mvc;
using MyApp.Services;
using MyApp.Models;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        // 🎯 Instanciação simples - AutoInject resolve tudo automaticamente!
        private readonly UsuarioService _usuarioService = new();

        [HttpPost]
        public async Task<ActionResult<Usuario>> CriarUsuario([FromBody] CriarUsuarioRequest request)
        {
            try
            {
                var usuario = await _usuarioService.CriarUsuarioAsync(request.Nome, request.Email);
                return CreatedAtAction(nameof(ObterUsuario), new { id = usuario.Id }, usuario);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> ObterUsuario(int id)
        {
            var usuario = await _usuarioService.ObterUsuarioAsync(id);
            
            if (usuario == null)
                return NotFound();

            return Ok(usuario);
        }

        [HttpGet]
        public async Task<ActionResult<List<Usuario>>> ListarUsuarios()
        {
            var usuarios = await _usuarioService.ListarUsuariosAsync();
            return Ok(usuarios);
        }
    }

    public class CriarUsuarioRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
```

## 🧪 7. Testando a Aplicação

### Criar Usuário
```bash
curl -X POST "https://localhost:5001/api/usuario" \
     -H "Content-Type: application/json" \
     -d '{"nome": "João Silva", "email": "joao@email.com"}'
```

### Listar Usuários
```bash
curl -X GET "https://localhost:5001/api/usuario"
```

### Obter Usuário por ID
```bash
curl -X GET "https://localhost:5001/api/usuario/1"
```

## 📊 O que Acontece Automaticamente

1. **Primeira requisição**:
   ```
   🔍 AutoInject procura por IUsuarioRepository
   ✅ Encontra UsuarioRepository
   📝 Registra: services.AddScoped<IUsuarioRepository, UsuarioRepository>()
   
   🔍 AutoInject procura por IEmailService  
   ✅ Encontra EmailService
   📝 Registra: services.AddScoped<IEmailService, EmailService>()
   
   💉 Injeta dependências em UsuarioService
   🚀 Executa a requisição
   ```

2. **Próximas requisições**:
   ```
   ⚡ Usa cache de mapeamentos (performance otimizada)
   💉 Injeta dependências rapidamente
   🚀 Executa a requisição
   ```

## 🎯 Vantagens Demonstradas

### ✅ Zero Configuração Manual
- Não precisou registrar `IUsuarioRepository` ou `IEmailService`
- Sistema encontrou e registrou automaticamente

### ✅ Código Limpo
- Sem construtores gigantes
- Foco na lógica de negócio
- Propriedades simples com `[Injectable]`

### ✅ Logging Automático
- `_logger` disponível automaticamente
- Tipado para a classe específica
- Zero configuração

### ✅ Scoped Automático
- Dependências são scoped por request HTTP
- Dispose automático
- Thread-safe

### ✅ Flexibilidade
- Pode misturar com registros manuais
- Funciona com qualquer implementação
- Compatível com ASP.NET Core DI

## 🔧 Logs de Exemplo

```
info: MyApp.Services.UsuarioService[0]
      Iniciando criação de usuário: João Silva - joao@email.com

info: MyApp.Services.UsuarioService[0]
      Usuário criado com sucesso: ID 1

📧 Email de boas-vindas enviado para joao@email.com
   Assunto: Bem-vindo, João Silva!
   Mensagem: Sua conta foi criada com sucesso.

info: MyApp.Services.UsuarioService[0]
      Email de boas-vindas enviado para joao@email.com
```

## 🎉 Resultado

Com **apenas 3 linhas de configuração** no `Program.cs`, você tem:

- ✅ Injeção de dependência automática
- ✅ Descoberta de implementações
- ✅ Logging configurado
- ✅ Scoping por request
- ✅ Performance otimizada

**Foque no que importa: sua lógica de negócio!** 🚀