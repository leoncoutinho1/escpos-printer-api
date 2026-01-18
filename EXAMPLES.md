# Exemplos de Uso da API

Este documento contém exemplos de como consumir a API ESC/POS Printer em diferentes linguagens e ferramentas.

## 📋 Índice

- [JavaScript/TypeScript](#javascripttypescript)
- [C#](#c)
- [Python](#python)
- [cURL](#curl)
- [PowerShell](#powershell)
- [Postman](#postman)

---

## JavaScript/TypeScript

### Usando Fetch API

```javascript
// Listar impressoras
async function getPrinters() {
  const response = await fetch('http://localhost:3031/api/printers');
  const printers = await response.json();
  console.log('Impressoras disponíveis:', printers);
  return printers;
}

// Enviar impressão
async function printReceipt(data) {
  const printRequest = {
    data: data, // Array de bytes
    jobName: 'Cupom Fiscal'
  };

  const response = await fetch('http://localhost:3031/api/print', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(printRequest)
  });

  const result = await response.json();
  
  if (result.success) {
    console.log(`✓ Impresso em: ${result.printerName}`);
  } else if (result.cancelled) {
    console.log('⚠ Impressão cancelada pelo usuário');
  } else {
    console.error(`✗ Erro: ${result.message}`);
  }
  
  return result;
}

// Exemplo de uso
const escPosData = [
  27, 64,           // ESC @ - Inicializar
  27, 97, 1,        // ESC a 1 - Centralizar
  72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100, 33, // "Hello World!"
  10, 10, 10,       // 3 quebras de linha
  27, 105           // ESC i - Cortar papel
];

printReceipt(escPosData);

// Imprimir diretamente na impressora padrão (sem modal de seleção)
async function printToDefaultPrinter(data) {
  const printRequest = {
    data: data,
    jobName: 'Cupom Fiscal',
    defaultPrinter: true  // Usa a impressora padrão sem perguntar
  };

  const response = await fetch('http://localhost:3031/api/print', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(printRequest)
  });

  const result = await response.json();
  
  if (result.success) {
    console.log(`✓ Impresso automaticamente em: ${result.printerName}`);
  } else {
    console.error(`✗ Erro: ${result.message}`);
  }
  
  return result;
}

// Exemplo: imprimir sem perguntar ao usuário
printToDefaultPrinter(escPosData);
```

### Usando Axios

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:3031/api'
});

// Listar impressoras
async function getPrinters() {
  try {
    const { data } = await api.get('/printers');
    return data;
  } catch (error) {
    console.error('Erro ao listar impressoras:', error.message);
    throw error;
  }
}

// Enviar impressão
async function print(byteArray, jobName = 'Print Job') {
  try {
    const { data } = await api.post('/print', {
      data: byteArray,
      jobName: jobName
    });
    return data;
  } catch (error) {
    console.error('Erro ao imprimir:', error.message);
    throw error;
  }
}
```

### TypeScript com Tipos

```typescript
interface PrinterInfo {
  name: string;
  isDefault: boolean;
  status: string;
}

interface PrintRequest {
  data: number[];
  jobName?: string;
  defaultPrinter?: boolean;
}

interface PrintResponse {
  success: boolean;
  message: string;
  printerName?: string;
  cancelled: boolean;
}

class PrinterApiClient {
  private baseUrl: string;

  constructor(baseUrl: string = 'http://localhost:3031/api') {
    this.baseUrl = baseUrl;
  }

  async getPrinters(): Promise<PrinterInfo[]> {
    const response = await fetch(`${this.baseUrl}/printers`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return await response.json();
  }

  async print(data: number[], jobName?: string): Promise<PrintResponse> {
    const request: PrintRequest = { data, jobName };
    
    const response = await fetch(`${this.baseUrl}/print`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return await response.json();
  }

  async printToDefault(data: number[], jobName?: string): Promise<PrintResponse> {
    const request: PrintRequest = { data, jobName, defaultPrinter: true };
    
    const response = await fetch(`${this.baseUrl}/print`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return await response.json();
  }
}

// Uso
const client = new PrinterApiClient();
const printers = await client.getPrinters();

// Imprimir com seleção de impressora
const result = await client.print([27, 64, 72, 101, 108, 108, 111]);

// Imprimir diretamente na impressora padrão
const resultDefault = await client.printToDefault([27, 64, 72, 101, 108, 108, 111], 'Auto Print');
```

---

## C#

### Usando HttpClient

```csharp
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

public class PrinterApiClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:3031/api";

    public PrinterApiClient()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    // Listar impressoras
    public async Task<List<PrinterInfo>> GetPrintersAsync()
    {
        var response = await _httpClient.GetAsync("/printers");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<PrinterInfo>>();
    }

    // Enviar impressão
    public async Task<PrintResponse> PrintAsync(byte[] data, string jobName = null)
    {
        var request = new PrintRequest
        {
            Data = data,
            JobName = jobName
        };

        var response = await _httpClient.PostAsJsonAsync("/print", request);
        return await response.Content.ReadFromJsonAsync<PrintResponse>();
    }

    // Enviar impressão para impressora padrão (sem modal)
    public async Task<PrintResponse> PrintToDefaultAsync(byte[] data, string jobName = null)
    {
        var request = new PrintRequest
        {
            Data = data,
            JobName = jobName,
            DefaultPrinter = true
        };

        var response = await _httpClient.PostAsJsonAsync("/print", request);
        return await response.Content.ReadFromJsonAsync<PrintResponse>();
    }
}

// Models
public class PrinterInfo
{
    public string Name { get; set; }
    public bool IsDefault { get; set; }
    public string Status { get; set; }
}

public class PrintRequest
{
    public byte[] Data { get; set; }
    public string JobName { get; set; }
    public bool DefaultPrinter { get; set; } = false;
}

public class PrintResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string PrinterName { get; set; }
    public bool Cancelled { get; set; }
}

// Exemplo de uso
var client = new PrinterApiClient();

// Listar impressoras
var printers = await client.GetPrintersAsync();
foreach (var printer in printers)
{
    Console.WriteLine($"{printer.Name} - {printer.Status}");
}

// Imprimir
byte[] escPosData = { 0x1B, 0x40, 0x48, 0x65, 0x6C, 0x6C, 0x6F };
var result = await client.PrintAsync(escPosData, "Test Print");

if (result.Success)
{
    Console.WriteLine($"✓ Impresso em: {result.PrinterName}");
}

// Imprimir diretamente na impressora padrão (sem modal)
var resultDefault = await client.PrintToDefaultAsync(escPosData, "Auto Print");

if (resultDefault.Success)
{
    Console.WriteLine($"✓ Impresso automaticamente em: {resultDefault.PrinterName}");
}
```

---

## Python

### Usando requests

```python
import requests
import json

class PrinterApiClient:
    def __init__(self, base_url='http://localhost:3031/api'):
        self.base_url = base_url
    
    def get_printers(self):
        """Lista todas as impressoras disponíveis"""
        response = requests.get(f'{self.base_url}/printers')
        response.raise_for_status()
        return response.json()
    
    def print_data(self, data, job_name=None):
        """Envia dados para impressão"""
        payload = {
            'data': data,
            'jobName': job_name
        }
        
        response = requests.post(
            f'{self.base_url}/print',
            json=payload,
            headers={'Content-Type': 'application/json'}
        )
        
        return response.json()

# Exemplo de uso
client = PrinterApiClient()

# Listar impressoras
printers = client.get_printers()
for printer in printers:
    default = " (Padrão)" if printer['isDefault'] else ""
    print(f"• {printer['name']}{default}")

# Imprimir
esc_pos_data = [
    27, 64,           # ESC @ - Inicializar
    27, 97, 1,        # ESC a 1 - Centralizar
    72, 101, 108, 108, 111,  # "Hello"
    10, 10, 10,       # 3 quebras de linha
    27, 105           # ESC i - Cortar papel
]

result = client.print_data(esc_pos_data, "Python Print Job")

if result['success']:
    print(f"✓ Impresso em: {result['printerName']}")
elif result['cancelled']:
    print("⚠ Impressão cancelada pelo usuário")
else:
    print(f"✗ Erro: {result['message']}")
```

### Usando aiohttp (async)

```python
import aiohttp
import asyncio

async def get_printers():
    async with aiohttp.ClientSession() as session:
        async with session.get('http://localhost:3031/api/printers') as response:
            return await response.json()

async def print_data(data, job_name=None):
    payload = {'data': data, 'jobName': job_name}
    
    async with aiohttp.ClientSession() as session:
        async with session.post(
            'http://localhost:3031/api/print',
            json=payload
        ) as response:
            return await response.json()

# Uso
async def main():
    printers = await get_printers()
    print(f"Encontradas {len(printers)} impressoras")
    
    result = await print_data([27, 64, 72, 101, 108, 108, 111])
    print(f"Resultado: {result['message']}")

asyncio.run(main())
```

---

## cURL

### Listar Impressoras

```bash
curl -X GET http://localhost:3031/api/printers \
  -H "Accept: application/json"
```

### Enviar Impressão

```bash
curl -X POST http://localhost:3031/api/print \
  -H "Content-Type: application/json" \
  -d '{
    "data": [27, 64, 72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100, 33, 10, 10, 10, 27, 105],
    "jobName": "cURL Test Print"
  }'
```

### Enviar Impressão para Impressora Padrão

```bash
curl -X POST http://localhost:3031/api/print \
  -H "Content-Type: application/json" \
  -d '{
    "data": [27, 64, 72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100, 33, 10, 10, 10, 27, 105],
    "jobName": "cURL Auto Print",
    "defaultPrinter": true
  }'
```

### Health Check

```bash
curl -X GET http://localhost:3031/health
```

---

## PowerShell

### Listar Impressoras

```powershell
$response = Invoke-RestMethod -Uri "http://localhost:3031/api/printers" -Method Get
$response | Format-Table Name, IsDefault, Status
```

### Enviar Impressão

```powershell
$printData = @{
    data = @(27, 64, 72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100, 33, 10, 10, 10, 27, 105)
    jobName = "PowerShell Test"
}

$json = $printData | ConvertTo-Json
$response = Invoke-RestMethod -Uri "http://localhost:3031/api/print" -Method Post -Body $json -ContentType "application/json"

if ($response.success) {
    Write-Host "✓ Impresso em: $($response.printerName)" -ForegroundColor Green
} else {
    Write-Host "✗ Erro: $($response.message)" -ForegroundColor Red
}
```

### Enviar Impressão para Impressora Padrão

```powershell
$printData = @{
    data = @(27, 64, 72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100, 33, 10, 10, 10, 27, 105)
    jobName = "PowerShell Auto Print"
    defaultPrinter = $true
}

$json = $printData | ConvertTo-Json
$response = Invoke-RestMethod -Uri "http://localhost:3031/api/print" -Method Post -Body $json -ContentType "application/json"

if ($response.success) {
    Write-Host "✓ Impresso automaticamente em: $($response.printerName)" -ForegroundColor Green
} else {
    Write-Host "✗ Erro: $($response.message)" -ForegroundColor Red
}
```

### Função Reutilizável

```powershell
function Invoke-EscPosPrint {
    param(
        [Parameter(Mandatory=$true)]
        [byte[]]$Data,
        
        [Parameter(Mandatory=$false)]
        [string]$JobName = "PowerShell Print Job",
        
        [Parameter(Mandatory=$false)]
        [string]$ApiUrl = "http://localhost:3031/api"
    )
    
    $body = @{
        data = $Data
        jobName = $JobName
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "$ApiUrl/print" -Method Post -Body $body -ContentType "application/json"
        return $response
    }
    catch {
        Write-Error "Erro ao imprimir: $_"
        return $null
    }
}

# Uso
$escData = [byte[]](27, 64, 72, 101, 108, 108, 111)
$result = Invoke-EscPosPrint -Data $escData -JobName "Meu Cupom"
```

---

## Postman

### Collection JSON

```json
{
  "info": {
    "name": "ESC/POS Printer API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Get Printers",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "http://localhost:3031/api/printers",
          "protocol": "http",
          "host": ["localhost"],
          "port": "3031",
          "path": ["api", "printers"]
        }
      }
    },
    {
      "name": "Print",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"data\": [27, 64, 72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100, 33, 10, 10, 10, 27, 105],\n  \"jobName\": \"Postman Test\"\n}"
        },
        "url": {
          "raw": "http://localhost:3031/api/print",
          "protocol": "http",
          "host": ["localhost"],
          "port": "3031",
          "path": ["api", "print"]
        }
      }
    },
    {
      "name": "Health Check",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "http://localhost:3031/health",
          "protocol": "http",
          "host": ["localhost"],
          "port": "3031",
          "path": ["health"]
        }
      }
    }
  ]
}
```

---

## 📝 Comandos ESC/POS Úteis

### Comandos Básicos

```javascript
const ESC = 0x1B;
const GS = 0x1D;

