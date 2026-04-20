using System.Threading.Channels;

namespace AIEduPlatform.Application.Common.Services
{
    public record TagExtractionRequest(Guid CourseId, Guid UserId);

    public interface ITagExtractionQueue
    {
        ValueTask EnqueueAsync(TagExtractionRequest request, CancellationToken cancellationToken = default);
        IAsyncEnumerable<TagExtractionRequest> DequeueAllAsync(CancellationToken cancellationToken);
    }

    public class TagExtractionQueue : ITagExtractionQueue
    {
        private readonly Channel<TagExtractionRequest> _channel;

        public TagExtractionQueue()
        {
            _channel = Channel.CreateUnbounded<TagExtractionRequest>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }

        public async ValueTask EnqueueAsync(TagExtractionRequest request, CancellationToken cancellationToken = default)
        {
            await _channel.Writer.WriteAsync(request, cancellationToken);
        }

        public IAsyncEnumerable<TagExtractionRequest> DequeueAllAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }
    }
}
