public record ExerciseDetailedResponse
{
    public int ExerciseId { get; init; }
    public string Name { get; init; }
    public IEnumerable<MuscleGroupMinimalResponse> MuscleGroups { get; init; }
    public ExerciseDetailedResponse(int id, string name, IEnumerable<MuscleGroupMinimalResponse> muscles)
        => (ExerciseId, Name, MuscleGroups) = (id, name, muscles);
}