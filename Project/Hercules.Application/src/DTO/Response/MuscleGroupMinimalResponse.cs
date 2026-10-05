public record MuscleGroupMinimalResponse
{
    public int MuscleGroupId { get; init; }
    public string Name {get; init; }
    public MuscleGroupMinimalResponse(int id, string name)
        => (MuscleGroupId, Name) = (id, name);
}