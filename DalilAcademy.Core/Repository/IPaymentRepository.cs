using DalilAcademy.Core.Models;

namespace DalilAcademy.Core.Repository
{
    public interface IPaymentRepository
    {
        Task<Payment> CreateAsync(Payment payment);
        Task<Payment?> GetLatestCreatedAsync(int enrollmentId);
        Task MarkFailedAsync(int paymentId);
        Task<bool> CompleteAsync(int paymentId, DateTime paidAt);
        Task<List<Payment>> GetByEnrollmentAsync(int enrollmentId);
    }
}
