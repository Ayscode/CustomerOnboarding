using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CustomerOnboarding.Models;

namespace CustomerOnboarding.Interfaces
{
    public interface IAuthService
    {
        Task<SendOtpResponse> SendOtp(string phoneNumber);
        Task<RegistrationResponse> Register(RegistrationRequest request);
        Task<RefreshTokenResponse> Authenticate(string email, Claim[] claims);

    }
}
