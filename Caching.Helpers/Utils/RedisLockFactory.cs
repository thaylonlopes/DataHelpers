using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System.Net;

namespace Caching.Helpers.Utils;

/// <summary>
/// Fábrica centralizada para criação e gestão do ciclo de vida da instância de locking distribuído RedLock.
/// </summary>
public static class RedisLockFactory
{
    private static RedLockFactory? _factory;

    /// <summary>
    /// Inicializa a fábrica do RedLock com base na connection string do cluster ou servidor Redis.
    /// </summary>
    /// <param name="connectionString">String de conexão contendo hosts, portas e credenciais do Redis.</param>
    public static void Initialize(string connectionString)
    {
        var endPoints = new List<RedLockEndPoint>();

        var endPoint = ConnectionMultiplexer.Connect(connectionString).GetEndPoints();
        foreach (var ep in endPoint)
        {
            if (ep is DnsEndPoint dnsEndPoint)
            {
                endPoints.Add(dnsEndPoint);
            }
            else if (ep is IPEndPoint ipEndPoint)
            {
                endPoints.Add(new DnsEndPoint(ipEndPoint.Address.ToString(), ipEndPoint.Port));
            }
        }

        _factory = RedLockFactory.Create(endPoints);
    }

    /// <summary>
    /// Obtém a instância inicializada da fábrica de bloqueio distribuído <see cref="IDistributedLockFactory"/>.
    /// </summary>
    /// <returns>A instância configurada de <see cref="IDistributedLockFactory"/>.</returns>
    /// <exception cref="InvalidOperationException">Lançada caso a fábrica ainda não tenha sido inicializada via <see cref="Initialize"/>.</exception>
    public static IDistributedLockFactory GetFactory()
    {
        if (_factory == null)
            throw new InvalidOperationException("RedisLockFactory is not initialized. Call Initialize() first.");

        return _factory;
    }
}
