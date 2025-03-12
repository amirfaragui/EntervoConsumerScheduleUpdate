using Entrvo.Api;
using Entrvo.Api.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;
using RestSharp.Serializers.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml;
using System.Xml.Serialization;

namespace Entrvo.Services
{
  //Test Url: "https://209.151.135.7:8443",
  //Username: "799",
  //Password: "S&Bca@4711"
  public class ParkingApi : IParkingApi
  {
    private readonly ILogger<ParkingApi> _logger;
    private readonly IApiSettingsProvider _settingsProvider;

    private RestClient _client;


    public ParkingApi(ILogger<ParkingApi> logger,
                     IApiSettingsProvider settingsProvider)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));

      var options = settingsProvider.GetApiOptions();
      Initialize(options);
    }


    public void Initialize(ApiOptions options)
    {
      _client = CreateClinet(options.Server, new HttpBasicAuthenticator(options.Username, options.Password));
    }

    private RestClient CreateClinet(string url, IAuthenticator authenticator)
    {
      if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

      var options = new RestClientOptions()
      {
        BaseUrl = new Uri(url),
        RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
        Authenticator = authenticator,
      };

      return new RestClient(options, configureSerialization: s =>
      {
        s.UseNewtonsoftJson();
        s.UseDotNetXmlSerializer("http://gsph.sub.com/payment/types");
      });
    }

    #region Consumers
    private RestRequest GenerateConsumerRequest(string resource, IDictionary<ConsumerFilter, object> filters)
    {
      var request = new RestRequest(resource)
      {
        Method = Method.Get
      };
      request.AddHeader("Accept", "application/json");

      foreach (var filter in filters)
      {
        var key = filter.Key.ToString();
        key = char.ToLower(key[0]) + key.Substring(1);
        var type = filter.Value.GetType();

        if (type == typeof(int))
        {
          request.AddQueryParameter(key, filter.Value.ToString());
        }
        else if (type == typeof(string))
        {
          request.AddQueryParameter(key, filter.Value.ToString());
        }
        else if (type == typeof(DateTime))
        {
          request.AddQueryParameter(key, ((DateTime)filter.Value).ToString("yyyy-MM-dd"));
        }
        else
        {
          throw new ArgumentException($"Invalid filter value type: {type}");
        }
      }

      return request;
    }

    public async IAsyncEnumerable<Consumer> FindConsumersAsync(IDictionary<ConsumerFilter, object> filters, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
      var request = GenerateConsumerRequest("CustomerMediaWebService/consumers", filters);

      IEnumerable<Consumer> consumers = null;

      try
      {
        var response = await _client.ExecuteGetAsync(request, cancellationToken);
        if (response.StatusCode ==  System.Net.HttpStatusCode.NoContent) yield break;
        try
        {
          var result = JsonConvert.DeserializeObject<ConsumerListResponse>(response.Content);
          consumers = result.Consumers.Consumers;
        }
        catch (JsonSerializationException)
        {
          var result = JsonConvert.DeserializeObject<SingleConsumerResponse>(response.Content);
          consumers = result.Consumers.Consumers;
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        throw;
      }

      foreach (var c in consumers)
      {
        yield return c;
      }
    }

    public async IAsyncEnumerable<ConsumerDetails> FindConsumerDetailssAsync(IDictionary<ConsumerFilter, object> filters, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
      var transformBlock = new TransformBlock<Consumer, ConsumerDetails?>(async c =>
      {
        var details = await GetConsumerDetailsAsync(c.ContractId, c.Id, cancellationToken);
        return details;
      }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 });

      await foreach (var c in FindConsumersAsync(filters, cancellationToken).ConfigureAwait(false))
      {
        transformBlock.Post(c);
      }
      transformBlock.Complete();

      while (await transformBlock.OutputAvailableAsync(cancellationToken))
      {
        while (transformBlock.TryReceive(out var result))
        {
          if (result != null)
            yield return result;
        }
      }
    }

    public async Task<ConsumerDetails?> GetConsumerDetailsAsync(string contractId, string consumerId, CancellationToken cancellationToken = default)
    {
      try
      {
        var detailRequest = new RestRequest(@$"CustomerMediaWebService/consumers/{contractId},{consumerId}/detail");
        detailRequest.AddHeader("Accept", "application/json");
        detailRequest.Method = Method.Get;

        var response = await _client.ExecuteGetAsync<ConsumerDetailResponse>(detailRequest, cancellationToken);
        //_logger.LogDebug(response.Content);
        var details = response.Data.ConsumerDetail;
        if (details?.Identification != null)
        {
          if (details.Identification.PtcptType == 2)
          {
            return details;
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
      }

      return null;
    }


    public async Task<int> CreateConsumerAsync(int templateId, ConsumerDetails consumer, CancellationToken cancellationToken = default)
    {
      try
      {        
        var contractId = consumer.Consumer.ContractId;
        var detailRequest = new RestRequest(@$"CustomerMediaWebService/contracts/{contractId}/consumers");
        detailRequest.AddQueryParameter("templateId", templateId.ToString());
        detailRequest.AddXmlBody(consumer, xmlNamespace: "http://gsph.sub.com/cust/types");
        detailRequest.AddHeader("Accept", "application/json");
        detailRequest.Method = Method.Post;

        var response = await _client.ExecutePostAsync<ConsumerDetailResponse>(detailRequest, cancellationToken);
        _logger.LogDebug(response.Content);
        var consumerId = response.Data.ConsumerDetail.Consumer.Id;
        return Convert.ToInt32(consumerId);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        throw;
      }
    }

    public async Task<bool> UpdateConsumerAsync(ConsumerDetails consumer, CancellationToken cancellationToken = default)
    {
      try
      {
        var contractId = consumer.Consumer.ContractId;
        var consumerId = consumer.Consumer.Id;
        var detailRequest = new RestRequest(@$"CustomerMediaWebService/consumers/{contractId},{consumerId}/detail");
        detailRequest.AddXmlBody(consumer, xmlNamespace: "http://gsph.sub.com/cust/types");
        detailRequest.AddHeader("Accept", "application/json");
        detailRequest.Method = Method.Put;

        var response = await _client.ExecutePutAsync<ConsumerDetailResponse>(detailRequest, cancellationToken);
        //_logger.LogDebug(response.Content);
        return response.StatusCode == System.Net.HttpStatusCode.OK;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        return false;
      }
    }

    #endregion

    #region Cashier
    public async Task<Cashier[]> GetCashiersAsync(CancellationToken cancellationToken = default)
    {
      try
      {
        var request = new RestRequest("PaymentWebService/cashiers");
        request.AddHeader("Accept", "application/json");
        request.Method = Method.Get;
        var response = await _client.ExecuteGetAsync(request, cancellationToken);
        switch (response.StatusCode)
        {
          case System.Net.HttpStatusCode.Unauthorized:
            throw new UnauthorizedAccessException();
        }

        if (response.IsSuccessful)
        {
          var result = JsonConvert.DeserializeObject<CashierListResponse>(response.Content);
          return result.Cashiers;
        }

        throw new ApiErrorException("Failed to get the cashier.") { StutusCode = response.StatusCode };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        throw;
      }
    }

    #endregion

    #region Shift
    public async Task<Shift> GetActiveShiftAsync(string cashierContractId, string cashierConsumerId, CancellationToken cancellationToken = default)
    {
      try
      {
        var request = new RestRequest($"PaymentWebService/cashiers/{cashierContractId},{cashierConsumerId}/shifts");
        request.AddHeader("Accept", "application/json");
        request.Method = Method.Get;
        var response = await _client.ExecuteGetAsync(request, cancellationToken);

        if (response.IsSuccessful)
        {
          if (response.Content == "null")
          {
            return null;
          }

          try
          {
            var result = JsonConvert.DeserializeObject<ShiftResponse>(response.Content);
            var shift = result.Shift;
            if (shift.FinishDateTime.HasValue)
            {
              return null;
            }
            return shift;
          }
          catch (JsonSerializationException)
          {
            var result = JsonConvert.DeserializeObject<ShiftListResponse>(response.Content);
            var shift = result.Shifts.FirstOrDefault(i => i.ShiftStatus == 1);
            return shift;
          }

        }

        throw new ApiErrorException("Failed to get active shift.") { StutusCode = response.StatusCode };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        throw;
      }
    }

    public async Task<Shift> CreateShiftAsync(Cashier cashier, Device device, CancellationToken cancellationToken = default)
    {
      try
      {
        var request = new RestRequest($"PaymentWebService/shifts");
        request.AddHeader("Accept", "application/json");
        request.Method = Method.Post;
        //request.XmlSerializer = new DotNetXmlSerializer("http://gsph.sub.com/payment/types");

        var body = new NewShift()
        {
          CashierContractId = cashier.CashierContractId,
          CashierConsumerId = cashier.CashierConsumerId,
          ComputerId = device.ComputerId,
          DeviceId = device.DeviceId,
          ShiftNo = DateTime.Now.ToString("yyMMddHHmm"),
          CreateDateTime = DateTime.Now,
        };

        request.AddXmlBody(body, "http://gsph.sub.com/payment/types");

        var response = await _client.ExecutePostAsync(request, cancellationToken);
        _logger.LogDebug(response.Content);

        if (response.IsSuccessful)
        {
          var shift = JsonConvert.DeserializeObject<Shift>(response.Content);
          return shift;
        }

        try
        {
          var error = JsonConvert.DeserializeObject<ErrorResponse>(response.Content);
          throw new ApiErrorException(error.Error) { StutusCode = response.StatusCode };
        }
        catch (JsonSerializationException)
        {
          throw new ApiErrorException("Failed to create shift.") { StutusCode = response.StatusCode };
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        throw;
      }
    }
    #endregion

    #region Device
    public async Task<Device[]> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
      try
      {
        var request = new RestRequest($"PaymentWebService/devices");
        request.AddHeader("Accept", "application/json");
        request.Method = Method.Get;
        var response = await _client.GetAsync<DeviceListResponse>(request, cancellationToken);
        return response.Devices;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        throw;
      }
    }
    #endregion

    #region UsageProfile
    public async Task<UsageProfile[]> GetUsageProfilesAsync(CancellationToken cancellationToken = default)
    {
      try
      {
        var request = new RestRequest($"CustomerMediaWebService/profiles");
        request.AddHeader("Accept", "application/json");
        request.Method = Method.Get;
        var response = await _client.ExecuteGetAsync(request, cancellationToken);
        var result = JsonConvert.DeserializeObject<ProfileListResponse>(response.Content);
        return result.Result.Profiles;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        throw;
      }

    }
    #endregion

    public async Task<Transaction> PostPayment(TransactionDetail transaction, CancellationToken cancellationToken = default)
    {
      try
      {
        var request = new RestRequest($"PaymentWebService/shifts/{transaction.Transaction.ShiftId}/salestransactions");
        request.AddHeader("Accept", "application/json");
        request.AddHeader("Content-Type", "application/xml");
        request.Method = Method.Put;
        //request.XmlSerializer = new DotNetXmlSerializer("http://gsph.sub.com/payment/types");
        //request.AddXmlBody(transaction, "http://gsph.sub.com/payment/types");

        using var stream = new MemoryStream();
        using var writer = new XmlTextWriter(stream, Encoding.UTF8);
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(TransactionDetail));

        var ns = new XmlSerializerNamespaces();
        ns.Add("pay", "http://gsph.sub.com/payment/types");

        serializer.Serialize(writer, transaction, ns);

        var body = Encoding.UTF8.GetString(stream.ToArray());
        _logger.LogDebug(body);

        request.AddParameter("application/xml", body, ParameterType.RequestBody);

        var response = await _client.ExecuteAsync(request, cancellationToken);
        _logger.LogDebug(body);

        if (response.IsSuccessful)
        {
          var result = JsonConvert.DeserializeObject<Transaction>(response.Content);
          return result;
        }

        try
        {
          var error = JsonConvert.DeserializeObject<ErrorResponse>(response.Content);
          throw new ApiErrorException(error.Error) { StutusCode = response.StatusCode };
        }
        catch (JsonSerializationException ex)
        {
          throw new ApiErrorException(ex.Message) { StutusCode = response.StatusCode };
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        throw;
      }

    }

  }
}
