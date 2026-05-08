using Flight_Reservation_App.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flight_Reservation_App.Services
{
    public class TicketDomain
    {
        private readonly Repository<Ticket> _ticketRepository;
        private readonly Repository<Trip> _tripRepository;

        public TicketDomain()
        {
            _ticketRepository = new Repository<Ticket>(GlobalUsing.connectionString);
            _tripRepository = new Repository<Trip>(GlobalUsing.connectionString);
        }

        public TicketDomain(string connectionString)
        {
            _ticketRepository = new Repository<Ticket>(connectionString);
            _tripRepository = new Repository<Trip>(connectionString);
        }

        public async Task AddTicket(Ticket ticket ,string? tripType= null)
        {
            await _tripRepository.AddAsync([nameof(Trip.TripType)], [tripType?? (object)DBNull.Value]);
            var lastestTrip = (await _tripRepository.GetAsync()).LastOrDefault();
            await _ticketRepository.AddAsync([
                nameof(Ticket.TicketId),
                nameof(Ticket.TicketPrice),
                nameof(Ticket.BookingId),
                nameof(Ticket.TripId),
                nameof(Ticket.AircraftID),
                nameof(Ticket.SeatNumber)],
            [
                    (object)ticket.TicketId,
                    (object)ticket.TicketPrice??DBNull.Value,
                    (object)ticket.BookingId?? DBNull.Value,
                    (object)lastestTrip.TripId?? DBNull.Value,
                    (object)ticket.AircraftID?? DBNull.Value,
                    (object)ticket.SeatNumber?? DBNull.Value]);

        }

        public async Task<int> GetMaxTicketId()
        {
            using (SqlConnection conn = new SqlConnection(GlobalUsing.connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(TicketID), 0) FROM Ticket", conn))
            {
                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task<List<Ticket>> GetTickets()
        {
            var tickets = await _ticketRepository.GetAsync();
            return tickets;
        }

        public async Task<List<Seat>> GetAvailableSeats()
        {
            var seats = new List<Seat>();

            var query = @"
        SELECT *
        FROM Seat s
        WHERE s.SeatNumber NOT IN
        (
            SELECT t.SeatNumber
            FROM Ticket t
            WHERE t.AircraftID = s.AircraftID
        )";

            using (SqlConnection conn = new SqlConnection(GlobalUsing.connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                await conn.OpenAsync();

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        seats.Add(new Seat
                        {
                            AircraftID = (int)reader["AircraftID"],
                            SeatNumber = reader["SeatNumber"].ToString(),
                            ClassType = reader["ClassType"].ToString()
                        });
                    }
                }
            }

            return seats;
        }

    }
}
