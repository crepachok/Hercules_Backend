public sealed class UsersService : ServiceBase
{
    private readonly IUsersRepository _uRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtProvider _jwtProvider;
    public UsersService(IPasswordHasher hasher, IUsersRepository repo, IJwtProvider jwtProvider, ICurrentUser user) : base(user)
        => (_hasher, _uRepo, _jwtProvider) = (hasher, repo, jwtProvider);
    public async Task<Result> Register(UserCredentialsDTO cred)
    {
        var user = await _uRepo.Get(cred.Username);
        if (user != null) return Result.Failure(
                ErrorType.Conflict,
                "Username is occupied");

        string passwordHash = _hasher.Generate(cred.Password);
        var newUser = new UserEntity(cred.Username, passwordHash, DateTimeOffset.UtcNow);
        var time = DateTimeOffset.UtcNow;

        await _uRepo.Post(newUser);
        return Result.Success();
    }
    public async Task<Result<string>> Login(UserCredentialsDTO cred)
    {
        var user = await _uRepo.Get(cred.Username);
        if (user == null) return Result<string>.Failure(ErrorType.NotFound);

        var verificationResult = _hasher.Verify(user.PasswordHash, cred.Password);
        if (!verificationResult) return Result<string>.Failure(ErrorType.Unauthorized);

        string token = _jwtProvider.GenerateToken(user);
        return Result<string>.Success(token);
    }
    public async Task<UserResponse?> GetSelf()
    {
        var userId = _user.UserId;
        var userEntity = await _uRepo.Get(userId);
        if (userEntity == null) return null;

        return userEntity.ToResponse();
    }
    public async Task<UserResponse?> Get(int userId)
    {
        var user = await _uRepo.Get(userId);
        if (user == null) return null;

        return user.ToResponse();
    }
    public async Task<UserResponse?> Get(UsernameRequest username)
    {
        var user = await _uRepo.Get(username.Username);
        if (user == null) return null;

        return user.ToResponse();
    }
    public async Task<Result> ChangePassword(PasswordRequest password, int? userId = null)
    {
        if (!userId.HasValue)
            userId = _user.UserId;

        var user = await _uRepo.Get(userId.Value);
        if (user == null) 
            return Result.Failure(ErrorType.NotFound);

        string newHash = _hasher.Generate(password.Password);
        user.SetPasswordHash(newHash);

        await _uRepo.Update(user);
        return Result.Success();
    }
    public async Task<Result> ChangeUsername(UsernameRequest username, int? userId = null)
    {
        if (!userId.HasValue)
            userId = _user.UserId;

        var userEntity = await _uRepo.Get(userId.Value);
        if (userEntity == null) 
            return Result.Failure(ErrorType.NotFound);

        userEntity.SetUsername(username.Username);
        await _uRepo.Update(userEntity);

        return Result.Success();
    }
    public async Task<Result> ChangePrivilege(int userId, Privilege privilege)
    {
        var user = await _uRepo.Get(userId);
        if (user == null)
            return Result.Failure(ErrorType.NotFound);

        user.SetPrivilege(privilege);
        await _uRepo.Update(user);

        return Result.Success();
    }
    public async Task<Result> Delete(int userId)
    {
        bool result = await _uRepo.Delete(userId);
        if (!result) 
            return Result.Failure(ErrorType.NotFound);

        return Result.Success();
    }
}