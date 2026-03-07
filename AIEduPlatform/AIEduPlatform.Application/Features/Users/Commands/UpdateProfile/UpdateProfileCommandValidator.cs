using FluentValidation;

namespace AIEduPlatform.Application.Features.Users.Commands.UpdateProfile
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.FirstName));

            RuleFor(x => x.LastName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.LastName));

            RuleFor(x => x.UserName)
                .MinimumLength(3)
                .MaximumLength(50)
                .When(x => !string.IsNullOrEmpty(x.UserName));

            RuleFor(x => x.Bio)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrEmpty(x.Bio));

            RuleFor(x => x.AvatarUrl)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.AvatarUrl));

            RuleFor(x => x.Website)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Website));

            RuleFor(x => x.LinkedInUrl)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.LinkedInUrl));

            RuleFor(x => x.Location)
                .MaximumLength(200)
                .When(x => !string.IsNullOrEmpty(x.Location));
        }
    }
}