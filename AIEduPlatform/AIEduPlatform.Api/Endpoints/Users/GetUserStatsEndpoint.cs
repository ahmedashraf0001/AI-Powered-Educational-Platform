using AIEduPlatform.Application.Features.Users.Queries.GetUserStats;
using AIEduPlatform.Core.DTOs.Stats;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Users;

public class GetUserStatsRequest
{
    [QueryParam]
    public Guid? UserId { get; set; }
}

public class GetUserStatsEndpoint : Endpoint<GetUserStatsRequest, UserProfileStats>
{
    private readonly IMediator _mediator;

    public GetUserStatsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/users/stats");
        Group<UsersGroup>();
        Summary(s =>
        {
            s.Summary = "Get user statistics";
            s.Description = "Returns learning statistics: courses enrolled/completed/taught, exams, study sessions, flashcards, and total study time. Defaults to the authenticated user if no userId is provided.";
            s.Response<UserProfileStats>(200, "User statistics");
            s.Response(401, "Not authenticated");
            s.Response(404, "User not found");
        });
    }

    public override async Task HandleAsync(GetUserStatsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserStatsQuery
        {
            UserId = req.UserId
        }, ct);

        await SendOkAsync(result, ct);
    }
}
