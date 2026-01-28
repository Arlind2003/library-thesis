using System.ComponentModel.DataAnnotations;

namespace Caktoje.Data.Bindings;

public class PutwallSectionBinding
{
    [Required]
    public int? Row { get; set; }
    [Required]
    public int? Column { get; set; }
    [Required]
    public long? PutwallId { get; set; }
}
