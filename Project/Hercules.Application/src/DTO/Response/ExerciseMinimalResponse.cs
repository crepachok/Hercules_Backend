public record ExerciseMinimalResponse
{
    public int ExerciseId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ExerciseMinimalResponse(int id, string name)
        => (ExerciseId, Name) = (id, name);
}