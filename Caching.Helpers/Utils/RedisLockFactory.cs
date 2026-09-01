using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System.Net;

namespace Caching.Helpers.Utils
{
    public static class RedisLockFactory
    {
        private static RedLockFactory? _factory;

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

        public static IDistributedLockFactory GetFactory()
        {
            if (_factory == null)
                throw new InvalidOperationException("RedisLockFactory is not initialized. Call Initialize() first.");

            return _factory;
        }
    }
}

