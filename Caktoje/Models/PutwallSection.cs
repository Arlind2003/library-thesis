using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caktoje.Models;

public class PutwallSection
{
    [Key]
    public long Id { get; set; }
    public required int Row { get; set; }
    public required int Column { get; set; }

    [ForeignKey(nameof(Putwall))]
    public required long PutwallId { get; set; }
    public Putwall? Putwall { get; set; }
}