using FluentValidation;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Dialogue.GenerateDialogueAudio
{
    public class GenerateDialogueAudioCommandValidator : AbstractValidator<GenerateDialogueAudioCommand>
    {
        private static readonly string[] ValidAudienceLevels = ["beginner", "intermediate", "advanced"];
        private static readonly string[] ValidDialogueLengths = ["short", "medium", "long"];
        private static readonly string[] ValidTeachingStyles = ["socratic", "explanatory", "interactive"];

        public GenerateDialogueAudioCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("Session ID is required.");

            RuleFor(x => x.AudienceLevel)
                .Must(level => ValidAudienceLevels.Contains(level.ToLowerInvariant()))
                .WithMessage($"Audience level must be one of: {string.Join(", ", ValidAudienceLevels)}.");

            RuleFor(x => x.NumberOfExchanges)
                .InclusiveBetween(2, 20).WithMessage("Number of exchanges must be between 2 and 20.");

            RuleFor(x => x.DialogueLength)
                .Must(len => ValidDialogueLengths.Contains(len.ToLowerInvariant()))
                .WithMessage($"Dialogue length must be one of: {string.Join(", ", ValidDialogueLengths)}.");

            RuleFor(x => x.TeachingStyle)
                .Must(style => ValidTeachingStyles.Contains(style.ToLowerInvariant()))
                .WithMessage($"Teaching style must be one of: {string.Join(", ", ValidTeachingStyles)}.");

            RuleFor(x => x.Topic)
                .MaximumLength(500).WithMessage("Topic must not exceed 500 characters.")
                .When(x => x.Topic is not null);

            RuleForEach(x => x.FocusConcepts)
                .NotEmpty().WithMessage("Focus concepts cannot contain empty strings.")
                .MaximumLength(200).WithMessage("Each focus concept must not exceed 200 characters.")
                .When(x => x.FocusConcepts is not null);
        }
    }
}
