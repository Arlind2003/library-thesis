namespace Caktoje.Data.Bindings;

public class BookBinding
{
    public required string Name { get; set; }
    public required string? Description { get; set; }
    public required string ISBN { get; set; }
    public required IFormFile? Image { get; set; }
    public required List<long> CategoryIds { get; set; }
    public required List<long> AuthorIds { get; set; }
}