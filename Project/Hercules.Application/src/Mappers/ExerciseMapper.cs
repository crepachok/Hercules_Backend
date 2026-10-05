public static class ExerciseMapper
{
    public static ExerciseMinimalResponse ToMinimalResponse(this ExerciseEntity exercise)
    {
        return new(exercise.Id, exercise.Name);
    }
    public static ExerciseResponse ToResponse(this ExerciseEntity exercise)
    {
        return new(exercise.Id, exercise.Name, exercise.Muscles.Select(m => m.Id));
    }
    public static ExerciseDetailedResponse ToDetailedResponse(this ExerciseEntity exercise)
    {
        return new (exercise.Id, exercise.Name, exercise.Muscles.Select(m => m.ToMinimalResponse()));
    }
}