# Arquitetura do Sistema - ESC/POS Printer API

## 📐 Visão Geral

Este documento descreve a arquitetura da solução ESC/POS Printer API, desenvolvida em .NET com Minimal API e Windows Forms.

## 🏛️ Arquitetura de Alto Nível

```
┌─────────────────┐
│   Cliente HTTP  │
│  (Browser/App)  │
└────────┬────────┘
         │ HTTP POST /api/print
         │ { data: byte[], jobName: string }
         ▼
┌─────────────────────────────────┐
│   EscPosPrinterApi.Api          │
│   (ASP.NET Core Minimal API)    │
│                                 │
│  • Recebe requisição HTTP       │
│  • Valida dados                 │
│  • Salva byte[] em arquivo temp │
│  • Inicia processo Windows Forms│
│  • Aguarda resposta             │
│  • Retorna resultado ao cliente │
└────────┬────────────────────────┘
         │ Process.Start()
         │ Passa caminho do arquivo temp
         ▼
┌─────────────────────────────────┐
│   EscPosPrinterApi.UI           │
│   (Windows Forms Application)   │
│                                 │
│  • Lê byte[] do arquivo temp    │
│  • Lista impressoras instaladas │
│  • Exibe interface gráfica      │
│  • Aguarda seleção do usuário   │
│  • Envia dados para impressora  │
│  • Salva resultado em JSON      │
│  • Retorna caminho via stdout   │
└────────┬────────────────────────┘
         │ Usa PrinterService
         ▼
┌─────────────────────────────────┐
│   EscPosPrinterApi.Core         │
│   (Class Library)               │
│                                 │
│  • Models (DTOs)                │
│  • Services (Business Logic)    │
│  • PrinterService (Windows API) │
└────────┬────────────────────────┘
         │ P/Invoke winspool.drv
         ▼
┌─────────────────────────────────┐
│   Windows Spooler Service       │
│   (winspool.drv)                │
│                                 │
│  • Gerencia fila de impressão   │
│  • Comunica com drivers         │
└────────┬────────────────────────┘
         │
         ▼
┌─────────────────────────────────┐
│   Impressora ESC/POS            │
│   (Hardware)                    │
└─────────────────────────────────┘
```

## 🔄 Fluxo de Dados Detalhado

### 1. Requisição de Impressão

```
Cliente → API
POST /api/print
Content-Type: application/json

{
  "data": [27, 64, 72, 101, 108, 108, 111, ...],
  "jobName": "Cupom Fiscal"
}
```

### 2. Processamento na API

```csharp
// 1. Validação
if (request.Data == null || request.Data.Length == 0)
    return BadRequest();

// 2. Persistência temporária
string tempFile = Path.GetTempPath() + Guid.NewGuid() + ".bin";
await File.WriteAllBytesAsync(tempFile, request.Data);

// 3. Iniciar UI
ProcessStartInfo psi = new ProcessStartInfo {
    FileName = "EscPosPrinterApi.UI.exe",
    Arguments = $"\"{tempFile}\""
};
var process = Process.Start(psi);

// 4. Aguardar conclusão
await process.WaitForExitAsync();

// 5. Ler resultado
string resultPath = await process.StandardOutput.ReadToEndAsync();
string resultJson = await File.ReadAllTextAsync(resultPath);
var response = JsonSerializer.Deserialize<PrintResponse>(resultJson);

// 6. Retornar ao cliente
return response.Success ? Ok(response) : BadRequest(response);
```

### 3. Processamento no Windows Forms

```csharp
// 1. Leitura dos dados
string tempFile = args[0];
byte[] printData = File.ReadAllBytes(tempFile);

// 2. Carregar impressoras
var printers = await printerService.GetPrintersAsync();

// 3. Exibir interface
using var form = new PrinterSelectionForm(printerService, printData);
var result = form.ShowDialog();

// 4. Processar seleção
if (result == DialogResult.OK) {
    await printerService.PrintAsync(selectedPrinter, printData);
}

// 5. Salvar resultado
string resultFile = Path.GetTempPath() + Guid.NewGuid() + ".json";
File.WriteAllText(resultFile, JsonSerializer.Serialize(response));

// 6. Retornar caminho via stdout
Console.WriteLine(resultFile);
```

