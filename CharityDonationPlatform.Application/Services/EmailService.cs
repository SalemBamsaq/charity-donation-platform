using CharityDonationPlatform.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CharityDonationPlatform.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _adminEmail;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpServer = _configuration["Email:SmtpServer"];
            _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["Email:SmtpUsername"];
            _smtpPassword = _configuration["Email:SmtpPassword"];
            _fromEmail = _configuration["Email:FromEmail"];
            _adminEmail = _configuration["Email:AdminEmail"];
        }

        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(to));

                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    client.EnableSsl = true;

                    await client.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                // Log the exception in a real application
                // For now, we'll just ignore email failures to prevent app crashes
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
        }

        public async Task SendDonationConfirmationEmailAsync(string to, string donorName, decimal amount, string campaignTitle)
        {
            var subject = $"Thank You for Your Donation to {campaignTitle}";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 10px; text-align: center; }}
                        .content {{ padding: 20px; border: 1px solid #ddd; }}
                        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Thank You for Your Donation!</h2>
                        </div>
                        <div class='content'>
                            <p>Dear {donorName},</p>
                            <p>Thank you for your generous donation of <strong>${amount:F2}</strong> to <strong>{campaignTitle}</strong>.</p>
                            <p>Your contribution makes a real difference and helps us continue our mission.</p>
                            <p>You can view your donation receipt and track the campaign's progress by logging into your account.</p>
                            <p>Thank you again for your support!</p>
                            <p>Sincerely,<br>The Charity Team</p>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message, please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendCampaignGoalReachedEmailAsync(string to, string campaignTitle)
        {
            var subject = $"Goal Reached for Campaign: {campaignTitle}";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 10px; text-align: center; }}
                        .content {{ padding: 20px; border: 1px solid #ddd; }}
                        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Campaign Goal Reached!</h2>
                        </div>
                        <div class='content'>
                            <p>Great news!</p>
                            <p>We're excited to inform you that the campaign <strong>{campaignTitle}</strong> has reached its funding goal!</p>
                            <p>This incredible achievement would not have been possible without the generous support of donors like you.</p>
                            <p>You can view the campaign details and updates by logging into your account.</p>
                            <p>Thank you for being part of this success!</p>
                            <p>Sincerely,<br>The Charity Team</p>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message, please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendNewDonationNotificationToAdminAsync(string campaignTitle, decimal amount, string donorName)
        {
            var subject = $"New Donation Received for {campaignTitle}";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #3498db; color: white; padding: 10px; text-align: center; }}
                        .content {{ padding: 20px; border: 1px solid #ddd; }}
                        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>New Donation Received</h2>
                        </div>
                        <div class='content'>
                            <p>Hello Admin,</p>
                            <p>A new donation has been received:</p>
                            <ul>
                                <li><strong>Campaign:</strong> {campaignTitle}</li>
                                <li><strong>Amount:</strong> ${amount:F2}</li>
                                <li><strong>Donor:</strong> {donorName}</li>
                                <li><strong>Date:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</li>
                            </ul>
                            <p>You can view all donation details in the admin dashboard.</p>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message, please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(_adminEmail, subject, body);
        }
    }
}