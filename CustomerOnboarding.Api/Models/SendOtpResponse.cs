using System.ComponentModel.DataAnnotations;

namespace CustomerOnboarding.Models
{
    public class SendOtpResponse
    {
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string OTP { get; set; }
        public DateTime ExpiryTime { get; set; }
        public string Message { get; set; }
    }
}
