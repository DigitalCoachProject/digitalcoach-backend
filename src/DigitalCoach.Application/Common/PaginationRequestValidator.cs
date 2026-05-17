using FluentValidation;

namespace DigitalCoach.Application.Common;

public abstract class PaginationRequestValidator<T> : AbstractValidator<T>
    where T : PaginationRequest
{
    protected PaginationRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
