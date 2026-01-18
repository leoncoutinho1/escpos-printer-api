namespace EscPosPrinterApi.Core.Models;

/// <summary>
/// Informações sobre uma impressora instalada
/// </summary>
public class PrinterInfo
{
    /// <summary>
    /// Nome da impressora
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Indica se é a impressora padrão do sistema
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Status da impressora
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
