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

        public TicketDomain()
        {
            _ticketRepository = new Repository<Ticket>(GlobalUsing.connectionString);
        }

        public TicketDomain(string connectionString)
        {
            _ticketRepository = new Repository<Ticket>(connectionString);
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
            WHERE t.AircraftId = s.AircraftId
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
                            AircraftId = (int)reader["AircraftId"],
                            SeatNumber = reader["SeatNumber"].ToString(),
                            ClassType = reader["ClassType"].ToString()
                        });
                    }
                }
            }

            return seats;
        }

        public async Task AddTicket(Ticket ticket)
        {
            await _ticketRepository.AddAsync([
                nameof(Ticket.TicketId),
                nameof(Ticket.TicketPrice),
                nameof(Ticket.BookingId),
                nameof(Ticket.TripId),
                nameof(Ticket.AircraftId),
                nameof(Ticket.SeatNumber)],
            [
                    (object)ticket.TicketId,
                    (object)ticket.TicketPrice??DBNull.Value,
                    (object)ticket.BookingId?? DBNull.Value,
                    (object)ticket.TripId?? DBNull.Value,
                    (object)ticket.AircraftId?? DBNull.Value,
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
    }
}
