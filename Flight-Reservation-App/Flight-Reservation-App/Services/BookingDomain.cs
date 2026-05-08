using Flight_Reservation_App;
using Flight_Reservation_App.Models;

public class BookingDomain
{
    private readonly Repository<Booking> repository;

    public BookingDomain()
    {
        repository = new Repository<Booking>(GlobalUsing.connectionString);
    }

    public BookingDomain(string connectionString)
    {
        repository = new Repository<Booking>(connectionString);
    }

    public async Task AddBooking(Booking b)
    {
        await repository.AddAsync(
            columns:
            [
                nameof(Booking.BookingId),
                nameof(Booking.BookingStatus),
                nameof(Booking.TotalPrice),
                nameof(Booking.PassportNum),
                nameof(Booking.BookingDate)
            ],

            values:
            [
                (object)b.BookingId,
                (object)b.BookingStatus?? DBNull.Value,
                (object)b.TotalPrice?? DBNull.Value,
                (object)b.PassportNum?? DBNull.Value,
                DateOnly.FromDateTime(DateTime.UtcNow)
            ]
        );
    }

    public async Task<List<Booking>> GetBookings()
    {
        string[] cols = {
            "BookingID AS BookingId",
            "BookingDate",
            "BookingStatus",
            "TotalPrice",
            "PassportNum"
        };
        return await repository.GetAsync(cols);
    }

    public async Task UpdateBooking(Booking b)
    {
        await repository.UpdateAsync(
            [nameof(Booking.BookingStatus)],
            [b.BookingStatus],

            [
                new()
                {
                    Column = nameof(Booking.BookingId),
                    Operator = "=",
                    Value = b.BookingId
                }
            ]
        );
    }

    public async Task DeleteBooking(int id)
    {
        await repository.DeleteAsync(
        [
            new()
            {
                Column = nameof(Booking.BookingId),
                Operator = "=",
                Value = id
            }
        ]);
    }
}