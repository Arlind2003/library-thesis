using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Caktoje.Constants.Enums;

namespace Caktoje.Models;

public class BookStock
{
    [Key]
    public long Id { get; set; }

    [ForeignKey(nameof(Book))]
    public required long BookId { get; set; }
    public Book? Book { get; set; }

    [ForeignKey(nameof(PutwallSection))]
    public required long? PutwallSectionId { get; set; }
    public PutwallSection? PutwallSection { get; set; }
    public required BookStockStateEnum State { get; set; }
}