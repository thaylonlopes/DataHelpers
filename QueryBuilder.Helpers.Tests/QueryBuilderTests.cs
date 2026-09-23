using System.Security;
using System.Text.Json;
using FluentAssertions;
using QueryBuilder.Helpers.DynamoDB;
using QueryBuilder.Helpers.Enums;
using QueryBuilder.Helpers.MongoDB;
using QueryBuilder.Helpers.MySQL;
using QueryBuilder.Helpers.Oracle;
using QueryBuilder.Helpers.PostgreSQL;
using QueryBuilder.Helpers.Security;
using QueryBuilder.Helpers.SQLServer;
using Xunit;

namespace QueryBuilder.Helpers.Tests;

public class QueryBuilderTests
{
    [Fact]
    public void SQLServerQueryBuilder_ShouldBuildCorrectSelectQuery()
    {
        var builder = new SQLServerQueryBuilder();

        var sql = builder
            .Select("Id", "Nome", "Preco")
            .From("Produtos")
            .Where("Ativo = 1")
            .And("Preco > 50")
            .OrderBy("Preco", ascending: false)
            .BuildQuery();

        sql.Should().Be("SELECT Id, Nome, Preco FROM Produtos WHERE Ativo = 1 AND Preco > 50 ORDER BY [Preco] DESC");
    }

    [Fact]
    public void SQLServerQueryBuilder_ShouldSupportWindowFunctions_RowNumberAndRank()
    {
        var builder = new SQLServerQueryBuilder();

        var sql = builder
            .Select("Id", "Nome", "Salario")
            .WithRowNumber(partitionBy: "DepartamentoId", orderBy: "Salario DESC", alias: "SeqSalario")
            .WithRank(partitionBy: "DepartamentoId", orderBy: "Salario DESC", alias: "RankSalario")
            .From("Funcionarios")
            .BuildQuery();

        sql.Should().Contain("ROW_NUMBER() OVER (PARTITION BY DepartamentoId ORDER BY Salario DESC) AS SeqSalario");
        sql.Should().Contain("RANK() OVER (PARTITION BY DepartamentoId ORDER BY Salario DESC) AS RankSalario");
        sql.Should().Contain("FROM Funcionarios");
    }

    [Fact]
    public void SQLServerQueryBuilder_ShouldSupportJoinAndDistinct()
    {
        var builder = new SQLServerQueryBuilder();

        var sql = builder
            .Select("p.Id", "c.Nome")
            .Distinct()
            .From("Produtos p")
            .Join("Categorias c", "p.CategoriaId = c.Id", JoinType.Left)
            .BuildQuery();

        sql.Should().Contain("DISTINCT");
        sql.Should().Contain("LEFT JOIN Categorias c ON p.CategoriaId = c.Id");
    }

    [Fact]
    public void PostgreSQLQueryBuilder_ShouldBuildCorrectLimitAndOffset()
    {
        var builder = new PostgreSQLQueryBuilder();

        var sql = builder
            .Select("Id", "Email")
            .From("Usuarios")
            .Where("Ativo = true")
            .Limit(10)
            .Offset(20)
            .BuildQuery();

        sql.Should().Be("SELECT Id, Email FROM Usuarios WHERE Ativo = true LIMIT 10 OFFSET 20");
    }

    [Fact]
    public void MySQLQueryBuilder_ShouldBuildCorrectQueryWithGroupByAndHaving()
    {
        var builder = new MySQLQueryBuilder();

        var sql = builder
            .Select("CategoriaId", "COUNT(*) as Total")
            .From("Produtos")
            .GroupBy("CategoriaId")
            .Having("COUNT(*) > 5")
            .BuildQuery();

        sql.Should().Be("SELECT CategoriaId, COUNT(*) as Total FROM Produtos GROUP BY CategoriaId HAVING COUNT(*) > 5");
    }

    [Fact]
    public void OracleQueryBuilder_ShouldBuildCorrectSelect()
    {
        var builder = new OracleQueryBuilder();

        var sql = builder
            .Select("ID", "NOME")
            .From("CLIENTES")
            .Where("STATUS = 'A'")
            .BuildQuery();

        sql.Should().Be("SELECT ID, NOME FROM CLIENTES WHERE STATUS = 'A'");
    }

