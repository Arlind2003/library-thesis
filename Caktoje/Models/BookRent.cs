using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caktoje.Models;

public class BookRent
{
    [Key]
    public long Id { get; set; }

    public required DateOnly RentDate { get; set; }
    public required DateOnly DueDate { get; set; }
    public required DateOnly? ReturnedDate { get; set; }

    [ForeignKey(nameof(BookStock))]
    public required long BookStockId { get; set; }
    public BookStock? BookStock { get; set; }
    public required string RenterId { get; set; }
}