using AIEduPlatform.Application.Features.Auth.Commands.Login;
using AIEduPlatform.Core.DTOs.Auth;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Auth;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginEndpoint : Endpoint<LoginRequest, AuthResponseDto>
{
    private readonly IMediator _mediator;

    public LoginEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
        Group<AuthGroup>();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginCommand
        {
            Email = req.Email,
            Password = req.Password
        }, ct);

        await SendOkAsync(result, ct);
    }
}
