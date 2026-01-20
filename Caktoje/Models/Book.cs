using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caktoje.Models;

public class Book
{
    [Key]
    [ForeignKey("Searchable")]
    public long Id { get; set; }
    public Searchable? Searchable { get; set; }

    [ForeignKey(nameof(Author))]
    public required long AuthorId { get; set; }
    public Author? Author { get; set; }
    public required string ISBN { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string CoverImageRelativeUrl { get; set; }
}