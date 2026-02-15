using AIEduPlatform.Application.Features.Auth.Commands.Register;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Auth;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class RegisterEndpoint : Endpoint<RegisterRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public RegisterEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/auth/register");
        AllowAnonymous();
        Group<AuthGroup>();
        Summary(s =>
        {
            s.Summary = "Register a new account";
            s.Description = "Creates a new student account. A welcome email is sent upon successful registration.";
            s.Response<ApiResponse<object>>(200, "Registration successful");
            s.Response(400, "Validation error or email/username already taken");
        });
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        await _mediator.Send(new RegisterCommand
        {
            Email = req.Email,
            UserName = req.UserName,
            Password = req.Password,
            ConfirmPassword = req.ConfirmPassword,
            FirstName = req.FirstName,
            LastName = req.LastName
        }, ct);

        await SendOkAsync(ApiResponse<object>.Ok(null!, "Registration successful. Please check your email."), ct);
    }
}
