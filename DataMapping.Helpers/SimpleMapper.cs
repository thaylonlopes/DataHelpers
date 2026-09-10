using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using DataMapping.Helpers.Common;
using DataMapping.Helpers.Interfaces;

namespace DataMapping.Helpers;

/// <summary>
/// Representa um par ordenado de tipos imutável utilizado como chave para indexação e busca de delegates de mapeamento em cache.
/// </summary>
public readonly record struct TypePair(Type SourceType, Type DestinationType);

/// <summary>
/// Cache delimitado thread-safe com política de evicção LRU (Least Recently Used) para armazenamento de delegates compilados.
/// Previne esgotamento de memória e sobrecarga do heap sob criação dinâmica de tipos.
/// </summary>
public class BoundedMappingCache
{
    private sealed class CacheNode
    {
        public TypePair Key { get; }
        public Delegate Value { get; set; }

        public CacheNode(TypePair key, Delegate value)
        {
            Key = key;
            Value = value;
        }
    }

    /// <summary>
    /// Capacidade máxima padrão do cache delimitado (2048 pares de tipos mapeados).
    /// </summary>
    public const int DefaultCapacity = 2048;

    private readonly object _syncRoot = new();
    private readonly int _capacity;
    private readonly Dictionary<TypePair, LinkedListNode<CacheNode>> _map;
    private readonly LinkedList<CacheNode> _lruOrder;

    /// <summary>
    /// Obtém a capacidade máxima configurada para este cache.
    /// </summary>
    public int Capacity => _capacity;

    /// <summary>
    /// Obtém o número atual de entradas ativas armazenadas no cache.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_syncRoot)
            {
                return _map.Count;
            }
        }
    }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="BoundedMappingCache"/> com a capacidade especificada.
    /// </summary>
    /// <param name="capacity">Limite máximo de entradas antes de acionar a evicção LRU.</param>
    /// <exception cref="ArgumentOutOfRangeException">Lançada quando a capacidade for menor ou igual a zero.</exception>
    public BoundedMappingCache(int capacity = DefaultCapacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "A capacidade do cache deve ser maior que zero.");
        }

        _capacity = capacity;
        _map = new Dictionary<TypePair, LinkedListNode<CacheNode>>(capacity);
        _lruOrder = new LinkedList<CacheNode>();
    }

    /// <summary>
    /// Obtém o delegate compilado correspondente ao par de tipos ou compila e adiciona atomicamente de forma thread-safe.
    /// </summary>
    /// <param name="key">Par de tipos fonte e destino.</param>
    /// <param name="valueFactory">Fábrica de compilação do delegate caso não resida no cache.</param>
    /// <returns>O delegate compilado.</returns>
    public Delegate GetOrAdd(TypePair key, Func<TypePair, Delegate> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(valueFactory);

        lock (_syncRoot)
        {
            if (_map.TryGetValue(key, out var existingNode))
            {
                PromoteToMostRecent(existingNode);
                return existingNode.Value.Value;
            }

            var compiledDelegate = valueFactory(key);
            AddInternal(key, compiledDelegate);
            return compiledDelegate;
        }
    }

    /// <summary>
    /// Limpa todas as entradas armazenadas no cache.
    /// </summary>
    public void Clear()
    {
        lock (_syncRoot)
        {
            _map.Clear();
            _lruOrder.Clear();
        }
    }

    /// <summary>
    /// Verifica se uma determinada chave de tipos está presente no cache.
    /// </summary>
    /// <param name="key">Par de tipos a pesquisar.</param>
    /// <returns>Verdadeiro se presente no cache, falso caso contrário.</returns>
    public bool ContainsKey(TypePair key)
    {
        lock (_syncRoot)
        {
            return _map.ContainsKey(key);
        }
    }

    private void AddInternal(TypePair key, Delegate value)
    {
        if (_map.Count >= _capacity)
        {
            EvictLeastRecentlyUsed();
        }

        var node = new LinkedListNode<CacheNode>(new CacheNode(key, value));
        _lruOrder.AddFirst(node);
        _map[key] = node;
    }

    private void PromoteToMostRecent(LinkedListNode<CacheNode> node)
    {
        if (node != _lruOrder.First)
        {
            _lruOrder.Remove(node);
            _lruOrder.AddFirst(node);
        }
    }

    private void EvictLeastRecentlyUsed()
    {
        var oldestNode = _lruOrder.Last;
        if (oldestNode != null)
        {
            _lruOrder.RemoveLast();
            _map.Remove(oldestNode.Value.Key);
        }
    }
}

