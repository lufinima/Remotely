using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remotely.Server.Models;
using Remotely.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Remotely.Server.Services
{
    public interface IPerfexCrmService
    {
        Task UpdateContact(Device device);
    }

    public class PerfexCrmService : IPerfexCrmService
    {
        private const string AUTH_TOKEN = "authtoken";
        private readonly IApplicationConfig _appConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PerfexCrmService> _logger;
        private readonly IServiceProvider _services;

        public PerfexCrmService(IApplicationConfig appConfig, 
            IHttpClientFactory httpClientFactory, 
            ILogger<PerfexCrmService> logger,
            IServiceProvider services)
        {
            _appConfig = appConfig;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _services = services;
        }

        public async Task UpdateContact(Device device)
        {
            var dataService = _services.GetRequiredService<IDataService>();
            var id = await GetContactId(device.Alias);
            if (string.IsNullOrEmpty(id))
            {
                dataService.WriteEvent($"PerfexCRM No contact id for email: {id}", Shared.Enums.EventType.Warning, device.OrganizationID);
                _logger.LogWarning("No contact id for email {0}", device.Alias);
                return;
            }

            var fieldId = _appConfig.PerfexRemotelyFieldId;
            if (string.IsNullOrEmpty(fieldId))
            {
                _logger.LogWarning("Perfex Remotely Field Id null or empty");
                return;
            }

            var key = _appConfig.PerfexApiKey;
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogWarning("PerfexCrm API key null or empty");
                return;
            }

            var json = new {
                title = $"{DateTime.Now:u}",
                custom_fields = new
                {
                    contacts = new Dictionary<string, string>
                    {
                        { $"{fieldId}", device.ID }
                    }
                }
            };

            if (Uri.TryCreate(_appConfig.PerfexApiUrl, UriKind.Absolute, out var uri))
            {
                try
                {
                    var url = $"{uri.AbsoluteUri.Trim('/')}/contacts/{id}";
                    using var client = _httpClientFactory.CreateClient();
                    client.DefaultRequestHeaders.Add(AUTH_TOKEN, key);
                    var response = await client.PutAsJsonAsync(url, json);
                    var content = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        dataService.WriteEvent($"PerfexCRM Contact [{id}] Updated", Shared.Enums.EventType.Info, device.OrganizationID);
                    }
                    _logger.LogInformation("Contact Update Response: {0}", content);

                    return;
                }
                catch (Exception ex)
                {
                    dataService.WriteEvent(ex, device.OrganizationID);
                    _logger.LogWarning(ex, "Failed to update PerfexCRM contact.");
                }
            }
            else
            {
                dataService.WriteEvent("PerfexCrm Address null or wrong format", Shared.Enums.EventType.Warning, device.OrganizationID);
                _logger.LogWarning("PerfexCrm Address null or wrong format: {0}", _appConfig.PerfexApiUrl);
            }
            return;
        }

        private async Task<string> GetContactId(string email)
        {
            if (Uri.TryCreate(_appConfig.PerfexApiUrl, UriKind.Absolute, out var uri))
            {
                try
                {
                    var key = _appConfig.PerfexApiKey;
                    if (string.IsNullOrEmpty(key))
                    {
                        _logger.LogWarning("PerfexCrm API key null or empty");
                        return null;
                    }

                    var url = $"{uri.AbsoluteUri.Trim('/')}/contacts/search/{email}";
                    using var client = _httpClientFactory.CreateClient();
                    client.DefaultRequestHeaders.Add(AUTH_TOKEN, key);
                    var stream = await client.GetStringAsync(url);
                    var contacts = JsonSerializer.Deserialize<List<PerfexContact>>(stream);

                    return contacts.First().Id;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to retrieve contact id from email {0}", email);
                }
            }
            else
            {
                _logger.LogWarning("PerfexCrm Address null or wrong format: {0}", _appConfig.PerfexApiUrl);
            }

            return null;
        }
    }
}
