using Entrvo;
using Entrvo.Models;

namespace EntrvoWebApp.Services
{

  public delegate TResult CreateInstanceCallback<TResult>(object key);
  public interface IFileParser
  {
    IAsyncEnumerable<TResult> ParseFileAsync<TResult>(ImportSourceModel job, CancellationToken cancellationToken, CreateInstanceCallback<TResult>? instanceFactory = null) where TResult : class, IDataEntity, new();
  }
}
