using Flight_Reservation_App.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flight_Reservation_App.Services
{
    internal class AircraftDomain
    {
        private readonly Repository<Aircraft> _aircraftRepo;
        private readonly Repository<Seat> _seatRepo;

        public AircraftDomain()
        {
            _aircraftRepo = new Repository<Aircraft>(GlobalUsing.connectionString);
            _seatRepo = new Repository<Seat>(GlobalUsing.connectionString);
        }

        public async Task<List<Aircraft>> GetAircrafts()
        {
            var aircrafts = await _aircraftRepo.GetAsync();
            return aircrafts;
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


        public async Task AddAircraft(Aircraft aircraft, int ecoSeats, int bussinessSeats, int firstClassSeats)
        {
            await _aircraftRepo.AddAsync([nameof(Aircraft.Model),
                nameof(Aircraft.PassengerCapacity),
                nameof(Aircraft.CargoCapacity),
            nameof(Aircraft.MaxTakeOffWeight)],
            [aircraft.Model,
                ecoSeats+bussinessSeats+firstClassSeats,
                aircraft.CargoCapacity,
                aircraft.MaxTakeOffWeight]);

            var lastAddedAircraft = (await _aircraftRepo.GetAsync()).LastOrDefault();
            for (int i = 0; i < ecoSeats; i++)
            {
                await _seatRepo.AddAsync([nameof(Seat.AircraftId),
                    nameof(Seat.SeatNumber),
                    nameof(Seat.ClassType)],
                    [lastAddedAircraft.AircraftId,
                    (i+1).ToString(),
                    "Economy"]);
            }
            for (int i = ecoSeats; i < ecoSeats+bussinessSeats; i++)
            {
                await _seatRepo.AddAsync([nameof(Seat.AircraftId),
                    nameof(Seat.SeatNumber),
                    nameof(Seat.ClassType)],
                    [lastAddedAircraft.AircraftId,
                    (i+1).ToString(),
                    "Business"]);
            }
            for (int i = ecoSeats + bussinessSeats; i < ecoSeats+bussinessSeats+firstClassSeats; i++)
            {
                await _seatRepo.AddAsync([nameof(Seat.AircraftId),
                    nameof(Seat.SeatNumber),
                    nameof(Seat.ClassType)],
                    [lastAddedAircraft.AircraftId,
                    (i + 1).ToString(),
                    "First Class"]);
            }
        }

        public async Task UpdateAircraft(Aircraft aircraft)
        {
            await _aircraftRepo.UpdateAsync([nameof(Aircraft.Model),
                nameof(Aircraft.CargoCapacity),
            nameof(Aircraft.MaxTakeOffWeight)],
            [aircraft.Model,
            aircraft.CargoCapacity,
            aircraft.MaxTakeOffWeight],
            [new Where() { Column = nameof(Aircraft.AircraftId),
            Operator="=",
            Value= aircraft.AircraftId
            }]);
        }

        public async Task DeleteAircraft(int id)
        {
            await _aircraftRepo.DeleteAsync([new Where() {
                Column= nameof(Aircraft.AircraftId),
                Operator="=",
                Value= id
            }]);
        }
    }
}
