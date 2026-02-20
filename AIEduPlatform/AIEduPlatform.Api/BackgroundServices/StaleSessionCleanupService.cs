using AIEduPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Api.BackgroundServices;

/// <summary>
/// Periodically cleans up stale study sessions that have been inactive for over 2 hours.
/// Sets EndedAt on inactive sessions so they are properly closed.
/// </summary>
public class StaleSessionCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StaleSessionCleanupService> _logger;
    private static readonly TimeSpan InactivityThreshold = TimeSpan.FromHours(2);
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);

    public StaleSessionCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<StaleSessionCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stale session cleanup service started. Check interval: {Interval}, Inactivity threshold: {Threshold}",
            CheckInterval, InactivityThreshold);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupStaleSessions(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during stale session cleanup");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CleanupStaleSessions(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var staleSessions = await unitOfWork.StudySessions.GetInactiveSessionsAsync(InactivityThreshold, ct);

        // Only close sessions that haven't been ended yet
        var sessionsToClose = staleSessions.Where(s => s.EndedAt == null).ToList();

        if (sessionsToClose.Count == 0)
            return;

        foreach (var session in sessionsToClose)
        {
            session.EndedAt = session.LastActivity;
            session.UpdatedAt = DateTime.UtcNow;
            unitOfWork.StudySessions.Update(session);
        }

        await unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Cleaned up {Count} stale study sessions (inactive for >{Threshold}h)",
            sessionsToClose.Count, InactivityThreshold.TotalHours);
    }
}
