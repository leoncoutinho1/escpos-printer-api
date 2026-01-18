using System.Text.Json.Serialization;
using EscPosPrinterApi.Core.Converters;

namespace EscPosPrinterApi.Core.Models;

/// <summary>
/// Representa uma requisição de impressão
/// </summary>
public class PrintRequest
{
    /// <summary>
    /// Byte array contendo os dados a serem impressos (comandos ESC/POS)
    /// Aceita tanto array de números [27, 64, ...] quanto string Base64
    /// </summary>
    [JsonConverter(typeof(ByteArrayConverter))]
    public byte[] Data { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Nome opcional para identificar a impressão
    /// </summary>
    public string? JobName { get; set; }

    /// <summary>
    /// Se true, usa a impressora padrão do sistema sem exibir o modal de seleção
    /// </summary>
    public bool DefaultPrinter { get; set; } = false;
}
