using System.Threading.Tasks;

namespace CharityDonationPlatform.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlMessage);
        Task SendDonationConfirmationEmailAsync(string to, string donorName, decimal amount, string campaignTitle);
        Task SendCampaignGoalReachedEmailAsync(string to, string campaignTitle);
        Task SendNewDonationNotificationToAdminAsync(string campaignTitle, decimal amount, string donorName);
    }
}