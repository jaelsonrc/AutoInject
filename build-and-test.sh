#!/bin/bash

echo "🔨 Testando build do AutoInject v1.1.0"

echo "🧹 Limpando builds anteriores..."
cd AutoInject
dotnet clean

echo "🏗️ Construindo em modo Release..."
dotnet build -c Release

if [ $? -eq 0 ]; then
    echo "✅ Build bem-sucedido!"
    
    echo "📦 Gerando pacote NuGet..."
    dotnet pack -c Release --no-build
    
    if [ $? -eq 0 ]; then
        echo "✅ Pacote gerado com sucesso!"
        echo "📁 Arquivo gerado: bin/Release/JZen.AutoInject.1.1.0.nupkg"
        
        # Listar arquivos gerados
        echo "📋 Arquivos no diretório Release:"
        ls -la bin/Release/
        
        echo ""
        echo "🎯 Pronto para publicação!"
        echo "Execute: bash ../publish-release.sh"
    else
        echo "❌ Erro ao gerar pacote NuGet"
        exit 1
    fi
else
    echo "❌ Erro no build"
    exit 1
fi