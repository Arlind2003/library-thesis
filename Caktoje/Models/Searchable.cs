using System.ComponentModel.DataAnnotations;
using Caktoje.Constants.Enums;

namespace Caktoje.Models;

public class Searchable
{
    [Key]
    public long Id { get; set; }
    public required string Name { get; set; }
    public required SearchableType Type { get; set; }
}