using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharityDonationPlatform.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentIntentAsync(decimal amount, string currency, string metadata);
        Task<bool> ConfirmPaymentAsync(string paymentIntentId);
        Task<dynamic> GetPaymentInfoAsync(string paymentIntentId);
    }
}
