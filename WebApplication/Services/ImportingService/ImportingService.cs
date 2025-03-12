using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Entrvo.DAL;
using Entrvo.Models;

namespace Entrvo.Services
{
  public class ImportingService : IImportingService
  {
    private readonly IServiceScopeFactory _serviceFactory;
    private readonly ILogger<ImportingService> _logger;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ConcurrentDictionary<Guid, ImportJobDescriptor> _jobDescriptors;
    private readonly IMqttService _mqttService;


    public ImportingService(IServiceScopeFactory serviceFactory,
                            IWebHostEnvironment hostingEnvironment,
                            ILogger<ImportingService> logger,
                            IMqttService mqttService)
    {
      _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
      _mqttService = mqttService ?? throw new ArgumentNullException(nameof(MqttService));

      _jobDescriptors = new ConcurrentDictionary<Guid, ImportJobDescriptor>();
    }

    public async Task<ImportJobDescriptor> AddJob(ImportSourceModel job)
    {
      if (job.Mappings == null)
        throw new Exception("Not column mappings defined.");

      var fieldsCount = job.Mappings.Count();

      var path = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data", job.FileName);
      if (File.Exists(path))
      {
        var descriptor = new ImportJobDescriptor
        {
          Source = job,
        };
        using (var reader = new StreamReader(path))
        {
          var count = await reader.ReadRecordAsync(job.Delimiter, fieldsCount).CountAsync();

          if (count > 1)
          {
            if (job.FirstLineHeader) count--;
          }
          descriptor.TotalRecords = count;
        }

        if (_jobDescriptors.TryAdd(descriptor.Id, descriptor))
          return descriptor;
      }

      throw new Exception($"Failed to add import job for '{job.FileName}'.");
    }

    public bool TryGetJob(Guid jobId, out ImportJobDescriptor? jobDescriptor)
    {
      return _jobDescriptors.TryGetValue(jobId, out jobDescriptor);
    }
    public bool TryGetJob<T>(out ImportJobDescriptor? jobDescriptor) where T : class, IDataEntity
    {
      jobDescriptor = null;

      var type = typeof(T);
      var match = _jobDescriptors.Where(i => i.Value.Source.TargetType == type).ToArray();
      if (match.Any())
      {
        jobDescriptor = match.First().Value;
        return true;
      }

      return false;
    }


    public Task StartJob(Guid jobId)
    {
      if (_jobDescriptors.TryGetValue(jobId, out var jobDescriptor))
      {
        if (jobDescriptor.Status == ImportJobStatus.Created)
        {
          jobDescriptor.CancellationTokenSource = new CancellationTokenSource();
          var token = jobDescriptor.CancellationTokenSource.Token;

          switch (jobDescriptor.Source.TargetType.Name)
          {
            case "Customer":
              jobDescriptor.Task = Task.Run(() => ImportCustomers(jobDescriptor, token), token);
              break;
            case "Permit":
              jobDescriptor.Task = Task.Run(() => ImportPermits(jobDescriptor, token), token);
              break;
            case "Email":
              jobDescriptor.Task = Task.Run(() => ImportEmails(jobDescriptor, token), token);
              break;
            case "Note":
            case "CustomerNote":
            case "PermitNote":
              jobDescriptor.Task = Task.Run(() => ImportNotes(jobDescriptor, token), token);
              break;
            default:
              throw new NotImplementedException();
          }

          //jobDescriptor.Task = Task.Run(() => ImportRoutine(jobDescriptor, token), token);
          jobDescriptor.Status = ImportJobStatus.Running;
          return jobDescriptor.Task;
        }
      }
      return null;
    }

    public bool IsFileInUse(string fileName)
    {
      return _jobDescriptors.Any(i => i.Value.Source.FileName == fileName);
    }

