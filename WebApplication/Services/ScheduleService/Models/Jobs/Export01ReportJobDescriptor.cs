namespace Entrvo.Services.Models
{
  public class Export01ReportJobDescriptor : ScheduleJobDescriptor, IJobDescriptor
  {
    private readonly IConsumerService _comsumerService;

    public Export01ReportJobDescriptor(IConsumerService consumerService)
    {
      _comsumerService = consumerService;
    }

    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
      return StartAsync(_comsumerService.GenerateO1ExportFile(cancellationToken), cancellationToken);
    }
  }
}
