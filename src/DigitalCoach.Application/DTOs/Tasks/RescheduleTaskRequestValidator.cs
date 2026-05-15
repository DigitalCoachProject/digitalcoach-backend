using FluentValidation;

namespace DigitalCoach.Application.DTOs.Tasks;

public sealed class RescheduleTaskRequestValidator : AbstractValidator<RescheduleTaskRequest>
{
    public RescheduleTaskRequestValidator()
    {
        RuleFor(x => x.NewPlannedDate)
            .Must(x => x != default)
            .WithMessage("New planned date is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(100);
    }
}
