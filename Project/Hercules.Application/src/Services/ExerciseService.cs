using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

public class ExerciseService : ServiceBase
{
    private readonly IExercisesRepository _eRepo;
    private readonly IEntityRepository<MuscleGroupEntity> _mRepo;
    private readonly IDatabase _redis;
    private readonly ILogger<ExerciseService> _logger;

    private const string _exercisesKey = "exercises:all";
    private const string _musclesKey = "musclegroups:all";
    private static bool WasInvalidated = false;
    public ExerciseService(IExercisesRepository eRepo, IEntityRepository<MuscleGroupEntity> mRepo, IDatabase redis, ILogger<ExerciseService> logger, ICurrentUser user) : base(user)
    { 
        (_eRepo, _mRepo, _redis, _logger) = (eRepo, mRepo, redis, logger);

        if (!WasInvalidated)
        {
            _redis.KeyDelete(_exercisesKey);
            _redis.KeyDelete(_musclesKey);
            WasInvalidated = true;
        }
    }
    public async Task<Result<IEnumerable<ExerciseResponse>>> GetAll()
    {
        var exercises = await GetCached(_exercisesKey, () => _eRepo.GetAll(1000));
        if (exercises is not { Length: > 0 })
            return Result<IEnumerable<ExerciseResponse>>.Failure(ErrorType.NotFound);

        return Result<IEnumerable<ExerciseResponse>>
            .Success(exercises.Select(e => e.ToResponse()));
    }
    public async Task<Result<IEnumerable<MuscleGroupResponse>>> GetAllMuscleGroups()
    {
        var muscles = await GetCached(_musclesKey, () => _mRepo.GetAll(1000));
        if (muscles is not { Length: > 0 })
            return Result<IEnumerable<MuscleGroupResponse>>.Failure(ErrorType.NotFound);

        return Result<IEnumerable<MuscleGroupResponse>>
            .Success(muscles.Select(m => m.ToResponse()));
    }
    public async Task<Result<IEnumerable<ExerciseResponse>>> GetFiltered(ExerciseSearchFilter filter)
    {
        var exercises = await _eRepo.GetFiltered(filter.Name, filter.MuscleGroups);
        if (exercises is not { Length: > 0 }) 
            return Result<IEnumerable<ExerciseResponse>>.Failure(ErrorType.NotFound);

        return Result<IEnumerable<ExerciseResponse>>
            .Success(exercises.Select(e => e.ToResponse()));
    }
    public async Task<Result<ExerciseResponse>> Get(int exerciseId)
    {
        var exercises = await GetCached(_exercisesKey, () => _eRepo.GetAll(1000));
        if (exercises is not { Length: > 0 })
            return Result<ExerciseResponse>.Failure(ErrorType.NotFound, "Cannot find exercises");

        var exercise = exercises.FirstOrDefault(e => e.Id == exerciseId);
        if (exercise == null)
            return Result<ExerciseResponse>.Failure(ErrorType.NotFound, $"No exercise with id: {exerciseId}");

        return Result<ExerciseResponse>
            .Success(exercise.ToResponse());
    }
    private async Task<T?> GetCached<T>(string key, Func<Task<T>> get)
    {
        string? cached = await _redis.StringGetAsync(key);
        T? result; 

        if (!string.IsNullOrEmpty(cached))
        {
            _logger.LogInformation($"Got cached: {cached}");
            result = JsonSerializer.Deserialize<T>(cached);
        } else {
            result = await get();
            string json = JsonSerializer.Serialize(result);

            await _redis.StringSetAsync(key, json, TimeSpan.FromMinutes(30));
        }

        return result;
    }
}