### 4. Impressão via Windows API

```csharp
// 1. Abrir impressora
OpenPrinter(printerName, out IntPtr hPrinter, IntPtr.Zero);

// 2. Iniciar documento
DOC_INFO_1 docInfo = new DOC_INFO_1 {
    pDocName = "ESC/POS Print Job",
    pDataType = "RAW"
};
StartDocPrinter(hPrinter, 1, ref docInfo);

// 3. Iniciar página
StartPagePrinter(hPrinter);

// 4. Enviar dados brutos
IntPtr pBytes = Marshal.AllocCoTaskMem(data.Length);
Marshal.Copy(data, 0, pBytes, data.Length);
WritePrinter(hPrinter, pBytes, data.Length, out int written);

// 5. Finalizar
EndPagePrinter(hPrinter);
EndDocPrinter(hPrinter);
ClosePrinter(hPrinter);
```

## 🗂️ Estrutura de Projetos

### EscPosPrinterApi.Core (Class Library)

**Responsabilidade**: Lógica de negócio compartilhada

```
EscPosPrinterApi.Core/
├── Models/
│   ├── PrintRequest.cs      # DTO para requisição
│   ├── PrintResponse.cs     # DTO para resposta
│   └── PrinterInfo.cs       # Informações da impressora
└── Services/
    ├── IPrinterService.cs   # Interface do serviço
    └── PrinterService.cs    # Implementação com Windows API
```

**Dependências**:
- `System.Drawing.Common` (para PrinterSettings)

### EscPosPrinterApi.Api (ASP.NET Core Web API)

**Responsabilidade**: Expor endpoints HTTP

```
EscPosPrinterApi.Api/
├── Program.cs               # Minimal API endpoints
├── appsettings.json         # Configurações (porta 3031)
└── EscPosPrinterApi.Api.csproj
```

**Dependências**:
- `Microsoft.AspNetCore.OpenApi`
- `Swashbuckle.AspNetCore`
- `EscPosPrinterApi.Core` (project reference)

**Endpoints**:
- `GET /api/printers` - Lista impressoras
- `POST /api/print` - Envia impressão
- `GET /health` - Health check

### EscPosPrinterApi.UI (Windows Forms)

**Responsabilidade**: Interface gráfica para seleção

```
EscPosPrinterApi.UI/
├── Program.cs                        # Entry point
├── PrinterSelectionForm.cs           # Lógica do formulário
├── PrinterSelectionForm.Designer.cs  # Layout do formulário
└── EscPosPrinterApi.UI.csproj
```

**Dependências**:
- `EscPosPrinterApi.Core` (project reference)

**Características**:
- Target Framework: `net10.0-windows`
- UseWindowsForms: `true`
- Modo: STAThread

## 🔐 Comunicação Inter-Processo (IPC)

A comunicação entre a API e o Windows Forms é feita através de:

1. **Arquivos Temporários**: Dados de entrada (byte array)
2. **Argumentos de Linha de Comando**: Caminho do arquivo de entrada
3. **Standard Output**: Caminho do arquivo de resultado
4. **Arquivos JSON**: Resultado da operação

### Vantagens desta abordagem:

✅ **Isolamento**: API e UI rodam em processos separados  
✅ **Simplicidade**: Não requer IPC complexo (pipes, sockets)  
✅ **Debugging**: Fácil de debugar e testar separadamente  
✅ **Segurança**: Arquivos temporários são limpos após uso  

### Desvantagens:

⚠️ **I/O Disk**: Operações de leitura/escrita em disco  
⚠️ **Latência**: Tempo de inicialização do processo  
⚠️ **Concorrência**: Múltiplas requisições = múltiplos processos  

## 🎯 Decisões de Design

### Por que Minimal API?

