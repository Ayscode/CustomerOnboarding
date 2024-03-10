using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CustomerOnboarding.Models;

namespace CustomerOnboarding.Interfaces
{
    public interface IUserService
    {
        Task<List<Customer>> GetCustomers();
        Task<Customer> GetCustomer(string userId);
    }
}
