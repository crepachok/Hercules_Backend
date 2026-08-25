using FluentValidation;

public class WorkoutSearchFilterValidator : AbstractValidator<WorkoutSearchFilter>
{
    public WorkoutSearchFilterValidator()
    {
        RuleFor(w => w.DateTo)
            .Must((filter, dateTo) => filter.DateFrom == null || dateTo == null || dateTo >= filter.DateFrom)
                .WithMessage("Date to should be earlier than date from");
    }
}