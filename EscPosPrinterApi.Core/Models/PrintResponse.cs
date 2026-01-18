namespace EscPosPrinterApi.Core.Models;

/// <summary>
/// Resposta da operação de impressão
/// </summary>
public class PrintResponse
{
    /// <summary>
    /// Indica se a impressão foi bem-sucedida
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensagem descritiva do resultado
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Nome da impressora selecionada (se houver)
    /// </summary>
    public string? PrinterName { get; set; }

    /// <summary>
    /// Indica se o usuário cancelou a operação
    /// </summary>
    public bool Cancelled { get; set; }
}
