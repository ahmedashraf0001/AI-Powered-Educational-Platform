using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetGradeBySubmission
{
    public class GetGradeBySubmissionQueryValidator : AbstractValidator<GetGradeBySubmissionQuery>
    {
        public GetGradeBySubmissionQueryValidator()
        {
            RuleFor(x => x.SubmissionId)
                .NotEmpty().WithMessage("Submission ID is required.");
        }
    }
}
