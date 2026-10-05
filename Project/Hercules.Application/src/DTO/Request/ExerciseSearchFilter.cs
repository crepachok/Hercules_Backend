public record ExerciseSearchFilter
{
    public string? Name { get; init; }
    public string[] MuscleGroups { get; init; } = [];
}