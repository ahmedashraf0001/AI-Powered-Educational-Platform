using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.ML.Settings
{
    public interface IRerankConcurrencyLimiter
    {
        SemaphoreSlim Semaphore { get; }
    }
    public class RerankConcurrencyLimiter : IRerankConcurrencyLimiter
    {
        public SemaphoreSlim Semaphore { get; }

        public RerankConcurrencyLimiter(int maxConcurrency)
        {
            Semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        }
    }
}
