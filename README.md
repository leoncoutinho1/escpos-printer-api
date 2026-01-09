# ESC/POS Printer API

API REST para enviar byte arrays para impressoras térmicas Perto usando Node.js + Express.

## 🚀 Características

- ✅ Listagem de impressoras USB conectadas
- ✅ Conexão com impressora específica
- ✅ Impressão de byte arrays (ESC/POS)
- ✅ Impressão de texto simples (para testes)
- ✅ Suporte a CORS
- ✅ Tratamento de erros robusto

## 📋 Pré-requisitos

- Node.js >= 16.x
- NPM ou Yarn
- Impressora térmica Perto conectada via USB
- Windows/Linux com drivers USB apropriados

### Drivers USB (Windows)

No Windows, pode ser necessário instalar drivers USB. A biblioteca `usb` usa `libusb`. Para Windows:

1. Baixe e instale [Zadig](https://zadig.akeo.ie/)
2. Execute Zadig e selecione sua impressora
3. Instale o driver WinUSB

## 🔧 Instalação

```bash
# Instalar dependências
npm install

# Modo desenvolvimento (com hot reload)
npm run dev

# Modo produção
npm start
```

## 📡 Endpoints da API

### Base URL
```
http://localhost:3000
```

### 1. Health Check
```http
GET /health
```

**Resposta:**
```json
{
  "status": "ok",
  "message": "ESC/POS Printer API is running"
}
```

### 2. Listar Impressoras
```http
GET /api/printer/list
```

**Resposta:**
```json
{
  "success": true,
  "count": 1,
  "printers": [
    {
      "index": 0,
      "vendorId": 1234,
      "productId": 5678,
      "manufacturer": 1,
      "product": 2
    }
  ]
}
```

### 3. Conectar à Impressora
```http
POST /api/printer/connect
Content-Type: application/json

{
  "printerIndex": 0
}
```

**Resposta:**
```json
{
  "success": true,
  "message": "Connected to printer successfully",
  "printer": {
    "vendorId": 1234,
    "productId": 5678
  }
}
```

### 4. Imprimir Byte Array
```http
POST /api/printer/print
Content-Type: application/json

{
  "byteArray": [27, 64, 27, 97, 1, 72, 101, 108, 108, 111, 10, 10, 10]
}
```

**Resposta:**
```json
{
  "success": true,
  "message": "Print job sent successfully",
  "bytesWritten": 13
}
```

### 5. Imprimir Texto (Teste)
```http
POST /api/printer/print-text
Content-Type: application/json

{
  "text": "Teste de impressão"
}
```

**Resposta:**
```json
{
  "success": true,
  "message": "Text printed successfully"
}
```

### 6. Desconectar
```http
POST /api/printer/disconnect
```

**Resposta:**
```json
{
  "success": true,
  "message": "Disconnected from printer"
}
```

## 🧪 Exemplos de Uso

### cURL

```bash
# Listar impressoras
curl http://localhost:3000/api/printer/list

# Conectar à primeira impressora
curl -X POST http://localhost:3000/api/printer/connect \
  -H "Content-Type: application/json" \
  -d '{"printerIndex": 0}'

# Imprimir byte array
curl -X POST http://localhost:3000/api/printer/print \
  -H "Content-Type: application/json" \
  -d '{"byteArray": [27, 64, 27, 97, 1, 72, 101, 108, 108, 111, 10, 10, 10]}'

# Imprimir texto de teste
curl -X POST http://localhost:3000/api/printer/print-text \
  -H "Content-Type: application/json" \
  -d '{"text": "Olá, Mundo!"}'
```

### JavaScript (Fetch)

```javascript
// Conectar à impressora
const connect = async () => {
  const response = await fetch('http://localhost:3000/api/printer/connect', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ printerIndex: 0 })
  });
  return await response.json();
};

// Imprimir byte array
const print = async (byteArray) => {
  const response = await fetch('http://localhost:3000/api/printer/print', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ byteArray })
  });
  return await response.json();
};

// Exemplo de uso
await connect();
await print([27, 64, 27, 97, 1, 72, 101, 108, 108, 111, 10, 10, 10]);
```

## 🔍 Comandos ESC/POS Comuns

Alguns comandos ESC/POS úteis para criar byte arrays:

```javascript
// Inicializar impressora
[27, 64]  // ESC @

// Alinhar texto
[27, 97, 0]  // Esquerda
[27, 97, 1]  // Centro
[27, 97, 2]  // Direita

// Tamanho do texto
[29, 33, 0]   // Normal
[29, 33, 17]  // Dupla altura
[29, 33, 32]  // Dupla largura
[29, 33, 51]  // Dupla altura e largura

// Estilo
[27, 69, 1]  // Negrito ON
[27, 69, 0]  // Negrito OFF
[27, 45, 1]  // Sublinhado ON
[27, 45, 0]  // Sublinhado OFF

// Cortar papel
[29, 86, 0]  // Corte total
[29, 86, 1]  // Corte parcial

// Quebra de linha
[10]  // Line feed
```

## 🛠️ Estrutura do Projeto

```
escpos-printer-api/
├── src/
│   ├── controllers/
│   │   └── printer.controller.js    # Lógica dos endpoints
│   ├── routes/
│   │   └── printer.routes.js        # Definição das rotas
│   ├── services/
│   │   └── printer.service.js       # Serviço de impressão
│   └── server.js                     # Servidor Express
├── package.json
└── README.md
```

## ⚠️ Troubleshooting

### Erro: "No USB printers found"
- Verifique se a impressora está conectada via USB
- Verifique se os drivers estão instalados corretamente
- No Windows, use Zadig para instalar WinUSB driver

### Erro: "LIBUSB_ERROR_NOT_SUPPORTED"
- No Windows, instale o driver WinUSB usando Zadig
- No Linux, pode ser necessário adicionar regras udev

### Erro: "Permission denied"
- No Linux, adicione seu usuário ao grupo `lp`:
  ```bash
  sudo usermod -a -G lp $USER
  ```

## 📝 Licença

MIT

## 🤝 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues ou pull requests.
# escpos-printer-api
