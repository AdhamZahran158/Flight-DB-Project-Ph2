using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flight_Reservation_App.Models;

namespace Flight_Reservation_App.Services
{
    public class FlightDomain
    {
        private readonly Repository<Flight> _flightRepo;
        private readonly Repository<Airport> _airportRepo;
        private readonly Repository<Aircraft> _aircraftRepo;

        public FlightDomain(string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=AirlineDB;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False;Application Name=\"SQL Server Management Studio\"")
        {
            _flightRepo = new Repository<Flight>(connectionString);
            _airportRepo = new Repository<Airport>(connectionString);
            _aircraftRepo = new Repository<Aircraft>(connectionString);
        }

        private object GetDbValue(object? value)
        {
            return value ?? DBNull.Value; // Essential for Null values of columns , since we use ADO.net not EF :sob:
        }

        public List<Flight> GetFlights()
        {
            return GetFlightsAsync().GetAwaiter().GetResult(); // SELECT * FROM Flights
        }

        public async Task<List<Flight>> GetFlightsAsync()
        {
            string[] cols = {
                "FlightID AS FlightId", "FlightNumber", "DistanceKM AS DistanceKm", 
                "Status", "AircraftID", "DepartureAirportID AS DepartureAirportId", 
                "DepartureTime", "ArrivalAirportID AS ArrivalAirportId", "ArrivalTime"
            };
            return await _flightRepo.GetAsync(cols);
        }

        public void AddFlight(Flight flight)
        {
            AddFlightAsync(flight).GetAwaiter().GetResult(); // INSERT INTO Flight (All columns of Flights) VALUES (all values);
        }

        public async Task AddFlightAsync(Flight flight)
        {
            string[] columns = { 
                "FlightID", "FlightNumber", "DistanceKM", "Status", "AircraftID", 
                "DepartureAirportID", "DepartureTime", "ArrivalAirportID", "ArrivalTime" 
            };
            
            object[] values = { 
                GetDbValue(flight.FlightId),
                GetDbValue(flight.FlightNumber), 
                GetDbValue(flight.DistanceKm), 
                GetDbValue(flight.Status), 
                GetDbValue(flight.AircraftID), 
                GetDbValue(flight.DepartureAirportId), 
                GetDbValue(flight.DepartureTime), 
                GetDbValue(flight.ArrivalAirportId), 
                GetDbValue(flight.ArrivalTime) 
            };

            await _flightRepo.AddAsync(columns, values);
        }

        public void UpdateFlight(Flight flight)
        {
            UpdateFlightAsync(flight).GetAwaiter().GetResult(); // UPDATE Flight SET Columns = values WHERE FlightID = flight.FlightId;
        }

        public async Task UpdateFlightAsync(Flight flight)
        {
            string[] columns = { 
                "FlightNumber", "DistanceKM", "Status", "AircraftID", 
                "DepartureAirportID", "DepartureTime", "ArrivalAirportID", "ArrivalTime" 
            };
            
            object[] values = { 
                GetDbValue(flight.FlightNumber), 
                GetDbValue(flight.DistanceKm), 
                GetDbValue(flight.Status), 
                GetDbValue(flight.AircraftID), 
                GetDbValue(flight.DepartureAirportId), 
                GetDbValue(flight.DepartureTime), 
                GetDbValue(flight.ArrivalAirportId), 
                GetDbValue(flight.ArrivalTime) 
            };

            Where[] conditions = {
                new Where { Column = "FlightID", Operator = "=", Value = flight.FlightId }
            };

            await _flightRepo.UpdateAsync(columns, values, conditions);
        }

        public void DeleteFlight(int flightId)
        {
            DeleteFlightAsync(flightId).GetAwaiter().GetResult(); //DELETE FROM Flight WHERE FlightID = flightId;
        }

        public async Task DeleteFlightAsync(int flightId)
        {
            Where[] conditions = {
                new Where { Column = "FlightID", Operator = "=", Value = flightId }
            };

            await _flightRepo.DeleteAsync(conditions);
        }

        public List<Airport> GetAirports()
        {
            return GetAirportsAsync().GetAwaiter().GetResult(); // SELECT * FROM Airport
        }

        public async Task<List<Airport>> GetAirportsAsync()
        {
            string[] cols = {
                "AirportID AS AirportId", "Name", "City", "Country", "IATACode AS Iatacode", "Longitude", "Latitude"
            };
            return await _airportRepo.GetAsync(cols);
        }
        
        public List<Aircraft> GetAircrafts()
        {
            return GetAircraftsAsync().GetAwaiter().GetResult(); // SELECT * FROM Aircraft
        }

        public async Task<List<Aircraft>> GetAircraftsAsync()
        {
            string[] cols = {
                "AircraftID", "Model", "PassengerCapacity", "CargoCapacity", "MaxTakeOffWeight"
            };
            return await _aircraftRepo.GetAsync(cols);
        }
    }
}
