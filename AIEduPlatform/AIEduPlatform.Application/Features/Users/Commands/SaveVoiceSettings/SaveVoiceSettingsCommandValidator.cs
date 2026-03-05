using FluentValidation;

namespace AIEduPlatform.Application.Features.Users.Commands.SaveVoiceSettings
{
    public class SaveVoiceSettingsCommandValidator : AbstractValidator<SaveVoiceSettingsCommand>
    {
        private static readonly string[] AllowedFormats = ["mp3", "wav", "ogg"];
        private static readonly int[] AllowedSampleRates = [22050, 24000, 44100, 48000];

        public SaveVoiceSettingsCommandValidator()
        {
            RuleFor(x => x.TeacherVoiceId)
                .NotEmpty().WithMessage("Teacher voice ID is required.")
                .MaximumLength(50);

            RuleFor(x => x.StudentVoiceId)
                .NotEmpty().WithMessage("Student voice ID is required.")
                .MaximumLength(50);

            RuleFor(x => x.TeacherSpeed)
                .InclusiveBetween(0.5, 2.0)
                .WithMessage("Teacher speed must be between 0.5 and 2.0.");

            RuleFor(x => x.StudentSpeed)
                .InclusiveBetween(0.5, 2.0)
                .WithMessage("Student speed must be between 0.5 and 2.0.");

            RuleFor(x => x.OutputFormat)
                .Must(f => AllowedFormats.Contains(f))
                .WithMessage($"Output format must be one of: {string.Join(", ", AllowedFormats)}.");

            RuleFor(x => x.SampleRate)
                .Must(r => AllowedSampleRates.Contains(r))
                .WithMessage($"Sample rate must be one of: {string.Join(", ", AllowedSampleRates)}.");

            RuleFor(x => x.PauseDurationMs)
                .InclusiveBetween(0, 5000)
                .WithMessage("Pause duration must be between 0 and 5000 ms.");

            RuleFor(x => x.PauseMultiplier)
                .InclusiveBetween(0.0, 3.0)
                .WithMessage("Pause multiplier must be between 0.0 and 3.0.");
        }
    }
}
