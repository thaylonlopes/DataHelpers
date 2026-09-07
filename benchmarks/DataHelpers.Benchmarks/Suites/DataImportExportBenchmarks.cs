using BenchmarkDotNet.Attributes;
using DataImportExport.Helpers.Utils;

namespace DataHelpers.Benchmarks.Suites;

[MemoryDiagnoser]
public class DataImportExportBenchmarks
{
    private string _sampleLine = null!;
    private string[] _sampleFields = null!;

    [GlobalSetup]
    public void Setup()
    {
        _sampleLine = "10492,2026-09-05,Empresa Brasil SA,58940.75,true,Sao Paulo,BR,01310-200,Approved,948271";
        _sampleFields = ["10492", "2026-09-05", "Empresa Brasil SA", "58940.75", "true", "Sao Paulo", "BR", "01310-200", "Approved", "948271"];
    }

    [Benchmark(Baseline = true, Description = "1. Parsing: String Split Array")]
    public int Parse_DelimitedFields_Split()
    {
        var count = 0;
        var parts = _sampleLine.Split(',');
        for (int i = 0; i < parts.Length; i++)
        {
            count += parts[i].Length;
        }
        return count;
    }

    [Benchmark(Description = "1. Parsing: SpanDelimitedParser Zero-Alloc")]
    public int Parse_DelimitedFields_SpanParser()
    {
        var count = 0;
        foreach (var field in SpanDelimitedParser.EnumerateFields(_sampleLine.AsSpan(), ','))
        {
            count += field.Length;
        }
        return count;
    }

    [Benchmark(Description = "2. Primitives: Traditional Parse")]
    public (int id, DateTime date, decimal amount) Parse_Primitives_Traditional()
    {
        var parts = _sampleLine.Split(',');
        var id = int.Parse(parts[0]);
        var date = DateTime.Parse(parts[1]);
        var amount = decimal.Parse(parts[3]);
        return (id, date, amount);
    }

    [Benchmark(Description = "2. Primitives: SpanDelimitedParser")]
    public (int id, DateTime date, decimal amount) Parse_Primitives_Span()
    {
        var id = 0;
        var date = DateTime.MinValue;
        var amount = 0m;
        var index = 0;

        foreach (var field in SpanDelimitedParser.EnumerateFields(_sampleLine.AsSpan(), ','))
        {
            if (index == 0)
            {
                SpanDelimitedParser.TryParseInt(field, out id);
            }
            else if (index == 1)
            {
                SpanDelimitedParser.TryParseDateTime(field, out date);
            }
            else if (index == 3)
            {
                SpanDelimitedParser.TryParseDecimal(field, out amount);
            }
            index++;
        }
        return (id, date, amount);
    }

    [Benchmark(Description = "3. Export: String.Join to StringWriter")]
    public async Task<int> Export_Rows_StringJoin()
    {
        using var writer = new StringWriter();
        for (int i = 0; i < 50; i++)
        {
            await writer.WriteLineAsync(string.Join(",", _sampleFields));
        }
        return writer.GetStringBuilder().Length;
    }

    [Benchmark(Description = "3. Export: PooledBufferWriter ArrayPool")]
    public async Task<int> Export_Rows_PooledBufferWriter()
    {
        using var writer = new StringWriter();
        for (int i = 0; i < 50; i++)
        {
            await PooledBufferWriter.WriteDelimitedRowAsync(writer, _sampleFields, ',');
        }
        return writer.GetStringBuilder().Length;
    }
}
