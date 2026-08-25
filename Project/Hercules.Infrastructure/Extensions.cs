using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql.Replication;
public static class InfrastructureExtensions
{
    public static void AddHasher(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
    }
    public static void AddAuth(this IServiceCollection services, JwtOptions jwtOptions)
    {
        if (jwtOptions == null) 
            throw new NullReferenceException("No Jwt options was found");

        services.AddSingleton<IJwtProvider, JwtProvider>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                }
            );

        // services.AddAuthorization(options =>
        // {
        //     options.AddPolicy("Admin", policy =>
        //     {
        //         policy.RequireClaim("Privilege", Privilege.Admin.ToString());
        //     });
        // });
    }
    public static void AddDbAndItsDependencies(this IServiceCollection services, string connString)
    {
        if (string.IsNullOrEmpty(connString))
            throw new InvalidOperationException("Infrastructure dependencies. Null connection string");

        services.AddDbContext<HerculesContext>(options => options.UseNpgsql(connString));

        services.AddScoped<QueryBuilder<ExerciseEntity>, ExerciseQueryBuilder>();
        services.AddScoped<QueryBuilder<MuscleGroupEntity>, MuscleGroupQueryBuilder>();
        services.AddScoped<QueryBuilder<TemplateEntity>, TemplateQueryBuilder>();
        services.AddScoped<QueryBuilder<UserEntity>, UserQueryBuilder>();
        services.AddScoped<QueryBuilder<WorkoutEntity>, WorkoutQueryBuilder>();

        services.AddScoped(typeof(IEntityRepository<>), typeof(EntityRepository<>));
        services.AddScoped<IExercisesRepository, ExercisesRepository>();
        services.AddScoped<ITemplatesRepository, TemplatesRepository>();
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IWorkoutsRepository, WorkoutsRepository>();
    }
    public static void MigrateDb(this IServiceProvider services)
    {
        using (var scope = services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<HerculesContext>();

            context.Database.Migrate();
        }
    } 
    public async static void SeedData(this IServiceProvider provider, string seedPath)
    {
        string seedString = File.ReadAllText(seedPath);   
        var seed = JsonConvert.DeserializeObject<SeedingData>(seedString);  

        if (seed == null) throw new InvalidOperationException("Null seed data");

        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<HerculesContext>();
            
            await SeedMuscleGroups(context, seed);
            await SeedExercises(context, seed);
        }
    }
    private static async Task SeedMuscleGroups(HerculesContext context, SeedingData seed)
    {
        string[] existing = await context.MuscleGroups
            .AsNoTracking()
            .Select(m => m.Name)
            .ToArrayAsync();

        string[] missing = seed.MuscleGroups
            .Where(m => !existing.Contains(m))
            .ToArray();

        if (missing.Length == 0) return;

        foreach(string muscleGroup in missing)
            context.MuscleGroups.Add(new MuscleGroupEntity(muscleGroup));

        await context.SaveChangesAsync();
    }
    private static async Task SeedExercises(HerculesContext context, SeedingData seed)
    {
        string[] existing = await context.Exercises
            .AsNoTracking()
            .Select(e => e.Name)
            .ToArrayAsync();
        
        ExerciseSeed[] missing = seed.Exercises
            .Where(e => !existing.Contains(e.Name))
            .ToArray();

        if (missing.Length == 0) return;

        MuscleGroupEntity[] muscleGroups = await context.MuscleGroups
            .ToArrayAsync();

        foreach(ExerciseSeed exercise in missing)
            context.Exercises.Add(new (exercise.Name, muscleGroups.Where(m => exercise.MuscleGroups.Contains(m.Name)).ToList()));

        await context.SaveChangesAsync();
    }
}