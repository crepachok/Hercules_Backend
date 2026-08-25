using System.Security.Claims;

public interface ICurrentUser
{
    ClaimsPrincipal User { get; } 
    int UserId { get; }
}