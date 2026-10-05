using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

public class ExerciseService : ServiceBase
{
    private readonly IEntityRepository<ExerciseEntity> _eRepo;
    private readonly IEntityRepository<MuscleGroupEntity> _mRepo;
    private readonly IDatabase _redis;
    private readonly ILogger<ExerciseService> _logger;

    private const string _exercisesKey = "exercises:all";
    private const string _musclesKey = "musclegroups:all";
    private static bool WasInvalidated = false;
    public ExerciseService(IEntityRepository<ExerciseEntity> eRepo, IEntityRepository<MuscleGroupEntity> mRepo, IDatabase redis, ILogger<ExerciseService> logger, ICurrentUser user) : base(user)
    { 
        (_eRepo, _mRepo, _redis, _logger) = (eRepo, mRepo, redis, logger);

        if (!WasInvalidated)
        {
            _redis.KeyDelete(_exercisesKey);
            _redis.KeyDelete(_musclesKey);
            WasInvalidated = true;
        }
    }
    public async Task<IEnumerable<TResponse>> GetAll<TResponse>(Func<ExerciseEntity, TResponse> map, ExerciseSearchFilter? filter = null) where TResponse : class
    {
        var exercises = await GetCached(_exercisesKey, () => _eRepo.GetAll(1000));

        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.Name))
            {
                exercises = exercises.Where(e => 
                    e.Name.ToLowerInvariant()
                    .Contains(filter.Name.ToLowerInvariant()))
                    .ToArray();
            }

            if (filter.MuscleGroups is { Length: > 0 })
            {
                exercises = exercises.Where(e =>
                {
                    HashSet<string> exerciseMuscles = e.Muscles
                        .Select(m => m.Name.ToLowerInvariant())
                        .ToHashSet();

                    return filter.MuscleGroups.All(m => exerciseMuscles.Contains(m.ToLowerInvariant()));
                }).ToArray();
            }
        }

        return exercises.Select(e => map(e));
    }
    public async Task<TResponse?> Get<TResponse>(int exerciseId, Func<ExerciseEntity, TResponse> map) where TResponse : class
    {
        var exercises = await GetCached(_exercisesKey, () => _eRepo.GetAll(1000));
        var exercise = exercises.FirstOrDefault(e => e.Id == exerciseId);
        
        return exercise != null ? map(exercise) : null;
    }
    public async Task<IEnumerable<TResponse>> GetAllMuscleGroups<TResponse>(Func<MuscleGroupEntity, TResponse> map) where TResponse : class
    {
        var muscles = await GetCached(_musclesKey, () => _mRepo.GetAll(1000));

        return muscles.Select(m => map(m));
    }
    private async Task<T> GetCached<T>(string key, Func<Task<T>> get)
    {
        string? cached = await _redis.StringGetAsync(key);
        T result; 

        if (!string.IsNullOrEmpty(cached))
        {
            _logger.LogInformation($"Got cached: {cached}");
            result = JsonSerializer.Deserialize<T>(cached)!;
        } else {
            result = await get();
            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.IgnoreCycles });

            await _redis.StringSetAsync(key, json, TimeSpan.FromMinutes(30));
        }

        return result;
    }
}