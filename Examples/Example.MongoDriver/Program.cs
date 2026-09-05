using MongoDB.Driver;
using MongoDriver.Helpers;

Console.WriteLine("=== Demo: TL.MongoDriver.Helpers ===");

var client = new MongoClient("mongodb://localhost:27017");
var db = client.GetDatabase("demodb");
var collection = db.GetCollection<dynamic>("users");

Console.WriteLine("MongoDriver client initialized successfully.");
