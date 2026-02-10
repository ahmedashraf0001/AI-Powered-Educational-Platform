using AIEduPlatform.Application.Features.Users.Queries.GetTeacherDashboard;
using AIEduPlatform.Core.DTOs.Stats;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Users;

public class GetTeacherDashboardEndpoint : EndpointWithoutRequest<TeacherDashboardStats>
{
    private readonly IMediator _mediator;

    public GetTeacherDashboardEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/users/teacher/dashboard");
        Roles("Teacher");
        Group<UsersGroup>();
        Summary(s =>
        {
            s.Summary = "Get teacher dashboard";
            s.Description = "Returns aggregated statistics for the teacher: total courses, published courses, enrolled students, exams created, pending grade approvals, and ungraded submissions.";
            s.Response<TeacherDashboardStats>(200, "Dashboard statistics");
            s.Response(401, "Not authenticated");
            s.Response(403, "Teacher role required");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTeacherDashboardQuery(), ct);
        await SendOkAsync(result, ct);
    }
}
