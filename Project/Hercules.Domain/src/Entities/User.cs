public class UserEntity : IEntityBase
{
    public int Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime RegistrationDate { get; private set; }
    public Privilege Privilege { get; private set; } = Privilege.User;
    public ICollection<TemplateEntity> Templates { get; private set; } = [];
    public ICollection<WorkoutEntity> Workouts { get; private set; } = [];

    public const int MinUsernameLength = 3;
    public const int MaxUsernameLength = 35;
    public const int MinPasswordLength = 6;
    public const int MaxPasswordLength = 35;
    public static readonly string UsernameRegex = $"^[a-zA-Z][a-zA-Z0-9._-]{{{MinUsernameLength - 1},{MaxUsernameLength - 1}}}$";

    static UserEntity()
    {
        if (MinUsernameLength > MaxUsernameLength)
            throw new Exception("User. MaxUsernameLength must be greater than MinUsernameLength");

        if (MinPasswordLength > MaxPasswordLength)
            throw new Exception("User. MaxPasswordLength must be greater than MinPasswordLength");
    }
    private UserEntity() {}
    public UserEntity(string username, string passwordHash, DateTimeOffset registrationDate)
    {
        SetUsername(username);
        SetPasswordHash(passwordHash);
        SetRegTime(registrationDate);
    }
    
    public void SetPasswordHash(string passwordHash) => PasswordHash = passwordHash;
    public void SetUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
            throw new DomainException("User. Cannot set an empty name");

        if (!username.Length.IsBetween(MinUsernameLength, MaxUsernameLength))
            throw new DomainException($"User. Username must be between {MinUsernameLength} and {MaxUsernameLength}");

        Username = username;
    }
    public void SetPrivilege(Privilege privilege) => Privilege = privilege;
    private void SetRegTime(DateTimeOffset time)
    {
        if (time < DateTimeOffset.UtcNow.AddMinutes(-3))
            throw new DomainException($"User. Invalid registration time");

        if (time > DateTimeOffset.UtcNow)
            throw new DomainException("User. Invalid registration time");

        RegistrationDate = time.DateTime;
    }
}