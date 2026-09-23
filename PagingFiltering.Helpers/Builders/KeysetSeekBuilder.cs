using System.Linq.Expressions;
using System.Reflection;
using PagingFiltering.Helpers.Models;

namespace PagingFiltering.Helpers.Builders;

/// <summary>
/// Construtor fluente para paginação por busca de chave (Keyset Seek Method) com suporte a ordenações compostas mistas e validação defensiva de nulidade.
/// </summary>
/// <typeparam name="T">O tipo da entidade a ser paginada.</typeparam>
public class KeysetSeekBuilder<T>
{
    private readonly List<IKeysetColumnRule<T>> _rules = new();

    /// <summary>
    /// Define a chave primária de ordenação.
    /// </summary>
    public KeysetSeekBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool ascending = true) where TKey : IComparable<TKey>
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ValidateNotNullable(keySelector);

        _rules.Clear();
        _rules.Add(new KeysetColumnRule<T, TKey>(keySelector.Compile(), ascending, typeof(TKey)));
        return this;
    }

    /// <summary>
    /// Define a chave primária de ordenação com validação reflexiva de nulidade.
    /// </summary>
    public KeysetSeekBuilder<T> OrderBy(Expression<Func<T, object?>> keySelector, bool ascending = true)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ValidateNotNullable(keySelector);

        _rules.Clear();
        var compiled = keySelector.Compile();
        _rules.Add(new KeysetObjectRule<T>(compiled, ascending));
        return this;
    }

    /// <summary>
    /// Define uma chave secundária para desempate ordenado.
    /// </summary>
    public KeysetSeekBuilder<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector, bool ascending = true) where TKey : IComparable<TKey>
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ValidateNotNullable(keySelector);

        if (_rules.Count == 0)
        {
            throw new InvalidOperationException("Deve invocar OrderBy antes de ThenBy.");
        }

        _rules.Add(new KeysetColumnRule<T, TKey>(keySelector.Compile(), ascending, typeof(TKey)));
        return this;
    }

    /// <summary>
    /// Define uma chave secundária para desempate ordenado com validação reflexiva de nulidade.
    /// </summary>
    public KeysetSeekBuilder<T> ThenBy(Expression<Func<T, object?>> keySelector, bool ascending = true)
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ValidateNotNullable(keySelector);

        if (_rules.Count == 0)
        {
            throw new InvalidOperationException("Deve invocar OrderBy antes de ThenBy.");
        }

        var compiled = keySelector.Compile();
        _rules.Add(new KeysetObjectRule<T>(compiled, ascending));
        return this;
    }

    private static void ValidateNotNullable(LambdaExpression keySelector)
    {
        var body = keySelector.Body;
        if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
        {
            body = unary.Operand;
        }

        if (Nullable.GetUnderlyingType(body.Type) != null)
        {
            throw new InvalidOperationException("Colunas da chave de busca (Keyset Seek) devem ser obrigatoriamente NOT NULL para garantir determinismo SQL.");
        }

        if (body is MemberExpression memberExpr && memberExpr.Member is PropertyInfo propInfo)
        {
            if (Nullable.GetUnderlyingType(propInfo.PropertyType) != null)
            {
                throw new InvalidOperationException("Colunas da chave de busca (Keyset Seek) devem ser obrigatoriamente NOT NULL para garantir determinismo SQL.");
            }
        }
    }

    /// <summary>
    /// Executa a paginação Keyset sobre a coleção de origem.
    /// </summary>
    public IEnumerable<T> ApplySeek(IEnumerable<T> source, object?[]? lastCursorValues, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "O tamanho da página deve ser maior que zero.");
        if (_rules.Count == 0) throw new InvalidOperationException("Nenhuma chave de ordenação foi configurada.");

        var ordered = OrderCollection(source);

        if (lastCursorValues != null && lastCursorValues.Length > 0)
        {
            ordered = FilterAfterCursor(ordered, lastCursorValues);
        }

        return ordered.Take(pageSize);
    }

    private IEnumerable<T> OrderCollection(IEnumerable<T> source)
    {
        IOrderedEnumerable<T>? ordered = null;
        for (int i = 0; i < _rules.Count; i++)
        {
            ordered = _rules[i].ApplyOrdering(source, ordered, i == 0);
        }
        return ordered ?? source;
    }

    private IEnumerable<T> FilterAfterCursor(IEnumerable<T> source, object?[] cursorValues)
    {
        return source.Where(item => IsAfterCursor(item, cursorValues));
    }

    private bool IsAfterCursor(T item, object?[] cursorValues)
    {
        for (int i = 0; i < _rules.Count; i++)
        {
            var rule = _rules[i];
            var cursorVal = i < cursorValues.Length ? cursorValues[i] : null;
            if (cursorVal == null) continue;

            var cmp = rule.CompareItemToCursor(item, cursorVal);

            if (rule.IsAscending)
            {
                if (cmp > 0) return true;
                if (cmp < 0) return false;
            }
            else
            {
                if (cmp < 0) return true;
                if (cmp > 0) return false;
            }
        }
        return false;
    }

    private interface IKeysetColumnRule<TEntity>
    {
        bool IsAscending { get; }
        IOrderedEnumerable<TEntity> ApplyOrdering(IEnumerable<TEntity> source, IOrderedEnumerable<TEntity>? current, bool isFirst);
        int CompareItemToCursor(TEntity item, object cursorValue);
    }

    private class KeysetColumnRule<TEntity, TKey> : IKeysetColumnRule<TEntity> where TKey : IComparable<TKey>
    {
        private readonly Func<TEntity, TKey> _getter;
        public bool IsAscending { get; }
        public Type KeyType { get; }

        public KeysetColumnRule(Func<TEntity, TKey> getter, bool ascending, Type keyType)
        {
            _getter = getter;
            IsAscending = ascending;
            KeyType = keyType;
        }

        public IOrderedEnumerable<TEntity> ApplyOrdering(IEnumerable<TEntity> source, IOrderedEnumerable<TEntity>? current, bool isFirst)
        {
            if (isFirst || current == null)
            {
                return IsAscending ? source.OrderBy(_getter) : source.OrderByDescending(_getter);
            }
            return IsAscending ? current.ThenBy(_getter) : current.ThenByDescending(_getter);
        }

        public int CompareItemToCursor(TEntity item, object cursorValue)
        {
            var itemValue = _getter(item);
            var typedCursor = (TKey)Convert.ChangeType(cursorValue, typeof(TKey));
            return itemValue.CompareTo(typedCursor);
        }
    }

    private class KeysetObjectRule<TEntity> : IKeysetColumnRule<TEntity>
    {
        private readonly Func<TEntity, object?> _getter;
        public bool IsAscending { get; }

        public KeysetObjectRule(Func<TEntity, object?> getter, bool ascending)
        {
            _getter = getter;
            IsAscending = ascending;
        }

        public IOrderedEnumerable<TEntity> ApplyOrdering(IEnumerable<TEntity> source, IOrderedEnumerable<TEntity>? current, bool isFirst)
        {
            if (isFirst || current == null)
            {
                return IsAscending ? source.OrderBy(x => (IComparable?)_getter(x)) : source.OrderByDescending(x => (IComparable?)_getter(x));
            }
            return IsAscending ? current.ThenBy(x => (IComparable?)_getter(x)) : current.ThenByDescending(x => (IComparable?)_getter(x));
        }

        public int CompareItemToCursor(TEntity item, object cursorValue)
        {
            var itemValue = _getter(item);
            if (itemValue is IComparable comp)
            {
                var converted = Convert.ChangeType(cursorValue, itemValue.GetType());
                return comp.CompareTo(converted);
            }
            return 0;
        }
    }
}
