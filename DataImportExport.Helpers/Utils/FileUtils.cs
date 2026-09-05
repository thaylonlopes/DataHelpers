namespace DataImportExport.Helpers.Utils
{
    public static class FileUtils
    {
        public static async Task<IEnumerable<string>> ReadLinesAsync(string filePath)
        {
            var lines = new List<string>();
            using var reader = new StreamReader(filePath);
            string? line;
            while ((line = await reader.ReadLineAsync()) is not null)
            {
                lines.Add(line);
            }
            return lines;
        }

        public static async Task WriteLinesAsync(string filePath, IEnumerable<string> lines)
        {
            using var writer = new StreamWriter(filePath);
            foreach (var line in lines)
            {
                await writer.WriteLineAsync(line ?? string.Empty);
            }
        }
    }
}
