using Entrvo.Api.Models;
using Entrvo.Services;
using EntrvoWebApp.Services.Models;
using NuGet.Configuration;

namespace EntrvoWebApp.Services
{
  public class EntrvoConsumerService : IEntrvoConsumerService
  {
    private readonly IParkingApi _api;
    private readonly ILogger<EntrvoConsumerService> _logger;
    private readonly IAccessProfileService _accessProfileService;
    private readonly ISettingsService _settingsService;

    public EntrvoConsumerService(IParkingApi api, ILogger<EntrvoConsumerService> logger, IAccessProfileService accessProfileService, ISettingsService settingsService)
    {
      _api = api;
      _logger = logger;
      _accessProfileService = accessProfileService;
      _settingsService = settingsService;
    }

    public async Task<ConsumerDetails?> UpdateConsumerAsync(EntrvoRecord record, CancellationToken cancellationToken)
    {
      try
      {
        var profile = await _accessProfileService.GetUsageProfileAsync(record.AccessProfile);

        if (profile == null)
        {
          throw new Exception($"Access profile {record.AccessProfile} not found");
        }

        var settings = await _settingsService.LoadSettingsAsync();
        var contractId = settings.Destination.ContractNumber;
        var templateId = settings.Destination.TemplateNumber;

        var cardNumber = record.GetId();
        _logger.LogInformation($"Update Consumer => Padded card number is {cardNumber}");

        var filters = new Dictionary<ConsumerFilter, object>
        {
          { ConsumerFilter.Cardno, cardNumber },
          { ConsumerFilter.MinContractId, 0 },
          { ConsumerFilter.MaxContractId, 0 },
        };

        var consumer = await _api.FindConsumerDetailssAsync(filters, cancellationToken).FirstOrDefaultAsync(cancellationToken);
        if (consumer == null)
        {

          consumer = new ConsumerDetails()
          {
            Consumer = new Consumer
            {
              ContractId = contractId.ToString(),
              ValidFrom = record.StartValidity?.ToString("yyyy-MM-dd"),
              ValidUntil = record.EndValidity?.ToString("yyyy-MM-dd"),
            },
            Person = new Person() { FirstName = record.FirstName, Surname = record.LastName },
            Identification = new Identification()
            {
              PtcptType = 2,
              CardNumber = cardNumber,
              IdentificationType = 55,
              ValidFrom = record.StartValidity?.ToString("yyyy-MM-dd"),
              ValidUntil = record.EndValidity?.ToString("yyyy-MM-dd"),
              UsageProfile = profile!,
              IgnorePresence = 1,
              Status = 0
            },
            Status = 0,
          //  Lpn1 = record.LPN1,
         //   Lpn2 = record.LPN2,
            Memo = $"{record.Note1}-{record.Note1Extended}".TrimEnd('-'),
            UserField1 = $"{record.Note2}-{record.LPN1}-{record.LPN2}",
            UserField2 = record.Note3,
            
            FirstName = record.FirstName,
            Surname = record.LastName,
          };

          if (record.AccessProfile != null && record.AccessProfile.Contains("PPU"))
          {
            consumer.Lpn2 = "PPU";
            consumer.Lpn3 = record.AdditionalValue;
          }
          else
          {
            consumer.Lpn2 = "Term";
            consumer.Lpn3 = string.Empty;
          }

            var consumerId = await _api.CreateConsumerAsync(templateId, consumer, cancellationToken);
          consumer.Consumer.Id = consumerId.ToString();
          _logger.LogInformation("Created consumer {ConsumerId} for card {CardNumber}", consumerId, cardNumber);
        }
        else
        {
          if (!string.IsNullOrWhiteSpace(record.FirstName))
          {
            consumer.Person.FirstName = record.FirstName;
            consumer.FirstName = record.FirstName;
          }
          if (!string.IsNullOrWhiteSpace(record.LastName))
          {
            consumer.Person.Surname = record.LastName;
            consumer.Surname = record.LastName;
          }
        //  if (!string.IsNullOrWhiteSpace(record.LPN1)) consumer.Lpn1 = record.LPN1;
       //   if (!string.IsNullOrWhiteSpace(record.LPN2)) consumer.Lpn2 = record.LPN2;
          if (!string.IsNullOrWhiteSpace(record.Note1)) consumer.Memo = $"{record.Note1}-{record.Note1Extended}".TrimEnd('-');
          if (!string.IsNullOrWhiteSpace(record.Note2)) consumer.UserField1 = $"{record.Note2}-{record.LPN1}-{record.LPN2}";
          if (!string.IsNullOrWhiteSpace(record.Note3)) consumer.UserField2 = record.Note3;

          consumer.Identification.UsageProfile = profile!;

          if (record.StartValidity.HasValue)
          {
            consumer.Identification.ValidFrom = record.StartValidity.Value.ToString("yyyy-MM-dd");
            consumer.Consumer.ValidFrom = record.StartValidity.Value.ToString("yyyy-MM-dd");
          }
          if (record.EndValidity.HasValue)
          {
            consumer.Identification.ValidUntil = record.EndValidity.Value.ToString("yyyy-MM-dd");
            consumer.Consumer.ValidUntil = record.EndValidity.Value.ToString("yyyy-MM-dd");
          }

          if (record.AccessProfile != null && record.AccessProfile.Contains("PPU"))
          {
            consumer.Lpn2 = "PPU";
            if (int.TryParse(consumer.Lpn3, out var oldValue) && int.TryParse(record.AdditionalValue, out var newValue))
            {
              consumer.Lpn3 = (oldValue + newValue).ToString();
            }
            if (!record.EndValidity.HasValue)
            {
              consumer.Identification.ValidUntil = DateTime.Today.AddYears(10).ToString("yyyy-MM-dd");
              consumer.Consumer.ValidUntil = DateTime.Today.AddYears(10).ToString("yyyy-MM-dd");
            }
          }
          else
          {
            consumer.Lpn2 = "Term";
            consumer.Lpn3 = string.Empty;
          }
          bool result =  await _api.UpdateConsumerAsync(consumer, cancellationToken);
          if (result)
          {
            _logger.LogInformation($"Updated consumer {consumer.Consumer.Id} for card {cardNumber}");
          }
          else
          {
            _logger.LogInformation($"Unable to update consumer {consumer.Consumer.Id} for card {cardNumber}");
           
          }
        }

        return consumer;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
      }

      return null;
    }

