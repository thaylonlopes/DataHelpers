using DataMapping.Helpers;

Console.WriteLine("=== Demo: TL.DataMapping ===");

var mapper = new SimpleMapper();
var src = new { Id = 1, Name = "Alice" };
var dest = mapper.Map<object, dynamic>(src);
Console.WriteLine($"Mapped object name: {dest.Name}");
