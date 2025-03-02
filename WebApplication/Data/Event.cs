using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Entrvo.DAL.Attributes;

namespace Entrvo.DAL
{
  public enum JobType
  {
    FtpDownload = 0,
    ManualUpload = 1,
    ConsumerDownload = 2,
    ConsumerUpdate = 3,
    Generate04Report = 4,
    Generate01Report = 5,
    ParseFile = 6,
    SendEmail = 7,
  }

  [AuditIgnore]
  public  class Event
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public DateTime Time { get; set; }

    public JobType Type { get; set; }

    [StringLength(256)]
    public string? Message { get; set; }

    public string? Details { get; set; }

    [StringLength(256)]
    public string? FileUrl { get; set; }

    public Event()
    {
      Time = DateTime.Now;
    }

  }
}
