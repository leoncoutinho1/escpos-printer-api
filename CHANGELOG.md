# Changelog

## [1.1.0] - 2026-01-16

### Adicionado
- **Parâmetro `defaultPrinter`**: Novo campo booleano no `PrintRequest` que permite impressão direta na impressora padrão do sistema sem exibir o modal de seleção.
  - Quando `defaultPrinter` é `true`, a API imprime automaticamente na impressora padrão configurada no Windows.
  - Quando `defaultPrinter` é `false` ou omitido, o comportamento padrão é mantido (exibe modal de seleção).

### Modificado
- **PrintRequest.cs**: Adicionado campo `DefaultPrinter` (bool, padrão: false)
- **IPrinterService.cs**: Adicionado método `GetDefaultPrinterAsync()` para obter a impressora padrão
- **PrinterService.cs**: Implementado método `GetDefaultPrinterAsync()`
- **Program.cs (UI)**: 
  - Convertido para `async Task Main` para suportar operações assíncronas
  - Adicionada lógica para processar segundo argumento indicando uso da impressora padrão
  - Implementada impressão direta quando `defaultPrinter` é true
- **Program.cs (API)**: Modificado para passar parâmetro `defaultPrinter` ao executável da UI

### Documentação
- **README.md**: Adicionada documentação sobre o parâmetro `defaultPrinter`
- **EXAMPLES.md**: Adicionados exemplos de uso em JavaScript, TypeScript, C#, cURL e PowerShell
- **test-print-default.json**: Criado arquivo de exemplo demonstrando o uso do parâmetro

## [1.0.0] - 2026-01-13

### Inicial
- Implementação inicial da API ESC/POS Printer
- Suporte para impressão com seleção manual de impressora
- Endpoints: `/api/printers`, `/api/print`, `/health`
