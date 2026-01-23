namespace Caktoje.Data.Resources;

public class AuthorResource
{
    public required long Id { get; set; }
    public required string Name { get; set; }
    public required string? Biography { get; set; }
    public required FileResource Image { get; set; }
}