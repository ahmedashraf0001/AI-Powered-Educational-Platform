using AIEduPlatform.Application.Features.Auth.Commands.Logout;
using FastEndpoints;
using MediatR;
using System.Security.Claims;

namespace AIEduPlatform.Api.Endpoints.Auth;

public class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutResponse
{
    public string Message { get; set; } = string.Empty;
}

public class LogoutEndpoint : Endpoint<LogoutRequest, LogoutResponse>
{
    private readonly IMediator _mediator;

    public LogoutEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/auth/logout");
        Group<AuthGroup>();
        Summary(s =>
        {
            s.Summary = "Logout";
            s.Description = "Revokes the user's refresh token to end the session.";
            s.Response<LogoutResponse>(200, "Logged out successfully");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(LogoutRequest req, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        await _mediator.Send(new LogoutCommand
        {
            UserId = userId,
            RefreshToken = req.RefreshToken
        }, ct);

        await SendOkAsync(new LogoutResponse 
        { 
            Message = "Logout successful." 
        }, ct);
    }
}