    [Fact]
    public void MongoDBQueryBuilder_ShouldBuildMongoQueryString()
    {
        var builder = new MongoDBQueryBuilder();

        var query = builder
            .Select("nome", "email")
            .From("usuarios")
            .Where("\"status\": \"ativo\"")
            .Limit(10)
            .BuildQuery();

        query.Should().NotBeNullOrWhiteSpace();
        query.Should().Contain("db.usuarios.find");
        query.Should().Contain("\"status\": \"ativo\"");
    }

    [Fact]
    public void MongoDBQueryBuilder_ShouldBuildAggregationPipeline()
    {
        var builder = new MongoDBQueryBuilder();
        var matchFilter = JsonDocument.Parse("{\"status\": \"aprovado\"}").RootElement;
        var groupFilter = JsonDocument.Parse("{\"_id\": \"$categoria\", \"total\": {\"$sum\": \"$valor\"}}").RootElement;

        var pipeline = builder
            .Aggregation()
            .Match(matchFilter)
            .Group(groupFilter)
            .BuildAggregationPipeline();

        pipeline.Should().HaveCount(2);
        pipeline[0].RootElement.ToString().Should().Contain("$match");
        pipeline[1].RootElement.ToString().Should().Contain("$group");
    }

    [Fact]
    public void DynamoDBQueryBuilder_ShouldBuildDynamoJsonQuery()
    {
        var builder = new DynamoDBQueryBuilder();
        builder.WithConditionExpression("PK = 'CLIENTE#100'");
        builder.UseIndex("GSI_Email");

        var query = builder
            .Select("Id", "Nome")
            .From("Clientes")
            .Limit(25)
            .BuildQuery();

        query.Should().NotBeNullOrWhiteSpace();
        query.Should().Contain("FROM Clientes");
        query.Should().Contain("KeyConditionExpression: \"PK = 'CLIENTE#100'\"");
        query.Should().Contain("IndexName: \"GSI_Email\"");
        query.Should().Contain("Limit: 25");
    }

    [Fact]
    public void QueryBuilders_WithPreAllocatedCapacity_ShouldGenerateCorrectQueries()
    {
        var sqlServer = new SQLServerQueryBuilder(512);
        var postgreSql = new PostgreSQLQueryBuilder(512);
        var mySql = new MySQLQueryBuilder(512);
        var oracle = new OracleQueryBuilder(512);

        var sql1 = sqlServer.Select("A", "B").From("Tab").Where("A = 1").BuildQuery();
        var sql2 = postgreSql.Select("A", "B").From("Tab").Where("A = 1").BuildQuery();
        var sql3 = mySql.Select("A", "B").From("Tab").Where("A = 1").BuildQuery();
        var sql4 = oracle.Select("A", "B").From("Tab").Where("A = 1").BuildQuery();

        sql1.Should().Be("SELECT A, B FROM Tab WHERE A = 1");
        sql2.Should().Be("SELECT A, B FROM Tab WHERE A = 1");
        sql3.Should().Be("SELECT A, B FROM Tab WHERE A = 1");
        sql4.Should().Be("SELECT A, B FROM Tab WHERE A = 1");
    }

    [Fact]
    public void QueryBuilder_WindowFunctions_ShouldFormatWithoutPartition()
    {
        var builder = new PostgreSQLQueryBuilder();
        var sql = builder
            .Select("Id")
            .WithRowNumber(partitionBy: "", orderBy: "Id ASC", alias: "RowNum")
            .WithRank(partitionBy: " ", orderBy: "Id ASC", alias: "RankNum")
            .From("Tab")
            .BuildQuery();

        sql.Should().Contain("ROW_NUMBER() OVER (ORDER BY Id ASC) AS RowNum");
        sql.Should().Contain("RANK() OVER (ORDER BY Id ASC) AS RankNum");
    }

