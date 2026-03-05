using AIEduPlatform.Application.Features.Users.Queries.GetStudentDashboard;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Stats;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Users;

public class GetStudentDashboardEndpoint : EndpointWithoutRequest<ApiResponse<StudentDashboardDto>>
{
    private readonly IMediator _mediator;

    public GetStudentDashboardEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/users/dashboard");
        Roles("Student");
        Group<UsersGroup>();
        Summary(s =>
        {
            s.Summary = "Get student academic performance dashboard";
            s.Description = "Returns comprehensive academic performance data including course progress, " +
                            "engagement analytics, exam statistics, grade trends, and submission history.";
            s.Response<ApiResponse<StudentDashboardDto>>(200, "Student dashboard data");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not a student");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStudentDashboardQuery(), ct);
        await SendOkAsync(ApiResponse<StudentDashboardDto>.Ok(result, "Dashboard loaded"), ct);
    }
}
