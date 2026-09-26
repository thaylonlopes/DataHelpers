using System.Buffers;

namespace DataImportExport.Helpers.Utils;

/// <summary>
/// Utilitário de escrita de alta performance que utiliza ArrayPool para descarregar linhas delimitadas com zero alocação persistente no Heap.
/// </summary>
public static class PooledBufferWriter
{
    private const int DefaultBufferSize = 4096;

    /// <summary>
    /// Escreve uma linha com campos delimitados de forma assíncrona em um <see cref="TextWriter"/> utilizando buffer reciclado via <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <param name="writer">O fluxo de escrita de texto de destino.</param>
    /// <param name="fields">A coleção de campos textuais a serem gravados.</param>
    /// <param name="delimiter">O caractere delimitador entre as colunas (padrão vírgula ',').</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da escrita.</returns>
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
