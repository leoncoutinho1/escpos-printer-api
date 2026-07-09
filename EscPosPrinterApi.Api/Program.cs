using System.Diagnostics;
using System.Text.Json;
using EscPosPrinterApi.Core.Models;
using EscPosPrinterApi.Core.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IPrinterService, PrinterService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configura o pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Endpoint para listar impressoras
app.MapGet("/api/printers", async (IPrinterService printerService) =>
{
    try
    {
        var printers = await printerService.GetPrintersAsync();
        return Results.Ok(printers);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Erro ao listar impressoras"
        );
    }
})
.WithName("GetPrinters")
.Produces<List<PrinterInfo>>(200)
.Produces(500);

// Endpoint para imprimir (agora imprime diretamente sem UI)
app.MapPost("/api/print", async ([FromBody] PrintRequest request, IPrinterService printerService, IConfiguration configuration) =>
{
    try
    {
        if (request.Data == null || request.Data.Length == 0)
        {
            return Results.BadRequest(new PrintResponse
            {
                Success = false,
                Message = "Dados de impressão não fornecidos"
            });
        }

        string? printerName = configuration["DefaultPrinterNetworkPath"];

        if (string.IsNullOrEmpty(printerName))
        {
            printerName = request.PrinterName;

            // Se solicitado impressora padrão ou nome não fornecido
            if (request.DefaultPrinter || string.IsNullOrEmpty(printerName))
            {
                printerName = await printerService.GetDefaultPrinterAsync();
            }
        }

        if (string.IsNullOrEmpty(printerName))
        {
            return Results.BadRequest(new PrintResponse
            {
                Success = false,
                Message = "Nenhuma impressora encontrada ou especificada"
            });
        }

        // Impressão direta usando o serviço
        bool success = await printerService.PrintAsync(printerName, request.Data);

        var response = new PrintResponse
        {
            Success = success,
            Message = success ? "Impressão enviada com sucesso" : "Falha ao enviar para a impressora",
            PrinterName = printerName
        };

        /* 
        // CÓDIGO ANTERIOR QUE ABRE A UI - COMENTADO PARA RODAR NO WINDOWS 7 SEM DEPENDÊNCIAS GRÁFICAS
        
        // Salva os dados em um arquivo temporário
        string tempFile = Path.Combine(Path.GetTempPath(), $"print_data_{Guid.NewGuid()}.bin");
        await File.WriteAllBytesAsync(tempFile, request.Data);

        // Caminho para o executável do Windows Forms
        string uiExePath = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "EscPosPrinterApi.UI", "bin", "Debug", "net6.0-windows",
            "EscPosPrinterApi.UI.exe"
        );

        // ... resto do código da UI ...
        */

        return response.Success ? Results.Ok(response) : Results.BadRequest(response);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Erro ao processar impressão"
        );
    }
})
.WithName("Print")
.Produces<PrintResponse>(200)
.Produces<PrintResponse>(400)
.Produces(500);

// Endpoint de health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
.WithName("HealthCheck");

app.Run();
