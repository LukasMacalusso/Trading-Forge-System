namespace TraderForge.Domain.Entities;

public class Administrator
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }

    public string Role { get; set; } = "SystemAdmin";

    public Administrator(string id, string email)
    {
        Id = id;
        Email = email;
        UserName = email;
    }
}