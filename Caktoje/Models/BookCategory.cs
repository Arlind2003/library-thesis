using System.ComponentModel.DataAnnotations.Schema;

namespace Caktoje.Models;

public class BookCategory
{
    [ForeignKey(nameof(Book))]
    public long BookId { get; set; }
    public Book? Book { get; set; }

    [ForeignKey(nameof(Category))]
    public long CategoryId { get; set; }
    public Category? Category { get; set; }
}