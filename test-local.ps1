# Script PowerShell para preparar AutoInject para teste local

Write-Host "🔧 Preparando AutoInject para teste local..." -ForegroundColor Green

# Navegar para o projeto AutoInject
Set-Location "AutoInject"

# Limpar builds anteriores
Write-Host "🧹 Limpando builds anteriores..." -ForegroundColor Yellow
dotnet clean | Out-Null

# Build em Release
Write-Host "🏗️ Fazendo build Release..." -ForegroundColor Yellow
$buildResult = dotnet build -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro no build!" -ForegroundColor Red
    exit 1
}

# Gerar pacote local
Write-Host "📦 Gerando pacote local..." -ForegroundColor Yellow
$packResult = dotnet pack -c Release --no-build

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro ao gerar pacote!" -ForegroundColor Red
    exit 1
}

# Criar pasta para pacotes locais se não existir
$localPackages = "../LocalPackages"
if (!(Test-Path $localPackages)) {
    New-Item -ItemType Directory -Path $localPackages | Out-Null
}

# Copiar pacote para pasta local
Write-Host "📁 Copiando pacote para pasta local..." -ForegroundColor Yellow
Copy-Item "bin/Release/JZen.AutoInject.1.1.0.nupkg" $localPackages

# Voltar para a raiz
Set-Location ".."

# Criar nuget.config se não existir
if (!(Test-Path "nuget.config")) {
    Write-Host "⚙️ Criando nuget.config..." -ForegroundColor Yellow
    $nugetConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="Local" value="./LocalPackages" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@
    $nugetConfig | Out-File -FilePath "nuget.config" -Encoding UTF8
}

Write-Host ""
Write-Host "✅ AutoInject preparado para teste local!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Para usar no seu projeto:" -ForegroundColor Cyan
Write-Host "   1. dotnet add package JZen.AutoInject --version 1.1.0" -ForegroundColor White
Write-Host "   2. Ou adicione ao .csproj:" -ForegroundColor White
Write-Host "      <PackageReference Include=`"JZen.AutoInject`" Version=`"1.1.0`" />" -ForegroundColor Gray
Write-Host ""
Write-Host "🔧 Para testar as novas funcionalidades:" -ForegroundColor Cyan
Write-Host "   - services.AddInjectBaseClasses();" -ForegroundColor White
Write-Host "   - services.AddInjectBaseClassesDebug();" -ForegroundColor White
Write-Host "   - services.AddInjectBaseClassesAdvanced();" -ForegroundColor White
Write-Host ""
Write-Host "📁 Pacote local em: ./LocalPackages/JZen.AutoInject.1.1.0.nupkg" -ForegroundColor Gray