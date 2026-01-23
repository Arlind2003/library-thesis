namespace Caktoje.Data.Resources;

public class BookStockResource
{
    public required long Id { get; set; }
    public required BookResource? Book { get; set; }
    public required string Putwall { get; set; }
    public required int Row { get; set; }
    public required int Column { get; set; }
}