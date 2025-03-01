using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace T2Importer.DAL
{
  [Table("01_Batchs")]
  public class Export01Batch
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public DateTime Time { get; set; }

    [StringLength(128)]
    public string? FileName { get; set; }
  }
}
