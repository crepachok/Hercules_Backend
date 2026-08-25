using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;

public sealed class WorkoutService : ServiceBase
{
    private readonly IWorkoutsRepository _wRepo;
    private readonly IExercisesRepository _eRepo;
    public WorkoutService(IWorkoutsRepository wRepo, IExercisesRepository eRepo, ICurrentUser user) : base(user)
        => (_wRepo, _eRepo) = (wRepo, eRepo);

    public async Task<WorkoutSummaryResponse[]> GetAll(int amount, int page)
    {
        int userId = _user.UserId;

        var workouts = await _wRepo.GetAll(userId, amount, page);
        return workouts.Select(w => w.ToSummaryDTO()).ToArray();
    }
    public async Task<WorkoutSummaryResponse[]> GetAllFiltered(WorkoutSearchFilter filter)
    {
        int userId = _user.UserId;

        var workouts = await _wRepo.GetAllFiltered(userId, filter);
        return workouts.Select(w => w.ToSummaryDTO()).ToArray();
    }
    public async Task<Result<WorkoutDetailedResponse>> Get(int id)
    {
        var accessResult = await CheckAccessAndGet(id);
        if (accessResult.IsFailure)
            return Result<WorkoutDetailedResponse>.Failure(accessResult.ErrorType);

        return Result<WorkoutDetailedResponse>.Success(accessResult.Value.ToDetailedDTO());
    }
    public async Task Start(DateTimeRequest time)
    {
        int userId = _user.UserId;

        var workout = new WorkoutEntity(userId, time.DateTime);
        await _wRepo.Post(workout);
    }
    public async Task<Result> Post(WorkoutRequest request)
    {
        int userId = _user.UserId;
        var workout = new WorkoutEntity(userId, request.StartTime);

        int[] requestIds = request.SessionExercises.Select(se => se.ExerciseId).ToArray();
        int[] realIds = (await _eRepo.Get(requestIds)).Select(e => e.Id).ToArray();

        int[] missingIds = requestIds.Where(id => !realIds.Contains(id)).ToArray();
        if (missingIds.Length > 0)
            return Result.Failure(ErrorType.NotFound, $"Cannot find exercises with ids: {string.Join(", ", missingIds)}");

        // ПРОВЕРКИ В ЭТИХ ЦИКЛАХ НА ВСЯКИЙ СЛУЧАЙ, ЕСЛИ ПОЯВЯТСЯ ДРУГИЕ ВОЗНОЖНОСТИ ПОЛУЧИТЬ FAILURE
        foreach (var se in request.SessionExercises)
        {
            var addSessionResult = workout.AddSessionExercise(se.ExerciseId);
            if (addSessionResult.IsFailure)
                    return addSessionResult;

            var sessionExercise = addSessionResult.Value;

            foreach (var set in se.Sets)
            {
                var addSetResult = workout.AddSet(sessionExercise, set.Weight, set.Reps);
                if (addSetResult.IsFailure)
                    return addSetResult;
            }
        }

        if (request.EndTime.HasValue)
        {
            var completeResult = workout.Complete(request.EndTime.Value);
            if (completeResult.IsFailure)
                return completeResult;
        }

        await _wRepo.Post(workout);
        return Result.Success();
    }
    public async Task<Result> Complete(int workoutId, DateTimeRequest time)
    {
        return await UpdateWorkout(workoutId, w => w.Complete(time.DateTime));
    }
    public async Task<Result> Delete(int workoutId)
    {
        var accessResult = await CheckAccessAndGet(workoutId);
        if (accessResult.IsFailure)
            return accessResult;

        await _wRepo.Delete(workoutId);
        return Result.Success();
    }
    public async Task<Result> AddSessionExercise(int workoutId, int exerciseId)
    {
        var exercise = await _eRepo.Get(exerciseId);
        if (exercise == null)
            return Result.Failure(ErrorType.NotFound, 
                $"No exercise was found with id: {exerciseId}");

        return await UpdateWorkout(workoutId, w => w.AddSessionExercise(exerciseId));
    }
    public async Task<Result> RemoveSessionExercise(int workoutId, int sessionId)
    {
        return await UpdateWorkout(workoutId, w => w.RemoveSessionExercise(sessionId));
    }
    public async Task<Result> AddSet(int workoutId, int sessionId, SetRequest request)
    {
        return await UpdateWorkout(workoutId, w => w.AddSet(sessionId, request.Weight, request.Reps));
    }
    public async Task<Result> UpdateSet(int workoutId, int setId, SetRequest request)
    {
        return await UpdateWorkout(workoutId, w => w.UpdateSet(setId, request.Weight, request.Reps));
    }
    public async Task<Result> DeleteSet(int workoutId, int setId)
    {
        return await UpdateWorkout(workoutId, w => w.RemoveSet(setId));
    }

    private async Task<Result<WorkoutEntity>> CheckAccessAndGet(int workoutId, bool isTracking = false)
    {
        var workout = await _wRepo.Get(workoutId, isTracking);
        if (workout == null) 
            return Result<WorkoutEntity>.Failure(ErrorType.NotFound);

        int userId = _user.UserId;
        if (workout.UserId != userId)
            return Result<WorkoutEntity>.Failure(ErrorType.Forbidden);

        return Result<WorkoutEntity>.Success(workout);
    }
    private async Task<Result> UpdateWorkout(int workoutId, Func<WorkoutEntity, Result> action)
    {
        var accessResult = await CheckAccessAndGet(workoutId, true);
        if (accessResult.IsFailure)
            return accessResult;

        var workout = accessResult.Value;

        var modifyResult = action(workout);
        if (modifyResult.IsFailure)
            return modifyResult;

        await _wRepo.Update(workout);

        return Result.Success();
    }
}