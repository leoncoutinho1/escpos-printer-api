# 🖨️ ESC/POS Printer API - Projeto Completo

## ✅ Status do Projeto

**Versão**: 1.0.0  
**Status**: ✅ Pronto para uso  
**Última atualização**: 2026-01-13

---

## 📁 Estrutura do Projeto

```
escpos-printer-api/
├── 📄 README.md                    # Documentação principal
├── 📄 ARCHITECTURE.md              # Arquitetura detalhada
├── 📄 EXAMPLES.md                  # Exemplos de uso
├── 📄 DEPLOYMENT.md                # Guia de deploy
├── 📄 .gitignore                   # Arquivos ignorados pelo Git
├── 📄 EscPosPrinterApi.sln         # Solução .NET
│
├── 🗂️ EscPosPrinterApi.Core/       # Biblioteca compartilhada
│   ├── Models/
│   │   ├── PrintRequest.cs
│   │   ├── PrintResponse.cs
│   │   └── PrinterInfo.cs
│   └── Services/
│       ├── IPrinterService.cs
│       └── PrinterService.cs
│
├── 🗂️ EscPosPrinterApi.Api/        # API HTTP
│   ├── Program.cs
│   ├── appsettings.json
│   └── EscPosPrinterApi.Api.csproj
│
├── 🗂️ EscPosPrinterApi.UI/         # Interface Windows Forms
│   ├── Program.cs
│   ├── PrinterSelectionForm.cs
│   ├── PrinterSelectionForm.Designer.cs
│   └── EscPosPrinterApi.UI.csproj
│
├── 🧪 test-api.ps1                 # Script de teste PowerShell
├── 🚀 start-api.sh                 # Script de inicialização
└── 📋 test-print.json              # Exemplo de requisição
```

---

## 🎯 Funcionalidades Implementadas

### ✅ API REST
- [x] Endpoint GET `/api/printers` - Lista impressoras
- [x] Endpoint POST `/api/print` - Envia impressão
- [x] Endpoint GET `/health` - Health check
- [x] Documentação Swagger
- [x] CORS configurado
- [x] Validação de entrada

### ✅ Windows Forms UI
- [x] Listagem de impressoras instaladas
- [x] Seleção interativa de impressora
- [x] Indicação de impressora padrão
- [x] Confirmação/Cancelamento
- [x] Feedback visual de status
- [x] Tratamento de erros

### ✅ Serviço de Impressão
- [x] Integração com Windows API (winspool.drv)
- [x] Envio de dados brutos (RAW)
- [x] Suporte a byte arrays ESC/POS
- [x] Detecção automática de impressoras
- [x] Tratamento de erros robusto

### ✅ Documentação
- [x] README completo
- [x] Arquitetura detalhada
- [x] Exemplos em múltiplas linguagens
- [x] Guia de deployment
- [x] Scripts de teste

---

## 🚀 Como Usar

### Início Rápido

```bash
# 1. Compilar
dotnet build EscPosPrinterApi.sln

# 2. Executar API
cd EscPosPrinterApi.Api
dotnet run

# 3. Testar (em outro terminal)
curl http://localhost:3031/api/printers
```

### Exemplo de Impressão

```bash
curl -X POST http://localhost:3031/api/print \
  -H "Content-Type: application/json" \
  -d '{
    "data": [27, 64, 72, 101, 108, 108, 111, 10, 10, 27, 105],
    "jobName": "Test Print"
  }'
```

---

## 📊 Tecnologias Utilizadas

| Tecnologia | Versão | Uso |
|------------|--------|-----|
| .NET | 10.0 | Framework principal |
| ASP.NET Core | 10.0 | API REST |
| Windows Forms | 10.0 | Interface gráfica |
| Swashbuckle | 7.2.0 | Documentação Swagger |
| System.Drawing.Common | 10.0.1 | Acesso a impressoras |

---

## 🏗️ Arquitetura

```
┌─────────────┐
│   Cliente   │
└──────┬──────┘
       │ HTTP
       ▼
┌─────────────┐
│     API     │ ◄── Minimal API .NET
└──────┬──────┘
       │ Process
       ▼
┌─────────────┐
│  UI (WinForms) │ ◄── Seleção de impressora
└──────┬──────┘
       │ P/Invoke
       ▼
┌─────────────┐
│  Windows API │ ◄── winspool.drv
└──────┬──────┘
       │
       ▼
┌─────────────┐
│  Impressora │ ◄── Hardware ESC/POS
└─────────────┘
```

---

