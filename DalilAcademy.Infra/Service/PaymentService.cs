using DalilAcademy.Core.DTOs.Enrollments;
using DalilAcademy.Core.DTOs.Payments;
using DalilAcademy.Core.Exceptions;
using DalilAcademy.Core.Models;
using DalilAcademy.Core.Repository;
using DalilAcademy.Core.Service;
using DalilAcademy.Infra.Payments;
using Microsoft.Extensions.Options;

namespace DalilAcademy.Infra.Service
{
    public class PaymentService(
        IEnrollmentRepository enrollmentRepository,
        IPaymentRepository paymentRepository,
        IPaymentGateway paymentGateway,
        IOptions<PayPalSettings> options) : IPaymentService
    {
        private readonly PayPalSettings _settings = options.Value;

        public async Task<PayEnrollmentResponseDto> PayAsync(
            int enrollmentId,
            CancellationToken cancellationToken = default)
        {
            var enrollment = await GetEnrollmentOrThrowAsync(enrollmentId);

            if (enrollment.Status == EnrollmentStatus.Paid)
                throw new ConflictException("This enrollment has already been paid.");

            var coursePrice = enrollment.Course?.Price
                ?? throw new NotFoundException(
                    $"Course for enrollment {enrollmentId} was not found.");

            if (coursePrice <= 0)
                throw new ConflictException(
                    "A free course does not require a PayPal payment.");

            if (_settings.JodToUsdRate <= 0)
                throw new InvalidOperationException(
                    "PayPal JodToUsdRate must be greater than zero.");

            var amountUsd = Math.Round(
                coursePrice * _settings.JodToUsdRate,
                2,
                MidpointRounding.AwayFromZero);

            var order = await paymentGateway.CreateOrderAsync(
                amountUsd,
                _settings.ReturnUrl,
                _settings.CancelUrl,
                cancellationToken);

            var payment = await paymentRepository.CreateAsync(new Payment
            {
                EnrollmentId = enrollmentId,
                Amount = amountUsd,
                Currency = "USD",
                PayPalOrderId = order.OrderId,
                Status = PaymentStatus.Created,
                CreatedAt = DateTime.UtcNow
            });

            return new PayEnrollmentResponseDto(
                payment.Id, order.OrderId, payment.Amount,
                payment.Currency, order.ApprovalUrl);
        }

        public async Task<EnrollmentResponseDto> CaptureAsync(
            int enrollmentId,
            CancellationToken cancellationToken = default)
        {
            var enrollment = await GetEnrollmentOrThrowAsync(enrollmentId);

            if (enrollment.Status == EnrollmentStatus.Paid)
                throw new ConflictException("This enrollment has already been paid.");

            await EnsureCourseHasCapacityAsync(enrollment);

            var payment = await paymentRepository.GetLatestCreatedAsync(enrollmentId);
            if (payment is null)
                throw new PaymentFailedException("Call /pay first.");

            if (DateTime.UtcNow - payment.CreatedAt >= TimeSpan.FromHours(3))
            {
                await paymentRepository.MarkFailedAsync(payment.Id);
                throw new PaymentFailedException(
                    "The PayPal order has expired. Call /pay again.");
            }

            var captured = await paymentGateway.CaptureOrderAsync(
                payment.PayPalOrderId, cancellationToken);

            if (!captured)
            {
                await paymentRepository.MarkFailedAsync(payment.Id);
                throw new PaymentFailedException(
                    "The payment was not approved or PayPal did not complete the capture. Call /pay again.");
            }

            var completed = await paymentRepository.CompleteAsync(
                payment.Id, DateTime.UtcNow);

            if (!completed)
                throw new NotFoundException(
                    $"Payment with ID {payment.Id} was not found.");

            return ToDto(await GetEnrollmentOrThrowAsync(enrollmentId));
        }

        public async Task<List<PaymentResponseDto>> GetHistoryAsync(int enrollmentId)
        {
            _ = await GetEnrollmentOrThrowAsync(enrollmentId);
            var payments = await paymentRepository.GetByEnrollmentAsync(enrollmentId);

            return payments.Select(payment => new PaymentResponseDto(
                payment.Id,
                payment.EnrollmentId,
                payment.Amount,
                payment.Currency,
                payment.PayPalOrderId,
                payment.Status,
                payment.CreatedAt,
                payment.PaidAt)).ToList();
        }

        private async Task EnsureCourseHasCapacityAsync(Enrollment enrollment)
        {
            var course = enrollment.Course
                ?? throw new NotFoundException(
                    $"Course for enrollment {enrollment.Id} was not found.");

            if (!course.MaxStudents.HasValue) return;

            var paidCount = await enrollmentRepository
                .CountPaidEnrollmentsForCourseAsync(course.Id);

            if (paidCount >= course.MaxStudents.Value)
                throw new ConflictException(
                    "This course has reached its maximum number of paid students.");
        }

        private async Task<Enrollment> GetEnrollmentOrThrowAsync(int id)
        {
            return await enrollmentRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(
                    $"Enrollment with ID {id} was not found.");
        }

        private static EnrollmentResponseDto ToDto(Enrollment enrollment)
        {
            return new EnrollmentResponseDto(
                enrollment.Id,
                enrollment.StudentId,
                enrollment.Student?.FullName ?? string.Empty,
                enrollment.CourseId,
                enrollment.Course?.Title ?? string.Empty,
                enrollment.Course?.Price ?? 0,
                enrollment.EnrolledAt,
                enrollment.Status);
        }
    }
}
