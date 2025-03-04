namespace Entrvo.Models
{
  public class JobProgress
  {
    public long TotalRecords { get; set; }
    public long TotalProcessed { get; set; }
    public long Succeed { get; set; }
    public long Failed { get; set; }

    public string Record { get; set; }
    public string Status { get; set; }

    public string FileName { get; set; }    

    public override string ToString()
    {
      return string.Format("[{0,-12}] {1}", Status.ToString().ToUpper(), Record);
    }
  }
}
