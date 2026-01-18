using System.Text.Json;
using System.Text.Json.Serialization;

namespace EscPosPrinterApi.Core.Converters;

/// <summary>
/// Conversor JSON customizado para aceitar byte arrays como array de números ou string Base64
/// </summary>
public class ByteArrayConverter : JsonConverter<byte[]>
{
    public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            // Se for string, assume Base64
            string? base64 = reader.GetString();
            return base64 != null ? Convert.FromBase64String(base64) : Array.Empty<byte>();
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            // Se for array, lê os números
            var bytes = new List<byte>();
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    break;
                }
                
                if (reader.TokenType == JsonTokenType.Number)
                {
                    bytes.Add(reader.GetByte());
                }
            }
            
            return bytes.ToArray();
        }
        
        throw new JsonException("Expected string (Base64) or array of numbers for byte array");
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        // Escreve como array de números para melhor legibilidade
        writer.WriteStartArray();
        foreach (byte b in value)
        {
            writer.WriteNumberValue(b);
        }
        writer.WriteEndArray();
    }
}
