namespace Entrvo.Services.Models
{
  public class Export04ReportJobDescriptor : ScheduleJobDescriptor, IJobDescriptor
  {
    private readonly IConsumerService _comsumerService;

    public Export04ReportJobDescriptor(IConsumerService consumerService)
    {
      _comsumerService = consumerService;
    }

    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
      return StartAsync(_comsumerService.GenerateO4ExportFile(cancellationToken), cancellationToken);
    }
  }
}
