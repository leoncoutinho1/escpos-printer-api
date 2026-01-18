# ESC/POS Printer API - .NET Edition

API desenvolvida em .NET com Minimal API e Windows Forms para impressão em impressoras ESC/POS.

## 🏗️ Arquitetura

O projeto é dividido em 3 componentes:

- **EscPosPrinterApi.Core**: Biblioteca com modelos e serviços compartilhados
- **EscPosPrinterApi.Api**: API Minimal .NET que recebe requisições HTTP
- **EscPosPrinterApi.UI**: Interface Windows Forms para seleção de impressora

## 🚀 Como Funciona

1. A API recebe uma requisição POST com um byte array (dados ESC/POS)
2. Os dados são salvos em um arquivo temporário
3. A API inicia o Windows Forms passando o caminho do arquivo
4. O usuário seleciona a impressora desejada na interface gráfica
5. Ao confirmar, os dados são enviados para a impressora
6. O resultado é retornado para a API
7. A API responde ao cliente com o status da impressão

## 📋 Pré-requisitos

- .NET 10.0 SDK ou superior
- Windows (devido ao uso de Windows Forms e API winspool.drv)
- Impressora ESC/POS instalada no sistema

## 🔧 Instalação e Execução

### 1. Compilar os projetos

```bash
# Compilar toda a solução
dotnet build

# Ou compilar individualmente
dotnet build EscPosPrinterApi.Core/EscPosPrinterApi.Core.csproj
dotnet build EscPosPrinterApi.UI/EscPosPrinterApi.UI.csproj
dotnet build EscPosPrinterApi.Api/EscPosPrinterApi.Api.csproj
```

### 2. Executar a API

```bash
cd EscPosPrinterApi.Api
dotnet run
```

A API estará disponível em: `http://localhost:3031`

## 📡 Endpoints da API

### GET /api/printers
Lista todas as impressoras instaladas no sistema.

**Resposta:**
```json
[
  {
    "name": "EPSON TM-T20",
    "isDefault": true,
    "status": "Disponível"
  }
]
```

### POST /api/print
Envia dados para impressão (abre interface gráfica para seleção).

**Requisição (Array de Números):**
```json
{
  "data": [27, 64, 27, 97, 1, ...],
  "jobName": "Cupom Fiscal"
}
```

**Requisição (Base64):**
```json
{
  "data": "G0BIZWxsbw==",
  "jobName": "Cupom Fiscal"
}
```

**Requisição com Impressora Padrão (sem modal):**
```json
{
  "data": [27, 64, 27, 97, 1, ...],
  "jobName": "Cupom Fiscal",
  "defaultPrinter": true
}
```

**Nota**: 
- O campo `data` aceita tanto um array de números quanto uma string Base64.
- O campo `defaultPrinter` (opcional, padrão: `false`) quando definido como `true`, envia a impressão diretamente para a impressora padrão do sistema sem exibir o modal de seleção.

**Resposta (Sucesso):**
```json
{
  "success": true,
  "message": "Impressão enviada com sucesso para EPSON TM-T20",
  "printerName": "EPSON TM-T20",
  "cancelled": false
}
```

**Resposta (Cancelado):**
```json
{
  "success": false,
  "message": "Operação cancelada pelo usuário",
  "printerName": null,
  "cancelled": true
}
```

### GET /health
Verifica se a API está funcionando.

**Resposta:**
```json
{
  "status": "healthy",
  "timestamp": "2026-01-13T14:30:00Z"
}
```

## 🧪 Testando a API

### Usando cURL

```bash
# Listar impressoras
curl http://localhost:3031/api/printers

# Enviar impressão (exemplo com comandos ESC/POS básicos)
curl -X POST http://localhost:3031/api/print \
  -H "Content-Type: application/json" \
  -d "{\"data\": [27, 64, 72, 101, 108, 108, 111, 10, 10, 10, 27, 105]}"
```

### Usando Swagger

Acesse `http://localhost:3031/swagger` para testar os endpoints interativamente.

## 📦 Estrutura de Dados ESC/POS

O byte array deve conter comandos ESC/POS válidos. Exemplos:

```csharp
// Inicializar impressora
byte[] init = { 0x1B, 0x40 };

// Texto centralizado
byte[] center = { 0x1B, 0x61, 0x01 };

// Cortar papel
byte[] cut = { 0x1B, 0x69 };

// Exemplo completo
byte[] receipt = {
    0x1B, 0x40,           // Inicializar
    0x1B, 0x61, 0x01,     // Centralizar
    0x48, 0x65, 0x6C, 0x6C, 0x6F,  // "Hello"
    0x0A, 0x0A, 0x0A,     // 3 quebras de linha
    0x1B, 0x69            // Cortar papel
};
```

## 🔒 Segurança

⚠️ **IMPORTANTE**: Esta API foi desenvolvida para uso local. Não exponha publicamente sem implementar:

- Autenticação/Autorização
- Rate limiting
- Validação rigorosa de entrada
- HTTPS

## 🛠️ Desenvolvimento

### Estrutura do Projeto

```
escpos-printer-api/
├── EscPosPrinterApi.sln
├── EscPosPrinterApi.Core/
│   ├── Models/
│   │   ├── PrintRequest.cs
│   │   ├── PrintResponse.cs
│   │   └── PrinterInfo.cs
│   └── Services/
│       ├── IPrinterService.cs
│       └── PrinterService.cs
├── EscPosPrinterApi.Api/
│   ├── Program.cs
│   └── appsettings.json
└── EscPosPrinterApi.UI/
    ├── Program.cs
    ├── PrinterSelectionForm.cs
    └── PrinterSelectionForm.Designer.cs
```

### Tecnologias Utilizadas

- **.NET 10.0**: Framework principal
- **ASP.NET Core Minimal API**: API HTTP
- **Windows Forms**: Interface gráfica
- **Windows API (winspool.drv)**: Comunicação com impressoras

## 📝 Licença

Este projeto é de código aberto e está disponível sob a licença MIT.

## 🤝 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues ou pull requests.

## 📞 Suporte

Para problemas ou dúvidas, abra uma issue no repositório.
