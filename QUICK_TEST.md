# Teste Rápido da API

## 1. Health Check
```bash
curl http://localhost:3031/health
```

## 2. Listar Impressoras
```bash
curl http://localhost:3031/api/printers
```

## 3. Imprimir (Array de Números)
```bash
curl -X POST http://localhost:3031/api/print \
  -H "Content-Type: application/json" \
  -d @test-print.json
```

Ou inline:
```bash
curl -X POST http://localhost:3031/api/print \
  -H "Content-Type: application/json" \
  -d '{
    "data": [27, 64, 72, 101, 108, 108, 111, 10, 10, 27, 105],
    "jobName": "Quick Test"
  }'
```

## 4. Imprimir (Base64)
```bash
curl -X POST http://localhost:3031/api/print \
  -H "Content-Type: application/json" \
  -d '{
    "data": "G0BIZWxsbwoKG2k=",
    "jobName": "Base64 Test"
  }'
```

## Decodificação Base64

Para converter byte array para Base64:
```bash
echo -n -e "\x1b\x40Hello\n\n\x1b\x69" | base64
```

Para converter Base64 para byte array:
```bash
echo "G0BIZWxsbwoKG2k=" | base64 -d | od -An -tx1
```