    [Fact]
    public void QueryBuilders_SelectAndGroupBy_ShouldFormatMultipleColumnsCorrectly()
    {
        var builders = new QueryBuilder.Helpers.Core.IQueryBuilder[]
        {
            new SQLServerQueryBuilder(),
            new PostgreSQLQueryBuilder(),
            new MySQLQueryBuilder(),
            new OracleQueryBuilder()
        };

        foreach (var b in builders)
        {
            var query = b.Select("Categoria", "Status")
                         .From("Pedidos")
                         .GroupBy("Categoria", "Status")
                         .BuildQuery();

            query.Should().Contain("SELECT Categoria, Status FROM Pedidos GROUP BY Categoria, Status");
        }
    }

    [Fact]
    public void SqlIdentifierValidator_ShouldValidateAndEscapeDialects()
    {
        SqlIdentifierValidator.ValidateAndEscape("Preco", SqlDialect.SqlServer).Should().Be("[Preco]");
        SqlIdentifierValidator.ValidateAndEscape("Preco", SqlDialect.PostgreSql).Should().Be("\"Preco\"");
        SqlIdentifierValidator.ValidateAndEscape("Preco", SqlDialect.MySql).Should().Be("`Preco`");
        SqlIdentifierValidator.ValidateAndEscape("Preco", SqlDialect.Oracle).Should().Be("\"Preco\"");

        SqlIdentifierValidator.ValidateAndEscape("u.Email", SqlDialect.SqlServer).Should().Be("[u].[Email]");
        SqlIdentifierValidator.ValidateAndEscape("u.Email", SqlDialect.PostgreSql).Should().Be("\"u\".\"Email\"");
        SqlIdentifierValidator.ValidateAndEscape("u.Email", SqlDialect.MySql).Should().Be("`u`.`Email`");
    }

    [Theory]
    [InlineData("Id; DROP TABLE Usuarios--")]
    [InlineData("Nome' OR '1'='1")]
    [InlineData("Preco/*injetado*/")]
    [InlineData("Coluna\\Invalida")]
    [InlineData("123Invalido")]
    public void SqlIdentifierValidator_ShouldThrowSecurityException_OnMaliciousInput(string maliciousInput)
    {
        var act = () => SqlIdentifierValidator.Validate(maliciousInput);
        act.Should().Throw<SecurityException>();
    }

    [Fact]
    public void QueryBuilder_OrderBy_WithMaliciousInput_ShouldThrowSecurityException()
    {
        var sqlServer = new SQLServerQueryBuilder();
        var postgres = new PostgreSQLQueryBuilder();
        var mysql = new MySQLQueryBuilder();

        var act1 = () => sqlServer.OrderBy("Id; DROP TABLE Usuarios--");
        var act2 = () => postgres.OrderBy("Email; DELETE FROM Logins;--");
        var act3 = () => mysql.OrderBy("Nome' OR '1'='1");

        act1.Should().Throw<SecurityException>();
        act2.Should().Throw<SecurityException>();
        act3.Should().Throw<SecurityException>();
    }

    [Fact]
    public void QueryBuilder_WhereLike_WithEscapeWildcards_ShouldEscapeCorrectly()
    {
        var builder = new SQLServerQueryBuilder();
        var sql = builder
            .Select("Id", "Descricao")
            .From("Itens")
            .WhereLike("Descricao", "100%_desconto[extra]", escapeWildcards: true)
            .BuildQuery();

        sql.Should().Be("SELECT Id, Descricao FROM Itens WHERE Descricao LIKE '%100[%][_]desconto[[]extra]%'");
    }

    [Fact]
    public void MongoDBQueryBuilder_GroupBy_ShouldBuildValidJsonPipelineWithoutMongoBson()
    {
        var builder = new MongoDBQueryBuilder();
        builder.GroupBy("Categoria", "Status");

        var pipeline = builder.BuildAggregationPipeline();
        pipeline.Should().HaveCount(1);

        var json = pipeline[0].RootElement.GetRawText();
        json.Should().Contain("\"$group\"");
        json.Should().Contain("\"Categoria\": \"$Categoria\"");
        json.Should().Contain("\"Status\": \"$Status\"");
    }
}
