using System.Threading.Channels;

namespace AIEduPlatform.Application.Common.Services
{
    /// <summary>
    /// Request for AI grading a submission
    /// </summary>
    public record AIGradingRequest(Guid SubmissionId, Guid TeacherId);

    /// <summary>
    /// Interface for the AI grading queue
    /// </summary>
    public interface IAIGradingQueue
    {
        ValueTask EnqueueAsync(AIGradingRequest request, CancellationToken cancellationToken = default);
        IAsyncEnumerable<AIGradingRequest> DequeueAllAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Queue for AI grading submissions in the background
    /// </summary>
    public class AIGradingQueue : IAIGradingQueue
    {
        private readonly Channel<AIGradingRequest> _channel;

        public AIGradingQueue()
        {
            _channel = Channel.CreateUnbounded<AIGradingRequest>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }

        public async ValueTask EnqueueAsync(AIGradingRequest request, CancellationToken cancellationToken = default)
        {
            await _channel.Writer.WriteAsync(request, cancellationToken);
        }

        public IAsyncEnumerable<AIGradingRequest> DequeueAllAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }
    }
}
