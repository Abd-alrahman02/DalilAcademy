using DalilAcademy.Core.DTOs.Enrollments;
using DalilAcademy.Core.DTOs.Payments;

namespace DalilAcademy.Core.Service
{
    public interface IPaymentService
    {
        Task<PayEnrollmentResponseDto> PayAsync(
            int enrollmentId,
            CancellationToken cancellationToken = default);

        Task<EnrollmentResponseDto> CaptureAsync(
            int enrollmentId,
            CancellationToken cancellationToken = default);
    }
}
