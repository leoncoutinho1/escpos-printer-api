using EscPosPrinterApi.Core.Models;
using EscPosPrinterApi.Core.Services;

namespace EscPosPrinterApi.UI
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main(string[] args)
        {
            ApplicationConfiguration.Initialize();

            // Se recebeu argumentos, processa como chamada da API
            if (args.Length > 0)
            {
                // O primeiro argumento deve ser o caminho do arquivo temporário com os dados
                string tempFilePath = args[0];
                
                // O segundo argumento (opcional) indica se deve usar a impressora padrão
                bool useDefaultPrinter = args.Length > 1 && args[1].Equals("true", StringComparison.OrdinalIgnoreCase);
                
                if (File.Exists(tempFilePath))
                {
                    byte[] printData = File.ReadAllBytes(tempFilePath);
                    var printerService = new PrinterService();
                    
                    PrintResponse result;
                    
                    if (useDefaultPrinter)
                    {
                        // Imprime diretamente na impressora padrão sem exibir o modal
                        var defaultPrinter = await printerService.GetDefaultPrinterAsync();
                        
                        if (string.IsNullOrEmpty(defaultPrinter))
                        {
                            result = new PrintResponse
                            {
                                Success = false,
                                Cancelled = false,
                                Message = "Nenhuma impressora padrão encontrada no sistema"
                            };
                        }
                        else
                        {
                            try
                            {
                                bool success = await printerService.PrintAsync(defaultPrinter, printData);
                                result = new PrintResponse
                                {
                                    Success = success,
                                    Cancelled = false,
                                    PrinterName = defaultPrinter,
                                    Message = success 
                                        ? $"Impressão enviada com sucesso para {defaultPrinter}" 
                                        : "Falha ao enviar dados para impressora"
                                };
                            }
                            catch (Exception ex)
                            {
                                result = new PrintResponse
                                {
                                    Success = false,
                                    Cancelled = false,
                                    PrinterName = defaultPrinter,
                                    Message = $"Erro ao imprimir: {ex.Message}"
                                };
                            }
                        }
                    }
                    else
                    {
                        // Exibe o modal de seleção de impressora
                        using var form = new PrinterSelectionForm(printerService, printData);
                        var dialogResult = form.ShowDialog();
                        result = form.Result;
                    }
                    
                    // Salva o resultado em um arquivo temporário
                    string resultFile = Path.Combine(Path.GetTempPath(), $"print_result_{Guid.NewGuid()}.json");
                    File.WriteAllText(resultFile, System.Text.Json.JsonSerializer.Serialize(result));
                    
                    // Escreve o caminho do arquivo de resultado no console para a API ler
                    Console.WriteLine(resultFile);
                    
                    // Limpa o arquivo temporário de entrada
                    try { File.Delete(tempFilePath); } catch { }
                }
            }
            else
            {
                // Modo standalone para testes
                Application.Run(new PrinterSelectionForm(new PrinterService(), new byte[] { 0x1B, 0x40 }));
            }
        }
    }
}