    public async Task<ConsumerDetails?> UpdateConsumerAsync(string contractId, string consumerId, EntrvoRecord record, CancellationToken cancellationToken)
    {
      try
      {
        var profile = await _accessProfileService.GetUsageProfileAsync(record.AccessProfile);

        if (profile == null)
        {
          throw new Exception($"Access profile {record.AccessProfile} not found");
        }

        var settings = await _settingsService.LoadSettingsAsync();
        var templateId = settings.Destination.TemplateNumber;

        var cardNumber = record.GetId();


        var consumer = await _api.GetConsumerDetailsAsync(contractId, consumerId, cancellationToken);
        if (consumer == null)
        {
          _logger.LogInformation($"UpdateConsumerAsync => consumer [{cardNumber}] not existing in entervo");
          return await UpdateConsumerAsync(record, cancellationToken);
        }
        else
        {
          if (!string.IsNullOrWhiteSpace(record.FirstName))
          {
            consumer.Person.FirstName = record.FirstName;
            consumer.FirstName = record.FirstName;
          }
          if (!string.IsNullOrWhiteSpace(record.LastName))
          {
            consumer.Person.Surname = record.LastName;
            consumer.Surname = record.LastName;
          }
          if (!string.IsNullOrWhiteSpace(record.LPN1)) consumer.Lpn1 = record.LPN1;
          if (!string.IsNullOrWhiteSpace(record.LPN2)) consumer.Lpn2 = record.LPN2;
          if (!string.IsNullOrWhiteSpace(record.Note1)) consumer.Memo = $"{record.Note1}-{record.Note1Extended}".TrimEnd('-');
          if (!string.IsNullOrWhiteSpace(record.Note2)) consumer.UserField1 = record.Note2;
          if (!string.IsNullOrWhiteSpace(record.Note3)) consumer.UserField2 = record.Note3;

          consumer.Identification.UsageProfile = profile!;

          if (record.StartValidity.HasValue)
          {
            consumer.Identification.ValidFrom = record.StartValidity.Value.ToString("yyyy-MM-dd");
            consumer.Consumer.ValidFrom = record.StartValidity.Value.ToString("yyyy-MM-dd");
          }
          if (record.EndValidity.HasValue)
          {
            consumer.Identification.ValidUntil = record.EndValidity.Value.ToString("yyyy-MM-dd");
            consumer.Consumer.ValidUntil = record.EndValidity.Value.ToString("yyyy-MM-dd");
          }

          if (record.AccessProfile != null && record.AccessProfile.Contains("PPU"))
          {
            consumer.Lpn2 = "PPU";
            if (int.TryParse(consumer.Lpn3, out var oldValue) && int.TryParse(record.AdditionalValue, out var newValue))
            {
              consumer.Lpn3 = (oldValue + newValue).ToString();
            }
          }
          else
          {
            consumer.Lpn2 = "Term";
            consumer.Lpn3 = string.Empty;
          }

          bool result = await _api.UpdateConsumerAsync(consumer, cancellationToken);
          if (result)
          {
            _logger.LogInformation($"Updated consumer {consumer.Consumer.Id} for card {cardNumber}");
          }
          else
          {
            _logger.LogInformation($"Unable to update consumer {consumer.Consumer.Id} for card {cardNumber}");

          }
        }

        return consumer;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
      }

      return null;
    }

  }
}
