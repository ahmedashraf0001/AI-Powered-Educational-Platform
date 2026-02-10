using AIEduPlatform.Application.Features.Users.Queries.GetUserProfile;
using AIEduPlatform.Core.DTOs.Users;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Users;

public class GetUserProfileRequest
{
    public Guid UserId { get; set; }
}

public class GetUserProfileEndpoint : Endpoint<GetUserProfileRequest, UserProfileDto>
{
    private readonly IMediator _mediator;

    public GetUserProfileEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/users/{UserId}");
        Group<UsersGroup>();
        Summary(s =>
        {
            s.Summary = "Get a user's profile";
            s.Description = "Returns the public profile of any user by their ID.";
            s.Response<UserProfileDto>(200, "User profile");
            s.Response(401, "Not authenticated");
            s.Response(404, "User not found");
        });
    }

    public override async Task HandleAsync(GetUserProfileRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserProfileQuery
        {
            UserId = req.UserId
        }, ct);

        await SendOkAsync(result, ct);
    }
}
