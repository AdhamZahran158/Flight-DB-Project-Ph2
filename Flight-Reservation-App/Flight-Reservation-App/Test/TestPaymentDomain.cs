using Flight_Reservation_App.Models;
using Flight_Reservation_App.Services;

namespace Flight_Reservation_App.Test
{
    public static class TestPaymentDomain
    {
        private const string ConnStr =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=AirlineDB;Integrated Security=True;" +
            "Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;" +
            "Encrypt=False;TrustServerCertificate=False;" +
            "Application Name=\"SQL Server Management Studio\"";

        private const string TempPassportNum = "PAYTEST00001";

        public static async Task RunTestsAsync()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("  Starting PaymentDomain Tests...");
            Console.WriteLine("========================================\n");

            var domain = new PaymentDomain(ConnStr);
            var tenantRepo = new Repository<Tenant>(ConnStr);
            var bookingRepo = new Repository<Booking>(ConnStr);
            var paymentRepo = new Repository<Payment>(ConnStr);

            bool createdTempTenant = false;
            bool createdTempBooking = false;
            int tempBookingId = -1;
            int tempPaymentId = -1;

            try
            {
                // ── Resolve BookingID FK dependency ───────────────────────────────
                Console.WriteLine("--- Resolving Booking FK dependency ---");

                // Clean up any leftover temp data
                var leftoverTenant = await tenantRepo.GetAsync(null,
                    new[] { new Where { Column = "PassportNum", Operator = "=", Value = TempPassportNum } });
                if (leftoverTenant.Count > 0)
                {
                    Console.WriteLine("Found leftover temp data — cleaning up...");
                    // delete payments → bookings → tenant in reverse FK order
                    var leftoverBookings = await bookingRepo.GetAsync(null,
                        new[] { new Where { Column = "PassportNum", Operator = "=", Value = TempPassportNum } });
                    foreach (var lb in leftoverBookings)
                    {
                        await paymentRepo.DeleteAsync(new[] { new Where { Column = "BookingID", Operator = "=", Value = lb.BookingId } });
                        await bookingRepo.DeleteAsync(new[] { new Where { Column = "BookingID", Operator = "=", Value = lb.BookingId } });
                    }
                    await tenantRepo.DeleteAsync(new[] { new Where { Column = "PassportNum", Operator = "=", Value = TempPassportNum } });
                }

                // Try to find an existing booking
                var existingBookings = await bookingRepo.GetAsync(
                    new[] { "BookingID AS BookingId", "PassportNum", "BookingStatus", "TotalPrice", "BookingDate" });

                if (existingBookings.Count > 0)
                {
                    tempBookingId = existingBookings[0].BookingId;
                    Console.WriteLine($"Using existing BookingID {tempBookingId} as FK owner.");
                }
                else
                {
                    // Seed tenant + booking
                    Console.WriteLine("No bookings found — seeding a temporary tenant and booking...");

                    await tenantRepo.AddAsync(
                        new[] { "PassportNum", "Fname", "Lname" },
                        new object[] { TempPassportNum, "Pay", "Tester" });
                    createdTempTenant = true;

                    var allBookings2 = await bookingRepo.GetAsync(new[] { "BookingID AS BookingId" });
                    tempBookingId = allBookings2.Count > 0 ? allBookings2.Max(b => b.BookingId) + 1 : 1;

                    await bookingRepo.AddAsync(
                        new[] { "BookingID", "BookingDate", "BookingStatus", "TotalPrice", "PassportNum" },
                        new object[] { tempBookingId, DateOnly.FromDateTime(DateTime.UtcNow), "Confirmed", 0m, TempPassportNum });
                    createdTempBooking = true;
                    Console.WriteLine($"Created temp BookingID {tempBookingId}.");
                }

                // Calculate a safe PaymentID
                var currentPayments = await domain.GetPaymentsAsync();
                tempPaymentId = currentPayments.Count > 0 ? currentPayments.Max(p => p.PaymentId) + 1 : 1;

                // ── Test 1: GetPayments (baseline) ────────────────────────────────
                Console.WriteLine("\n--- Testing GetPayments (baseline) ---");
                Console.WriteLine($"Total payments currently in database: {currentPayments.Count}");
                foreach (var p in currentPayments)
                    PrintPayment(p);

                // ── Test 2: AddPayment ────────────────────────────────────────────
                Console.WriteLine("\n--- Testing AddPayment ---");
                var newPayment = new Payment
                {
                    PaymentId = tempPaymentId,
                    PaymentMethod = "Credit Card",
                    PaymentStatus = "Pending",
                    BookingId = tempBookingId,
                    PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow)
                };

                await domain.AddPaymentAsync(newPayment);
                Console.WriteLine($"Successfully sent Add request for PaymentID {tempPaymentId}.");

                // ── Test 3: GetPayments (verify Add) ──────────────────────────────
                Console.WriteLine("\n--- Testing GetPayments (verifying Add) ---");
                var allAfterAdd = await domain.GetPaymentsAsync();
                Console.WriteLine($"Total payments after Add: {allAfterAdd.Count} (was {currentPayments.Count})");

                var addedPayment = allAfterAdd.Find(p => p.PaymentId == tempPaymentId);
                if (addedPayment != null)
                {
                    Console.WriteLine("Success! Retrieved the newly added payment:");
                    PrintPayment(addedPayment);
                }
                else
                {
                    Console.WriteLine("Failure: Could not retrieve the newly added payment.");
                    return;
                }

