namespace Caktoje.Data.Bindings;

public class BookRentBinding
{
    public required string RenterId { get; set; }
    public required long BookStockId { get; set; }
    public required DateTime ExpiresAt { get; set; }
}