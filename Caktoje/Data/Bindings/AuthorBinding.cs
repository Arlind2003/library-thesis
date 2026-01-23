namespace Caktoje.Data.Bindings;

public class AuthorBinding
{
    public required string FullName { get; set; }
    public required string? Biography { get; set; }
    public required IFormFile? Image { get; set; }
}