/// <summary>
/// Mapeador de objetos ultrarrápido baseado em árvores de expressões compiladas com cache thread-safe delimitado LRU em memória e suporte a tipos aninhados, coleções e conversores customizados.
/// </summary>
public class SimpleMapper : IMapper
{
    private static BoundedMappingCache _mapCache = new(BoundedMappingCache.DefaultCapacity);
    private static readonly ConcurrentDictionary<TypePair, Delegate> _customConverters = new();

    /// <summary>
    /// Obtém a capacidade máxima configurada do cache de delegates de mapeamento.
    /// </summary>
    public static int CacheCapacity => _mapCache.Capacity;

    /// <summary>
    /// Obtém a quantidade atual de pares de tipos compilados em cache.
    /// </summary>
    public static int CacheCount => _mapCache.Count;

    /// <summary>
    /// Reconfigura a capacidade máxima do cache delimitado LRU de delegates de mapeamento.
    /// </summary>
    /// <param name="capacity">Nova capacidade máxima estrita.</param>
    public static void ConfigureCacheCapacity(int capacity)
    {
        _mapCache = new BoundedMappingCache(capacity);
    }

    /// <summary>
    /// Limpa todas as entradas de delegates do cache de mapeamentos.
    /// </summary>
    public static void ClearCache()
    {
        _mapCache.Clear();
    }

    /// <summary>
    /// Registra uma função conversora customizada de tipo.
    /// </summary>
    /// <typeparam name="TSource">Tipo de origem.</typeparam>
    /// <typeparam name="TDestination">Tipo de destino.</typeparam>
    /// <param name="converter">A função de conversão.</param>
    public static void RegisterConverter<TSource, TDestination>(Func<TSource, TDestination> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        _customConverters[new TypePair(typeof(TSource), typeof(TDestination))] = converter;
        _mapCache.Clear();
    }

    /// <summary>
    /// Registra uma instância de conversor customizado.
    /// </summary>
    /// <typeparam name="TSource">Tipo de origem.</typeparam>
    /// <typeparam name="TDestination">Tipo de destino.</typeparam>
    /// <param name="converter">Instância de <see cref="ITypeConverter{TSource, TDestination}"/>.</param>
    public static void RegisterConverter<TSource, TDestination>(ITypeConverter<TSource, TDestination> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        RegisterConverter<TSource, TDestination>(converter.Convert);
    }

    /// <summary>
    /// Mapeia as propriedades públicas compatíveis de <typeparamref name="TSource"/> para uma nova instância de <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">O tipo de origem.</typeparam>
    /// <typeparam name="TDestination">O tipo de destino com construtor padrão acessível.</typeparam>
    /// <param name="source">O objeto de origem a ser mapeado.</param>
    /// <returns>A instância mapeada de destino.</returns>
    /// <exception cref="ArgumentNullException">Lançada quando a instância de origem for nula.</exception>
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var key = new TypePair(typeof(TSource), typeof(TDestination));
        var mapDelegate = (Func<TSource, TDestination>)_mapCache.GetOrAdd(key, _ => CreateMapDelegate<TSource, TDestination>());

