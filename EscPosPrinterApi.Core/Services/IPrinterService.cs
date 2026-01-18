using EscPosPrinterApi.Core.Models;

namespace EscPosPrinterApi.Core.Services;

/// <summary>
/// Interface para o serviço de impressão
/// </summary>
public interface IPrinterService
{
    /// <summary>
    /// Obtém a lista de impressoras instaladas no sistema
    /// </summary>
    Task<List<PrinterInfo>> GetPrintersAsync();

    /// <summary>
    /// Envia dados para impressão na impressora especificada
    /// </summary>
    Task<bool> PrintAsync(string printerName, byte[] data);

    /// <summary>
    /// Obtém o nome da impressora padrão do sistema
    /// </summary>
    Task<string?> GetDefaultPrinterAsync();
}
