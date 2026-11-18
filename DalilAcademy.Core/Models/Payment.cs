namespace DalilAcademy.Core.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string PayPalOrderId { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; } = PaymentStatus.Created;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
