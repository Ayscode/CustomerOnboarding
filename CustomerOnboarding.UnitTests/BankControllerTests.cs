using Xunit;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using CustomerOnboarding.Services;
using CustomerOnboarding.Interfaces;
using CustomerOnboarding.Integrations.WemaAPI;
using Microsoft.Extensions.Options;
using CustomerOnboarding.Models.Configurations;
using Microsoft.Extensions.Logging;

public class WemaAPIServiceTests
{
    private Mock<IMemoryCache> _cacheMock;
    private Mock<ILogger<WemaAPIService>> _log;
    private Mock<IUtilityService> _utilityServiceMock;
    private WemaAPIService _wemaAPIService;

    public WemaAPIServiceTests()
    {
        var wemaAPISettings = Options.Create(new WemaAPISettings()
        {
            GetBanksUrl = "https://wema-alatdev-apimgt.azure-api.net/alat-test/api/Shared/GetAllBanks",
            SubscriptionHeader = "Ocp-Apim-Subscription-Key",
            SubscriptionKey = "55566DEF78901ABC234DE56FGHIJ"
        });

        _cacheMock = new Mock<IMemoryCache>();
        _utilityServiceMock = new Mock<IUtilityService>();
        _log = new Mock<ILogger<WemaAPIService>>();

        _wemaAPIService = new WemaAPIService(wemaAPISettings, _utilityServiceMock.Object, _log.Object, _cacheMock.Object);

    }

    [Fact]
    public void GetBanks_ExceptionIsCaught_ReturnsGetBanksResponseWithWithHasErrorTrue()
    {
        //Arrange
        var wemaAPISettings = Options.Create(new WemaAPISettings()
        {
            GetBanksUrl = null,
            SubscriptionHeader = "Ocp-Apim-Subscription-Key",
            SubscriptionKey = "8878b2f2d31d4f5aad221a59754b45e7"
        });

        _wemaAPIService = new WemaAPIService(wemaAPISettings, _utilityServiceMock.Object, _log.Object, _cacheMock.Object);
        //Act
        var result = _wemaAPIService.GetBanks().Result;

        //Assert
        Assert.True(result.hasError);
    }

    [Fact]
    public void GetBanks_ApiCallFails_ReturnsGetBanksResponseWithWithHasErrorTrue()
    {
        //Arrange
        var wemaAPISettings = Options.Create(new WemaAPISettings()
        {
            GetBanksUrl = "failtest_url",
            SubscriptionHeader = "Ocp-Apim-Subscription-Key",
            SubscriptionKey = "8878b2f2d31d4f5aad221a59754b45e7"
        });

        _wemaAPIService = new WemaAPIService(wemaAPISettings, _utilityServiceMock.Object, _log.Object, _cacheMock.Object);
        //Act
        var result = _wemaAPIService.GetBanks().Result;

        //Assert
        Assert.True(result.hasError);
    }


    [Fact]
    public void GetBanks_ApiCallSucceeds_ReturnsGetBanksResponseWithWithHasErrorFalse()
    {
        //Arrange

        var wemaAPISettings = Options.Create(new WemaAPISettings()
        {
            GetBanksUrl = "https://wema-alatdev-apimgt.azure-api.net/alat-test/api/Shared/GetAllBanks",
            SubscriptionHeader = "Ocp-Apim-Subscription-Key",
            SubscriptionKey = "55566DEF78901ABC234DE56FGHIJ"
        });

        _wemaAPIService = new WemaAPIService(wemaAPISettings, _utilityServiceMock.Object, _log.Object, _cacheMock.Object);

        //Act
        var result = _wemaAPIService.GetBanks().Result;

        //Assert
        Assert.False(result.hasError);
        Assert.True(result.result.Any());
    }
}