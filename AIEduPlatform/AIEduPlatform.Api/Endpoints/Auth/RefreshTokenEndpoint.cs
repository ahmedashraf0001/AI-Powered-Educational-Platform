using AIEduPlatform.Application.Features.Auth.Commands.RefreshToken;
using AIEduPlatform.Core.DTOs.Auth;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Auth;

public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenEndpoint : Endpoint<RefreshTokenRequest, ApiResponse<TokenResponseDto>>
{
    private readonly IMediator _mediator;

    public RefreshTokenEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/auth/refresh-token");
        AllowAnonymous();
        Group<AuthGroup>();
        Summary(s =>
        {
            s.Summary = "Refresh access token";
            s.Description = "Exchanges an expired access token and a valid refresh token for a new token pair.";
            s.ExampleRequest = new RefreshTokenRequest
            {
                AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                RefreshToken = "dGhpcyBpcyBhIHNhbXBsZSByZWZyZXNoIHRva2Vu..."
            };
            s.Response<ApiResponse<TokenResponseDto>>(200, "New tokens returned");
            s.Response(400, "Invalid or expired tokens");
        });
    }

    public override async Task HandleAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new RefreshTokenCommand
        {
            AccessToken = req.AccessToken,
            RefreshToken = req.RefreshToken
        }, ct);

        await SendOkAsync(ApiResponse<TokenResponseDto>.Ok(result), ct);
    }
}
