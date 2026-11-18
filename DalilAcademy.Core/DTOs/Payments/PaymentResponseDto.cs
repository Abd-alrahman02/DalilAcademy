using DalilAcademy.Core.Models;

namespace DalilAcademy.Core.DTOs.Payments
{
    public record PaymentResponseDto(
        int Id,
        int EnrollmentId,
        decimal Amount,
        string Currency,
        string PayPalOrderId,
        PaymentStatus Status,
        DateTime CreatedAt,
        DateTime? PaidAt);
}
