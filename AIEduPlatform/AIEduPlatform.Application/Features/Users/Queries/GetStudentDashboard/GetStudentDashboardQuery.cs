using AIEduPlatform.Core.DTOs.Stats;
using MediatR;

namespace AIEduPlatform.Application.Features.Users.Queries.GetStudentDashboard
{
    public record GetStudentDashboardQuery : IRequest<StudentDashboardDto>;
}
