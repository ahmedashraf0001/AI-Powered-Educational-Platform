using AIEduPlatform.Application.Features.Auth.Commands.RegisterTeacher;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Auth;

public class RegisterTeacherRequest
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Qualifications { get; set; } = string.Empty;
    public string Subjects { get; set; } = string.Empty;
}

public class RegisterTeacherEndpoint : Endpoint<RegisterTeacherRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public RegisterTeacherEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/auth/register/teacher");
        AllowAnonymous();
        Group<AuthGroup>();
        Summary(s =>
        {
            s.Summary = "Register as a teacher";
            s.Description = "Creates a new teacher account with bio, qualifications, and subjects. A verification email is sent upon successful registration. You must verify your email before logging in.";
            s.Response<ApiResponse<object>>(200, "Registration successful — check email for verification link");
            s.Response(400, "Validation error or email/username already taken");
        });
    }

    public override async Task HandleAsync(RegisterTeacherRequest req, CancellationToken ct)
    {
        await _mediator.Send(new RegisterTeacherCommand
        {
            Email = req.Email,
            UserName = req.UserName,
            Password = req.Password,
            ConfirmPassword = req.ConfirmPassword,
            FullName = req.FullName,
            Bio = req.Bio,
            Qualifications = req.Qualifications,
            Subjects = req.Subjects
        }, ct);

        await SendOkAsync(ApiResponse<object>.Ok(null!, "Registration successful. Please check your email to verify your account."), ct);
    }
}
