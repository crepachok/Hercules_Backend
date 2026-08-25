using FluentValidation;

public class WorkoutRequestValidator : AbstractValidator<WorkoutRequest>
{
    public WorkoutRequestValidator()
    {
        RuleFor(w => w.StartTime)
            .GreaterThanOrEqualTo(DateTimeOffset.Parse(ValidationConstants.MinimalTime));

        RuleFor(w => w.EndTime)
            .Must((request, end) =>
            {
                if (end.HasValue)
                    return end.Value >= request.StartTime;
                return true;
            }).WithMessage("Start time should be earlier than end time");

        RuleFor(w => w)
            .Must(request =>
            {
                if (!request.EndTime.HasValue)
                    return true;

                if (request.SessionExercises.Count() == 0)
                    return false;

                bool hasEmptySessionExercises = request.SessionExercises
                    .Count(se => se.Sets.Count() == 0) > 0;

                if (hasEmptySessionExercises)
                    return false;
                return true;
            }).WithMessage("Completed workouts should have non empty session exercises");

        RuleFor(w => w.SessionExercises)
            .Must(se => se.All(se => se.Sets.All(set => set.Reps.IsBetween(SetEntity.MinReps, SetEntity.MaxReps))))
                .WithMessage($"Set reps should be between {SetEntity.MinReps} and {SetEntity.MaxReps}");

        RuleFor(w => w.SessionExercises)
            .Must(se => se.All(se => se.Sets.All(set => set.Weight.IsBetween(SetEntity.MinWeight, SetEntity.MaxWeight))))
                .WithMessage($"Set weight should be between {SetEntity.MinWeight} and {SetEntity.MaxWeight}");
    }
}