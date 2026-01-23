using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caktoje.Models;

public class Book
{
    [Key]
    [ForeignKey(nameof(Searchable))]
    public long Id { get; set; }
    public Searchable? Searchable { get; set; }
    public required string ISBN { get; set; }
    public required string Name { get; set; }
    public required string? Description { get; set; }
    public required List<BookCategory> Categories { get; set; }
    public required List<BookAuthor> Authors { get; set; }
    public required List<BookStock> BookStocks { get; set; }
}