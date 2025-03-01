namespace T2WebApplication.Models
{

  public class HistoryAction
  {
    public string text { get; set; }
    public string url { get; set; }
  }
  public class History
  {
    public int Id { get; set; }
    public DateTime EventTime { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public int Age { get; set; }

    public IList<HistoryAction> Actions { get; set; }

    public History()
    {
      Actions = new List<HistoryAction>();
    }
  }

}
