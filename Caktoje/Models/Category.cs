using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caktoje.Models;

public class Category
{
    [Key]
    [ForeignKey(nameof(Searchable))]
    public long Id { get; set; }
    
    public Searchable? Searchable { get; set; }
    public required string Name { get; set; }
}

