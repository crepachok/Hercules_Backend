public class TemplateEntity : IEntityBase
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public UserEntity User { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public ICollection<ExerciseEntity> Exercises { get; private set; } = [];

    public static string NameofExercises => nameof(Exercises);

    public const int MinNameLength = 3; 
    public const int MaxNameLength = 100; 

    static TemplateEntity()
    {
        if (MinNameLength > MaxNameLength)
            throw new Exception("Template. MaxNameLength must be greater than MinNameLength");
    }
    private TemplateEntity() {}
    public TemplateEntity(int userId, string name, ICollection<ExerciseEntity> exercises)
    {
        UserId = userId;
        SetName(name);
        SetExercises(exercises);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new DomainException("Templates. Cannot set an empty name");

        if (!name.Length.IsBetween(MinNameLength, MaxNameLength))
            throw new DomainException($"Templates. Name length must be between {MinNameLength} and {MaxNameLength}");

        Name = name;
    }
    public void SetExercises(ICollection<ExerciseEntity> exercises)
    {
        if (exercises.Count() == 0)
            throw new DomainException("Template. Cannot set an empty set of exercises");

        Exercises = exercises.DistinctBy(e => e.Id).ToList();
    }
}