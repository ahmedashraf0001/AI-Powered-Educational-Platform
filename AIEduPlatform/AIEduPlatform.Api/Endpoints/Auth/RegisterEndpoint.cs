using AIEduPlatform.Application.Features.Auth.Commands.Register;
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

public class RegisterResponse
{
    public string Message { get; set; } = string.Empty;
}

public class RegisterEndpoint : Endpoint<RegisterRequest, RegisterResponse>
{
    private readonly IMediator _mediator;

    public RegisterEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/auth/register");
        AllowAnonymous();
        Group<AuthGroup>();
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

        await SendOkAsync(new RegisterResponse 
        { 
            Message = "Registration successful. Please check your email." 
        }, ct);
    }
}