- ✅ Menos boilerplate
- ✅ Performance superior
- ✅ Código mais limpo e direto
- ✅ Ideal para APIs simples

### Por que Windows Forms?

- ✅ Nativo do Windows
- ✅ Fácil de criar interfaces
- ✅ Suporte completo a P/Invoke
- ✅ Não requer runtime adicional (WPF, etc)

### Por que separar em 3 projetos?

- ✅ **Separation of Concerns**: Cada projeto tem uma responsabilidade
- ✅ **Reusabilidade**: Core pode ser usado em outros projetos
- ✅ **Testabilidade**: Fácil de testar isoladamente
- ✅ **Manutenibilidade**: Mudanças em uma camada não afetam outras

## 🔧 Tecnologias e Padrões

### Tecnologias

- **.NET 10.0**: Framework principal
- **ASP.NET Core**: Web API
- **Windows Forms**: UI Desktop
- **P/Invoke**: Interop com Windows API
- **System.Drawing.Common**: Acesso a PrinterSettings

### Padrões de Projeto

- **Dependency Injection**: Injeção de serviços na API
- **Repository Pattern**: PrinterService abstrai acesso a impressoras
- **DTO Pattern**: Models para transferência de dados
- **Facade Pattern**: PrinterService simplifica Windows API

### Princípios SOLID

- **S**ingle Responsibility: Cada classe tem uma responsabilidade
- **O**pen/Closed: Extensível via interfaces
- **L**iskov Substitution: IPrinterService pode ter múltiplas implementações
- **I**nterface Segregation: Interfaces pequenas e focadas
- **D**ependency Inversion: Dependência de abstrações (IPrinterService)

## 📊 Diagrama de Sequência

```
Cliente          API              UI              PrinterService    Windows
  │               │                │                     │              │
  ├─POST /print──>│                │                     │              │
  │               ├─Save temp file─┤                     │              │
  │               ├─Start process──>│                     │              │
  │               │                ├─Read temp file──────┤              │
  │               │                ├─GetPrinters()──────>│              │
  │               │                │                     ├─Query────────>│
  │               │                │                     <─Printers─────┤
  │               │                <─Show UI────────────┤              │
  │               │                │ [User selects]      │              │
  │               │                ├─PrintAsync()───────>│              │
  │               │                │                     ├─OpenPrinter──>│
  │               │                │                     ├─WritePrinter─>│
  │               │                │                     <─Success──────┤
  │               │                <─Result─────────────┤              │
  │               │                ├─Save result JSON───┤              │
  │               │                ├─Output path────────>│              │
  │               <─Process exit───┤                     │              │
  │               ├─Read result────┤                     │              │
  <─Response─────┤                │                     │              │
```

## 🚀 Escalabilidade e Performance

### Limitações Atuais

- ⚠️ Processo síncrono (bloqueia até usuário responder)
- ⚠️ Um processo UI por requisição
- ⚠️ Sem fila de impressão
- ⚠️ Sem cache de impressoras

### Melhorias Futuras

1. **Fila de Impressão**: Implementar sistema de filas (RabbitMQ, Redis)
2. **Pool de Processos**: Reutilizar processos UI
3. **Cache**: Cachear lista de impressoras
4. **Async/Await**: Melhorar uso de async em toda a stack
5. **SignalR**: Notificações em tempo real para o cliente

## 🔒 Segurança

### Implementado

- ✅ Validação de entrada (byte array não vazio)
- ✅ Limpeza de arquivos temporários
- ✅ CORS configurável

### Recomendações para Produção

- 🔐 Implementar autenticação (JWT, API Key)
- 🔐 Rate limiting
- 🔐 Validação rigorosa de byte array (tamanho máximo)
- 🔐 HTTPS obrigatório
- 🔐 Sanitização de nomes de impressoras
- 🔐 Logs de auditoria

## 📝 Conclusão

Esta arquitetura fornece uma solução robusta e extensível para impressão ESC/POS via API HTTP, mantendo a flexibilidade de permitir que o usuário selecione a impressora através de uma interface gráfica intuitiva.
