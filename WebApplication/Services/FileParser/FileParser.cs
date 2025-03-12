using Entrvo;
using Entrvo.Models;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;

namespace EntrvoWebApp.Services
{
  public class FileParser : IFileParser
  {
    private readonly ILogger<FileParser> _logger;

    public FileParser(ILogger<FileParser> logger)
    {
      _logger = logger;
    }


    public async IAsyncEnumerable<TResult> ParseFileAsync<TResult>(ImportSourceModel job, [EnumeratorCancellation] CancellationToken cancellationToken, CreateInstanceCallback<TResult>? instanceFactory = null) where TResult : class, IDataEntity, new()
    {
      var fieldsCount = job.Mappings.Count();

      var properties = typeof(TResult).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(i => i.Name, i => i);

      var transformBlock = new TransformBlock<string[], TResult?>(fields =>
      {
        if (fields.Count() == job.Mappings.Count())
        {
          var columns = fields.Zip(job.Mappings).Select(i => new ImportColumnModel
          {
            Text = i.First,
            Format = i.Second.Format,
            Column = i.Second.Target
          })
            .Where(i => i.Column != "-NotUsed-")
            .ToList();

          var key = job.KeySelector?.Invoke(fields) ?? fields[0];          
          TResult newInstance = instanceFactory?.Invoke(key) ?? Activator.CreateInstance<TResult>();

          bool success = true;
          foreach (var column in columns)
          {
            var columnName = column.Column.ToString();
            if (properties.ContainsKey(columnName))
            {
              var prop = properties[columnName];
              if (!newInstance.TrySetValue(prop, column, out var value))
              {
                _logger.LogWarning($"Failed to set value {column.Text} to {typeof(TResult).Name}: {columnName}");
                success = false;
                break;
              }
            }
          }

          if (success)
          {
            return newInstance;
          }
        }

        return default;

      }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 3 });

      var path = job.FileName;
      if (File.Exists(path))
      {
        try
        {
          using (var reader = new StreamReader(path))
          {
            if (job.FirstLineHeader)
            {
              await reader.ReadLineAsync();
            }
            await foreach (var fields in reader.ReadRecordAsync(job.Delimiter, fieldsCount, cancellationToken))
            {
              transformBlock.Post(fields);
            }
          }
        }
        catch (OperationCanceledException)
        {
        }
      }
      transformBlock.Complete();

      while (await transformBlock.OutputAvailableAsync())
      {
        while (transformBlock.TryReceive(out var result))
        {
          if (result != null)
          {
            yield return result;
          }          
        }
      }
    }
  }
}

