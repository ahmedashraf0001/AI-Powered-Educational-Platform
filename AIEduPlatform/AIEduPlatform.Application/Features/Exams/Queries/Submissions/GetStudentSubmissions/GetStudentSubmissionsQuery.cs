using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetStudentSubmissions
{
    public record GetStudentSubmissionsQuery : IRequest<List<SubmissionDto>>
    {
    }
}
