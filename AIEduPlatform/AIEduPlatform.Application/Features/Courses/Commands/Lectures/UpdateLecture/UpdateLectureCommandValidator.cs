using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Commands.Lectures.UpdateLecture
{
    public class UpdateLectureCommandValidator : AbstractValidator<UpdateLectureCommand>
    {
        public UpdateLectureCommandValidator()
        {
            RuleFor(x => x.LectureId)
                .NotEmpty().WithMessage("Lecture ID is required.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Lecture title is required.")
                .MaximumLength(200).WithMessage("Lecture title must not exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Lecture description must not exceed 1000 characters.");

            RuleFor(x => x.OrderIndex)
                .GreaterThanOrEqualTo(0).WithMessage("Order index must be greater than or equal to 0.");
        }
    }
}
