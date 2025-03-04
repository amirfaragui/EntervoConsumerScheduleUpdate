using Entrvo.DAL;
using Microsoft.IO;
using System.Diagnostics;
using System.Reflection;

namespace Entrvo
{
  public class RequestResponseLoggingMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    private readonly IServiceScopeFactory _socpeFactory;

    static readonly string _applicationName;

    static RequestResponseLoggingMiddleware()
    {
      _applicationName = Assembly.GetExecutingAssembly().GetName().Name;
    }

    public RequestResponseLoggingMiddleware(RequestDelegate next,
                                            ILoggerFactory logger,
                                            IServiceScopeFactory scopeFactory)
    {
      _next = next ?? throw new ArgumentNullException(nameof(next));
      _logger = logger.CreateLogger<RequestResponseLoggingMiddleware>();
      _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
      _socpeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    public async Task Invoke(HttpContext context)
    {
      await LogRequest(context);
      await LogResponse(context);
    }

    private async Task LogRequest(HttpContext context)
    {
      context.Request.EnableBuffering();

      await using var requestStream = _recyclableMemoryStreamManager.GetStream();
      await context.Request.Body.CopyToAsync(requestStream);
      //_logger.LogInformation($"Http Request Information:{Environment.NewLine}" +
      //                       $"Schema:{context.Request.Scheme} " +
      //                       $"Host: {context.Request.Host} " +
      //                       $"Path: {context.Request.Path} " +
      //                       $"QueryString: {context.Request.QueryString} " +
      //                       $"Request Body: {ReadStreamInChunks(requestStream)}");
      context.Request.Body.Position = 0;
    }

    private async Task LogResponse(HttpContext context)
    {
      var originalBodyStream = context.Response.Body;
      await using var responseBody = _recyclableMemoryStreamManager.GetStream();
      context.Response.Body = responseBody;

      var watch = Stopwatch.StartNew();
      await _next(context);
      watch.Stop();

      context.Response.Body.Seek(0, SeekOrigin.Begin);
      var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
      context.Response.Body.Seek(0, SeekOrigin.Begin);

      try
      {
        using var scope = _socpeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.AccessLogs.Add(new WebAccessAuditLog()
        {
          Time = DateTimeOffset.Now,
          ApplicationName = _applicationName,
          Elapsed = (int)watch.ElapsedMilliseconds,
          Origin = context.Connection.RemoteIpAddress?.ToString(),
          Method = context.Request.Method,
          RequestUri = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}",
          StatusCode = context.Response.StatusCode,
          User = context.User?.Identity?.Name,
        });

        await dbContext.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
      }

      await responseBody.CopyToAsync(originalBodyStream);
    }

    private static string ReadStreamInChunks(Stream stream)
    {
      const int readChunkBufferLength = 4096;
      stream.Seek(0, SeekOrigin.Begin);
      using var textWriter = new StringWriter();
      using var reader = new StreamReader(stream);
      var readChunk = new char[readChunkBufferLength];
      int readChunkLength;
      do
      {
        readChunkLength = reader.ReadBlock(readChunk,
                                           0,
                                           readChunkBufferLength);
        textWriter.Write(readChunk, 0, readChunkLength);
      } while (readChunkLength > 0);
      return textWriter.ToString();
    }
  }

  static class RequestResponseLoggingMiddlewareExtensions
  {
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
  }
}
