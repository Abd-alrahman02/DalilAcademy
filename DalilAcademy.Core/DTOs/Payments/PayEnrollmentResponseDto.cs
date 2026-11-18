namespace DalilAcademy.Core.DTOs.Payments
{
    public record PayEnrollmentResponseDto(
        int PaymentId,
        string OrderId,
        decimal Amount,
        string Currency,
        string ApprovalUrl);
}
