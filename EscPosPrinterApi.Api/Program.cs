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
.WithOpenApi()
.Produces<List<PrinterInfo>>(200)
.Produces(500);

// Endpoint para imprimir (abre interface gráfica)
app.MapPost("/api/print", async ([FromBody] PrintRequest request) =>
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

        // Salva os dados em um arquivo temporário
        string tempFile = Path.Combine(Path.GetTempPath(), $"print_data_{Guid.NewGuid()}.bin");
        await File.WriteAllBytesAsync(tempFile, request.Data);

        // Caminho para o executável do Windows Forms
        string uiExePath = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "EscPosPrinterApi.UI", "bin", "Debug", "net10.0-windows",
            "EscPosPrinterApi.UI.exe"
        );

        // Normaliza o caminho
        uiExePath = Path.GetFullPath(uiExePath);

        if (!File.Exists(uiExePath))
        {
            // Tenta caminho alternativo (quando executado de release)
            uiExePath = Path.Combine(
                AppContext.BaseDirectory,
                "EscPosPrinterApi.UI.exe"
            );
        }

        if (!File.Exists(uiExePath))
        {
            return Results.Problem(
                detail: $"Executável da UI não encontrado em: {uiExePath}",
                statusCode: 500,
                title: "Erro de configuração"
            );
        }

        // Inicia o processo do Windows Forms
        var processStartInfo = new ProcessStartInfo
        {
            FileName = uiExePath,
            Arguments = request.DefaultPrinter 
                ? $"\"{tempFile}\" true" 
                : $"\"{tempFile}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = false
        };

        using var process = Process.Start(processStartInfo);
        
        if (process == null)
        {
            return Results.Problem(
                detail: "Não foi possível iniciar o processo da interface gráfica",
                statusCode: 500,
                title: "Erro ao abrir interface"
            );
        }

        // Aguarda o processo terminar
        await process.WaitForExitAsync();

        // Lê o resultado do arquivo temporário
        string resultFilePath = (await process.StandardOutput.ReadToEndAsync()).Trim();

        PrintResponse response;

        if (File.Exists(resultFilePath))
        {
            string resultJson = await File.ReadAllTextAsync(resultFilePath);
            response = JsonSerializer.Deserialize<PrintResponse>(resultJson) ?? new PrintResponse
            {
                Success = false,
                Message = "Erro ao deserializar resposta"
            };

            // Limpa o arquivo de resultado
            try { File.Delete(resultFilePath); } catch { }
        }
        else
        {
            response = new PrintResponse
            {
                Success = false,
                Cancelled = true,
                Message = "Nenhum resultado retornado pela interface"
            };
        }

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
.WithOpenApi()
.Produces<PrintResponse>(200)
.Produces<PrintResponse>(400)
.Produces(500);

// Endpoint de health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
.WithName("HealthCheck")
.WithOpenApi();

app.Run();