## 📝 Endpoints da API

### GET /api/printers
Lista todas as impressoras instaladas.

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
Envia dados para impressão.

**Requisição:**
```json
{
  "data": [27, 64, 72, 101, 108, 108, 111],
  "jobName": "Cupom Fiscal"
}
```

**Resposta:**
```json
{
  "success": true,
  "message": "Impressão enviada com sucesso",
  "printerName": "EPSON TM-T20",
  "cancelled": false
}
```

### GET /health
Verifica status da API.

**Resposta:**
```json
{
  "status": "healthy",
  "timestamp": "2026-01-13T14:30:00Z"
}
```

---

## 🧪 Testes

### Teste Automatizado (PowerShell)

```powershell
.\test-api.ps1
```

### Teste Manual

```bash
# 1. Verificar saúde
curl http://localhost:3031/health

# 2. Listar impressoras
curl http://localhost:3031/api/printers

# 3. Imprimir teste
curl -X POST http://localhost:3031/api/print \
  -H "Content-Type: application/json" \
  -d @test-print.json
```

---

## 📦 Deployment

### Desenvolvimento
```bash
dotnet run --project EscPosPrinterApi.Api
```

### Produção (Standalone)
```bash
dotnet publish -c Release --self-contained -r win-x64
```

### Serviço Windows
```powershell
nssm install EscPosPrinterApi "C:\path\to\EscPosPrinterApi.Api.exe"
nssm start EscPosPrinterApi
```

Veja [DEPLOYMENT.md](DEPLOYMENT.md) para mais opções.

---

## 🔒 Segurança

### ⚠️ Avisos Importantes

- Esta API foi desenvolvida para **uso local**
- **NÃO exponha publicamente** sem implementar:
  - ✅ Autenticação (JWT, API Key)
  - ✅ HTTPS
  - ✅ Rate limiting
  - ✅ Validação rigorosa de entrada

### Recomendações

1. Use apenas em redes confiáveis
2. Configure firewall apropriadamente
3. Limite tamanho do byte array
4. Implemente logs de auditoria
5. Use HTTPS em produção

---

## 🐛 Troubleshooting

### API não inicia
```bash
# Verificar se a porta está em uso
netstat -ano | findstr :3031

# Verificar logs
dotnet run --project EscPosPrinterApi.Api
```

### UI não abre
- Verifique o caminho do executável no código
- Confirme que há sessão interativa disponível
- Execute como administrador se necessário

### Impressora não encontrada
```powershell
# Listar impressoras instaladas
Get-Printer | Select-Object Name, DriverName, PortName
```

---

## 📚 Documentação Adicional

- [README.md](README.md) - Documentação principal
- [ARCHITECTURE.md](ARCHITECTURE.md) - Arquitetura detalhada
- [EXAMPLES.md](EXAMPLES.md) - Exemplos de uso
- [DEPLOYMENT.md](DEPLOYMENT.md) - Guia de deployment

---

## 🤝 Contribuindo

Contribuições são bem-vindas! Para contribuir:

1. Fork o projeto
2. Crie uma branch (`git checkout -b feature/nova-funcionalidade`)
3. Commit suas mudanças (`git commit -am 'Adiciona nova funcionalidade'`)
4. Push para a branch (`git push origin feature/nova-funcionalidade`)
5. Abra um Pull Request

---

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo LICENSE para mais detalhes.

---

## 👨‍💻 Autor

**Leonardo Coutinho**

- GitHub: [@leoncoutinho1](https://github.com/leoncoutinho1)
- Email: contato@exemplo.com

---

## 🙏 Agradecimentos

- Comunidade .NET
- Desenvolvedores de impressoras ESC/POS
- Todos os contribuidores

---

## 📞 Suporte

Para problemas ou dúvidas:

- 🐛 [Abra uma issue](https://github.com/leoncoutinho1/escpos-printer-api/issues)
- 💬 [Discussões](https://github.com/leoncoutinho1/escpos-printer-api/discussions)
- 📧 Email: contato@exemplo.com

---

## 🗺️ Roadmap

### Versão 1.1 (Planejado)
- [ ] Suporte a impressão de imagens
- [ ] Fila de impressão assíncrona
- [ ] Cache de impressoras
- [ ] Logs estruturados

### Versão 2.0 (Futuro)
- [ ] Suporte a Linux (CUPS)
- [ ] Interface web para configuração
- [ ] Autenticação JWT
- [ ] Métricas e monitoramento

---

**Desenvolvido com ❤️ usando .NET**
