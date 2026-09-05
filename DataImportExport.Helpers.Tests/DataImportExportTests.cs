using System.Globalization;
using System.IO;
using ClosedXML.Excel;
using CsvHelper.Configuration;
using DataImportExport.Helpers;
using DataImportExport.Helpers.Models;
using FluentAssertions;
using Xunit;

namespace DataImportExport.Helpers.Tests;

public class DataImportExportTests
{
    public class SampleRecord
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Valor { get; set; }
    }

    [Fact]
    public async Task CsvExporterAndImporter_ShouldExportAndImportCorrectly()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");
        var csvSettings = new CsvSettings();
        var exporter = new CsvDataExporter(csvSettings);
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
        var logger = new SimpleLogger();
        var importer = new CsvDataImporter(logger, csvConfig, csvSettings);

        var records = new List<SampleRecord>
        {
            new() { Id = 1, Nome = "Produto A", Valor = 100.50m },
            new() { Id = 2, Nome = "Produto B", Valor = 250.00m }
        };

        try
        {
            await exporter.ExportAsync(tempFile, records);
            var imported = (await importer.ImportAsync<SampleRecord>(tempFile)).ToList();

            imported.Should().HaveCount(2);
            imported[0].Id.Should().Be(1);
            imported[0].Nome.Should().Be("Produto A");
            imported[0].Valor.Should().Be(100.50m);
            imported[1].Id.Should().Be(2);
            imported[1].Nome.Should().Be("Produto B");
            imported[1].Valor.Should().Be(250.00m);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task JsonExporterAndImporter_ShouldExportAndImportCorrectly()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        var logger = new SimpleLogger();
        var config = new JsonConfigurationOptions();
        var exporter = new JsonDataExporter(logger, config);
        var importer = new JsonDataImporter(logger, config);
        var records = new List<SampleRecord>
        {
            new() { Id = 10, Nome = "Serviço X", Valor = 999.90m }
        };

        try
        {
            await exporter.ExportAsync(tempFile, records);
            var imported = (await importer.ImportAsync<SampleRecord>(tempFile)).ToList();

            imported.Should().HaveCount(1);
            imported[0].Id.Should().Be(10);
            imported[0].Nome.Should().Be("Serviço X");
            imported[0].Valor.Should().Be(999.90m);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ExcelExporterAndImporter_ShouldExportAndImportCorrectly()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");
        var exporter = new ExcelDataExporter();
        var importer = new ExcelDataImporter();
        var records = new List<SampleRecord>
        {
            new() { Id = 100, Nome = "Item Excel", Valor = 50.0m }
        };

        try
        {
            await exporter.ExportAsync(tempFile, records);
            var imported = (await importer.ImportAsync<SampleRecord>(tempFile)).ToList();

            imported.Should().HaveCount(1);
            imported[0].Id.Should().Be(100);
            imported[0].Nome.Should().Be("Item Excel");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task XmlExporterAndImporter_ShouldExportAndImportViaStreaming()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xml");
        var exporter = new XmlDataExporter();
        var importer = new XmlDataImporter();
        var records = new List<SampleRecord>
        {
            new() { Id = 50, Nome = "XML Item 1", Valor = 300.0m },
            new() { Id = 51, Nome = "XML Item 2", Valor = 450.0m }
        };

        try
        {
            await exporter.ExportAsync(tempFile, records);
            var imported = (await importer.ImportAsync<SampleRecord>(tempFile)).ToList();

            imported.Should().HaveCount(2);
            imported[0].Id.Should().Be(50);
            imported[0].Nome.Should().Be("XML Item 1");
            imported[0].Valor.Should().Be(300.0m);
            imported[1].Id.Should().Be(51);
            imported[1].Nome.Should().Be("XML Item 2");
            imported[1].Valor.Should().Be(450.0m);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ColumnMap_ShouldMapPropertyAndColumnAliasesCorrectly()
    {
        var map = new ColumnMap<SampleRecord>()
            .Map(x => x.Id, "CODIGO_PRODUTO")
            .Map(x => x.Nome, "DESCRICAO");

        map.GetColumnName("Id").Should().Be("CODIGO_PRODUTO");
        map.GetColumnName("Nome").Should().Be("DESCRICAO");
        map.GetColumnName("Valor").Should().Be("Valor");

        map.GetPropertyName("CODIGO_PRODUTO").Should().Be("Id");
        map.GetPropertyName("DESCRICAO").Should().Be("Nome");
    }

    [Fact]
    public void SpanDelimitedParser_EnumerateFields_ShouldExtractAllTokensWithoutHeapAllocation()
    {
        ReadOnlySpan<char> line = "1001,Notebook Pro,4500.50,true,2026-09-05";
        var fields = new List<string>();

        foreach (var field in Utils.SpanDelimitedParser.EnumerateFields(line, ','))
        {
            fields.Add(field.ToString());
        }

        fields.Should().HaveCount(5);
        fields[0].Should().Be("1001");
        fields[1].Should().Be("Notebook Pro");
        fields[2].Should().Be("4500.50");
        fields[3].Should().Be("true");
        fields[4].Should().Be("2026-09-05");
    }

    [Fact]
    public void SpanDelimitedParser_TypeParsers_ShouldConvertPrimitivesCorrectly()
    {
        Utils.SpanDelimitedParser.TryParseInt(" 12345 ".AsSpan(), out var intVal).Should().BeTrue();
        intVal.Should().Be(12345);

        Utils.SpanDelimitedParser.TryParseDecimal(" 199.99 ".AsSpan(), out var decVal).Should().BeTrue();
        decVal.Should().Be(199.99m);

        Utils.SpanDelimitedParser.TryParseDouble(" 3.1415 ".AsSpan(), out var dblVal).Should().BeTrue();
        dblVal.Should().BeApproximately(3.1415, 0.0001);

        Utils.SpanDelimitedParser.TryParseBool(" true ".AsSpan(), out var boolVal).Should().BeTrue();
        boolVal.Should().BeTrue();

        Utils.SpanDelimitedParser.TryParseBool(" 1 ".AsSpan(), out var boolValNumeric).Should().BeTrue();
        boolValNumeric.Should().BeTrue();

        Utils.SpanDelimitedParser.TryParseGuid("d3b07384-d113-46d8-9999-73de8fed0001".AsSpan(), out var guidVal).Should().BeTrue();
        guidVal.Should().Be(Guid.Parse("d3b07384-d113-46d8-9999-73de8fed0001"));
    }

    [Fact]
    public async Task FileUtils_StreamLinesAsync_ShouldYieldLinesCorrectly()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.txt");
        var expectedLines = new List<string> { "Linha 1", "Linha 2", "Linha 3" };

        try
        {
            await Utils.FileUtils.WriteLinesAsync(tempFile, expectedLines);

            var readLines = new List<string>();
            await foreach (var line in Utils.FileUtils.StreamLinesAsync(tempFile))
            {
                readLines.Add(line);
            }

            readLines.Should().BeEquivalentTo(expectedLines);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
