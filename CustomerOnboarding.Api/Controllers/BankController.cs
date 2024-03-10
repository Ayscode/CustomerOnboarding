using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CustomerOnboarding.Integrations.WemaTechTest;
using CustomerOnboarding.Interfaces;
using CustomerOnboarding.Middleware;

namespace CustomerOnboarding.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankController : ControllerBase
    {
        private readonly IWemaAPIService _wemaAPIService;

        public BankController(IWemaAPIService wemaAPIService)
        {
            _wemaAPIService = wemaAPIService;
        }


        /// <summary>
        /// This end point fetches list of banks
        /// </summary>
        /// <param></param>
        /// <returns>GetBanksResponse model</returns>
        /// <remarks>
        /// </remarks>
        [AllowAnonymous]
        [HttpGet("GetBanks")]
        [Produces(typeof(GetBanksResponse))]
        [ProducesErrorResponseType(typeof(ErrorDetails))]
        public async Task<IActionResult> GetBanks()
        {
            return Ok(await _wemaAPIService.GetBanks());
        }
    }
}
