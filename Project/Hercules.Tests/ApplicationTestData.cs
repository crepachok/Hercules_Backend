internal static partial class TestData
{
    internal static TemplateRequest GetTemplateRequest(int exerciseCount = 3)
    {
        var request = new TemplateRequest();

        typeof(TemplateRequest).GetProperty(nameof(request.Name))!
            .SetValue(request, new string('a', TemplateEntity.MinNameLength));

        typeof(TemplateRequest).GetProperty(nameof(request.ExerciseIds))!
            .SetValue(request, Enumerable.Range(1, exerciseCount).ToArray());

        return request;
    }
    internal static TemplateEntity GetTemplate(int userId = 1, int length = 3)
    {
        return new TemplateEntity(userId, "Training", GetExercises(3).ToArray());
    }
    internal static IEnumerable<ExerciseEntity> GetExercises(int length = 3)
    {
        for (int i = 0; i < length; i++)
        {
            var exercise = new ExerciseEntity(new string('a', ExerciseEntity.MinNameLength),
                new MuscleGroupEntity[]
                {
                    new (new string('z', MuscleGroupEntity.MinNameLength)),
                    new (new string('x', MuscleGroupEntity.MinNameLength)),
                    new (new string('c', MuscleGroupEntity.MinNameLength))
                });
            
            typeof(ExerciseEntity).GetProperty("Id")!.SetValue(exercise, i + 1);
            yield return exercise;
        }
    }
    internal static UserCredentialsDTO GetCredentials(string? username = null, string? password = null)
    {
        var cred = new UserCredentialsDTO();

        typeof(UserCredentialsDTO).GetProperty(nameof(cred.Username))!
            .SetValue(cred, username ?? new string('a', UserEntity.MinUsernameLength));

        typeof(UserCredentialsDTO).GetProperty(nameof(cred.Password))!
            .SetValue(cred, password ?? new string('b', UserEntity.MinPasswordLength));

        return cred;
    }
    internal static UserEntity GetUser()
    {
        var cred = GetCredentials();

        return new (
            cred.Username,
            cred.Password,
            DateTimeOffset.UtcNow);
    }
    internal static PasswordRequest GetPasswordRequest(string? password = null)
    {
        if (password == null)
            password = new ('a', UserEntity.MinPasswordLength);

        if (!password.Length.IsBetween(UserEntity.MinPasswordLength, UserEntity.MaxPasswordLength))
            throw new ArgumentException("Password is invalid");

        var request = new PasswordRequest();

        typeof(PasswordRequest).GetProperty(nameof(request.Password))!
            .SetValue(request, password);

        return request;
    }
    internal static UsernameRequest GetUsernameRequest(string? username = null)
    {
        if (username == null)
            username = new ('b', UserEntity.MinPasswordLength);

        if (!username.Length.IsBetween(UserEntity.MinUsernameLength, UserEntity.MaxUsernameLength))
            throw new ArgumentException("Username is invalid");

        var request = new UsernameRequest();

        typeof(UsernameRequest).GetProperty(nameof(request.Username))!
            .SetValue(request, username);

        return request;
    }
    internal static WorkoutEntity GetEmptyWorkout(int userId = 1)
    {
        return new (userId, DateTimeOffset.UtcNow);
    }
    internal static WorkoutEntity GetFilledWorkout(int userId = 1)
    {
        int workoutId = 1;
        var workout = new WorkoutEntity(userId, DateTimeOffset.UtcNow);
        typeof(WorkoutEntity).GetProperty(nameof(workout.Id))!
            .SetValue(workout, workoutId);

        workout.AddSessionExercise(1);
        var session = workout.SessionExercises.First();
        typeof(SessionExerciseEntity).GetProperty(nameof(session.Id))!
            .SetValue(session, 1);

        for (int i = 0; i < 3; i++)
        {
            var set = new SetEntity(15, 15, i + 1);

            typeof(SetEntity).GetProperty(nameof(set.Id))!
                .SetValue(set, i + 1);

            session.Sets.Add(set);
        }

        return workout;
    }
    internal static DateTimeRequest GetDateTimeRequest(DateTimeOffset? time = null)
    {
        if (!time.HasValue)
            time = DateTimeOffset.UtcNow;

        var request = new DateTimeRequest();

        typeof(DateTimeRequest).GetProperty(nameof(request.DateTime))!
            .SetValue(request, time);

        return request;
    }
    internal static WorkoutRequest GetWorkoutRequest(IEnumerable<int> exerciseIds, bool isCompleted = true, bool hasSessionExercises = true, bool hasSets = true)
    {
        var request = new WorkoutRequest();

        if (hasSessionExercises)
        {
            var sessionExercises = new List<SessionExerciseRequest>();
            foreach (int exerciseId in exerciseIds)
                sessionExercises.Add(GetSessionExerciseRequest(exerciseId, hasSets));
            
            typeof(WorkoutRequest).GetProperty(nameof(request.SessionExercises))!
                .SetValue(request, sessionExercises); 
        }

        typeof(WorkoutRequest).GetProperty(nameof(request.StartTime))!
            .SetValue(request, DateTimeOffset.UtcNow.AddMinutes(-30));

        if (isCompleted)
            typeof(WorkoutRequest).GetProperty(nameof(request.EndTime))!
                .SetValue(request, DateTimeOffset.UtcNow);

        return request;
    }
    private static SessionExerciseRequest GetSessionExerciseRequest(int exerciseId, bool hasSets = true)
    {
        var request = new SessionExerciseRequest();

        if (hasSets)
        {
            var sets = new SetRequest[] { GetSetRequest(50, 50) };
            typeof(SessionExerciseRequest).GetProperty(nameof(request.Sets))!
                .SetValue(request, sets);
        }

        typeof(SessionExerciseRequest).GetProperty(nameof(request.ExerciseId))!
            .SetValue(request, exerciseId);

        return request;
    }
    internal static SetRequest GetSetRequest(int weight, int reps)
    {
        if (!weight.IsBetween(SetEntity.MinWeight, SetEntity.MaxWeight))
            throw new ArgumentException("Weight is invalid");

        if (!reps.IsBetween(SetEntity.MinReps, SetEntity.MaxReps))
            throw new ArgumentException("Reps is invalid");

        var request = new SetRequest();

        typeof(SetRequest).GetProperty(nameof(request.Weight))!
            .SetValue(request, weight);
        typeof(SetRequest).GetProperty(nameof(request.Reps))!
            .SetValue(request, reps);

        return request;
    }
}