using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Queries.Lectures.GetCourseLectures
{
    public class GetCourseLecturesQueryValidator : AbstractValidator<GetCourseLecturesQuery>
    {
        public GetCourseLecturesQueryValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("Course ID is required.");
        }
    }
}
