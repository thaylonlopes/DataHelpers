# 📊 TL.DataImportExport.Helpers

[![NuGet](https://img.shields.io/nuget/v/TL.DataImportExport.Helpers.svg?style=flat-square&label=TL.DataImportExport.Helpers)](https://www.nuget.org/packages/TL.DataImportExport.Helpers/)
[![.NET](https://img.shields.io/badge/.NET-net8.0%20%7C%20net9.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE.txt)

> **High-throughput data import and export utilities for .NET: streaming CSV, Excel XLSX, JSON, XML, and fluent column mapping.**  
> *Utilitários de importação e exportação de dados em alta velocidade para .NET: streaming de CSV, planilhas Excel XLSX, JSON, XML e mapeamento fluente de colunas.*

O **`TL.DataImportExport.Helpers`** fornece abstrações unificadas para exportação e ingestão de grandes volumes de dados estruturados em múltiplos formatos corporativos (CSV, Excel `.xlsx`, JSON e XML), priorizando processamento em *streaming* para minimizar o consumo de memória.

---

## 📦 Instalação

Adicione o pacote ao seu projeto através do .NET CLI:

```bash
dotnet add package TL.DataImportExport.Helpers
```

---

## 🚀 Funcionalidades Principais

| Categoria | Componentes & Métodos | Descrição |
| :--- | :--- | :--- |
| **Sanitização de CSV** | `SafeCsvFormulaStringConverter` | Sanitização preventiva em células CSV prefixando strings que iniciam com `=`, `+`, `-`, `@`, `\t`, `\r` com `'` para neutralizar execução indevida de fórmulas. |
| **Guardrail de Memória Excel** | `MaxRowsLimit` (ClosedXML) | Limite configurável (padrão 15.000 linhas) no `ExcelDataImporter` disparando `InvalidOperationException` defensiva contra Out-Of-Memory (OOM). |
| **Logging Corporativo BCL** | `Microsoft.Extensions.Logging.ILogger` | Integração universal com `ILogger` e fallback seguro para `NullLogger.Instance`, erradicando `Console.WriteLine`. |
| **Manipulação de CSV** | `CsvDataImporter`, `CsvDataExporter` | Leitura e escrita via streaming baseadas no `CsvHelper`, com suporte a delimitadores e encodings customizados. |
| **Planilhas Excel (XLSX)** | `ExcelDataImporter`, `ExcelDataExporter` | Geração e parsing de planilhas formatadas via `ClosedXML` sem dependência de dependências nativas. |
| **Streaming JSON** | `JsonDataImporter`, `JsonDataExporter` | Processamento assíncrono de grandes arrays JSON diretamente a partir de `Stream` com `System.Text.Json`. |
| **Processamento XML** | `XmlDataImporter`, `XmlDataExporter` | Leitura e geração de XML tipado com proteção contra ataques de entidade externa (XXE). |
| **Mapeamento Fluente** | `ColumnMap<T>` | Configuração declarativa de mapeamento entre nomes de colunas do arquivo físico e propriedades do C#. |

---

## 💡 Exemplos de Uso

### 1. Injeção de Dependência no `Program.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;

builder.Services.AddDataImportExport();
```

### 2. Exportação Assíncrona para CSV e Excel (XLSX)

```csharp
using DataImportExport.Helpers;

public class RelatorioService
{
    private readonly CsvDataExporter _csvExporter;
    private readonly ExcelDataExporter _excelExporter;

    public RelatorioService(CsvDataExporter csvExporter, ExcelDataExporter excelExporter)
    {
        _csvExporter = csvExporter;
        _excelExporter = excelExporter;
    }

    public async Task GerarRelatoriosAsync(List<VendaDto> vendas)
    {
        await _csvExporter.ExportAsync(vendas, "relatorio_vendas.csv");
        await _excelExporter.ExportAsync(vendas, "relatorio_vendas.xlsx");
    }
}

public record VendaDto(string Protocolo, decimal Valor, DateTime Data);
```

---

## 🏛️ Decisões Arquiteturais e Segurança

Para detalhes sobre governança de streams, sanitização de fórmulas Excel e isolamento:
- 📄 [ADR-006: Decisões Arquiteturais do TL.DataImportExport.Helpers](../docs/adr/ADR-006-pacote-dataimportexport-helpers.md)

---

## 📄 Licença

Distribuído sob a licença [MIT](../LICENSE.txt).