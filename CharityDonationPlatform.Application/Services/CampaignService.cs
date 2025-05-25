using CharityDonationPlatform.Application.Interfaces;
using CharityDonationPlatform.Domain.Models;
using CharityDonationPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CharityDonationPlatform.Application.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public CampaignService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<Campaign>> GetAllCampaignsAsync()
        {
            return await _context.Campaigns
                .Include(c => c.CreatedBy)
                .OrderByDescending(c => c.Created)
                .ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetActiveCampaignsAsync()
        {
            return await _context.Campaigns
                .Where(c => c.IsActive && c.EndDate >= DateTime.UtcNow)
                .Include(c => c.CreatedBy)
                .OrderByDescending(c => c.Created)
                .ToListAsync();
        }

        public async Task<Campaign> GetCampaignByIdAsync(int id)
        {
            return await _context.Campaigns
                .Include(c => c.CreatedBy)
                .Include(c => c.Donations)
                    .ThenInclude(d => d.Donor)
                .Include(c => c.Updates)
                    .ThenInclude(u => u.CreatedBy)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Campaign> CreateCampaignAsync(Campaign campaign, string userId)
        {
            campaign.CreatedById = userId;
            campaign.Created = DateTime.UtcNow;

            await _context.Campaigns.AddAsync(campaign);
            await _context.SaveChangesAsync();

            // Get admin users to notify them about the new campaign
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null)
            {
                var adminUserIds = await _context.UserRoles
                    .Where(ur => ur.RoleId == adminRole.Id)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                foreach (var adminId in adminUserIds)
                {
                    if (adminId != userId) // Don't notify the creator
                    {
                        await _notificationService.CreateNotificationAsync(
                            adminId,
                            "New Campaign Created",
                            $"A new campaign '{campaign.Title}' has been created.",
                            "CampaignCreated",
                            $"CampaignId:{campaign.Id}"
                        );
                    }
                }
            }

            return campaign;
        }

        public async Task<Campaign> UpdateCampaignAsync(Campaign campaign)
        {
            var existingCampaign = await _context.Campaigns.FindAsync(campaign.Id);

            if (existingCampaign == null)
                return null;

            // Update properties
            existingCampaign.Title = campaign.Title;
            existingCampaign.Description = campaign.Description;
            existingCampaign.TargetAmount = campaign.TargetAmount;
            existingCampaign.StartDate = campaign.StartDate;
            existingCampaign.EndDate = campaign.EndDate;
            existingCampaign.ImageUrl = campaign.ImageUrl;
            existingCampaign.IsActive = campaign.IsActive;
            existingCampaign.Updated = DateTime.UtcNow;

            _context.Campaigns.Update(existingCampaign);
            await _context.SaveChangesAsync();

            return existingCampaign;
        }

        public async Task<bool> DeleteCampaignAsync(int id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);

            if (campaign == null)
                return false;

            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddCampaignUpdateAsync(CampaignUpdate update, string userId)
        {
            update.CreatedById = userId;
            update.Created = DateTime.UtcNow;

            await _context.CampaignUpdates.AddAsync(update);
            await _context.SaveChangesAsync();

            // Get all donors of this campaign to notify them
            var donorIds = await _context.Donations
                .Where(d => d.CampaignId == update.CampaignId)
                .Select(d => d.DonorId)
                .Distinct()
                .ToListAsync();

            var campaign = await _context.Campaigns.FindAsync(update.CampaignId);

            foreach (var donorId in donorIds)
            {
                await _notificationService.CreateNotificationAsync(
                    donorId,
                    $"Update on {campaign.Title}",
                    $"New update: {update.Title}",
                    "CampaignUpdate",
                    $"CampaignId:{update.CampaignId},UpdateId:{update.Id}"
                );
            }

            return true;
        }

        public async Task<IEnumerable<CampaignUpdate>> GetCampaignUpdatesAsync(int campaignId)
        {
            return await _context.CampaignUpdates
                .Where(u => u.CampaignId == campaignId)
                .Include(u => u.CreatedBy)
                .OrderByDescending(u => u.Created)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountRaisedAsync()
        {
            return await _context.Donations.SumAsync(d => d.Amount);
        }

        public async Task<int> GetTotalDonorsCountAsync()
        {
            return await _context.Donations
                .Select(d => d.DonorId)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> GetActiveCampaignsCountAsync()
        {
            return await _context.Campaigns
                .Where(c => c.IsActive && c.EndDate >= DateTime.UtcNow)
                .CountAsync();
        }
    }
}