public record SessionExerciseRequest
{
    public int ExerciseId { get; init; }
    public IEnumerable<SetRequest> Sets { get; init; } = [];
}