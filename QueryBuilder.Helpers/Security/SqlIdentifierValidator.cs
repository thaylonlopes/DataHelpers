using System.Security;
using System.Text.RegularExpressions;

namespace QueryBuilder.Helpers.Security;

/// <summary>
/// Dialetos de banco de dados suportados para escape de identificadores SQL.
/// </summary>
public enum SqlDialect
{
    /// <summary>
    /// Microsoft SQL Server (delimitação por colchetes [coluna]).
    /// </summary>
    SqlServer,

    /// <summary>
    /// PostgreSQL (delimitação por aspas duplas "coluna").
    /// </summary>
    PostgreSql,

    /// <summary>
    /// MySQL / MariaDB (delimitação por crases `coluna`).
    /// </summary>
    MySql,

    /// <summary>
    /// Oracle Database (delimitação por aspas duplas "COLUNA").
    /// </summary>
    Oracle
}

/// <summary>
/// Utilitário de segurança para validação estrita de identificadores SQL e escape dialetal (anti-SQL Injection / CWE-89).
/// </summary>
public static class SqlIdentifierValidator
{
    private static readonly Regex ValidIdentifierRegex = new(
        @"^[a-zA-Z_][a-zA-Z0-9_.]*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly string[] DangerousSequences = { ";", "--", "/*", "*/", "'", "\"", "\\" };

    /// <summary>
    /// Valida se um identificador (tabela, coluna ou alias) é estritamente seguro.
    /// </summary>
    /// <param name="identifier">O identificador a ser validado.</param>
    /// <exception cref="ArgumentException">Lançada quando nulo ou vazio.</exception>
    /// <exception cref="SecurityException">Lançada quando caracteres maliciosos ou fora do padrão forem detectados.</exception>
    public static void Validate(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("O identificador SQL não pode ser nulo ou vazio.", nameof(identifier));
        }

        foreach (var seq in DangerousSequences)
        {
            if (identifier.Contains(seq, StringComparison.Ordinal))
            {
                throw new SecurityException($"Identificador SQL contém sequência maliciosa proibida: '{seq}'.");
            }
        }

        if (!ValidIdentifierRegex.IsMatch(identifier))
        {
            throw new SecurityException($"Identificador SQL inválido ou potencialmente malicioso: '{identifier}'.");
        }
    }

    /// <summary>
    /// Valida e aplica a delimitação correta do dialeto especificado.
    /// </summary>
    /// <param name="identifier">O identificador a ser validado e delimitado.</param>
    /// <param name="dialect">O dialeto SQL alvo.</param>
    /// <returns>O identificador devidamente sanitizado e delimitado.</returns>
    public static string ValidateAndEscape(string identifier, SqlDialect dialect)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("O identificador SQL não pode ser nulo ou vazio.", nameof(identifier));
        }

        var parts = identifier.Split('.');
        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i].Trim();
            if ((part.StartsWith('[') && part.EndsWith(']')) ||
                (part.StartsWith('"') && part.EndsWith('"')) ||
                (part.StartsWith('`') && part.EndsWith('`')))
            {
                part = part[1..^1];
            }

            Validate(part);
            parts[i] = EscapeSingle(part, dialect);
        }

        return string.Join(".", parts);
    }

    private static string EscapeSingle(string part, SqlDialect dialect)
    {
        return dialect switch
        {
            SqlDialect.SqlServer => $"[{part}]",
            SqlDialect.PostgreSql => $"\"{part}\"",
            SqlDialect.MySql => $"`{part}`",
            SqlDialect.Oracle => $"\"{part}\"",
            _ => $"[{part}]"
        };
    }

    /// <summary>
    /// Escapa caracteres coringa (%, _ e [) para prevenir ataques de Wildcard DoS / LIKE Injection (CWE-400).
    /// </summary>
    /// <param name="input">O texto a ser sanitizado para predicado LIKE.</param>
    /// <returns>O texto com caracteres coringa neutralizados.</returns>
    public static string EscapeLikeWildcards(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        return input
            .Replace("[", "[[]")
            .Replace("%", "[%]")
            .Replace("_", "[_]");
    }
}
