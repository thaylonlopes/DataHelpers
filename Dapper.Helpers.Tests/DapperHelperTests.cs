using System.Data;
using Dapper;
using Dapper.Helpers;
using Dapper.Helpers.Context;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Dapper.Helpers.Tests;

public class DapperHelperTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DapperHelper _sut;

    public record Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public double Preco { get; set; }
        public int Ativo { get; set; }
    }

    public DapperHelperTests()
    {
        _connection = new SqliteConnection("Data Source=InMemoryDapperDb;Mode=Memory;Cache=Shared");
        _connection.Open();

        _connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Produtos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL,
                Preco REAL NOT NULL,
                Ativo INTEGER NOT NULL
            );");

        _sut = new DapperHelper(_connection);
    }

    public void Dispose()
    {
        _connection.Execute("DROP TABLE IF EXISTS Produtos;");
        _connection.Close();
        _connection.Dispose();
    }

    [Fact]
    public async Task InsertAsync_And_QueryAsync_ShouldInsertAndRetrieveEntities()
    {
        var produto = new Produto { Nome = "Teclado Mecânico", Preco = 350.0, Ativo = 1 };

        var rowsAffected = await _sut.InsertAsync("INSERT INTO Produtos (Nome, Preco, Ativo) VALUES (@Nome, @Preco, @Ativo)", produto);
        var produtos = (await _sut.QueryAsync<Produto>("SELECT * FROM Produtos WHERE Nome = @Nome", new { Nome = "Teclado Mecânico" })).ToList();

        rowsAffected.Should().Be(1);
        produtos.Should().HaveCount(1);
        produtos[0].Nome.Should().Be("Teclado Mecânico");
        produtos[0].Preco.Should().Be(350.0);
    }

    [Fact]
    public async Task QuerySingleAsync_ShouldReturnSingleEntityOrNull()
    {
        await _sut.InsertAsync("INSERT INTO Produtos (Nome, Preco, Ativo) VALUES (@Nome, @Preco, @Ativo)",
            new Produto { Nome = "Mouse Sem Fio", Preco = 120.0, Ativo = 1 });

        var found = await _sut.QuerySingleAsync<Produto>("SELECT * FROM Produtos WHERE Nome = @Nome", new { Nome = "Mouse Sem Fio" });
        var notFound = await _sut.QuerySingleAsync<Produto>("SELECT * FROM Produtos WHERE Nome = @Nome", new { Nome = "Inexistente" });

        found.Should().NotBeNull();
        found!.Nome.Should().Be("Mouse Sem Fio");
        notFound.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_And_DeleteAsync_ShouldModifyAndRemoveRecords()
    {
        await _sut.InsertAsync("INSERT INTO Produtos (Nome, Preco, Ativo) VALUES (@Nome, @Preco, @Ativo)",
            new Produto { Nome = "Monitor 4K", Preco = 2000.0, Ativo = 1 });

        var updated = await _sut.ExecuteAsync("UPDATE Produtos SET Preco = @Preco WHERE Nome = @Nome", new { Preco = 1800.0, Nome = "Monitor 4K" });
        var produtoAfterUpdate = await _sut.QuerySingleAsync<Produto>("SELECT * FROM Produtos WHERE Nome = @Nome", new { Nome = "Monitor 4K" });

        var deleted = await _sut.DeleteAsync("DELETE FROM Produtos WHERE Nome = @Nome", new { Nome = "Monitor 4K" });
        var produtoAfterDelete = await _sut.QuerySingleAsync<Produto>("SELECT * FROM Produtos WHERE Nome = @Nome", new { Nome = "Monitor 4K" });

        updated.Should().Be(1);
        produtoAfterUpdate!.Preco.Should().Be(1800.0);

        deleted.Should().Be(1);
        produtoAfterDelete.Should().BeNull();
    }

    [Fact]
    public async Task UnitOfWork_Commit_ShouldPersistTransactionData()
    {
        using var uow = new DapperUnitOfWork(_connection);
        var tx = await uow.BeginTransactionAsync();

        await _connection.ExecuteAsync("INSERT INTO Produtos (Nome, Preco, Ativo) VALUES (@Nome, @Preco, @Ativo)",
            new Produto { Nome = "Headset 7.1", Preco = 450.0, Ativo = 1 }, transaction: tx);
        await uow.CommitAsync();

        var saved = await _sut.QuerySingleAsync<Produto>("SELECT * FROM Produtos WHERE Nome = @Nome", new { Nome = "Headset 7.1" });
        saved.Should().NotBeNull();
        saved!.Preco.Should().Be(450.0);
    }

    [Fact]
    public async Task UnitOfWork_Rollback_ShouldRevertTransactionData()
    {
        using var uow = new DapperUnitOfWork(_connection);
        var tx = await uow.BeginTransactionAsync();

        await _connection.ExecuteAsync("INSERT INTO Produtos (Nome, Preco, Ativo) VALUES (@Nome, @Preco, @Ativo)",
            new Produto { Nome = "Gabinete RGB", Preco = 600.0, Ativo = 1 }, transaction: tx);
        await uow.RollbackAsync();

        var found = await _sut.QuerySingleAsync<Produto>("SELECT * FROM Produtos WHERE Nome = @Nome", new { Nome = "Gabinete RGB" });
        found.Should().BeNull();
    }

    [Fact]
    public async Task BulkInsertAsync_ShouldInsertBatchOfRecordsSuccessfully()
    {
        var batch = Enumerable.Range(1, 50).Select(i => new Produto
        {
            Nome = $"Produto_Bulk_{i}",
            Preco = 10.0 * i,
            Ativo = 1
        }).ToList();

        var inserted = await _sut.BulkInsertAsync("Produtos", batch, batchSize: 20);
        var count = await _connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Produtos WHERE Nome LIKE 'Produto_Bulk_%'");

        inserted.Should().Be(50);
        count.Should().Be(50);
    }

    [Fact]
    public async Task Methods_ShouldThrowArgumentException_OnNullOrWhiteSpaceSql()
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _sut.QueryAsync<Produto>(""));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _sut.QuerySingleAsync<Produto>("   "));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _sut.ExecuteAsync(null!));
    }
}
