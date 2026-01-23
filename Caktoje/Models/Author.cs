using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caktoje.Models;

public class Author
{
    [Key]
    [ForeignKey(nameof(Searchable))]
    public long Id { get; set; }
    public Searchable? Searchable { get; set; }
    public required string FullName { get; set; }
    public required string? Biography { get; set; }
}