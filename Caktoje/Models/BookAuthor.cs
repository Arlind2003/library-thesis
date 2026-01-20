using System.ComponentModel.DataAnnotations.Schema;

namespace Caktoje.Models;

public class BookAuthor
{
    [ForeignKey(nameof(Book))]
    public long BookId { get; set; }
    public Book? Book { get; set; }

    [ForeignKey(nameof(Author))]
    public long AuthorId { get; set; }
    public Author? Author { get; set; }
}