    private async Task ImportCustomers(ImportJobDescriptor job, CancellationToken cancellationToken)
    {
      var properties = typeof(Customer).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(i => i.Name, i => i);

      var path = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data", job.Source.FileName);
      if (File.Exists(path))
      {
        using var scope = _serviceFactory.CreateScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        using (var reader = new StreamReader(path))
        {
          int count = 0, succeed = 0, failed = 0;
          if (job.Source.FirstLineHeader)
          {
            await reader.ReadLineAsync();
          }

          var fieldsCount = job.Source.Mappings.Count();

          try
          {
            await foreach (var fields in reader.ReadRecordAsync(job.Source.Delimiter, fieldsCount, job.CancellationTokenSource.Token))
            {
              count++;
              var processed = false;

              if (fields.Count() == job.Source.Mappings.Count())
              {
                var newCustomer = false;

                var columns = fields.Zip(job.Source.Mappings).Select(i => new ImportColumnModel
                {
                  Text = i.First,
                  Format = i.Second.Format,
                  Column = i.Second.Target
                })
                  .Where(i => i.Column != "-NotUsed-")
                  .ToList();


                var customerIdColumn = columns.FirstOrDefault(i => i.Column == "CustomerUID");
                if (customerIdColumn == null)
                {
                  failed++;
                  goto report_status;
                }
                columns.Remove(customerIdColumn);
                var customerId = Convert.ToInt32(customerIdColumn.Text);

                var customer = await dbContext.Customers.FindAsync(customerId);
                if (customer == null)
                {
                  customer = new Customer() { CustomerUID = customerId };
                  dbContext.Customers.Add(customer);
                  newCustomer = true;
                }

                var entity = (IDataEntity<int>)customer;
                foreach (var column in columns)
                {
                  var key = column.Column.ToString();
                  if (properties.ContainsKey(key))
                  {
                    var prop = properties[key];
                    if (!entity.TrySetValue(prop, column, out var value))
                    {
                      _logger.LogWarning($"Failed to set value {column.Text} to customer {key}");
                      dbContext.ChangeTracker.Clear(); failed++;
                      goto report_status;
                    }
                  }
                }

                try
                {
                  await dbContext.SaveChangesAsync().ConfigureAwait(false);
                  processed = true;
                  succeed++;
                }
                catch (Exception ex)
                {
                  _logger.LogError(ex, $"Failed to import customer {entity.GetId()}");
                  failed++;
                }
                finally
                {
                  dbContext.ChangeTracker.Clear();
                }
              }
              else
              {
                failed++;
              }

            report_status:
              var progress = new ImportJobProgress
              {
                TotalRecords = job.TotalRecords,
                TotalProcessed = succeed + failed,
                Succeed = succeed,
                Failed = failed,
                RecordProcessed = processed
              };

              await _mqttService.PublishAsync($"progress/{job.Id:N}", progress, MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);

            }

            job.Status = ImportJobStatus.Completed;
          }
          catch (OperationCanceledException)
          {
            job.Status = ImportJobStatus.Cancelled;

          }
          finally
          {
            if (_jobDescriptors.TryRemove(job.Id, out var o))
            {
              o.Dispose();
            }
          }
        }

        try
        {
          File.Delete(path);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex.ToString());
        }

      }
    }

