using System.Text.Json.Serialization;

public class ExerciseEntity : IEntityBase
{
    public int Id { get; init; }
    [JsonInclude] public string Name { get; private set; } = string.Empty;
    [JsonInclude] public ICollection<MuscleGroupEntity> Muscles { get; private set; } = [];
    private ICollection<TemplateEntity> _templates { get; set; } = [];
    private ICollection<SessionExerciseEntity> _sessionExercises { get; set; } = [];

    public static string NameofTemplates => nameof(_templates);
    public static string NameofSessionExercises => nameof(_sessionExercises);

    public const int MinNameLength = 3;
    public const int MaxNameLength = 75;

    static ExerciseEntity()
    {
        if (MinNameLength > MaxNameLength)
            throw new Exception("Exercise. MaxNameLength must be greater than MinNameLength");
    }
    [JsonConstructor] public ExerciseEntity() {}
    public ExerciseEntity(string name, ICollection<MuscleGroupEntity> muscles)
    {
        SetName(name);
        SetMuscleGroups(muscles);
    }

    public ExerciseEntity SetName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new DomainException("Exercise. Cannot set an empty name");

        if (!name.Length.IsBetween(MinNameLength, MaxNameLength))
            throw new DomainException($"Exercise. Name length must be between {MinNameLength} and {MaxNameLength}");

        Name = name;
        return this;
    }
    public bool AddMuscleGroup(MuscleGroupEntity muscle)
    {
        if (Muscles.Any(m => m.Name == muscle.Name))
            return false;

        Muscles.Add(muscle);
        return true;
    }
    public bool RemoveMuscleGroup(MuscleGroupEntity muscle)
    {
        var toRemove = Muscles.FirstOrDefault(m => m.Name == muscle.Name);
        if (toRemove == null)
            return false;

        Muscles.Remove(toRemove);
        return true;
    }
    public ExerciseEntity SetMuscleGroups(ICollection<MuscleGroupEntity> muscles)
    {
        if (muscles is not { Count: > 0 })
            throw new DomainException("Exercise. Cannot set an empty set of Muscle Groups");

        Muscles = muscles
            .Where(m => m != null)
            .DistinctBy(m => m.Name)
            .ToList();

        return this;
    }
}