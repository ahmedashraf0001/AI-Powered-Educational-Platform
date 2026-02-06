using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetAvailableExamsForStudent
{
    public record GetAvailableExamsForStudentQuery : IRequest<List<ExamDto>>
    {
    }
}
