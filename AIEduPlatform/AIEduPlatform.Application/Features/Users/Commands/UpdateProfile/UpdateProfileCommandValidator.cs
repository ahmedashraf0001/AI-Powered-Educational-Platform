using FluentValidation;

namespace AIEduPlatform.Application.Features.Users.Commands.UpdateProfile
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(100)
                .When(x => x.FirstName is not null);

            RuleFor(x => x.LastName)
                .MaximumLength(100)
                .When(x => x.LastName is not null);

            RuleFor(x => x.UserName)
                .MinimumLength(3)
                .MaximumLength(50)
                .When(x => x.UserName is not null);
        }
    }
}
