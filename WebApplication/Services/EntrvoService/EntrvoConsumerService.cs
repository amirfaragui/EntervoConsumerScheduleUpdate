using Entrvo.Api.Models;
using Entrvo.Services;
using EntrvoWebApp.Services.Models;

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
      var cardNumber = record.GetId();
      var filters = new Dictionary<ConsumerFilter, object>
      {
        { ConsumerFilter.Cardno, cardNumber }
      };

      try
      {
        var consumer = await _api.FindConsumerDetailssAsync(filters, cancellationToken).FirstOrDefaultAsync(cancellationToken);
        if (consumer == null)
        {
          var profile = await _accessProfileService.GetUsageProfileAsync(record.AccessProfile);

          if (profile == null)
          {
            throw new Exception($"Access profile {record.AccessProfile} not found");            
          }

          var settings = await _settingsService.LoadSettingsAsync();
          var contractId = settings.Destination.ContractNumber;
          var templateId = settings.Destination.TemplateNumber;

          consumer = new ConsumerDetails()
          {
            Consumer = new Consumer {
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
            Lpn1 = record.LPN1,
            Lpn2 = record.LPN2,
            Memo = $"{record.Note1}-{record.Note1Extended}".TrimEnd('-'),
            UserField1 = record.Note2,
            UserField2 = record.Note3,
            FirstName = record.FirstName,
            Surname = record.LastName,             
          };

          if (record.AccessProfile != null && record.AccessProfile.Contains("PPU"))
          {
            consumer.Lpn3 = record.AdditionalValue;
          }

          var consumerId = await _api.CreateConsumerAsync(templateId, consumer, cancellationToken);
          _logger.LogInformation("Created consumer {ConsumerId} for card {CardNumber}", consumerId, cardNumber);
        }
        else
        {
          consumer.Consumer.ValidFrom = record.StartValidity?.ToString("yyyy-MM-dd");
          consumer.Consumer.ValidUntil = record.EndValidity?.ToString("yyyy-MM-dd");
          consumer.Person.FirstName = record.FirstName;
          consumer.Person.Surname = record.LastName;
          consumer.Identification.ValidFrom = record.StartValidity?.ToString("yyyy-MM-dd");
          consumer.Identification.ValidUntil = record.EndValidity?.ToString("yyyy-MM-dd");
          consumer.Lpn1 = record.LPN1;
          consumer.Lpn2 = record.LPN2;
          consumer.Memo = $"{record.Note1}-{record.Note1Extended}".TrimEnd('-');
          consumer.UserField1 = record.Note2;
          consumer.UserField2 = record.Note3;
          consumer.FirstName = record.FirstName;
          consumer.Surname = record.LastName;

          if (record.AccessProfile != null && record.AccessProfile.Contains("PPU"))
          {
            if (int.TryParse(consumer.Lpn3, out var oldValue) && int.TryParse(record.AdditionalValue, out var newValue))
            {
              consumer.Lpn3 = (oldValue + newValue).ToString();
            }
          }

          await _api.UpdateConsumerAsync(consumer, cancellationToken);
          _logger.LogInformation($"Updated consumer {consumer.Consumer.Id} for card {cardNumber}");
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
