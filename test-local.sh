#!/bin/bash

echo "🔧 Preparando AutoInject para teste local..."

# Navegar para o projeto AutoInject
cd AutoInject

# Limpar builds anteriores
echo "🧹 Limpando builds anteriores..."
dotnet clean

# Build em Release
echo "🏗️ Fazendo build Release..."
dotnet build -c Release

if [ $? -ne 0 ]; then
    echo "❌ Erro no build!"
    exit 1
fi

# Gerar pacote local
echo "📦 Gerando pacote local..."
dotnet pack -c Release --no-build

if [ $? -ne 0 ]; then
    echo "❌ Erro ao gerar pacote!"
    exit 1
fi

# Criar pasta para pacotes locais se não existir
LOCAL_PACKAGES="../LocalPackages"
mkdir -p "$LOCAL_PACKAGES"

# Copiar pacote para pasta local
echo "📁 Copiando pacote para pasta local..."
cp "bin/Release/JZen.AutoInject.1.1.0.nupkg" "$LOCAL_PACKAGES/"

# Voltar para a raiz
cd ..

# Criar nuget.config se não existir
if [ ! -f "nuget.config" ]; then
    echo "⚙️ Criando nuget.config..."
    cat > nuget.config << EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="Local" value="./LocalPackages" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
EOF
fi

echo ""
echo "✅ AutoInject preparado para teste local!"
echo ""
echo "📋 Para usar no seu projeto:"
echo "   1. dotnet add package JZen.AutoInject --version 1.1.0"
echo "   2. Ou adicione ao .csproj:"
echo "      <PackageReference Include=\"JZen.AutoInject\" Version=\"1.1.0\" />"
echo ""
echo "🔧 Para testar as novas funcionalidades:"
echo "   - services.AddInjectBaseClasses();"
echo "   - services.AddInjectBaseClassesDebug();"
echo "   - services.AddInjectBaseClassesAdvanced();"
echo ""
echo "📁 Pacote local em: ./LocalPackages/JZen.AutoInject.1.1.0.nupkg"