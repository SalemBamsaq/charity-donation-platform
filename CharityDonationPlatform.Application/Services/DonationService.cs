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
    public class DonationService : IDonationService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public DonationService(
            ApplicationDbContext context,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<Donation> CreateDonationAsync(Donation donation)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Add donation
                await _context.Donations.AddAsync(donation);

                // Update campaign amount raised
                var campaign = await _context.Campaigns.FindAsync(donation.CampaignId);
                if (campaign == null)
                    throw new Exception("Campaign not found");

                campaign.AmountRaised += donation.Amount;

                // Check if the campaign reached its goal
                bool goalReached = false;
                if (campaign.AmountRaised >= campaign.TargetAmount &&
                    campaign.AmountRaised - donation.Amount < campaign.TargetAmount)
                {
                    goalReached = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Create notification for the donor
                await _notificationService.CreateNotificationAsync(
                    donation.DonorId,
                    "Donation Successful",
                    $"Thank you for your donation of ${donation.Amount} to {campaign.Title}.",
                    "DonationMade",
                    $"DonationId:{donation.Id}"
                );

                // Create notification for admin users
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole != null)
                {
                    var adminUserIds = await _context.UserRoles
                        .Where(ur => ur.RoleId == adminRole.Id)
                        .Select(ur => ur.UserId)
                        .ToListAsync();

                    var donor = await _context.Users.FindAsync(donation.DonorId);

                    foreach (var adminId in adminUserIds)
                    {
                        await _notificationService.CreateNotificationAsync(
                            adminId,
                            "New Donation Received",
                            $"${donation.Amount} donation received for {campaign.Title} from {(donation.IsAnonymous ? "Anonymous" : donor.FirstName + " " + donor.LastName)}.",
                            "DonationReceived",
                            $"DonationId:{donation.Id}"
                        );
                    }

                    // Send confirmation email to donor
                    await _emailService.SendDonationConfirmationEmailAsync(
                        donor.Email,
                        donor.FirstName + " " + donor.LastName,
                        donation.Amount,
                        campaign.Title
                    );

                    // Send notification to admin
                    await _emailService.SendNewDonationNotificationToAdminAsync(
                        campaign.Title,
                        donation.Amount,
                        donation.IsAnonymous ? "Anonymous" : donor.FirstName + " " + donor.LastName
                    );
                }

                // If goal reached, notify all donors and admin
                if (goalReached)
                {
                    // Notify campaign creator
                    await _notificationService.CreateNotificationAsync(
                        campaign.CreatedById,
                        "Campaign Goal Reached!",
                        $"Your campaign '{campaign.Title}' has reached its funding goal of ${campaign.TargetAmount}!",
                        "CampaignGoalReached",
                        $"CampaignId:{campaign.Id}"
                    );

                    // Notify all donors of this campaign
                    var donorIds = await _context.Donations
                        .Where(d => d.CampaignId == campaign.Id)
                        .Select(d => d.DonorId)
                        .Distinct()
                        .ToListAsync();

                    foreach (var donorId in donorIds)
                    {
                        if (donorId != donation.DonorId) // Don't double notify the current donor
                        {
                            await _notificationService.CreateNotificationAsync(
                                donorId,
                                "Campaign Goal Reached!",
                                $"A campaign you donated to, '{campaign.Title}', has reached its funding goal of ${campaign.TargetAmount}!",
                                "CampaignGoalReached",
                                $"CampaignId:{campaign.Id}"
                            );
                        }
                    }

                    // Send email to campaign creator
                    var campaignCreator = await _context.Users.FindAsync(campaign.CreatedById);
                    if (campaignCreator != null)
                    {
                        await _emailService.SendCampaignGoalReachedEmailAsync(
                            campaignCreator.Email,
                            campaign.Title
                        );
                    }
                }

                return donation;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Donation>> GetDonationsByCampaignIdAsync(int campaignId)
        {
            return await _context.Donations
                .Where(d => d.CampaignId == campaignId)
                .Include(d => d.Donor)
                .Include(d => d.Campaign)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Donation>> GetDonationsByUserIdAsync(string userId)
        {
            return await _context.Donations
                .Where(d => d.DonorId == userId)
                .Include(d => d.Campaign)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();
        }

        public async Task<Donation> GetDonationByIdAsync(int id)
        {
            return await _context.Donations
                .Include(d => d.Campaign)
                .Include(d => d.Donor)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<decimal> GetTotalDonationAmountAsync()
        {
            return await _context.Donations.SumAsync(d => d.Amount);
        }

        public async Task<decimal> GetTotalDonationAmountByCampaignAsync(int campaignId)
        {
            return await _context.Donations
                .Where(d => d.CampaignId == campaignId)
                .SumAsync(d => d.Amount);
        }

        public async Task<IEnumerable<KeyValuePair<string, decimal>>> GetTopDonorsAsync(int count)
        {
            var topDonors = await _context.Donations
                .Where(d => !d.IsAnonymous)
                .GroupBy(d => d.DonorId)
                .Select(g => new
                {
                    DonorId = g.Key,
                    TotalAmount = g.Sum(d => d.Amount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .Take(count)
                .ToListAsync();

            var result = new List<KeyValuePair<string, decimal>>();

            foreach (var donor in topDonors)
            {
                var user = await _context.Users.FindAsync(donor.DonorId);
                if (user != null)
                {
                    result.Add(new KeyValuePair<string, decimal>(
                        $"{user.FirstName} {user.LastName}",
                        donor.TotalAmount
                    ));
                }
            }

            return result;
        }
    }
}