# Script para testar a API de impressão ESC/POS

Write-Host "=== ESC/POS Printer API - Test Script ===" -ForegroundColor Cyan
Write-Host ""

$apiUrl = "http://localhost:3031"

# Função para testar se a API está rodando
function Test-ApiHealth {
    try {
        $response = Invoke-RestMethod -Uri "$apiUrl/health" -Method Get
        Write-Host "✓ API está rodando" -ForegroundColor Green
        Write-Host "  Status: $($response.status)" -ForegroundColor Gray
        Write-Host "  Timestamp: $($response.timestamp)" -ForegroundColor Gray
        return $true
    }
    catch {
        Write-Host "✗ API não está respondendo" -ForegroundColor Red
        Write-Host "  Certifique-se de que a API está rodando com: dotnet run --project EscPosPrinterApi.Api" -ForegroundColor Yellow
        return $false
    }
}

# Função para listar impressoras
function Get-Printers {
    Write-Host ""
    Write-Host "=== Listando Impressoras ===" -ForegroundColor Cyan
    try {
        $printers = Invoke-RestMethod -Uri "$apiUrl/api/printers" -Method Get
        
        if ($printers.Count -eq 0) {
            Write-Host "Nenhuma impressora encontrada" -ForegroundColor Yellow
        }
        else {
            foreach ($printer in $printers) {
                $defaultTag = if ($printer.isDefault) { " (Padrão)" } else { "" }
                Write-Host "  • $($printer.name)$defaultTag" -ForegroundColor White
                Write-Host "    Status: $($printer.status)" -ForegroundColor Gray
            }
        }
    }
    catch {
        Write-Host "Erro ao listar impressoras: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Função para enviar impressão de teste
function Send-TestPrint {
    Write-Host ""
    Write-Host "=== Enviando Impressão de Teste ===" -ForegroundColor Cyan
    
    # Comandos ESC/POS para imprimir "Hello World" centralizado
    $printData = @{
        data = @(
            27, 64,           # ESC @ - Inicializar impressora
            27, 97, 1,        # ESC a 1 - Centralizar
            72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100, 33,  # "Hello World!"
            10, 10, 10,       # 3 quebras de linha
            27, 105           # ESC i - Cortar papel
        )
        jobName = "PowerShell Test Print"
    }
    
    try {
        $json = $printData | ConvertTo-Json
        Write-Host "Enviando dados para impressão..." -ForegroundColor Gray
        
        $response = Invoke-RestMethod -Uri "$apiUrl/api/print" -Method Post -Body $json -ContentType "application/json"
        
        if ($response.success) {
            Write-Host "✓ Impressão enviada com sucesso!" -ForegroundColor Green
            Write-Host "  Impressora: $($response.printerName)" -ForegroundColor Gray
            Write-Host "  Mensagem: $($response.message)" -ForegroundColor Gray
        }
        elseif ($response.cancelled) {
            Write-Host "⚠ Impressão cancelada pelo usuário" -ForegroundColor Yellow
        }
        else {
            Write-Host "✗ Falha na impressão" -ForegroundColor Red
            Write-Host "  Mensagem: $($response.message)" -ForegroundColor Gray
        }
    }
    catch {
        Write-Host "Erro ao enviar impressão: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Função para enviar impressão de teste na impressora padrão
function Send-TestPrintToDefault {
    Write-Host ""
    Write-Host "=== Enviando Impressão para Impressora Padrão ===" -ForegroundColor Cyan
    
    # Comandos ESC/POS para imprimir "Auto Print Test" centralizado
    $printData = @{
        data = @(
            27, 64,           # ESC @ - Inicializar impressora
            27, 97, 1,        # ESC a 1 - Centralizar
            65, 117, 116, 111, 32, 80, 114, 105, 110, 116, 32, 84, 101, 115, 116,  # "Auto Print Test"
            10, 10, 10,       # 3 quebras de linha
            27, 105           # ESC i - Cortar papel
        )
        jobName = "PowerShell Auto Print Test"
        defaultPrinter = $true
    }
    
    try {
        $json = $printData | ConvertTo-Json
        Write-Host "Enviando dados para impressão automática..." -ForegroundColor Gray
        
        $response = Invoke-RestMethod -Uri "$apiUrl/api/print" -Method Post -Body $json -ContentType "application/json"
        
        if ($response.success) {
            Write-Host "✓ Impressão enviada automaticamente com sucesso!" -ForegroundColor Green
            Write-Host "  Impressora: $($response.printerName)" -ForegroundColor Gray
            Write-Host "  Mensagem: $($response.message)" -ForegroundColor Gray
        }
        else {
            Write-Host "✗ Falha na impressão" -ForegroundColor Red
            Write-Host "  Mensagem: $($response.message)" -ForegroundColor Gray
        }
    }
    catch {
        Write-Host "Erro ao enviar impressão: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Menu principal
function Show-Menu {
    Write-Host ""
    Write-Host "=== Menu ===" -ForegroundColor Cyan
    Write-Host "1. Testar Health Check"
    Write-Host "2. Listar Impressoras"
    Write-Host "3. Enviar Impressão de Teste (com seleção)"
    Write-Host "4. Enviar Impressão para Impressora Padrão (sem seleção)"
    Write-Host "5. Executar Todos os Testes"
    Write-Host "0. Sair"
    Write-Host ""
}

# Loop principal
$running = $true
while ($running) {
    Show-Menu
    $choice = Read-Host "Escolha uma opção"
    
    switch ($choice) {
        "1" { Test-ApiHealth }
        "2" { Get-Printers }
        "3" { Send-TestPrint }
        "4" { Send-TestPrintToDefault }
        "5" {
            if (Test-ApiHealth) {
                Get-Printers
                Send-TestPrint
                Send-TestPrintToDefault
            }
        }
        "0" {
            Write-Host "Encerrando..." -ForegroundColor Gray
            $running = $false
        }
        default {
            Write-Host "Opção inválida" -ForegroundColor Red
        }
    }
}
