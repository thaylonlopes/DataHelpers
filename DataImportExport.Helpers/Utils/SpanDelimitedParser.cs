using System.Globalization;

namespace DataImportExport.Helpers.Utils;

/// <summary>
/// Utilitário de alta performance baseado em ReadOnlySpan para parsing de linhas delimitadas sem alocações na Heap.
/// </summary>
public static class SpanDelimitedParser
{
    /// <summary>
    /// Cria um enumerador baseado em ref struct para iterar sobre os campos de uma linha delimitada com zero alocação de memória.
    /// </summary>
    /// <param name="line">O span de caracteres representando a linha completa.</param>
    /// <param name="delimiter">O caractere delimitador (padrão vírgula ',').</param>
    /// <returns>O enumerador <see cref="SpanFieldEnumerator"/> para iteração.</returns>
    public static SpanFieldEnumerator EnumerateFields(ReadOnlySpan<char> line, char delimiter = ',')
    {
        return new SpanFieldEnumerator(line, delimiter);
    }

    /// <summary>
    /// Tenta converter um span de caracteres em um inteiro de 32 bits invariante.
    /// </summary>
    /// <param name="span">O span contendo o texto numérico.</param>
    /// <param name="value">O valor inteiro resultante da conversão.</param>
    /// <returns><c>true</c> se a conversão foi bem-sucedida; caso contrário, <c>false</c>.</returns>
    public static bool TryParseInt(ReadOnlySpan<char> span, out int value)
    {
        return int.TryParse(span.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>
    /// Tenta converter um span de caracteres em um decimal invariante.
    /// </summary>
    /// <param name="span">O span contendo o texto numérico decimal.</param>
    /// <param name="value">O valor decimal resultante da conversão.</param>
    /// <returns><c>true</c> se a conversão foi bem-sucedida; caso contrário, <c>false</c>.</returns>
    public static bool TryParseDecimal(ReadOnlySpan<char> span, out decimal value)
    {
        return decimal.TryParse(span.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>
    /// Tenta converter um span de caracteres em um ponto flutuante de precisão dupla (double).
    /// </summary>
    /// <param name="span">O span contendo o texto numérico.</param>
    /// <param name="value">O valor double resultante da conversão.</param>
    /// <returns><c>true</c> se a conversão foi bem-sucedida; caso contrário, <c>false</c>.</returns>
    public static bool TryParseDouble(ReadOnlySpan<char> span, out double value)
    {
        return double.TryParse(span.Trim(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>
    /// Tenta converter um span de caracteres em um DateTime utilizando cultura invariante.
    /// </summary>
    /// <param name="span">O span contendo o texto da data/hora.</param>
    /// <param name="value">O valor DateTime resultante da conversão.</param>
    /// <returns><c>true</c> se a conversão foi bem-sucedida; caso contrário, <c>false</c>.</returns>
    public static bool TryParseDateTime(ReadOnlySpan<char> span, out DateTime value)
    {
        return DateTime.TryParse(span.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
    }

    /// <summary>
    /// Tenta converter um span de caracteres em um identificador exclusivo global (Guid).
    /// </summary>
    /// <param name="span">O span contendo a representação textual do Guid.</param>
    /// <param name="value">O Guid resultante da conversão.</param>
    /// <returns><c>true</c> se a conversão foi bem-sucedida; caso contrário, <c>false</c>.</returns>
    public static bool TryParseGuid(ReadOnlySpan<char> span, out Guid value)
    {
        return Guid.TryParse(span.Trim(), out value);
    }

    /// <summary>
    /// Tenta converter um span de caracteres em um booleano (suportando true/false e 1/0).
    /// </summary>
    /// <param name="span">O span contendo o texto booleano.</param>
    /// <param name="value">O valor booleano resultante da conversão.</param>
    /// <returns><c>true</c> se a conversão foi bem-sucedida; caso contrário, <c>false</c>.</returns>
    public static bool TryParseBool(ReadOnlySpan<char> span, out bool value)
    {
        var trimmed = span.Trim();
        if (bool.TryParse(trimmed, out value))
        {
            return true;
        }

        if (trimmed.Equals("1", StringComparison.OrdinalIgnoreCase))
        {
            value = true;
            return true;
        }

        if (trimmed.Equals("0", StringComparison.OrdinalIgnoreCase))
        {
            value = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Enumerador por ref struct para iterar sobre campos de texto delimitado sem alocações no Garbage Collector.
    /// </summary>
    public ref struct SpanFieldEnumerator
    {
        private ReadOnlySpan<char> _remaining;
        private readonly char _delimiter;
        private ReadOnlySpan<char> _current;
        private bool _started;

        /// <summary>
        /// Inicializa uma nova instância do enumerador sobre o span de caracteres de entrada.
        /// </summary>
        /// <param name="source">O span com os dados a serem delimitados.</param>
        /// <param name="delimiter">O caractere delimitador.</param>
        public SpanFieldEnumerator(ReadOnlySpan<char> source, char delimiter)
        {
            _remaining = source;
            _delimiter = delimiter;
            _current = default;
            _started = false;
        }

        /// <summary>
        /// Obtém o span referente ao campo atual da iteração.
        /// </summary>
        public readonly ReadOnlySpan<char> Current => _current;

        /// <summary>
        /// Retorna a própria instância do enumerador para suporte a instrução foreach.
        /// </summary>
        /// <returns>A instância do enumerador.</returns>
        public readonly SpanFieldEnumerator GetEnumerator() => this;

        /// <summary>
        /// Avança para o próximo campo delimitado da linha.
        /// </summary>
        /// <returns><c>true</c> se houver um próximo campo; caso contrário, <c>false</c>.</returns>
        public bool MoveNext()
        {
            if (!_started)
            {
                _started = true;
                if (_remaining.IsEmpty)
                {
                    return false;
                }
            }
            else if (_remaining.IsEmpty)
            {
                return false;
            }

            var delimiterIndex = _remaining.IndexOf(_delimiter);
            if (delimiterIndex == -1)
            {
                _current = _remaining;
                _remaining = default;
                return true;
            }

            _current = _remaining[..delimiterIndex];
            _remaining = _remaining[(delimiterIndex + 1)..];
            return true;
        }
    }
}
