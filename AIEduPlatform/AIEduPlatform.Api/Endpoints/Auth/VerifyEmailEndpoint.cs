using AIEduPlatform.Application.Features.Auth.Commands.VerifyEmail;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Auth;

public class VerifyEmailRequest
{
    [QueryParam]
    public string Token { get; set; } = string.Empty;
    [QueryParam]
    public string Email { get; set; } = string.Empty;
}

public class VerifyEmailEndpoint : Endpoint<VerifyEmailRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public VerifyEmailEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/auth/verify-email");
        AllowAnonymous();
        Group<AuthGroup>();
        Summary(s =>
        {
            s.Summary = "Verify email address";
            s.Description = "Validates the verification token sent via email and marks the user as verified. Required before login.";
            s.Response<ApiResponse<object>>(200, "Email verified successfully");
            s.Response(400, "Invalid or expired token");
            s.Response(404, "User not found");
        });
    }

    public override async Task HandleAsync(VerifyEmailRequest req, CancellationToken ct)
    {
        await _mediator.Send(new VerifyEmailCommand
        {
            Token = req.Token,
            Email = req.Email
        }, ct);

        await SendOkAsync(ApiResponse<object>.Ok(null!, "Email verified successfully. You can now log in."), ct);
    }
}
