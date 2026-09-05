namespace DataImportExport.Helpers.Utils
{
    public static class FileUtils
    {
        public static async Task<IEnumerable<string>> ReadLinesAsync(string filePath)
        {
            var lines = new List<string>();
            using var reader = new StreamReader(filePath);
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line is null)
                {
                    break;
                }
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
