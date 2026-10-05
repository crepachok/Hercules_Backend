public record ExerciseResponse
{
    public int ExerciseId { get; init; }
    public string Name { get; init; } = string.Empty;
    public IEnumerable<int> MuscleGroupsIds { get; init; } = [];
    public ExerciseResponse(int id, string name, IEnumerable<int> musclesIds)
        => (ExerciseId, Name, MuscleGroupsIds) = (id, name, musclesIds);
}