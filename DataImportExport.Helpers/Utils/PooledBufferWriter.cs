using System.Buffers;

namespace DataImportExport.Helpers.Utils;

/// <summary>
/// Utilitario de escrita de alta performance que utiliza ArrayPool para descarregar linhas delimitadas com zero alocacao persistente no Heap.
/// </summary>
public static class PooledBufferWriter
{
    private const int DefaultBufferSize = 4096;

    public static async Task WriteDelimitedRowAsync(TextWriter writer, IEnumerable<string?> fields, char delimiter = ',')
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(fields);

        var buffer = ArrayPool<char>.Shared.Rent(DefaultBufferSize);
        try
        {
            var position = 0;
            var isFirst = true;

            foreach (var field in fields)
            {
                if (!isFirst)
                {
                    if (position >= buffer.Length)
                    {
                        await writer.WriteAsync(buffer.AsMemory(0, position));
                        position = 0;
                    }
                    buffer[position++] = delimiter;
                }
                isFirst = false;

                if (string.IsNullOrEmpty(field))
                {
                    continue;
                }

                var fieldOffset = 0;
                var fieldLength = field.Length;

                while (fieldOffset < fieldLength)
                {
                    var available = buffer.Length - position;
                    if (available == 0)
                    {
                        await writer.WriteAsync(buffer.AsMemory(0, position));
                        position = 0;
                        available = buffer.Length;
                    }

                    var count = Math.Min(available, fieldLength - fieldOffset);
                    field.CopyTo(fieldOffset, buffer, position, count);
                    position += count;
                    fieldOffset += count;
                }
            }

            if (position > 0)
            {
                await writer.WriteAsync(buffer.AsMemory(0, position));
            }
            await writer.WriteLineAsync();
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
}
