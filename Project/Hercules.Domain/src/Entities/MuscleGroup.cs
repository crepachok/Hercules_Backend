using System.Text.Json.Serialization;

public class MuscleGroupEntity : IEntityBase
{
    public int Id { get; init; }
    [JsonInclude] public string Name { get; private set; } = string.Empty;
    [JsonInclude] public ICollection<ExerciseEntity> Exercises { get; private set; } = [];

    public const int MinNameLength = 3;
    public const int MaxNameLength = 75;
    
    static MuscleGroupEntity()
    {
        if (MinNameLength > MaxNameLength)
            throw new Exception("MuscleGroup. MaxNameLength must be greater than MinNameLength");
    }
    [JsonConstructor] public MuscleGroupEntity() {}
    public MuscleGroupEntity(string name) => SetName(name);

    public MuscleGroupEntity SetName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new DomainException("MuscleGroup. Cannot set an empty name");

        if (!name.Length.IsBetween(MinNameLength, MaxNameLength))
            throw new DomainException($"MuscleGroup. Name length must be between {MinNameLength} and {MaxNameLength}");

        Name = name;
        return this;
    }
}