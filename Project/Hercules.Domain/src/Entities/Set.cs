public class SetEntity : IEntityBase
{
    public int Id { get; private set; }
    public int SessionExerciseId { get; private set; }
    public SessionExerciseEntity SessionExercise { get; private set; } = null!;
    public int Weight { get; private set; }
    public int Reps { get; private set; }
    public int Order { get; private set; }

    public const int MinWeight = 0;
    public const int MaxWeight = 999;
    public const int MinReps = 1;
    public const int MaxReps = 9999; 

    static SetEntity()
    {
        if (MinWeight > MaxWeight)
            throw new Exception("Set. MaxWeight must be greater than MinWeight");

        if (MinReps > MaxReps)
            throw new Exception("Set. MaxReps must be greater than MinReps");
    }
    private SetEntity() {}
    internal SetEntity(int weight, int reps, int order)
    {
        Order = order;
        SetWeight(weight);
        SetReps(reps);
    }
    internal SetEntity SetWeight(int weight)
    {
        if (!weight.IsBetween(MinWeight, MaxWeight))
            throw new DomainException($"Set. Wight must be between {MinWeight} and {MaxWeight}");

        Weight = weight;
        return this;
    }
    internal SetEntity SetReps(int reps)
    {
        if (!reps.IsBetween(MinReps, MaxReps))
            throw new DomainException($"Set. Reps must be between {MinReps} and {MaxReps}");

        Reps = reps;
        return this;
    }
}