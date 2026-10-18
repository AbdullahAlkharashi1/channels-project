namespace Ticketing.Api.Users;

public class AppUser
{
    public long Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public Role Role { get; private set; }
    public bool Enabled { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private AppUser()
    {
    }

    public AppUser(string username, string passwordHash, Role role, DateTimeOffset createdAt)
    {
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = createdAt;
        Enabled = true;
    }
}