        return mapDelegate(source);
    }

    /// <summary>
    /// Mapeia uma coleção inteira de objetos de origem para uma nova lista de objetos de destino.
    /// </summary>
    /// <typeparam name="TSource">O tipo do item de origem.</typeparam>
    /// <typeparam name="TDestination">O tipo do item de destino.</typeparam>
    /// <param name="source">A coleção de origem.</param>
    /// <returns>Uma lista contendo os itens mapeados.</returns>
    public IEnumerable<TDestination> MapCollection<TSource, TDestination>(IEnumerable<TSource> source)
    {
        if (source == null)
        {
            return Enumerable.Empty<TDestination>();
        }

        var list = new List<TDestination>();
        foreach (var item in source)
        {
            if (item != null)
            {
                list.Add(Map<TSource, TDestination>(item));
            }
        }
        return list;
    }

    /// <summary>
    /// Tenta mapear o objeto de origem retornando um <see cref="Result{T}"/> sem lançar exceções.
    /// </summary>
    /// <typeparam name="TSource">O tipo do objeto de origem.</typeparam>
    /// <typeparam name="TDestination">O tipo do objeto de destino.</typeparam>
    /// <param name="source">O objeto de origem.</param>
    /// <returns>O resultado contendo a instância mapeada ou a falha.</returns>
    public Result<TDestination> TryMap<TSource, TDestination>(TSource? source)
    {
        if (source is null)
        {
            return Result<TDestination>.Failure("A instância de origem fornecida é nula.");
        }

        try
        {
            var destination = Map<TSource, TDestination>(source);
            return Result<TDestination>.Success(destination);
        }
        catch (Exception ex)
        {
            return Result<TDestination>.Failure($"Falha no mapeamento entre '{typeof(TSource).Name}' e '{typeof(TDestination).Name}': {ex.Message}");
        }
    }

    private static Func<TSource, TDestination> CreateMapDelegate<TSource, TDestination>()
    {
        var sourceType = typeof(TSource);
        var destType = typeof(TDestination);

        if (_customConverters.TryGetValue(new TypePair(sourceType, destType), out var customConverter))
        {
            return (Func<TSource, TDestination>)customConverter;
        }

        var parameter = Expression.Parameter(sourceType, "source");
        var destinationProperties = destType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        var memberBindings = new List<MemberBinding>();

        foreach (var destProp in destinationProperties)
        {
            var sourceProp = sourceType.GetProperty(destProp.Name, BindingFlags.Public | BindingFlags.Instance);
            if (sourceProp == null || !sourceProp.CanRead)
            {
                continue;
            }

            var sourcePropType = sourceProp.PropertyType;
            var destPropType = destProp.PropertyType;

            if (destPropType.IsAssignableFrom(sourcePropType))
            {
                var propAccess = Expression.Property(parameter, sourceProp);
                memberBindings.Add(Expression.Bind(destProp, propAccess));
                continue;
            }

            if (_customConverters.TryGetValue(new TypePair(sourcePropType, destPropType), out var propertyConverter))
            {
                var propAccess = Expression.Property(parameter, sourceProp);
                var converterConst = Expression.Constant(propertyConverter);
                var invokeExpr = Expression.Invoke(converterConst, propAccess);
                memberBindings.Add(Expression.Bind(destProp, invokeExpr));
                continue;
            }

            if (IsGenericEnumerable(sourcePropType, out var sourceItemType) &&
                IsGenericEnumerable(destPropType, out var destItemType))
            {
                var propAccess = Expression.Property(parameter, sourceProp);
                var mapCollectionMethod = typeof(SimpleMapper).GetMethod(nameof(MapListInternal), BindingFlags.NonPublic | BindingFlags.Static)?
                    .MakeGenericMethod(sourceItemType, destItemType);

                if (mapCollectionMethod != null)
                {
                    var mapListCall = Expression.Call(mapCollectionMethod, propAccess);
                    memberBindings.Add(Expression.Bind(destProp, mapListCall));
                    continue;
                }
            }

            if (sourcePropType.IsClass && destPropType.IsClass && sourcePropType != typeof(string) && destPropType != typeof(string))
            {
                var propAccess = Expression.Property(parameter, sourceProp);
                var mapNestedMethod = typeof(SimpleMapper).GetMethod(nameof(MapNestedInternal), BindingFlags.NonPublic | BindingFlags.Static)?
                    .MakeGenericMethod(sourcePropType, destPropType);

                if (mapNestedMethod != null)
                {
                    var mapCall = Expression.Call(mapNestedMethod, propAccess);
                    memberBindings.Add(Expression.Bind(destProp, mapCall));
                }
            }
        }

        var memberInit = Expression.MemberInit(Expression.New(destType), memberBindings);
        var lambda = Expression.Lambda<Func<TSource, TDestination>>(memberInit, parameter);

        return lambda.Compile();
    }

    private static bool IsGenericEnumerable(Type type, out Type itemType)
    {
        if (type.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
        {
            itemType = type.GetGenericArguments()[0];
            return true;
        }

        var enumInterface = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        if (enumInterface != null)
        {
            itemType = enumInterface.GetGenericArguments()[0];
            return true;
        }

        itemType = typeof(object);
        return false;
    }

    private static List<TDestItem>? MapListInternal<TSourceItem, TDestItem>(IEnumerable<TSourceItem>? sourceList)
    {
        if (sourceList == null) return null;
        var mapper = new SimpleMapper();
        return sourceList.Select(item => mapper.Map<TSourceItem, TDestItem>(item)).ToList();
    }

    private static TDest? MapNestedInternal<TSource, TDest>(TSource? source) where TSource : class where TDest : class, new()
    {
        if (source == null) return null;
        var mapper = new SimpleMapper();
        return mapper.Map<TSource, TDest>(source);
    }
}
