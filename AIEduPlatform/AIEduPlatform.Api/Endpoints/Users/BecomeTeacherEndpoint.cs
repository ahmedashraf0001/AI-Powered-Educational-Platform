using AIEduPlatform.Application.Features.Users.Commands.BecomeTeacher;
using AIEduPlatform.Core.DTOs.Auth;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Users;

public class BecomeTeacherResponse
{
    public AuthResponseDto Tokens { get; set; } = default!;
}

public class BecomeTeacherEndpoint : EndpointWithoutRequest<ApiResponse<BecomeTeacherResponse>>
{
    private readonly IMediator _mediator;

    public BecomeTeacherEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/users/become-teacher");
        Group<UsersGroup>();
        Summary(s =>
        {
            s.Summary = "Become a teacher";
            s.Description = "Adds the Teacher role to the authenticated user and returns fresh tokens with the updated role. The user keeps all student capabilities.";
            s.Response<ApiResponse<BecomeTeacherResponse>>(200, "Teacher role granted");
            s.Response(400, "Already a teacher");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var tokens = await _mediator.Send(new BecomeTeacherCommand(), ct);

        await SendOkAsync(ApiResponse<BecomeTeacherResponse>.Ok(
            new BecomeTeacherResponse { Tokens = tokens },
            "You are now a teacher! You can create and manage courses."), ct);
    }
}
