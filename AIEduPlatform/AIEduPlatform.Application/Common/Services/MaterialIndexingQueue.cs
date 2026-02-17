using System.Threading.Channels;

namespace AIEduPlatform.Application.Common.Services
{
    public record MaterialIndexingRequest(Guid CourseId, Guid UserId);

    public interface IMaterialIndexingQueue
    {
        ValueTask EnqueueAsync(MaterialIndexingRequest request, CancellationToken cancellationToken = default);
        IAsyncEnumerable<MaterialIndexingRequest> DequeueAllAsync(CancellationToken cancellationToken);
    }

    public class MaterialIndexingQueue : IMaterialIndexingQueue
    {
        private readonly Channel<MaterialIndexingRequest> _channel;

        public MaterialIndexingQueue()
        {
            _channel = Channel.CreateUnbounded<MaterialIndexingRequest>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }

        public async ValueTask EnqueueAsync(MaterialIndexingRequest request, CancellationToken cancellationToken = default)
        {
            await _channel.Writer.WriteAsync(request, cancellationToken);
        }

        public IAsyncEnumerable<MaterialIndexingRequest> DequeueAllAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }
    }
}
