namespace Backend.Models.DTOs;

public class CreateUserRequest
{
    public string Title { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
