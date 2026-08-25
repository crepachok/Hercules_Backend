using System.Security.Claims;

public class CurrentUserProvider : ICurrentUser
{
    public ClaimsPrincipal User { get; init; }
    public virtual int UserId {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                throw new InvalidTokenException("Id claim not found");
            if (!int.TryParse(claim.Value, out int userId))
                throw new InvalidTokenException("Cannot parse id claim value");

            return userId;
        }
    }
    public CurrentUserProvider(ClaimsPrincipal user) => User = user;
}