using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace MongoDriver.Helpers.Utils;

/// <summary>
/// Utilitário thread-safe para registro idempotente de convenções e serializadores BSON do MongoDB.
/// </summary>
public static class BsonRegistrationHelper
{
    private static readonly object SyncLock = new();
    private static readonly HashSet<string> RegisteredConventions = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<Type> RegisteredSerializers = new();

    /// <summary>
    /// Registra um pacote de convenções do MongoDB de forma idempotente, prevenindo exceções em chamadas duplicadas.
    /// </summary>
    /// <param name="name">Nome único da convenção.</param>
    /// <param name="pack">O pacote de convenções.</param>
    /// <param name="filter">Filtro opcional de tipos aos quais a convenção se aplica.</param>
    public static void RegisterConvention(string name, IConventionPack pack, Func<Type, bool>? filter = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(pack);

        lock (SyncLock)
        {
            if (RegisteredConventions.Contains(name))
            {
                return;
            }

            try
            {
                ConventionRegistry.Register(name, pack, filter ?? (_ => true));
                RegisteredConventions.Add(name);
            }
            catch
            {
                RegisteredConventions.Add(name);
            }
        }
    }

    /// <summary>
    /// Registra um serializador BSON de forma idempotente.
    /// </summary>
    /// <typeparam name="T">O tipo gerenciado pelo serializador.</typeparam>
    /// <param name="serializer">Instância do serializador BSON.</param>
    public static void RegisterSerializer<T>(IBsonSerializer<T> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);

        lock (SyncLock)
        {
            if (RegisteredSerializers.Contains(typeof(T)))
            {
                return;
            }

            try
            {
                BsonSerializer.RegisterSerializer(typeof(T), serializer);
                RegisteredSerializers.Add(typeof(T));
            }
            catch (BsonSerializationException)
            {
                RegisteredSerializers.Add(typeof(T));
            }
        }
    }

    /// <summary>
    /// Registra convenções padrão recomendadas de forma idempotente.
    /// </summary>
    public static void RegisterStandardConventions()
    {
        var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true)
        };

        RegisterConvention("StandardConventions", pack);
    }
}
