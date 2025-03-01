namespace T2WebApplication.Models
{
  public class SettingsModel
  {
    public FtpSourceModel Source { get; set; }
    public ApiDestinationEditModel Destination { get; set; }

    public bool IsConsumerDatabaseInitialized { get; set; }

    public SettingsModel() {
      Source = new FtpSourceModel();
      Destination = new ApiDestinationEditModel();
    }
  }
}
