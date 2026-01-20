using System.ComponentModel.DataAnnotations;
using Caktoje.Constants.Enums;

namespace Caktoje.Models;

public class DayClosed
{
    [Key]
    public long Id { get; set; }
    public required DateOnly Date { get; set; }
    public required RecurringEnum RecurringType { get; set; }
}
