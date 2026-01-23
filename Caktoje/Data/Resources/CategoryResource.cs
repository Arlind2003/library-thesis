namespace Caktoje.Data.Resources;

public class CategoryResource
{
    public required long Id { get; set; }
    public required string Name { get; set; }
    public required FileResource Image { get; set; }
}