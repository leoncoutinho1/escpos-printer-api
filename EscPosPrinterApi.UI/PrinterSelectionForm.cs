using EscPosPrinterApi.Core.Models;
using EscPosPrinterApi.Core.Services;

namespace EscPosPrinterApi.UI
{
    /// <summary>
    /// Formulário para seleção de impressora
    /// </summary>
    public partial class PrinterSelectionForm : Form
    {
        private readonly IPrinterService _printerService;
        private readonly byte[] _printData;
        private List<PrinterInfo> _printers = new();

        public PrintResponse Result { get; private set; } = new PrintResponse
        {
            Success = false,
            Cancelled = true,
            Message = "Operação cancelada pelo usuário"
        };

        public PrinterSelectionForm(IPrinterService printerService, byte[] printData)
        {
            _printerService = printerService;
            _printData = printData;
            
            InitializeComponent();
            LoadPrinters();
        }

        private async void LoadPrinters()
        {
            try
            {
                _printers = await _printerService.GetPrintersAsync();

                listBoxPrinters.Items.Clear();
                
                foreach (var printer in _printers)
                {
                    var displayText = printer.IsDefault 
                        ? $"{printer.Name} (Padrão)" 
                        : printer.Name;
                    
                    listBoxPrinters.Items.Add(displayText);
                    
                    // Seleciona a impressora padrão automaticamente
                    if (printer.IsDefault)
                    {
                        listBoxPrinters.SelectedIndex = listBoxPrinters.Items.Count - 1;
                    }
                }

                if (listBoxPrinters.Items.Count == 0)
                {
                    MessageBox.Show(
                        "Nenhuma impressora encontrada no sistema.",
                        "Aviso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao carregar impressoras: {ex.Message}",
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private async void ButtonConfirm_Click(object sender, EventArgs e)
        {
            await ConfirmPrint();
        }

        private async void ListBoxPrinters_DoubleClick(object sender, EventArgs e)
        {
            await ConfirmPrint();
        }

        private async Task ConfirmPrint()
        {
            if (listBoxPrinters.SelectedIndex < 0)
            {
                MessageBox.Show(
                    "Por favor, selecione uma impressora.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            var selectedPrinter = _printers[listBoxPrinters.SelectedIndex];

            try
            {
                buttonConfirm.Enabled = false;
                buttonCancel.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                bool success = await _printerService.PrintAsync(selectedPrinter.Name, _printData);

                Result = new PrintResponse
                {
                    Success = success,
                    Cancelled = false,
                    PrinterName = selectedPrinter.Name,
                    Message = success 
                        ? $"Impressão enviada com sucesso para {selectedPrinter.Name}" 
                        : "Falha ao enviar dados para impressora"
                };

                if (success)
                {
                    MessageBox.Show(
                        Result.Message,
                        "Sucesso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        Result.Message,
                        "Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                Result = new PrintResponse
                {
                    Success = false,
                    Cancelled = false,
                    PrinterName = selectedPrinter.Name,
                    Message = $"Erro ao imprimir: {ex.Message}"
                };

                MessageBox.Show(
                    Result.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                this.Cursor = Cursors.Default;
                buttonConfirm.Enabled = true;
                buttonCancel.Enabled = true;
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Result = new PrintResponse
            {
                Success = false,
                Cancelled = true,
                Message = "Operação cancelada pelo usuário"
            };

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
