using DalilAcademy.Core.DTOs.Payments;

namespace DalilAcademy.Core.Service
{
    public interface IPaymentGateway
    {
        Task<PayPalOrderResult> CreateOrderAsync(
            decimal amountUsd,
            string returnUrl,
            string cancelUrl,
            CancellationToken cancellationToken = default);

        Task<bool> CaptureOrderAsync(
            string orderId,
            CancellationToken cancellationToken = default);
    }
}
