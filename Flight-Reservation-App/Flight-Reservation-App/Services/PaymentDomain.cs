using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flight_Reservation_App.Models;

namespace Flight_Reservation_App.Services
{
    public class PaymentDomain
    {
        private readonly Repository<Payment> _paymentRepo;

        public PaymentDomain()
        {
            _paymentRepo = new Repository<Payment>(GlobalUsing.connectionString);
        }

        public PaymentDomain(string connectionString)
        {
            _paymentRepo = new Repository<Payment>(connectionString);
        }

        // SELECT * FROM Payment
        public async Task<List<Payment>> GetPaymentsAsync()
        {
            string[] cols =
            {
                "PaymentID AS PaymentId",
                "PaymentMethod",
                "PaymentStatus",
                "BookingID AS BookingId",
                "PaymentDate"
            };

            return await _paymentRepo.GetAsync(cols);
        }

        public List<Payment> GetPayments()
        {
            return GetPaymentsAsync().GetAwaiter().GetResult();
        }

        // SELECT * FROM Payment WHERE BookingID = @p0
        public async Task<List<Payment>> GetPaymentsByBookingAsync(int bookingId)
        {
            string[] cols =
            {
                "PaymentID AS PaymentId",
                "PaymentMethod",
                "PaymentStatus",
                "BookingID AS BookingId",
                "PaymentDate"
            };

            Where[] conditions =
            {
                new Where { Column = "BookingID", Operator = "=", Value = bookingId }
            };

            return await _paymentRepo.GetAsync(cols, conditions);
        }

        public List<Payment> GetPaymentsByBooking(int bookingId)
        {
            return GetPaymentsByBookingAsync(bookingId).GetAwaiter().GetResult();
        }

        // INSERT INTO Payment (...) VALUES (...)
        public async Task AddPaymentAsync(Payment payment)
        {
            string[] columns =
            {
                "PaymentID",
                "PaymentMethod",
                "PaymentStatus",
                "BookingID",
                "PaymentDate"
            };

            object[] values =
            {
                (object?)payment.PaymentId      ?? DBNull.Value,
                (object?)payment.PaymentMethod  ?? DBNull.Value,
                (object?)payment.PaymentStatus  ?? DBNull.Value,
                (object?)payment.BookingId      ?? DBNull.Value,
                payment.PaymentDate.HasValue
                    ? (object)payment.PaymentDate.Value
                    : DateOnly.FromDateTime(DateTime.UtcNow)
            };

            await _paymentRepo.AddAsync(columns, values);
        }

        public void AddPayment(Payment payment)
        {
            AddPaymentAsync(payment).GetAwaiter().GetResult();
        }

        // UPDATE Payment SET PaymentStatus = @p0 WHERE PaymentID = @w0
        public async Task UpdatePaymentStatusAsync(int paymentId, string newStatus)
        {
            string[] columns = { "PaymentStatus" };
            object[] values = { newStatus };

            Where[] conditions =
            {
                new Where { Column = "PaymentID", Operator = "=", Value = paymentId }
            };

            await _paymentRepo.UpdateAsync(columns, values, conditions);
        }

        public void UpdatePaymentStatus(int paymentId, string newStatus)
        {
            UpdatePaymentStatusAsync(paymentId, newStatus).GetAwaiter().GetResult();
        }

        // DELETE FROM Payment WHERE PaymentID = @p0
        public async Task DeletePaymentAsync(int paymentId)
        {
            Where[] conditions =
            {
                new Where { Column = "PaymentID", Operator = "=", Value = paymentId }
            };

            await _paymentRepo.DeleteAsync(conditions);
        }

        public void DeletePayment(int paymentId)
        {
            DeletePaymentAsync(paymentId).GetAwaiter().GetResult();
        }
    }
}