using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CustomerOnboarding.Interfaces;
using CustomerOnboarding.Middleware;
using CustomerOnboarding.Models;

namespace CustomerOnboarding.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authenticationService;
        public AccountController(IAuthService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// This endpoint is used for send otp to sign up a customer, the rolecode specifies the role of the user.
        /// </summary>
        /// <param name="request">This is the request payload: RegistrationRequest model</param>
        /// <returns>RegistrationResponse model</returns>
        [AllowAnonymous]
        [HttpPost("{phoneNumber}/send-otp")]
        [Produces(typeof(SendOtpResponse))]
        [ProducesErrorResponseType(typeof(ErrorDetails))]
        public async Task<ActionResult<SendOtpResponse>> Register(string phoneNumber)
        {
            return Ok(await _authenticationService.SendOtp(phoneNumber));
        }

        /// <summary>
        /// This endpoint is used for signin up a customer, the rolecode specifies the role of the user.
        /// </summary>
        /// <param name="request">This is the request payload: RegistrationRequest model</param>
        /// <returns>RegistrationResponse model</returns>
        
        [AllowAnonymous]
        [HttpPost("register")]
        [Produces(typeof(RegistrationResponse))]
        [ProducesErrorResponseType(typeof(ErrorDetails))]
        public async Task<ActionResult<RegistrationResponse>> Register([FromBody] RegistrationRequest request)
        {
            return Ok(await _authenticationService.Register(request));
        }
    }
}
