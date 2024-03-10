using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CustomerOnboarding.Data;
using CustomerOnboarding.Entities;
using CustomerOnboarding.Exceptions;
using CustomerOnboarding.Interfaces;
using CustomerOnboarding.Models;
using CustomerOnboarding.Models.Configurations;

namespace CustomerOnboarding.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtSettings _jwtSettings;
        private readonly NotificationService _notify;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly ILogger<AuthService> logger;


        public AuthService(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                                IOptions<JwtSettings> jwtSettings,
                                NotificationService notify,
                                IRefreshTokenGenerator refreshTokenGenerator,
                                SignInManager<ApplicationUser> signInManager,
                                ILogger<AuthService> _log)
        {
            _context = context;
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _signInManager = signInManager;
            _notify = notify;
            _refreshTokenGenerator = refreshTokenGenerator;
            logger = _log;
        }

        public async Task<SendOtpResponse> SendOtp(string phoneNumber)
        {
            try
            {
                if (phoneNumber.Length != 11)
                {
                    throw new BadRequestException("Phone number must be 11 digits long");
                }

                var existingOTP = await _context.MockOTP.FirstOrDefaultAsync(x => x.phoneNumber == phoneNumber);
                if (existingOTP != null)
                {
                    existingOTP.Attempts += 1;
                    _context.MockOTP.Update(existingOTP);
                    await _context.SaveChangesAsync();

                    if (existingOTP.Attempts > 3)
                    {
                        return new SendOtpResponse { Message = "OTP already sent maximum number of times, kindly contact admin!" };
                    }

                    // check if the otp has not expired
                    if (existingOTP.ExpiryTime > DateTime.UtcNow)
                    {
                        return new SendOtpResponse { Message = "OTP already sent!" };
                    }
                }
                
                // Generate a new OTP
                var otp = new Random().Next(10000, 99999).ToString();
            
                // Create a new mock OTP entry with the generated OTP, phone number, and expiry time
                var mockOTP = new MockOTP
                {
                    phoneNumber = phoneNumber,
                    OTP = otp,
                    RequestTime = DateTime.UtcNow,
                    ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                    Attempts = 1
                };
                _context.MockOTP.Add(mockOTP);
                await _context.SaveChangesAsync();

                // Create a response with the phone number, generated OTP, and expiry time
                var sendOtpResponse = new SendOtpResponse
                {
                    PhoneNumber = phoneNumber,
                    OTP = otp,
                    ExpiryTime = DateTime.UtcNow.AddMinutes(5)
                };
    
                return sendOtpResponse;
            }
            catch (Exception ex)
            {
                // Log and handle any exceptions
                logger.LogInformation($"Error in SendOTP: {ex}");
                throw new BadRequestException($"{ex}");
            }
        }

        public async Task<AuthResponse> Login(AuthRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                throw new BadRequestException($"User with {request.Email} not found.");
            }

            if (user.Status != StatusType.Active.ToString())
            {
                throw new BadRequestException("Your account is not active");
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, request.Password, false, true);

            if (signInResult.IsLockedOut)
            {
                throw new BadRequestException("You have been temporarily locked out");
            }

            if (!signInResult.Succeeded)
            {
                throw new BadRequestException($"Credentials for '{request.Email} aren't valid', you have {3 - user.AccessFailedCount} attempt left.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                        new Claim(ClaimTypes.Name, user.Email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenLifespan),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            if (user.PhoneNumberConfirmed)
            {
                var response = new AuthResponse()
                {
                    PhoneNumber = user.PhoneNumber,
                    Status = user.Status,
                    UserName = user.UserName,
                    UserId = user.Id,
                    Role = user.Role,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    TokenType = "Bearer",
                    ExpiresIn = tokenDescriptor.Expires.Value,
                    RefreshToken = await _refreshTokenGenerator.GenerateToken(),
                    IsEmailConfirmed = user.EmailConfirmed,
                    IsPhoneNumberConfirmed = user.PhoneNumberConfirmed
                };

                user.AccessFailedCount = 0;
                user.JwtRefreshToken = response.RefreshToken;
                await _userManager.UpdateAsync(user);

                return response;
            }
            else
            {
                //send Otp
                await _notify.SendOtp(user);

                throw new BadRequestException($"Please verify the otp sent to your email: {request.Email}.");
            }

            throw new BadRequestException($"User with {request.Email} not found.");
        }
        public async Task<RefreshTokenResponse> Authenticate(string email, Claim[] claims)
        {
            var token = GenerateTokenString(email, DateTime.UtcNow, claims);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user != null)
            {
                var refreshToken = await _refreshTokenGenerator.GenerateToken();

                user.JwtRefreshToken = refreshToken;
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var response = new RefreshTokenResponse
                {
                    JwtToken = token,
                    RefreshToken = refreshToken
                };

                return response;
            }
            else
            {
                throw new BadRequestException("Invalid user credentials");
            }
        }
        public async Task<RegistrationResponse> Register(RegistrationRequest request)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email || x.PhoneNumber == request.PhoneNumber);
            if (existingUser != null)
            {
                throw new BadRequestException("User already exist, kindly Login with your email and password");
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                State = request.StateOfResidence,
                UserName = request.Email,
                Lga = request.Lga
            };

            //Generate Otp
            user.Otp = new Random().Next(10000, 99999).ToString();
            user.DateCreated = DateTime.UtcNow;
            user.Status = StatusType.Active.ToString();
            user.OtpLifeSpan = DateTime.UtcNow;
            user.DateLastModified = DateTime.UtcNow;
            user.SaltProperty = CryptoServices.CreateRandomSalt();
            user.Role = UserRole.Customer.ToString();

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, UserRole.Customer.ToString());

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenKey = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Email)
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenLifespan),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(tokenKey),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);

                var registrationResponse = new RegistrationResponse
                {
                    PhoneNumber = user.PhoneNumber,
                    Status = user.Status,
                    UserName = user.UserName,
                    UserId = user.Id,
                    Role = user.Role,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    TokenType = "Bearer",
                    ExpiresIn = tokenDescriptor.Expires.Value,
                    RefreshToken = await _refreshTokenGenerator.GenerateToken(),
                    IsEmailConfirmed = user.EmailConfirmed,
                    IsPhoneNumberConfirmed = user.PhoneNumberConfirmed
                };

                return registrationResponse;
            }
            else
            {
                throw new BadRequestException($"{result.Errors}");
            }
        }

        
        private string GenerateTokenString(string username, DateTime expires, Claim[] claims = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                 claims ?? new Claim[]
                 {
                    new Claim(ClaimTypes.Name, username)
                 }),
                //NotBefore = expires,
                Expires = expires.AddMinutes(_jwtSettings.TokenLifespan),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
    }
}
