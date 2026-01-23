using System.ComponentModel.DataAnnotations;

namespace Caktoje.Models;


public class Putwall
{
    [Key]
    public long Id { get; set; }
    public required string Name { get; set; }
    public required int Rows { get; set; }
    public required int Columns { get; set; }
}