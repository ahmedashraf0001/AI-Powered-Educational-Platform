using AIEduPlatform.Api.Extensions;
using AIEduPlatform.Application.Features.Auth.Commands.Login;
using AIEduPlatform.Core.DTOs.Auth;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Auth;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginEndpoint : Endpoint<LoginRequest, ApiResponse<AuthResponseDto>>
{
    private readonly IMediator _mediator;

    public LoginEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
        Group<AuthGroup>();
        Options(x => x.RequireRateLimiting(RateLimitingExtensions.LoginPolicy));
        Summary(s =>
        {
            s.Summary = "Login";
            s.Description = "Authenticates user credentials and returns JWT access + refresh tokens.";
            s.ExampleRequest = new LoginRequest
            {
                Email = "student@example.com",
                Password = "P@ssw0rd123"
            };
            s.Response<ApiResponse<AuthResponseDto>>(200, "Login successful — tokens returned");
            s.Response(400, "Invalid email or password");
        });
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginCommand
        {
            Email = req.Email,
            Password = req.Password
        }, ct);

        await SendOkAsync(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."), ct);
    }
}
