using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Caktoje.Constants.Enums;

namespace Caktoje.Models;

public class Searchable
{
    [Key]
    public long Id { get; set; }
    public required string Name { get; set; }
    public required SearchableType Type { get; set; }

    [ForeignKey(nameof(File))]
    public required long FileId { get; set; }
    public File? File { get; set; }
}