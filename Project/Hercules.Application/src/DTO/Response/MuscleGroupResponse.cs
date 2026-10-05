public record MuscleGroupResponse
{
    public int MuscleGroupId { get; init; }
    public string Name { get; init; }
    public IEnumerable<int> ExerciseIds { get; init; }
    public MuscleGroupResponse(int id, string name, IEnumerable<int> exerciseIds)
        => (MuscleGroupId, Name, ExerciseIds) = (id, name, exerciseIds);
}