using Newtonsoft.Json;
using System.Net;

namespace SnB.Models
{
  public class Error
  {
    [JsonProperty("errCode")]
    public int Code { get; set; }

    public string ShortMsg { get; set; }

    public string Message { get; set; }
  }

  class ErrorResponse
  {
    public Error Error { get; set; }
  }

  public class ApiErrorException : Exception
  {
    public Error Error { get; set; }
    public HttpStatusCode StutusCode { get; set; }

    public ApiErrorException(Error error) : base(error.Message)
    {
      Error = error;
    }
  }
}
