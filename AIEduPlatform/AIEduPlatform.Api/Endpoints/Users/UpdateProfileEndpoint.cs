using AIEduPlatform.Application.Features.Users.Commands.UpdateProfile;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Users;

public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
}

public class UpdateProfileEndpoint : Endpoint<UpdateProfileRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public UpdateProfileEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/users/me");
        Group<UsersGroup>();
        Summary(s =>
        {
            s.Summary = "Update my profile";
            s.Description = "Updates the authenticated user's first name, last name, or username. Only non-null fields are updated.";
            s.Response<ApiResponse<object>>(200, "Profile updated");
            s.Response(400, "Username already taken or validation error");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateProfileCommand
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            UserName = req.UserName
        }, ct);

        await SendOkAsync(ApiResponse<object>.Ok(null!, "Profile updated successfully."), ct);
    }
}
