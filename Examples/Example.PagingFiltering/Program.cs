using PagingFiltering.Helpers.Implementations;

Console.WriteLine("=== Demo: TL.PagingFiltering.Helpers ===");

var helper = new PaginationHelper<string>();
var items = Enumerable.Range(1, 50).Select(i => $"Item_{i}").ToList();
var page = helper.ApplyPagination(items, pageNumber: 2, pageSize: 10);
Console.WriteLine($"Page {page.PageNumber} of {page.TotalPages}: {string.Join(", ", page.Items)}");
