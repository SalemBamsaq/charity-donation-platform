using CharityDonationPlatform.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CharityDonationPlatform.Application.Services
{
    public class StripePaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly string? _secretKey;

        public StripePaymentService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = _configuration["Stripe:SecretKey"];
            if (!string.IsNullOrEmpty(_secretKey))
            {
                StripeConfiguration.ApiKey = _secretKey;
            }
        }

        public async Task<string> CreatePaymentIntentAsync(decimal amount, string currency, string metadata)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Convert to cents/smallest currency unit
                Currency = currency.ToLower(),
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    { "metadata", metadata }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return paymentIntent.Id;
        }

        public async Task<bool> ConfirmPaymentAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);

                return paymentIntent.Status == "succeeded";
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<dynamic> GetPaymentInfoAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);

                // SIMPLE FIX: Use the Created property directly since it's already a DateTime
                var createdDateTime = paymentIntent.Created;

                return new
                {
                    Id = paymentIntent.Id,
                    Amount = paymentIntent.Amount / 100.0m, // Convert from cents to dollars
                    Currency = paymentIntent.Currency,
                    Status = paymentIntent.Status,
                    Created = createdDateTime, // No conversion needed
                    PaymentMethod = paymentIntent.PaymentMethodId ?? string.Empty,
                    Metadata = paymentIntent.Metadata ?? new Dictionary<string, string>()
                };
            }
            catch (Exception)
            {
                // Return safe default values if Stripe call fails
                return new
                {
                    Id = paymentIntentId,
                    Amount = 0m,
                    Currency = "USD",
                    Status = "unknown",
                    Created = DateTime.UtcNow,
                    PaymentMethod = string.Empty,
                    Metadata = new Dictionary<string, string>()
                };
            }
        }
    }
}