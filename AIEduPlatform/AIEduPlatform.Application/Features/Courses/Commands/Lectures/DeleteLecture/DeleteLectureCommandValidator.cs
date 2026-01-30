using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Commands.Lectures.DeleteLecture
{
    public class DeleteLectureCommandValidator : AbstractValidator<DeleteLectureCommand>
    {
        public DeleteLectureCommandValidator()
        {
            RuleFor(x => x.LectureId)
                .NotEmpty().WithMessage("Lecture ID is required.");
        }
    }
}
