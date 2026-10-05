public static class MuscleGroupMapper
{
    public static MuscleGroupMinimalResponse ToMinimalResponse(this MuscleGroupEntity muscle)
    {
        return new(muscle.Id, muscle.Name);
    }
    public static MuscleGroupResponse ToResponse(this MuscleGroupEntity muscle)
    {
        return new(muscle.Id, muscle.Name, muscle.Exercises.Select(e => e.Id));
    }
    public static MuscleGroupDetailedResponse ToDetailedResponse(this MuscleGroupEntity muscle)
    {
        return new(muscle.Id, muscle.Name, muscle.Exercises.Select(e => e.ToMinimalResponse()));
    }
}