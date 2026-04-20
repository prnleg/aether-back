namespace Aether.Domain.Entities;

public class UserProfile
{
    public Guid UserId { get; private set; }
    public string? SteamId { get; private set; }

    private UserProfile() { }

    public UserProfile(Guid userId, string? steamId = null)
    {
        UserId = userId;
        SteamId = steamId;
    }

    public void SetSteamId(string steamId) => SteamId = steamId;
}
