public record MuscleGroupDetailedResponse
{
    public int MuscleGroupId { get; init; }
    public string Name {get; init; } = string.Empty;
    public IEnumerable<ExerciseMinimalResponse> Exercises { get; init; }
    public MuscleGroupDetailedResponse(int id, string name, IEnumerable<ExerciseMinimalResponse> exercises)
        => (MuscleGroupId, Name, Exercises) = (id, name, exercises);
}