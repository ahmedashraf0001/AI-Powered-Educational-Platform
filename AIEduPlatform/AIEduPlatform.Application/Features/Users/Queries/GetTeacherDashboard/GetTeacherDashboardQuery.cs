using AIEduPlatform.Core.DTOs.Stats;
using MediatR;

namespace AIEduPlatform.Application.Features.Users.Queries.GetTeacherDashboard
{
    public record GetTeacherDashboardQuery : IRequest<TeacherDashboardStats>;
}
