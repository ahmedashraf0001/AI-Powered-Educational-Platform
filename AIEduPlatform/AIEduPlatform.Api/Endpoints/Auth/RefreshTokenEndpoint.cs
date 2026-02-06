using AIEduPlatform.Application.Features.Auth.Commands.RefreshToken;
using AIEduPlatform.Core.DTOs.Auth;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Auth;

public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenEndpoint : Endpoint<RefreshTokenRequest, TokenResponseDto>
{
    private readonly IMediator _mediator;

    public RefreshTokenEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/auth/refresh-token");
        AllowAnonymous();
        Group<AuthGroup>();
    }

    public override async Task HandleAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new RefreshTokenCommand
        {
            AccessToken = req.AccessToken,
            RefreshToken = req.RefreshToken
        }, ct);

        await SendOkAsync(result, ct);
    }
}
