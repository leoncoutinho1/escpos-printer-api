# Guia de Deploy - ESC/POS Printer API

Este documento descreve como fazer o deploy da aplicação em diferentes ambientes.

## 📋 Pré-requisitos

- Windows 10/11 ou Windows Server 2019+
- .NET 10.0 Runtime (ou SDK para desenvolvimento)
- Impressora ESC/POS instalada e configurada

## 🚀 Deploy Local (Desenvolvimento)

### 1. Clonar o Repositório

```bash
git clone https://github.com/leoncoutinho1/escpos-printer-api.git
cd escpos-printer-api
```

### 2. Restaurar Dependências

```bash
dotnet restore
```

### 3. Compilar

```bash
dotnet build EscPosPrinterApi.sln --configuration Release
```

### 4. Executar

```bash
cd EscPosPrinterApi.Api
dotnet run
```

A API estará disponível em: `http://localhost:3031`

---

## 📦 Deploy em Produção

### Opção 1: Publicação Standalone

Esta opção cria um executável independente que não requer .NET instalado.

```bash
# Publicar API
dotnet publish EscPosPrinterApi.Api/EscPosPrinterApi.Api.csproj \
  --configuration Release \
  --runtime win-x64 \
  --self-contained true \
  --output ./publish/api

# Publicar UI
dotnet publish EscPosPrinterApi.UI/EscPosPrinterApi.UI.csproj \
  --configuration Release \
  --runtime win-x64 \
  --self-contained true \
  --output ./publish/ui
```

**Estrutura de Deploy:**
```
C:\EscPosPrinterApi\
├── api\
│   ├── EscPosPrinterApi.Api.exe
│   ├── appsettings.json
│   └── ... (outros arquivos)
└── ui\
    ├── EscPosPrinterApi.UI.exe
    └── ... (outros arquivos)
```

**Atualizar caminho da UI no código:**

Edite `EscPosPrinterApi.Api\Program.cs` linha ~75:

```csharp
string uiExePath = Path.Combine(
    AppContext.BaseDirectory,
    "..", "ui",
    "EscPosPrinterApi.UI.exe"
);
```

### Opção 2: Publicação Framework-Dependent

Requer .NET Runtime instalado no servidor.

```bash
# Publicar API
dotnet publish EscPosPrinterApi.Api/EscPosPrinterApi.Api.csproj \
  --configuration Release \
  --output ./publish/api

# Publicar UI
dotnet publish EscPosPrinterApi.UI/EscPosPrinterApi.UI.csproj \
  --configuration Release \
  --output ./publish/ui
```

---

## 🪟 Deploy como Serviço do Windows

### 1. Instalar NSSM (Non-Sucking Service Manager)

```powershell
# Usando Chocolatey
choco install nssm

# Ou baixar de: https://nssm.cc/download
```

### 2. Criar o Serviço

```powershell
# Navegar até o diretório de publicação
cd C:\EscPosPrinterApi\api

# Instalar como serviço
nssm install EscPosPrinterApi "C:\EscPosPrinterApi\api\EscPosPrinterApi.Api.exe"

# Configurar diretório de trabalho
nssm set EscPosPrinterApi AppDirectory "C:\EscPosPrinterApi\api"

# Configurar log
nssm set EscPosPrinterApi AppStdout "C:\EscPosPrinterApi\logs\output.log"
nssm set EscPosPrinterApi AppStderr "C:\EscPosPrinterApi\logs\error.log"

# Iniciar o serviço
nssm start EscPosPrinterApi
```

### 3. Gerenciar o Serviço

```powershell
# Verificar status
nssm status EscPosPrinterApi

# Parar
nssm stop EscPosPrinterApi

# Reiniciar
nssm restart EscPosPrinterApi

# Remover
nssm remove EscPosPrinterApi confirm
```

---

## 🌐 Deploy com IIS (Internet Information Services)

### 1. Instalar IIS

```powershell
# PowerShell como Administrador
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
Enable-WindowsOptionalFeature -Online -FeatureName IIS-CommonHttpFeatures
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpErrors
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ApplicationDevelopment
Enable-WindowsOptionalFeature -Online -FeatureName IIS-NetFxExtensibility45
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HealthAndDiagnostics
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpLogging
Enable-WindowsOptionalFeature -Online -FeatureName IIS-Security
Enable-WindowsOptionalFeature -Online -FeatureName IIS-RequestFiltering
Enable-WindowsOptionalFeature -Online -FeatureName IIS-Performance
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerManagementTools
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ManagementConsole
```

### 2. Instalar ASP.NET Core Hosting Bundle

Baixe e instale de: https://dotnet.microsoft.com/download/dotnet/10.0

### 3. Publicar Aplicação

```bash
dotnet publish EscPosPrinterApi.Api/EscPosPrinterApi.Api.csproj \
  --configuration Release \
  --output C:\inetpub\wwwroot\EscPosPrinterApi
```

### 4. Configurar Site no IIS

1. Abra o **IIS Manager**
2. Clique com botão direito em **Sites** → **Add Website**
3. Configure:
   - **Site name**: EscPosPrinterApi
   - **Physical path**: `C:\inetpub\wwwroot\EscPosPrinterApi`
   - **Binding**: HTTP, Port 3031
4. Configure o **Application Pool**:
   - **.NET CLR Version**: No Managed Code
   - **Managed Pipeline Mode**: Integrated
   - **Identity**: ApplicationPoolIdentity (ou conta com acesso às impressoras)

### 5. Configurar Permissões

