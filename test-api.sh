#!/bin/bash

echo "=== Testando ESC/POS Printer API ==="
echo ""

API_URL="http://localhost:3031"

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Teste 1: Health Check
echo -e "${CYAN}1. Testando Health Check...${NC}"
response=$(curl -s -w "\n%{http_code}" "$API_URL/health")
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

if [ "$http_code" = "200" ]; then
    echo -e "${GREEN}✓ API está saudável${NC}"
    echo "$body" | jq '.'
else
    echo -e "${RED}✗ Falha no health check (HTTP $http_code)${NC}"
fi

echo ""

# Teste 2: Listar Impressoras
echo -e "${CYAN}2. Listando impressoras...${NC}"
response=$(curl -s -w "\n%{http_code}" "$API_URL/api/printers")
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

if [ "$http_code" = "200" ]; then
    echo -e "${GREEN}✓ Impressoras listadas com sucesso${NC}"
    echo "$body" | jq '.'
else
    echo -e "${RED}✗ Falha ao listar impressoras (HTTP $http_code)${NC}"
fi

echo ""

# Teste 3: Impressão com Array de Números
echo -e "${CYAN}3. Testando impressão (Array de Números)...${NC}"
response=$(curl -s -w "\n%{http_code}" -X POST "$API_URL/api/print" \
  -H "Content-Type: application/json" \
  -d '{
    "data": [27, 64, 27, 97, 1, 72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100, 33, 10, 10, 10, 27, 105],
    "jobName": "Bash Test - Array"
  }')

http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

if [ "$http_code" = "200" ]; then
    echo -e "${GREEN}✓ Impressão enviada com sucesso${NC}"
    echo "$body" | jq '.'
elif [ "$http_code" = "400" ]; then
    echo -e "${YELLOW}⚠ Impressão cancelada ou falhou${NC}"
    echo "$body" | jq '.'
else
    echo -e "${RED}✗ Erro ao imprimir (HTTP $http_code)${NC}"
    echo "$body"
fi

echo ""

# Teste 4: Impressão com Base64
echo -e "${CYAN}4. Testando impressão (Base64)...${NC}"
# Base64 de: ESC @ (inicializar) + "Hello" + LF + LF + LF + ESC i (cortar)
base64_data="G0BIZWxsbwoKChtp"

response=$(curl -s -w "\n%{http_code}" -X POST "$API_URL/api/print" \
  -H "Content-Type: application/json" \
  -d "{
    \"data\": \"$base64_data\",
    \"jobName\": \"Bash Test - Base64\"
  }")

http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | sed '$d')

if [ "$http_code" = "200" ]; then
    echo -e "${GREEN}✓ Impressão Base64 enviada com sucesso${NC}"
    echo "$body" | jq '.'
elif [ "$http_code" = "400" ]; then
    echo -e "${YELLOW}⚠ Impressão cancelada ou falhou${NC}"
    echo "$body" | jq '.'
else
    echo -e "${RED}✗ Erro ao imprimir (HTTP $http_code)${NC}"
    echo "$body"
fi

echo ""
echo -e "${CYAN}=== Testes concluídos ===${NC}"
