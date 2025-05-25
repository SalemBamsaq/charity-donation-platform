using CharityDonationPlatform.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CharityDonationPlatform.Application.Interfaces
{
    public interface IDonationService
    {
        Task<Donation> CreateDonationAsync(Donation donation);
        Task<IEnumerable<Donation>> GetDonationsByCampaignIdAsync(int campaignId);
        Task<IEnumerable<Donation>> GetDonationsByUserIdAsync(string userId);
        Task<Donation> GetDonationByIdAsync(int id);
        Task<decimal> GetTotalDonationAmountAsync();
        Task<decimal> GetTotalDonationAmountByCampaignAsync(int campaignId);
        Task<IEnumerable<KeyValuePair<string, decimal>>> GetTopDonorsAsync(int count);
    }
}