namespace DigitalCoach.Application.DTOs.Tasks;

public sealed class CreateTaskRequestValidator : TaskRequestValidatorBase<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        ConfigureTaskRules(
            x => x.Name,
            x => x.Description,
            x => x.PlannedDate,
            x => x.Deadline,
            x => x.Priority,
            x => x.Difficulty,
            x => x.Status);
    }
}