    private async Task ImportPermits(ImportJobDescriptor job, CancellationToken cancellationToken)
    {
      var properties = typeof(Permit).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(i => i.Name, i => i);

      var path = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data", job.Source.FileName);
      if (File.Exists(path))
      {
        using var scope = _serviceFactory.CreateScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        using (var reader = new StreamReader(path))
        {
          int count = 0, succeed = 0, failed = 0;
          if (job.Source.FirstLineHeader)
          {
            await reader.ReadLineAsync();
          }

          var fieldsCount = job.Source.Mappings.Count();

          try
          {
            await foreach (var fields in reader.ReadRecordAsync(job.Source.Delimiter, fieldsCount, job.CancellationTokenSource.Token))
            {
              count++;
              var processed = false;

              if (fields.Count() == job.Source.Mappings.Count())
              {
                var newPermit = false;

                var columns = fields.Zip(job.Source.Mappings).Select(i => new ImportColumnModel
                {
                  Text = i.First,
                  Format = i.Second.Format,
                  Column = i.Second.Target
                })
                  .Where(i => i.Column != "-NotUsed-")
                  .ToList();


                var permitIdColumn = columns.FirstOrDefault(i => i.Column == "PermitUID");
                if (permitIdColumn == null)
                {
                  failed++;
                  goto report_status;
                }
                columns.Remove(permitIdColumn);
                var permitId = Convert.ToInt32(permitIdColumn.Text);

                var permit = await dbContext.Permits.FindAsync(permitId);
                if (permit == null)
                {
                  permit = new Permit() { PermitUID = permitId };
                  dbContext.Permits.Add(permit);
                  newPermit = true;
                }

                var entity = (IDataEntity<int>)permit;
                foreach (var column in columns)
                {
                  var key = column.Column.ToString();
                  if (properties.ContainsKey(key))
                  {
                    var prop = properties[key];
                    if (!entity.TrySetValue(prop, column, out var value))
                    {
                      _logger.LogWarning($"Failed to set value {column.Text} to permit {key}");
                      dbContext.ChangeTracker.Clear();
                      failed++;
                      goto report_status;
                    }
                  }
                }

                try
                {
                  await dbContext.SaveChangesAsync().ConfigureAwait(false);
                  processed = true;
                  succeed++;
                }
                catch (Exception ex)
                {
                  _logger.LogError(ex, $"Failed to import permit {entity.GetId()}.");
                  failed++;
                }
                finally
                {
                  dbContext.ChangeTracker.Clear();
                }
              }
              else
              {
                failed++;
              }

            report_status:
              var progress = new ImportJobProgress
              {
                TotalRecords = job.TotalRecords,
                TotalProcessed = succeed + failed,
                Succeed = succeed,
                Failed = failed,
                RecordProcessed = processed
              };

              await _mqttService.PublishAsync($"progress/{job.Id:N}", progress, MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);

            }

            job.Status = ImportJobStatus.Completed;
          }
          catch (OperationCanceledException)
          {
            job.Status = ImportJobStatus.Cancelled;

          }
          finally
          {
            if (_jobDescriptors.TryRemove(job.Id, out var o))
            {
              o.Dispose();
            }
          }
        }

        try
        {
          File.Delete(path);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex.ToString());
        }
      }
    }


    private async Task ImportEmails(ImportJobDescriptor job, CancellationToken cancellationToken)
    {
      var properties = typeof(Email).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(i => i.Name, i => i);

      var path = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data", job.Source.FileName);
      if (File.Exists(path))
      {
        using var scope = _serviceFactory.CreateScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        using (var reader = new StreamReader(path))
        {
          int count = 0, succeed = 0, failed = 0;
          if (job.Source.FirstLineHeader)
          {
            await reader.ReadLineAsync();
          }

          var fieldsCount = job.Source.Mappings.Count();

          try
          {
            await foreach (var fields in reader.ReadRecordAsync(job.Source.Delimiter, fieldsCount, job.CancellationTokenSource.Token))
            {
              count++;
              var processed = false;

              if (fields.Count() == job.Source.Mappings.Count())
              {
                var newEmail = false;

                var columns = fields.Zip(job.Source.Mappings).Select(i => new ImportColumnModel
                {
                  Text = i.First,
                  Format = i.Second.Format,
                  Column = i.Second.Target
                })
                  .Where(i => i.Column != "-NotUsed-")
                  .ToList();


                var emailIdColumn = columns.FirstOrDefault(i => i.Column == "EmailAddressUID");
                if (emailIdColumn == null)
                {
                  failed++;
                  goto report_status;
                }
                columns.Remove(emailIdColumn);
                var emailId = Convert.ToInt32(emailIdColumn.Text);

                var email = await dbContext.Emails.FindAsync(emailId);
                if (email == null)
                {
                  email = new Email() { EmailAddressUID = emailId };
                  dbContext.Emails.Add(email);
                  newEmail = true;
                }

                var entity = (IDataEntity<int>)email;
                foreach (var column in columns)
                {
                  var key = column.Column.ToString();
                  if (properties.ContainsKey(key))
                  {
                    var prop = properties[key];
                    if (!entity.TrySetValue(prop, column, out var value))
                    {
                      _logger.LogWarning($"Failed to set value {column.Text} to email {key}");
                      dbContext.ChangeTracker.Clear(); failed++;
                      goto report_status;
                    }
                  }
                }

                try
                {
                  await dbContext.SaveChangesAsync().ConfigureAwait(false);
                  processed = true;
                  succeed++;
                }
                catch (Exception ex)
                {
                  _logger.LogError(ex, $"Failed to import email {entity.GetId()}.");
                  failed++;
                }
                finally
                {
                  dbContext.ChangeTracker.Clear();
                }
              }
              else
              {
                failed++;
              }

            report_status:
              var progress = new ImportJobProgress
              {
                TotalRecords = job.TotalRecords,
                TotalProcessed = succeed + failed,
                Succeed = succeed,
                Failed = failed,
                RecordProcessed = processed
              };

              await _mqttService.PublishAsync($"progress/{job.Id:N}", progress, MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);

            }

            job.Status = ImportJobStatus.Completed;
          }
          catch (OperationCanceledException)
          {
            job.Status = ImportJobStatus.Cancelled;
          }
          finally
          {
            if (_jobDescriptors.TryRemove(job.Id, out var o))
            {
              o.Dispose();
            }
          }
        }

        try
        {
          File.Delete(path);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex.ToString());
        }
      }
    }


    private async Task ImportNotes(ImportJobDescriptor job, CancellationToken cancellationToken)
    {
      var genericNoteProperties = typeof(Note).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(i => i.Name, i => i);
      var customerNoteProperties = typeof(CustomerNote).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(i => i.Name, i => i);
      var permitNoteProperties = typeof(PermitNote).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(i => i.Name, i => i);

      var path = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data", job.Source.FileName);
      if (File.Exists(path))
      {
        using var scope = _serviceFactory.CreateScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        using (var reader = new StreamReader(path))
        {
          int count = 0, succeed = 0, failed = 0;
          if (job.Source.FirstLineHeader)
          {
            await reader.ReadLineAsync();
          }

          var fieldsCount = job.Source.Mappings.Count();

          try
          {
            await foreach (var fields in reader.ReadRecordAsync(job.Source.Delimiter, fieldsCount))
            {
              count++;
              var processed = false;

              if (fields.Count() == job.Source.Mappings.Count())
              {
                var newNote = false;

                var columns = fields.Zip(job.Source.Mappings).Select(i => new ImportColumnModel
                {
                  Text = i.First,
                  Format = i.Second.Format,
                  Column = i.Second.Target
                })
                  .Where(i => i.Column != "-NotUsed-")
                  .ToList();


                var noteIdColumn = columns.FirstOrDefault(i => i.Column == "NoteUID");
                if (noteIdColumn == null)
                {
                  failed++;
                  goto report_status;
                }
                columns.Remove(noteIdColumn);
                var noteId = Convert.ToInt32(noteIdColumn.Text);

                var sourceColumn = columns.FirstOrDefault(i => i.Column == "TableUIDofSourceObject");
                if (sourceColumn == null)
                {
                  failed++;
                  goto report_status;
                }

                var sourceType = int.Parse(sourceColumn.Text);
                columns.Remove(sourceColumn);


                IDataEntity<int> entity = null;
                IDictionary<string, PropertyInfo> properties = null;


                switch (sourceType)
                {
                  case 1:
                    var customerNote = await dbContext.CustomerNotes.FirstOrDefaultAsync(i => i.NoteUID == noteId);
                    if (customerNote == null)
                    {
                      customerNote = new CustomerNote() { NoteUID = noteId };
                      dbContext.CustomerNotes.Add(customerNote);
                      newNote = true;
                    }
                    entity = (IDataEntity<int>)customerNote;
                    properties = customerNoteProperties;
                    break;

                  case 10:
                    var permitNote = await dbContext.PermitNotes.FirstOrDefaultAsync(i => i.NoteUID == noteId);
                    if (permitNote == null)
                    {
                      permitNote = new PermitNote() { NoteUID = noteId };
                      dbContext.PermitNotes.Add(permitNote);
                      newNote = true;
                    }
                    entity = (IDataEntity<int>)permitNote;
                    properties = permitNoteProperties;
                    break;

                  default:
                    var note = await dbContext.Notes.FirstOrDefaultAsync(i => i.NoteUID == noteId);
                    if (note == null)
                    {
                      note = new Note() { NoteUID = noteId };
                      dbContext.Notes.Add(note);
                      newNote = true;
                    }
                    entity = (IDataEntity<int>)note;
                    properties = genericNoteProperties;
                    break;
                }

                if (entity != null)
                {
                  foreach (var column in columns)
                  {
                    var key = column.Column.ToString();
                    if (properties.ContainsKey(key))
                    {
                      var prop = properties[key];
                      if (!entity.TrySetValue(prop, column, out var value))
                      {
                        _logger.LogWarning($"Failed to set value {column.Text} to note {key}");
                        dbContext.ChangeTracker.Clear();
                        failed++;
                        goto report_status;
                      }
                    }
                  }

                  try
                  {
                    await dbContext.SaveChangesAsync().ConfigureAwait(false);
                    processed = true;
                    succeed++;
                  }
                  catch (Exception ex)
                  {
                    _logger.LogError(ex, $"Failed to import note {entity.GetId()}.");
                    failed++;
                  }
                  finally
                  {
                    dbContext.ChangeTracker.Clear();
                  }
                }
                else
                {
                  failed++;
                  processed = true;
                }
              }
              else
              {
                failed++;
              }

            report_status:
              var progress = new ImportJobProgress
              {
                TotalRecords = job.TotalRecords,
                TotalProcessed = succeed + failed,
                Succeed = succeed,
                Failed = failed,
                RecordProcessed = processed
              };

              await _mqttService.PublishAsync($"progress/{job.Id:N}", progress, MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);
            }

            job.Status = ImportJobStatus.Completed;
          }
          catch (OperationCanceledException)
          {
            job.Status = ImportJobStatus.Cancelled;
          }
          finally
          {
            if (_jobDescriptors.TryRemove(job.Id, out var o))
            {
              o.Dispose();
            }
          }
        }

        try
        {
          File.Delete(path);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex.ToString());
        }
      }
    }
  }
}
