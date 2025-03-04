namespace Entrvo.Models
{
  public class ImportJobResponse
  {
    public bool Success { get; set; }
    public Guid JobId { get; set; }
    public string Message { get; set; }    
  }
}
