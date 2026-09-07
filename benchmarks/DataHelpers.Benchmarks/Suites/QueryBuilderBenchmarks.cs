using System.Text;
using BenchmarkDotNet.Attributes;
using QueryBuilder.Helpers.SQLServer;

namespace DataHelpers.Benchmarks.Suites;

[MemoryDiagnoser]
public class QueryBuilderBenchmarks
{
    private string[] _columns = null!;

    [GlobalSetup]
    public void Setup()
    {
        _columns = ["Id", "OrderNumber", "CustomerName", "CustomerEmail", "TotalAmount", "CreatedAt", "Status", "City"];
    }

    [Benchmark(Baseline = true, Description = "1. Query: String Interpolation Naive")]
    public string Build_Query_StringInterpolation()
    {
        var cols = string.Join(", ", _columns);
        return $"SELECT {cols} FROM Orders WHERE TotalAmount >= 1000.00 AND Status = 'Processing' ORDER BY TotalAmount DESC";
    }

    [Benchmark(Description = "1. Query: SQLServerQueryBuilder Pre-Allocated")]
    public string Build_Query_QueryBuilder()
    {
        return new SQLServerQueryBuilder(256)
            .Select(_columns)
            .From("Orders")
            .Where("TotalAmount >= 1000.00")
            .And("Status = 'Processing'")
            .OrderBy("TotalAmount", false)
            .BuildQuery();
    }

    [Benchmark(Description = "2. Columns: String.Join Projection")]
    public string Build_Columns_StringJoin()
    {
        var sb = new StringBuilder("SELECT ");
        sb.Append(string.Join(", ", _columns));
        sb.Append(" FROM Orders");
        return sb.ToString();
    }

    [Benchmark(Description = "2. Columns: QueryBuilder AppendColumns")]
    public string Build_Columns_AppendColumns()
    {
        return new SQLServerQueryBuilder()
            .Select(_columns)
            .From("Orders")
            .BuildQuery();
    }

    [Benchmark(Description = "3. WindowFunc: Interpolated String")]
    public string Build_WindowFunctions_Interpolated()
    {
        var sb = new StringBuilder();
        sb.Append("ROW_NUMBER() OVER (PARTITION BY CustomerId ORDER BY TotalAmount DESC) AS RowNum, ");
        sb.Append("RANK() OVER (PARTITION BY CustomerId ORDER BY TotalAmount DESC) AS OrderRank");
        return sb.ToString();
    }

    [Benchmark(Description = "3. WindowFunc: QueryBuilder WithRowNumber and WithRank")]
    public string Build_WindowFunctions_QueryBuilder()
    {
        return new SQLServerQueryBuilder()
            .Select("Id")
            .WithRowNumber("CustomerId", "TotalAmount DESC", "RowNum")
            .WithRank("CustomerId", "TotalAmount DESC", "OrderRank")
            .From("Orders")
            .BuildQuery();
    }
}
