using System.ComponentModel.DataAnnotations;

namespace Web.Entities;

public class BasicEntity
{
    [Key]
    public int Id { get; set; }
    public DateTime RecordDate { get; set; }
    public bool Active { get; set; }
    [StringLength(512)] public string Description { get; set; } = null!;
}