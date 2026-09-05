using Dapper.Helpers;
using Microsoft.Data.Sqlite;

Console.WriteLine("=== Demo: TL.Dapper.Helpers with In-Memory SQLite ===");

using var connection = new SqliteConnection("Data Source=InMemorySample;Mode=Memory;Cache=Shared");
connection.Open();

var dapper = new DapperHelper(connection);

await dapper.ExecuteAsync("CREATE TABLE IF NOT EXISTS Users (Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);");

await dapper.ExecuteAsync("INSERT INTO Users (Name, Age) VALUES (@Name, @Age);", new { Name = "Alice", Age = 30 });
await dapper.ExecuteAsync("INSERT INTO Users (Name, Age) VALUES (@Name, @Age);", new { Name = "Bob", Age = 25 });

var users = await dapper.QueryAsync<dynamic>("SELECT * FROM Users");

Console.WriteLine("\nUsers retrieved:");
foreach (var u in users)
{
    Console.WriteLine($"Id={u.Id}, Name={u.Name}, Age={u.Age}");
}
