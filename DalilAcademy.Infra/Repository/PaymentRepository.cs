using DalilAcademy.Core.Models;
using DalilAcademy.Core.Repository;
using DalilAcademy.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace DalilAcademy.Infra.Repository
{
    public class PaymentRepository(AppDbContext context) : IPaymentRepository
    {
        public async Task<Payment> CreateAsync(Payment payment)
        {
            var openPayments = await context.Payments
                .Where(p => p.EnrollmentId == payment.EnrollmentId &&
                            p.Status == PaymentStatus.Created)
                .ToListAsync();

            foreach (var oldPayment in openPayments)
                oldPayment.Status = PaymentStatus.Failed;

            context.Payments.Add(payment);
            await context.SaveChangesAsync();
            return payment;
        }

        public Task<Payment?> GetLatestCreatedAsync(int enrollmentId)
        {
            return context.Payments
                .AsNoTracking()
                .Where(p => p.EnrollmentId == enrollmentId &&
                            p.Status == PaymentStatus.Created)
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .FirstOrDefaultAsync();
        }

        public Task<List<Payment>> GetByEnrollmentAsync(int enrollmentId)
        {
            return context.Payments
                .AsNoTracking()
                .Where(p => p.EnrollmentId == enrollmentId)
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .ToListAsync();
        }

        public async Task MarkFailedAsync(int paymentId)
        {
            var payment = await context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment is null) return;

            payment.Status = PaymentStatus.Failed;
            await context.SaveChangesAsync();
        }

        public async Task<bool> CompleteAsync(int paymentId, DateTime paidAt)
        {
            var payment = await context.Payments
                .Include(p => p.Enrollment)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment is null) return false;

            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = paidAt;
            payment.Enrollment.Status = EnrollmentStatus.Paid;

            await context.SaveChangesAsync();
            return true;
        }
    }
}
