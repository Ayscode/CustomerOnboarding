using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerOnboarding.Entities
{
    public class MockOTP
    {
        [Key]
        public long Id { get; set; }
        public string phoneNumber { get; set; }
        public string OTP { get; set; }
        public DateTime RequestTime { get; set; }
        public DateTime ExpiryTime { get; set; }
        public int Attempts { get; set; }
        public string? StatusCode { get; set; }
        public string? StatusMessage { get; set; }
    }
}
