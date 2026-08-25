public record WorkoutRequest
{
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset? EndTime { get; init; }
    public IEnumerable<SessionExerciseRequest> SessionExercises { get; init; } = [];
}