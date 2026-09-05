using QueryBuilder.Helpers.PostgreSQL;

Console.WriteLine("=== Demo: TL.QueryBuilder.Helpers ===");

var queryBuilder = new PostgreSQLQueryBuilder();
string query = queryBuilder
    .Select("Id", "Name", "Age")
    .From("Users")
    .Where("Age > 18")
    .And("Active = true")
    .OrderBy("Name")
    .BuildQuery();

Console.WriteLine($"Generated PostgreSQL Query:\n{query}");