```powershell
# Dar permissão ao Application Pool
icacls "C:\inetpub\wwwroot\EscPosPrinterApi" /grant "IIS AppPool\EscPosPrinterApi:(OI)(CI)F" /T
```

### 6. Atualizar web.config

O arquivo `web.config` é criado automaticamente, mas você pode personalizá-lo:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\EscPosPrinterApi.Api.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

---

## 🐳 Deploy com Docker (Experimental)

⚠️ **Nota**: Windows Forms não funciona nativamente em containers Linux. Esta configuração requer Windows Containers.

### Dockerfile (API)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0-nanoserver-ltsc2022 AS base
WORKDIR /app
EXPOSE 3031

FROM mcr.microsoft.com/dotnet/sdk:10.0-nanoserver-ltsc2022 AS build
WORKDIR /src
COPY ["EscPosPrinterApi.Api/EscPosPrinterApi.Api.csproj", "EscPosPrinterApi.Api/"]
COPY ["EscPosPrinterApi.Core/EscPosPrinterApi.Core.csproj", "EscPosPrinterApi.Core/"]
RUN dotnet restore "EscPosPrinterApi.Api/EscPosPrinterApi.Api.csproj"
COPY . .
WORKDIR "/src/EscPosPrinterApi.Api"
RUN dotnet build "EscPosPrinterApi.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EscPosPrinterApi.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EscPosPrinterApi.Api.dll"]
```

### docker-compose.yml

```yaml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "3031:3031"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:3031
    volumes:
      - C:\EscPosPrinterApi\ui:C:\app\ui
```

---

## ⚙️ Configurações de Produção

### appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Urls": "http://0.0.0.0:3031",
  "Kestrel": {
    "Limits": {
      "MaxRequestBodySize": 10485760,
      "RequestHeadersTimeout": "00:01:00"
    }
  }
}
```

### Variáveis de Ambiente

```powershell
# Definir ambiente de produção
$env:ASPNETCORE_ENVIRONMENT = "Production"

# Configurar URLs
$env:ASPNETCORE_URLS = "http://0.0.0.0:3031"

# Caminho da UI (se necessário)
$env:UI_EXECUTABLE_PATH = "C:\EscPosPrinterApi\ui\EscPosPrinterApi.UI.exe"
```

---

## 🔒 Segurança em Produção

### 1. Firewall

```powershell
# Permitir porta 3031
New-NetFirewallRule -DisplayName "ESC/POS Printer API" -Direction Inbound -LocalPort 3031 -Protocol TCP -Action Allow
```

### 2. HTTPS (Recomendado)

Gere um certificado SSL e configure no `appsettings.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://0.0.0.0:3032",
        "Certificate": {
          "Path": "certificate.pfx",
          "Password": "your-password"
        }
      }
    }
  }
}
```

### 3. Autenticação (Opcional)

Adicione autenticação JWT ou API Key para proteger os endpoints.

---

## 📊 Monitoramento

### Logs

Os logs são salvos em:
- **Console**: Durante desenvolvimento
- **Arquivo**: Configure em `appsettings.json`
- **Event Viewer**: Quando rodando como serviço

### Health Check

Monitore a saúde da API:

```bash
curl http://localhost:3031/health
```

Configure um monitor externo (Nagios, Zabbix, etc.) para verificar este endpoint periodicamente.

---

## 🔄 Atualização

### Processo de Atualização

1. **Backup**: Faça backup da versão atual
2. **Parar Serviço**: `nssm stop EscPosPrinterApi`
3. **Atualizar Arquivos**: Copie novos binários
4. **Testar**: Execute manualmente para verificar
5. **Iniciar Serviço**: `nssm start EscPosPrinterApi`

### Script de Atualização

```powershell
# update.ps1
param(
    [string]$SourcePath = ".\publish\api",
    [string]$TargetPath = "C:\EscPosPrinterApi\api"
)

Write-Host "Parando serviço..." -ForegroundColor Yellow
nssm stop EscPosPrinterApi

Write-Host "Fazendo backup..." -ForegroundColor Yellow
$backupPath = "$TargetPath.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
Copy-Item -Path $TargetPath -Destination $backupPath -Recurse

Write-Host "Atualizando arquivos..." -ForegroundColor Yellow
Copy-Item -Path "$SourcePath\*" -Destination $TargetPath -Recurse -Force

Write-Host "Iniciando serviço..." -ForegroundColor Yellow
nssm start EscPosPrinterApi

Write-Host "Verificando status..." -ForegroundColor Yellow
Start-Sleep -Seconds 5
nssm status EscPosPrinterApi

Write-Host "Atualização concluída!" -ForegroundColor Green
```

---

## 🐛 Troubleshooting

### API não inicia

```powershell
# Verificar logs
Get-Content C:\EscPosPrinterApi\logs\error.log -Tail 50

# Verificar porta em uso
netstat -ano | findstr :3031

# Testar manualmente
cd C:\EscPosPrinterApi\api
.\EscPosPrinterApi.Api.exe
```

### UI não abre

- Verifique se o caminho do executável está correto
- Confirme que o usuário do serviço tem permissão para abrir janelas
- Verifique se há sessão interativa disponível

### Impressora não encontrada

- Verifique se a impressora está instalada: `Get-Printer`
- Confirme que o usuário do serviço tem acesso às impressoras
- Teste com uma conta de administrador

---

## 📞 Suporte

Para problemas ou dúvidas sobre deploy, abra uma issue no GitHub:
https://github.com/leoncoutinho1/escpos-printer-api/issues
