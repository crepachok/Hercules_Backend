public class SessionExerciseEntity : IEntityBase
{
    public int Id { get; private set; }
    public int WorkoutId { get; private set; }
    public WorkoutEntity Workout { get; private set;} = null!;
    public int ExerciseId { get; private set; }
    public ExerciseEntity Exercise { get; private set; } = null!;
    public ICollection<SetEntity> Sets { get; private set; } = [];
    public int Order { get; private set; }

    private SessionExerciseEntity() {}
    internal SessionExerciseEntity(int exerciseId, int order)
        => (ExerciseId, Order) = (exerciseId, order);

    internal void AddSet(int weight, int reps)
    {
        int maxOrder = Sets.Count() > 0 ? Sets.Max(s => s.Order) : 0;
        var set = new SetEntity(weight, reps, ++maxOrder); 

        Sets.Add(set);
    }
    internal bool UpdateSet(int setId, int weight, int reps)
    {
        var set = Sets.FirstOrDefault(s => s.Id == setId);
        
        if (set == null) return false;

        set.SetWeight(weight);
        set.SetReps(reps);

        return true;
    }
    internal bool RemoveSet(int setId)
    {
        var set = Sets.FirstOrDefault(s => s.Id == setId);

        if (set == null) return false;

        Sets.Remove(set);
        return true;
    }
}