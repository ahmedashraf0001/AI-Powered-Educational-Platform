using AIEduPlatform.Core.DTOs.RAG;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface INotificationService
    {
        /// <summary>
        /// Notify a specific teacher about indexing completion
        /// </summary>
        /// <param name="userId">Teacher's user ID</param>
        /// <param name="response">Indexing response with results</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task NotifyIndexingCompletedAsync(Guid userId, RagIndexResponse response, CancellationToken cancellationToken = default);
    }
}
