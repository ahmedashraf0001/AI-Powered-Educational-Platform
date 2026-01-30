using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.SearchCourses
{
    public class SearchCoursesQueryValidator : AbstractValidator<SearchCoursesQuery>
    {
        public SearchCoursesQueryValidator()
        {
            RuleFor(x => x.Keyword)
                .NotEmpty().WithMessage("Search keyword is required.")
                .MinimumLength(2).WithMessage("Search keyword must be at least 2 characters.");
        }
    }
}
