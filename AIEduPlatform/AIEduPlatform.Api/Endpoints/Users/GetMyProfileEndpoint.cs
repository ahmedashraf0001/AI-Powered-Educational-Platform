using AIEduPlatform.Application.Features.Users.Queries.GetMyProfile;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Users;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Users;

public class GetMyProfileEndpoint : EndpointWithoutRequest<ApiResponse<UserProfileDto>>
{
    private readonly IMediator _mediator;

    public GetMyProfileEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/users/me");
        Group<UsersGroup>();
        Summary(s =>
        {
            s.Summary = "Get my profile";
            s.Description = "Returns the authenticated user's profile including roles, name, and timestamps.";
            s.Response<ApiResponse<UserProfileDto>>(200, "User profile");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyProfileQuery(), ct);
        await SendOkAsync(ApiResponse<UserProfileDto>.Ok(result), ct);
    }
}
