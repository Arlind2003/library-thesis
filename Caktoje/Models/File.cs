using System.ComponentModel.DataAnnotations;

namespace Caktoje.Models;

public class File
{
    [Key]
    public long Id { get; set; }
    public required string RelativeDirectory { get; set; }
    public required string FileName { get; set; }
}