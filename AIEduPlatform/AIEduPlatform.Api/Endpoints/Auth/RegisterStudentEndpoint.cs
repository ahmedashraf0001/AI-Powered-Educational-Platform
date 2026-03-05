using AIEduPlatform.Application.Features.Auth.Commands.RegisterStudent;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Auth;

public class RegisterStudentRequest
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? GradeLevel { get; set; }
    public string? Interests { get; set; }
}

public class RegisterStudentEndpoint : Endpoint<RegisterStudentRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public RegisterStudentEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/auth/register/student");
        AllowAnonymous();
        Group<AuthGroup>();
        Summary(s =>
        {
            s.Summary = "Register as a student";
            s.Description = "Creates a new student account. A verification email is sent upon successful registration. You must verify your email before logging in.";
            s.Response<ApiResponse<object>>(200, "Registration successful — check email for verification link");
            s.Response(400, "Validation error or email/username already taken");
        });
    }

    public override async Task HandleAsync(RegisterStudentRequest req, CancellationToken ct)
    {
        await _mediator.Send(new RegisterStudentCommand
        {
            Email = req.Email,
            UserName = req.UserName,
            Password = req.Password,
            ConfirmPassword = req.ConfirmPassword,
            FullName = req.FullName,
            GradeLevel = req.GradeLevel,
            Interests = req.Interests
        }, ct);

        await SendOkAsync(ApiResponse<object>.Ok(null!, "Registration successful. Please check your email to verify your account."), ct);
    }
}
