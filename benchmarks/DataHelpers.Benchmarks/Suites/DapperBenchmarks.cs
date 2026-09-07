using System.Data;
using BenchmarkDotNet.Attributes;
using Dapper.Helpers;
using Dapper.Helpers.Context;
using DataHelpers.Benchmarks.Models;
using Microsoft.Data.Sqlite;

namespace DataHelpers.Benchmarks.Suites;

[MemoryDiagnoser]
public class DapperBenchmarks
{
    private SqliteConnection _sharedConnection = null!;
    private DapperHelper _dapperHelper = null!;
    private DapperUnitOfWork _uow = null!;
    private readonly string _connectionString = "Data Source=DapperBench;Mode=Memory;Cache=Shared";

    [GlobalSetup]
    public void Setup()
    {
        _sharedConnection = new SqliteConnection(_connectionString);
        _sharedConnection.Open();

        using var cmd = _sharedConnection.CreateCommand();
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS Orders (Id INTEGER PRIMARY KEY, OrderNumber TEXT, Amount REAL);";
        cmd.ExecuteNonQuery();

        _dapperHelper = new DapperHelper(_sharedConnection);
        _uow = new DapperUnitOfWork(_sharedConnection);

        for (int i = 1; i <= 100; i++)
        {
            using var insertCmd = _sharedConnection.CreateCommand();
            insertCmd.CommandText = $"INSERT OR IGNORE INTO Orders VALUES ({i}, 'ORD-{i}', {i * 10.0});";
            insertCmd.ExecuteNonQuery();
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _sharedConnection.Dispose();
    }

    [Benchmark(Baseline = true, Description = "1. Connection: Open and Close Per Call")]
    public int Dapper_Connection_Reopen()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM Orders;";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    [Benchmark(Description = "1. Connection: DapperUnitOfWork Managed Connection")]
    public int Dapper_Connection_UnitOfWork()
    {
        using var cmd = _uow.Connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM Orders;";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    [Benchmark(Description = "2. Query: ADO.NET DbCommand Manual Reader")]
    public List<DapperOrderModel> Dapper_Query_ManualReader()
    {
        var list = new List<DapperOrderModel>();
        using var cmd = _sharedConnection.CreateCommand();
        cmd.CommandText = "SELECT Id, OrderNumber, Amount FROM Orders LIMIT 20;";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new DapperOrderModel
            {
                Id = reader.GetInt32(0),
                OrderNumber = reader.GetString(1),
                Amount = reader.GetDouble(2)
            });
        }
        return list;
    }

    [Benchmark(Description = "2. Query: DapperHelper QueryAsync Direct")]
    public async Task<List<DapperOrderModel>> Dapper_Query_DapperHelper()
    {
        var result = await _dapperHelper.QueryAsync<DapperOrderModel>("SELECT Id, OrderNumber, Amount FROM Orders LIMIT 20;");
        return result.ToList();
    }

    [Benchmark(Description = "3. Batch: Multiple Separate Commands Without Transaction")]
    public int Dapper_Batch_NonTransactional()
    {
        var count = 0;
        for (int i = 0; i < 10; i++)
        {
            using var cmd = _sharedConnection.CreateCommand();
            cmd.CommandText = $"SELECT Amount FROM Orders WHERE OrderNumber = 'ORD-{i}';";
            var res = cmd.ExecuteScalar();
            if (res != null) count++;
        }
        return count;
    }

    [Benchmark(Description = "3. Batch: Transactional Execution with Unit of Work")]
    public async Task<int> Dapper_Batch_TransactionalUoW()
    {
        var tx = await _uow.BeginTransactionAsync();
        var count = 0;
        try
        {
            for (int i = 0; i < 10; i++)
            {
                var res = await _dapperHelper.QuerySingleAsync<double>(
                    $"SELECT Amount FROM Orders WHERE OrderNumber = 'ORD-{i}';",
                    transaction: tx);
                if (res > 0) count++;
            }
            await _uow.CommitAsync();
        }
        catch
        {
            await _uow.RollbackAsync();
        }
        return count;
    }
}

public class DapperOrderModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public double Amount { get; set; }
}
