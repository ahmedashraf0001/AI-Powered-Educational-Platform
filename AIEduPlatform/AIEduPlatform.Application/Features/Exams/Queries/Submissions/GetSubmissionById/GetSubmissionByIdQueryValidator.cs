using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetSubmissionById
{
    public class GetSubmissionByIdQueryValidator : AbstractValidator<GetSubmissionByIdQuery>
    {
        public GetSubmissionByIdQueryValidator()
        {
            RuleFor(x => x.SubmissionId)
                .NotEmpty().WithMessage("Submission ID is required.");
        }
    }
}
