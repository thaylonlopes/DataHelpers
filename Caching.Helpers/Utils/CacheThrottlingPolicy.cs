using Polly;
using Polly.RateLimit;

namespace Caching.Helpers.Utils
{
    public static class CacheThrottlingPolicy
    {
        public static AsyncRateLimitPolicy CreateThrottlingPolicy(int numberOfExecutions, TimeSpan perTimeSpan)
        {
            return Policy
                .RateLimitAsync(numberOfExecutions, perTimeSpan);
        }
    }
}
