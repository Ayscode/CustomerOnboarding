using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using CustomerOnboarding.Data;
using CustomerOnboarding.Entities;
using CustomerOnboarding.Exceptions;
using CustomerOnboarding.Interfaces;
using CustomerOnboarding.Models;
using CustomerOnboarding.Models.Configurations;
using CustomerOnboarding.Services;
using CustomerOnboarding.UnitTests.Mockings;
using Xunit;
using CustomerOnboarding;


namespace CustomerOnboarding.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;
        private readonly List<ApplicationUser> _users;

        public AuthServiceTests()
        {
            _users = new List<ApplicationUser> {
                new ApplicationUser() { Email = "user1@gmail.com", PhoneNumber = "012345677" },
                new ApplicationUser() { Email = "user2@gmail.com", PhoneNumber = "232345677" }
            };

            _userManager = MockUserManager.GetUserManager<ApplicationUser>(_users);

            var httpContextAccessor = new HttpContextAccessor();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            var optionsAccessor = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<CustomerOnboarding.Services.AuthService>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<ApplicationUser>>();
            _signInManager = new Mock<SignInManager<ApplicationUser>>(_userManager.Object, httpContextAccessor, claimsFactoryMock.Object, null, null, null, null);

            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "CustomerOnboarding").Options;
            _context = new ApplicationDbContext(dbContextOptions);

            var jwtSettings = Options.Create(new JwtSettings() { SecretKey = "84322CFB66934ECC86D547C5CF4F2EFC", Issuer = "CustomerOnboarding", Audience = "CustomerOnboardingUser", TokenLifespan = 60 });
            var appSettings = Options.Create(new AppSettings() { OtpLifespan = 5, ApiKey = "843-22CFB-6693-4ECC86-D54-7C5-CF4-F2EFC" });
            var emailSettings = Options.Create(new EmailSettings() { ApiKey= "843-22CFB-6693-4ECC86-D54-7C5-CF4-F2EFC", FromName= "CustomerOnboarding" , FromAddress = "noreply@CustomerOnboarding.com" });
            var refreshTokenGenerator = new Mock<IRefreshTokenGenerator>();

            _authService = new AuthService(_context, _userManager.Object, jwtSettings, refreshTokenGenerator.Object, _signInManager.Object, logger.Object , appSettings);
        }

        [Fact]
        public async Task SendOtp_ReturnsSendOtpResponse_WhenPhoneNumberIsValid()
        {
            // Arrange
            // var authService = new AuthService(_context, _userManager.Object, jwtSettings, refreshTokenGenerator.Object, _signInManager.Object, logger.Object , appSettings);
            string validPhoneNumber = "08123456789";

            // Act
            var result = await _authService.SendOtp(validPhoneNumber);

            // Assert
            Assert.IsType<SendOtpResponse>(result);
        }

        [Fact]
        public async Task SendOtp_ThrowsBadRequestException_WhenPhoneNumberIsInvalid()
        {
            // Arrange
            // var authService = new AuthService(_context, _userManager.Object, jwtSettings, refreshTokenGenerator.Object, _signInManager.Object, logger.Object , appSettings);
            string invalidPhoneNumber = "123";

            // Act and Assert
            await Assert.ThrowsAsync<BadRequestException>(async () => await _authService.SendOtp(invalidPhoneNumber));
        }

        [Fact]
        public async Task Register_UserAlreadyExist_ThrowsBadRequestException()
        {
            // Arrange
            var newUser = new ApplicationUser
            {
                Email = "test@gmail.com",
                PhoneNumber = "08123456789",
            };

            _userManager.Setup(x => x.CreateAsync(newUser));
            await _context.AddAsync(newUser);
            await _context.SaveChangesAsync();
            var newRegisterRequest = new RegistrationRequest { Email = newUser.Email, PhoneNumber = newUser.PhoneNumber };

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _authService.Register(newRegisterRequest));
        }

        [Fact]
        public async Task Register_UserIsNotCreatedSuccessfully_ThrowsBadRequestException()
        {
            // Arrange
            var request = new RegistrationRequest
            {
                Email = "test@gmail.com",
                PhoneNumber = "08123456789",
                Password = "password123?"
            };
            var user = new ApplicationUser
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                State = request.StateOfResidence,
                UserName = request.Email
            };
            user.Otp = new Random().Next(10000, 99999).ToString();
            user.DateCreated = DateTime.Now;
            user.Status = StatusType.Active.ToString();
            user.OtpLifeSpan = DateTime.Now;
            user.DateLastModified = DateTime.Now;
            user.SaltProperty = CryptoServices.CreateRandomSalt();
            user.Role = UserRole.Customer.ToString();

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _authService.Register(request));
        }

        [Fact]
        public async Task Register_UserCreatedSuccessfully_ReturnsResgistrationResponse()
        {
            // Arrange
            var request = new RegistrationRequest
            {
                Email = "test@gmail.com",
                PhoneNumber = "08123456789",
                Password = "Password123?"
            };
            var user = new ApplicationUser
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                State = request.StateOfResidence,
                UserName = request.Email
            };
            user.Otp = new Random().Next(10000, 99999).ToString();
            user.DateCreated = DateTime.Now;
            user.Status = StatusType.Active.ToString();
            user.OtpLifeSpan = DateTime.Now;
            user.DateLastModified = DateTime.Now;
            user.SaltProperty = CryptoServices.CreateRandomSalt();
            user.Role = UserRole.Customer.ToString();

            // Act
            var result = await _authService.Register(request);

            // Assert
            Assert.IsType<RegistrationResponse>(result);
        }
    }
}
