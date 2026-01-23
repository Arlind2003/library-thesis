namespace Caktoje.Data.Resources;

public class BookRentResource
{
    public required long Id { get; set; }
    public required UserResource Renter { get; set; }
    public required BookStockResource BookStock { get; set; }
    public required DateTime RentedAt { get; set; }
    public required DateTime ExpiresAt { get; set; }
    public required DateTime? ReturnedAt { get; set; }
}

public class UserResource
{
    public required string Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
}