// Inicializar impressora
const INIT = [ESC, 0x40];

// Alinhamento
const ALIGN_LEFT = [ESC, 0x61, 0x00];
const ALIGN_CENTER = [ESC, 0x61, 0x01];
const ALIGN_RIGHT = [ESC, 0x61, 0x02];

// Texto
const BOLD_ON = [ESC, 0x45, 0x01];
const BOLD_OFF = [ESC, 0x45, 0x00];

const UNDERLINE_ON = [ESC, 0x2D, 0x01];
const UNDERLINE_OFF = [ESC, 0x2D, 0x00];

// Tamanho
const DOUBLE_HEIGHT = [ESC, 0x21, 0x10];
const DOUBLE_WIDTH = [ESC, 0x21, 0x20];
const NORMAL_SIZE = [ESC, 0x21, 0x00];

// Cortar papel
const CUT_PAPER = [ESC, 0x69];
const PARTIAL_CUT = [GS, 0x56, 0x01];

// Quebra de linha
const LF = [0x0A];
```

### Exemplo de Cupom Completo

```javascript
function buildReceipt(items, total) {
  const encoder = new TextEncoder();
  const receipt = [
    ...INIT,
    ...ALIGN_CENTER,
    ...BOLD_ON,
    ...encoder.encode("MINHA LOJA"),
    ...LF,
    ...BOLD_OFF,
    ...encoder.encode("CNPJ: 00.000.000/0001-00"),
    ...LF, ...LF,
    ...ALIGN_LEFT,
    ...encoder.encode("CUPOM FISCAL"),
    ...LF,
    ...encoder.encode("--------------------------------"),
    ...LF,
  ];

  // Adiciona itens
  items.forEach(item => {
    receipt.push(
      ...encoder.encode(`${item.name}`),
      ...LF,
      ...encoder.encode(`  ${item.qty} x R$ ${item.price.toFixed(2)}`),
      ...LF
    );
  });

  // Finaliza
  receipt.push(
    ...encoder.encode("--------------------------------"),
    ...LF,
    ...BOLD_ON,
    ...encoder.encode(`TOTAL: R$ ${total.toFixed(2)}`),
    ...LF,
    ...BOLD_OFF,
    ...LF, ...LF, ...LF,
    ...CUT_PAPER
  );

  return receipt;
}

// Uso
const items = [
  { name: "Produto A", qty: 2, price: 10.50 },
  { name: "Produto B", qty: 1, price: 25.00 }
];
const total = 46.00;

const receiptData = buildReceipt(items, total);
await printReceipt(receiptData);
```

---

## 🔗 Links Úteis

- [Documentação ESC/POS](https://reference.epson-biz.com/modules/ref_escpos/index.php)
- [Swagger UI](http://localhost:3031/swagger)
- [Repositório GitHub](https://github.com/leoncoutinho1/escpos-printer-api)
