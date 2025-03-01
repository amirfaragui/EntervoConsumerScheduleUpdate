namespace T2WebApplication.Services.Models
{
  public abstract class ScheduleJobDescriptor: IJobDescriptor
  {
    private readonly Guid _id = Guid.NewGuid();
    private readonly DateTime _timestamp = DateTime.Now;

    protected TaskCompletionSource<bool> _taskCompletionSource;

    public ScheduleJobDescriptor() 
    {
      _taskCompletionSource = new TaskCompletionSource<bool>();
    }
    public JobStatus Status { get; set; }

    public Guid JobId => _id;
    public DateTime Timestamp => _timestamp;

    public Task Completion
    {
      get { return _taskCompletionSource.Task; }
    }

    public abstract Task StartAsync(CancellationToken cancellationToken);

    protected Task StartAsync(Task task, CancellationToken cancellationToken = default)
    {
      Status = JobStatus.Running;


      Task.Factory.StartNew(async () => 
      {
        await task.ContinueWith(t =>
        {
          if (t.IsCanceled)
          {
            Status = JobStatus.Cancelled;
            _taskCompletionSource.TrySetCanceled();
          }
          else if (t.IsFaulted)
          {
            Status = JobStatus.Fault;
            _taskCompletionSource?.TrySetException(t.Exception);
          }
          else
          {
            Status = JobStatus.Completed;
            _taskCompletionSource.TrySetResult(t.IsCompletedSuccessfully);
          }
        }, cancellationToken);
      }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

      return Completion;
    }
  }

}
