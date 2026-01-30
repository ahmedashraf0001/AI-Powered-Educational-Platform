using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Queries.Lectures.GetLectureMaterials
{
    public class GetLectureMaterialsQueryValidator : AbstractValidator<GetLectureMaterialsQuery>
    {
        public GetLectureMaterialsQueryValidator()
        {
            RuleFor(x => x.LectureId)
                .NotEmpty().WithMessage("Lecture ID is required.");
        }
    }
}
