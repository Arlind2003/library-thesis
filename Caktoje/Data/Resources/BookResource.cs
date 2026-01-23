namespace Caktoje.Data.Resources;

public class BookResource
{
    public required long Id { get; set; }
    public required string Name { get; set; }
    public required string? Description { get; set; }
    public required List<BookStockResource> BookStocks { get; set; }
    public required List<AuthorResource> Authors { get; set; }
    public required List<CategoryResource> Categories { get; set; }
    public required FileResource Image { get; set; }
}