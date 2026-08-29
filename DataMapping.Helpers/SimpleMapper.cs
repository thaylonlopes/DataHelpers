using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using DataMapping.Helpers.Common;
using DataMapping.Helpers.Interfaces;

namespace DataMapping.Helpers;

/// <summary>
/// Mapeador de objetos ultrarrápido baseado em árvores de expressões compiladas com cache thread-safe em memória e suporte a tipos aninhados, coleções e conversores customizados.
/// </summary>
public class SimpleMapper : IMapper
{
    private static readonly ConcurrentDictionary<(Type SourceType, Type DestinationType), Delegate> _mapCache = new();
    private static readonly ConcurrentDictionary<(Type SourceType, Type DestinationType), Delegate> _customConverters = new();

    /// <summary>
    /// Registra uma função conversora customizada de tipo.
    /// </summary>
    /// <typeparam name="TSource">Tipo de origem.</typeparam>
    /// <typeparam name="TDestination">Tipo de destino.</typeparam>
    /// <param name="converter">A função de conversão.</param>
    public static void RegisterConverter<TSource, TDestination>(Func<TSource, TDestination> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        _customConverters[(typeof(TSource), typeof(TDestination))] = converter;
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

        var key = (typeof(TSource), typeof(TDestination));
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

        if (_customConverters.TryGetValue((sourceType, destType), out var customConverter))
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

            if (_customConverters.TryGetValue((sourcePropType, destPropType), out var propertyConverter))
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
