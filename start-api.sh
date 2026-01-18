#!/bin/bash

echo "=== ESC/POS Printer API - Startup Script ==="
echo ""

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Verifica se o .NET está instalado
if ! command -v dotnet &> /dev/null; then
    echo -e "${YELLOW}⚠ .NET SDK não encontrado${NC}"
    echo "Por favor, instale o .NET SDK 10.0 ou superior"
    exit 1
fi

echo -e "${GREEN}✓ .NET SDK encontrado:${NC} $(dotnet --version)"
echo ""

# Compila a solução
echo -e "${CYAN}Compilando a solução...${NC}"
dotnet build EscPosPrinterApi.sln

if [ $? -ne 0 ]; then
    echo -e "${YELLOW}⚠ Erro na compilação${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}✓ Compilação concluída com sucesso${NC}"
echo ""

# Inicia a API
echo -e "${CYAN}Iniciando a API...${NC}"
echo "URL: http://localhost:3031"
echo "Swagger: http://localhost:3031/swagger"
echo ""
echo "Pressione Ctrl+C para parar a API"
echo ""

cd EscPosPrinterApi.Api
dotnet run
