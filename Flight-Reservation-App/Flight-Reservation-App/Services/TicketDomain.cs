using Flight_Reservation_App.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flight_Reservation_App.Services
{
    internal class TicketDomain
    {
        private readonly Repository<Ticket> _ticketRepository;

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
                nameof(Ticket.TicketPrice),
                nameof(Ticket.BookingId),
                nameof(Ticket.TripId),
                nameof(Ticket.AircraftId),
                nameof(Ticket.SeatNumber)],
            [
                    (object)ticket.TicketPrice??DBNull.Value,
                    (object)ticket.BookingId?? DBNull.Value,
                    (object)ticket.TripId?? DBNull.Value,
                    (object)ticket.AircraftId?? DBNull.Value,
                    (object)ticket.SeatNumber?? DBNull.Value]);
        }
    }
}
