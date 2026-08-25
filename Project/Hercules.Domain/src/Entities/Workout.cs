public class WorkoutEntity : IEntityBase
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public UserEntity User { get; private set; } = null!;
    public DateTime StartTime { get; private set; } = default;
    public DateTime? EndTime { get; private set; } = null;
    public ICollection<SessionExerciseEntity> SessionExercises { get; set; } = [];
    public bool IsCompleted { get => EndTime.HasValue; }

    public static string NameofSessionExercises => nameof(SessionExercises);
    public static string NameofUser => nameof(User);

    private WorkoutEntity() { }
    public WorkoutEntity(int userId, DateTimeOffset startTime)
    {
        UserId = userId;
        SetStartTime(startTime);
    }

    public Result Complete(DateTimeOffset endTime)
    {
        if (IsCompleted)
            return Result.Failure(ErrorType.InvalidOperation, "Workout is already completed");

        if (SessionExercises.Count == 0)
            return Result.Failure(ErrorType.InvalidOperation, "Cannot complete an empty workout");

        if (SessionExercises.Any(s => s.Sets.Count == 0))
            return Result.Failure(ErrorType.InvalidOperation, "Cannot complete workout with an empty session exercise");

        SetEndTime(endTime);
        return Result.Success();
    }
    #region SessionExercises
    public Result<SessionExerciseEntity> AddSessionExercise(int exerciseId)
    {
        if (IsCompleted)
            return Result<SessionExerciseEntity>.Failure(ErrorType.InvalidOperation, "Cannot change completed workout");

        int maxOrder = SessionExercises.Count() > 0 ? SessionExercises.Max(s => s.Order) : 0;
        var sessionExercise = new SessionExerciseEntity(exerciseId, ++maxOrder);

        SessionExercises.Add(sessionExercise);
        return Result<SessionExerciseEntity>.Success(sessionExercise);
    }
    public Result RemoveSessionExercise(int sessionExerciseId)
    {
        if (IsCompleted)
            return Result.Failure(ErrorType.InvalidOperation, "Cannot change completed workout");

        var sessionExercise = SessionExercises.FirstOrDefault(s => s.Id == sessionExerciseId);
        if (sessionExercise == null) return Result.Failure(ErrorType.NotFound);

        SessionExercises.Remove(sessionExercise);
        return Result.Success();
    }
    #endregion

    #region Sets
    public Result AddSet(int sessionExerciseId, int weight, int reps)
    {
        if (IsCompleted)
            return Result.Failure(ErrorType.InvalidOperation, "Cannot change completed workout");

        var sessionExercise = SessionExercises.FirstOrDefault(s => s.Id == sessionExerciseId);
        if (sessionExercise == null) return Result.Failure(ErrorType.NotFound);

        sessionExercise.AddSet(weight, reps);
        return Result.Success();

    }
    public Result AddSet(SessionExerciseEntity sessionExercise, int weight, int reps)
    {
        if (IsCompleted)
            return Result.Failure(ErrorType.InvalidOperation, "Cannot change completed workout");

        sessionExercise.AddSet(weight, reps);
        return Result.Success();
    }
    public Result UpdateSet(int setId, int weight, int reps)
    {
        if (IsCompleted)
            return Result.Failure(ErrorType.InvalidOperation, "Cannot change completed workout");

        var sessionExercise = SessionExercises.FirstOrDefault(s => s.Sets.Any(s => s.Id == setId));
        if (sessionExercise == null) return Result.Failure(ErrorType.NotFound);

        sessionExercise.UpdateSet(setId, weight, reps);

        return Result.Success();
    }
    public Result RemoveSet(int setId)
    {
        if (IsCompleted)
            return Result.Failure(ErrorType.InvalidOperation, "Cannot change completed workout");

        var sessionExercise = SessionExercises.FirstOrDefault(s => s.Sets.Any(s => s.Id == setId));
        if (sessionExercise == null) return Result.Failure(ErrorType.NotFound);

        sessionExercise.RemoveSet(setId);

        return Result.Success();
    }
    #endregion
    private void SetStartTime(DateTimeOffset start)
    {
        if (start < DateTimeOffset.Parse(ValidationConstants.MinimalTime))
            throw new DomainException($"Workout. Invalid start time");

        if (start.UtcDateTime > DateTimeOffset.UtcNow)
            throw new DomainException("Workout. Invalid start time");

        StartTime = start.DateTime;
    }
    private void SetEndTime(DateTimeOffset end)
    {
        if (end.DateTime < StartTime)
            throw new DomainException("Workout. EndTime cannot be earlier than StartTime");

        if (end.UtcDateTime > DateTimeOffset.UtcNow)
            throw new DomainException("Workout. Invalid start time");

        EndTime = end.DateTime;
    }
}