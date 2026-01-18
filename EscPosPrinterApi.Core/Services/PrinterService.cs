using System.Drawing.Printing;
using System.Runtime.InteropServices;
using EscPosPrinterApi.Core.Models;

namespace EscPosPrinterApi.Core.Services;

/// <summary>
/// Implementação do serviço de impressão usando Windows API
/// </summary>
public class PrinterService : IPrinterService
{
    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool StartDocPrinter(IntPtr hPrinter, int level, ref DOC_INFO_1 pDocInfo);

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct DOC_INFO_1
    {
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pDocName;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string? pOutputFile;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string? pDataType;
    }

    /// <summary>
    /// Obtém a lista de impressoras instaladas no sistema
    /// </summary>
    public Task<List<PrinterInfo>> GetPrintersAsync()
    {
        var printers = new List<PrinterInfo>();
        var defaultPrinter = new PrinterSettings().PrinterName;

        foreach (string printerName in PrinterSettings.InstalledPrinters)
        {
            printers.Add(new PrinterInfo
            {
                Name = printerName,
                IsDefault = printerName == defaultPrinter,
                Status = "Disponível"
            });
        }

        return Task.FromResult(printers);
    }

    /// <summary>
    /// Envia dados brutos (byte array) para a impressora especificada
    /// </summary>
    public Task<bool> PrintAsync(string printerName, byte[] data)
    {
        IntPtr hPrinter = IntPtr.Zero;
        bool success = false;

        try
        {
            // Abre a impressora
            if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
            {
                throw new Exception($"Não foi possível abrir a impressora: {printerName}");
            }

            // Configura o documento
            DOC_INFO_1 docInfo = new DOC_INFO_1
            {
                pDocName = "ESC/POS Print Job",
                pOutputFile = null,
                pDataType = "RAW"
            };

            // Inicia o documento
            if (!StartDocPrinter(hPrinter, 1, ref docInfo))
            {
                throw new Exception("Não foi possível iniciar o documento de impressão");
            }

            // Inicia a página
            if (!StartPagePrinter(hPrinter))
            {
                throw new Exception("Não foi possível iniciar a página de impressão");
            }

            // Aloca memória não gerenciada para os dados
            IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(data.Length);
            try
            {
                Marshal.Copy(data, 0, pUnmanagedBytes, data.Length);

                // Envia os dados para a impressora
                int dwWritten;
                if (!WritePrinter(hPrinter, pUnmanagedBytes, data.Length, out dwWritten))
                {
                    throw new Exception("Erro ao enviar dados para a impressora");
                }

                success = dwWritten == data.Length;
            }
            finally
            {
                Marshal.FreeCoTaskMem(pUnmanagedBytes);
            }

            // Finaliza a página e o documento
            EndPagePrinter(hPrinter);
            EndDocPrinter(hPrinter);
        }
        finally
        {
            if (hPrinter != IntPtr.Zero)
            {
                ClosePrinter(hPrinter);
            }
        }

        return Task.FromResult(success);
    }

    /// <summary>
    /// Obtém o nome da impressora padrão do sistema
    /// </summary>
    public Task<string?> GetDefaultPrinterAsync()
    {
        try
        {
            var printerSettings = new PrinterSettings();
            return Task.FromResult<string?>(printerSettings.PrinterName);
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }
}
