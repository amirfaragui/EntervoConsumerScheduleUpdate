using System.ComponentModel.DataAnnotations;

namespace Entrvo.Models
{
  public class DataFolderModel
  {

    [Required]
    [Display(Name = "Monitoring Folder")]
    public string MonitoringFolder { get; set; }

    [Display(Name = "Backup Folder")]
    public string BackupFolder { get; set; }

    public DataFolderModel()
    {
    }
  }


}
