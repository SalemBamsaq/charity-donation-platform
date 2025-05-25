using CharityDonationPlatform.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CharityDonationPlatform.Application.Interfaces
{
    public interface ICampaignService
    {
        Task<IEnumerable<Campaign>> GetAllCampaignsAsync();
        Task<IEnumerable<Campaign>> GetActiveCampaignsAsync();
        Task<Campaign> GetCampaignByIdAsync(int id);
        Task<Campaign> CreateCampaignAsync(Campaign campaign, string userId);
        Task<Campaign> UpdateCampaignAsync(Campaign campaign);
        Task<bool> DeleteCampaignAsync(int id);
        Task<bool> AddCampaignUpdateAsync(CampaignUpdate update, string userId);
        Task<IEnumerable<CampaignUpdate>> GetCampaignUpdatesAsync(int campaignId);
        Task<decimal> GetTotalAmountRaisedAsync();
        Task<int> GetTotalDonorsCountAsync();
        Task<int> GetActiveCampaignsCountAsync();
    }
}