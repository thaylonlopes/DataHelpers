using System.Globalization;

namespace DataImportExport.Helpers.Utils;

/// <summary>
/// Utilitario de alta performance baseado em ReadOnlySpan para parsing de linhas delimitadas sem alocacoes na Heap.
/// </summary>
public static class SpanDelimitedParser
{
    public static SpanFieldEnumerator EnumerateFields(ReadOnlySpan<char> line, char delimiter = ',')
    {
        return new SpanFieldEnumerator(line, delimiter);
    }

    public static bool TryParseInt(ReadOnlySpan<char> span, out int value)
    {
        return int.TryParse(span.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseDecimal(ReadOnlySpan<char> span, out decimal value)
    {
        return decimal.TryParse(span.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseDouble(ReadOnlySpan<char> span, out double value)
    {
        return double.TryParse(span.Trim(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseDateTime(ReadOnlySpan<char> span, out DateTime value)
    {
        return DateTime.TryParse(span.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
    }

    public static bool TryParseGuid(ReadOnlySpan<char> span, out Guid value)
    {
        return Guid.TryParse(span.Trim(), out value);
    }

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

    public ref struct SpanFieldEnumerator
    {
        private ReadOnlySpan<char> _remaining;
        private readonly char _delimiter;
        private ReadOnlySpan<char> _current;
        private bool _started;

        public SpanFieldEnumerator(ReadOnlySpan<char> source, char delimiter)
        {
            _remaining = source;
            _delimiter = delimiter;
            _current = default;
            _started = false;
        }

        public readonly ReadOnlySpan<char> Current => _current;

        public readonly SpanFieldEnumerator GetEnumerator() => this;

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