                // ── Test 4: GetPaymentsByBooking ──────────────────────────────────
                Console.WriteLine("\n--- Testing GetPaymentsByBooking ---");
                var byBooking = await domain.GetPaymentsByBookingAsync(tempBookingId);
                Console.WriteLine($"Payments for BookingID {tempBookingId}: {byBooking.Count}");
                foreach (var p in byBooking)
                    PrintPayment(p);

                bool foundInBooking = byBooking.Any(p => p.PaymentId == tempPaymentId);
                Console.WriteLine($"New payment found under its booking: {(foundInBooking ? "PASS" : "FAIL")}");

                // ── Test 5: UpdatePaymentStatus ───────────────────────────────────
                Console.WriteLine("\n--- Testing UpdatePaymentStatus ---");
                await domain.UpdatePaymentStatusAsync(tempPaymentId, "Paid");
                Console.WriteLine($"Sent UpdatePaymentStatus request for PaymentID {tempPaymentId} → 'Paid'.");

                // Verify update
                var afterUpdate = await domain.GetPaymentsAsync();
                var updatedPayment = afterUpdate.Find(p => p.PaymentId == tempPaymentId);
                if (updatedPayment != null)
                {
                    Console.WriteLine("Retrieved payment after update:");
                    PrintPayment(updatedPayment);
                    bool statusOk = updatedPayment.PaymentStatus == "Paid";
                    Console.WriteLine($"Status updated to 'Paid': {(statusOk ? "PASS" : "FAIL")}");
                }
                else
                {
                    Console.WriteLine("Failure: Could not retrieve payment after update.");
                }

                // ── Test 6: AddPayment with null PaymentDate (uses today fallback) ─
                Console.WriteLine("\n--- Testing AddPayment with null PaymentDate (today fallback) ---");
                int fallbackPaymentId = tempPaymentId + 1;
                var nullDatePayment = new Payment
                {
                    PaymentId = fallbackPaymentId,
                    PaymentMethod = "Cash",
                    PaymentStatus = "Pending",
                    BookingId = tempBookingId,
                    PaymentDate = null   // domain should default to UtcNow
                };

                await domain.AddPaymentAsync(nullDatePayment);
                Console.WriteLine($"Successfully sent Add request for PaymentID {fallbackPaymentId} with null date.");

                var afterFallback = await domain.GetPaymentsAsync();
                var fallbackPayment = afterFallback.Find(p => p.PaymentId == fallbackPaymentId);
                if (fallbackPayment != null)
                {
                    Console.WriteLine($"Payment date stored: {fallbackPayment.PaymentDate}");
                    bool dateOk = fallbackPayment.PaymentDate == DateOnly.FromDateTime(DateTime.UtcNow);
                    Console.WriteLine($"Date defaulted to today: {(dateOk ? "PASS" : "FAIL")}");
                }

                // ── Test 7: DeletePayment ─────────────────────────────────────────
                Console.WriteLine("\n--- Testing DeletePayment ---");
                await domain.DeletePaymentAsync(tempPaymentId);
                await domain.DeletePaymentAsync(fallbackPaymentId);
                Console.WriteLine($"Sent Delete requests for PaymentIDs {tempPaymentId} and {fallbackPaymentId}.");

                var afterDelete = await domain.GetPaymentsAsync();
                bool mainGone = afterDelete.All(p => p.PaymentId != tempPaymentId);
                bool fallbackGone = afterDelete.All(p => p.PaymentId != fallbackPaymentId);
                Console.WriteLine($"Main payment deleted:     {(mainGone ? "PASS" : "FAIL")}");
                Console.WriteLine($"Fallback payment deleted: {(fallbackGone ? "PASS" : "FAIL")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred during testing: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // ── Cleanup: remove temp booking / tenant if we created them ───────
                if (createdTempBooking)
                {
                    Console.WriteLine($"\n--- Cleanup: removing temp BookingID {tempBookingId} ---");
                    await paymentRepo.DeleteAsync(new[] { new Where { Column = "BookingID", Operator = "=", Value = tempBookingId } });
                    await bookingRepo.DeleteAsync(new[] { new Where { Column = "BookingID", Operator = "=", Value = tempBookingId } });
                    Console.WriteLine("Temp booking removed.");
                }

                if (createdTempTenant)
                {
                    Console.WriteLine($"--- Cleanup: removing temp tenant '{TempPassportNum}' ---");
                    await tenantRepo.DeleteAsync(new[] { new Where { Column = "PassportNum", Operator = "=", Value = TempPassportNum } });
                    Console.WriteLine("Temp tenant removed.");
                }
            }

            Console.WriteLine("\n========================================");
            Console.WriteLine("  PaymentDomain Tests Completed.");
            Console.WriteLine("========================================\n");
        }

        private static void PrintPayment(Payment p)
        {
            if (p == null) { Console.WriteLine("     [Payment object is null]"); return; }
            Console.WriteLine($"     [Payment] ID: {p.PaymentId} | Method: {p.PaymentMethod} | Status: {p.PaymentStatus} | BookingID: {p.BookingId} | Date: {p.PaymentDate}");
        }
    }
}