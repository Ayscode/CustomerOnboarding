using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Threading.Tasks;
using CustomerOnboarding.Integrations.WemaTechTest;
using CustomerOnboarding.Interfaces;
using CustomerOnboarding.Models.Configurations;
using Microsoft.Extensions.Caching.Memory;

namespace CustomerOnboarding.Services
{
    public class WemaAPIService : IWemaAPIService
    {
        private readonly WemaAPISettings _wemaAPISettings;
        private readonly IUtilityService _utilityService;
        private readonly ILogger<WemaAPIService> _log;
        private readonly IMemoryCache _cache;


        public WemaAPIService(IOptions<WemaAPISettings> wemaAPISettings,
             IUtilityService utilityService,
             ILogger<WemaAPIService> log,
             IMemoryCache memoryCache)
        {
            _wemaAPISettings = wemaAPISettings.Value;
            _utilityService = utilityService;
            _log = log;
            _cache = memoryCache;
        }


        public async Task<GetBanksResponse> GetBanks()
        {
            try
            {

                var cacheKey = "GetBanksResponse";
                if (_cache.TryGetValue(cacheKey, out GetBanksResponse cachedResult))
                {
                    return cachedResult;
                }


                var requestTime = DateTime.UtcNow;
                var uri = _wemaAPISettings.GetBanksUrl;

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add(_wemaAPISettings.SubscriptionHeader, _wemaAPISettings.SubscriptionKey);
                    var response = await client.GetAsync(uri);
                    var responseTime = DateTime.UtcNow;

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<GetBanksResponse>(await response.Content.ReadAsStringAsync());

                        await _utilityService.SaveApiLogs("", "", requestTime, responseTime,
                            uri, response.Content.ToString(), "", response.StatusCode.ToString(), response.ReasonPhrase, "WemaAPI", "", null, null);
                        
                        var cacheEntryOptions = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                        };
                        _cache.Set(cacheKey, result, cacheEntryOptions);


                        return result;
                    }
                    else
                    {
                        await _utilityService.SaveApiLogs("", "", requestTime, responseTime,
                        uri, response.Content.ToString(), "", response.StatusCode.ToString(), response.ReasonPhrase,
                        "WemaAPI", "", null, null);

                        return new GetBanksResponse
                        {
                            errorMessage = "Service unavailable",
                            hasError = true
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError($".GetBanks ->  {ex}");

                return new GetBanksResponse
                {
                    errorMessage = "Service unavailable",
                    hasError = true
                };
            }
        }    }
}
