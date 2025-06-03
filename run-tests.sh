#!/bin/bash

echo "🧪 Executando testes do AutoInject"

echo "🔍 Testando projeto principal..."
cd AutoInject
dotnet build

echo "🔍 Testando projeto de testes..."
cd ../AutoInject.Tests
dotnet build

echo "🚀 Executando todos os testes..."
dotnet test --verbosity normal

if [ $? -eq 0 ]; then
    echo "✅ Todos os testes passaram!"
    echo "🎯 Projeto pronto para publicação!"
else
    echo "❌ Alguns testes falharam"
    echo "🔧 Verifique os erros antes de publicar"
    exit 1
fi