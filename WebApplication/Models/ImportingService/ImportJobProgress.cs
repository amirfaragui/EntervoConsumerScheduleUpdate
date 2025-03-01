namespace T2WebApplication.Models
{

  public class ImportJobProgress
  {
    public int TotalRecords { get; set; }
    public int TotalProcessed { get; set; }
    public int Succeed { get; set; }
    public int Failed { get; set; } 

    public string Record { get; set; }
    public bool RecordProcessed { get; set; }
  }
}
