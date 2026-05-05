using System;
using System.Linq;
using System.Threading.Tasks;
using Flight_Reservation_App.Models;
using Flight_Reservation_App.Services;

namespace Flight_Reservation_App.Test
{
    public static class TestFlightDomain
    {
        public static async Task RunTestsAsync()
        {
            Console.WriteLine("Starting FlightDomain Tests...\n");
            
            var domain = new FlightDomain();

            try
            {
                // Test 1: Add Flight
                Console.WriteLine("--- Testing AddFlight ---");
                
                // Fetch valid foreign keys to prevent NULL constraint errors
                var airports = await domain.GetAirportsAsync();
                var aircrafts = await domain.GetAircraftsAsync();
                
                string connStr = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=AirlineDB;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False;Application Name=\"SQL Server Management Studio\"";
                
                if (airports.Count < 2)
                {
                    Console.WriteLine("Database has less than 2 airports. Seeding dummy airports...");
                    var airportRepo = new Repository<Airport>(connStr);
                    int nextAirportId = airports.Count > 0 ? airports.Max(a => a.AirportId) + 1 : 1;
                    string[] airportCols = { "AirportID", "Name", "City", "Country", "IATACode" };
                    
                    if (airports.Count == 0) await airportRepo.AddAsync(airportCols, new object[] { nextAirportId, "Test Airport 1", "Test City 1", "Test Country", "TA1" });
                    await airportRepo.AddAsync(airportCols, new object[] { nextAirportId + 1, "Test Airport 2", "Test City 2", "Test Country", "TA2" });
                    
                    airports = await domain.GetAirportsAsync();
                }

                if (aircrafts.Count == 0)
                {
                    Console.WriteLine("Database has no aircraft. Seeding a dummy aircraft...");
                    var aircraftRepo = new Repository<Aircraft>(connStr);
                    string[] aircraftCols = { "AircraftID", "Model", "PassengerCapacity" };
                    await aircraftRepo.AddAsync(aircraftCols, new object[] { 1, "Boeing 737 Test", 180 });
                    
                    aircrafts = await domain.GetAircraftsAsync();
                }

                // Test: Get Airports
                Console.WriteLine("\n--- Testing GetAirports ---");
                Console.WriteLine($"Total airports retrieved: {airports.Count}");
                foreach(var a in airports)
                {
                    PrintAirport(a);
                }

                // Test: Get Aircrafts
                Console.WriteLine("\n--- Testing GetAircrafts ---");
                Console.WriteLine($"Total aircraft retrieved: {aircrafts.Count}");
                foreach(var a in aircrafts)
                {
                    PrintAircraft(a);
                }
                Console.WriteLine();

                // Calculate a safe FlightID to avoid Primary Key violations
                var currentFlights = await domain.GetFlightsAsync();
                int nextFlightId = currentFlights.Count > 0 ? currentFlights.Max(f => f.FlightId) + 1 : 1;

                var newFlight = new Flight
                {
                    FlightId = nextFlightId,
                    FlightNumber = "TEST-ADD",
                    DistanceKm = 800,
                    Status = "Scheduled",
                    DepartureTime = DateTime.Now.AddDays(2),
                    ArrivalTime = DateTime.Now.AddDays(2).AddHours(3),
                    AircraftId = aircrafts[0].AircraftId,
                    DepartureAirportId = airports[0].AirportId,
                    ArrivalAirportId = airports[1].AirportId
                };
                
                await domain.AddFlightAsync(newFlight);
                Console.WriteLine("Successfully sent Add request for flight 'TEST-ADD'.");
                
                // Test 2: Get Flights (and verify Add)
                Console.WriteLine("\n--- Testing GetFlights ---");
                var allFlights = await domain.GetFlightsAsync();
                Console.WriteLine($"Total flights retrieved from database: {allFlights.Count}");
                
                var addedFlight = allFlights.Find(f => f.FlightNumber == "TEST-ADD");
                if (addedFlight != null)
                {
                    Console.WriteLine($"Success! Retrieved the newly added flight:");
                    PrintFlight(addedFlight);
                    
                    // Test 3: Update Flight
                    Console.WriteLine("\n--- Testing UpdateFlight ---");
                    addedFlight.Status = "Boarding";
                    await domain.UpdateFlightAsync(addedFlight);
                    Console.WriteLine($"Sent Update request to change flight {addedFlight.FlightId} status to 'Boarding'.");
                    
                    // Verify Update
                    var flightsAfterUpdate = await domain.GetFlightsAsync();
                    var updatedFlight = flightsAfterUpdate.Find(f => f.FlightId == addedFlight.FlightId);
                    Console.WriteLine("Retrieved flight after update:");
                    PrintFlight(updatedFlight);
                    
                    // Test 4: Delete Flight
                    Console.WriteLine("\n--- Testing DeleteFlight ---");
                    await domain.DeleteFlightAsync(addedFlight.FlightId);
                    Console.WriteLine($"Sent Delete request for flight {addedFlight.FlightId}.");

                    // Verify Delete
                    var flightsAfterDelete = await domain.GetFlightsAsync();
                    var deletedFlight = flightsAfterDelete.Find(f => f.FlightId == addedFlight.FlightId);
                    if (deletedFlight == null)
                    {
                        Console.WriteLine($"Success! Verified that flight {addedFlight.FlightId} no longer exists in the database. (Query returned null/not found)");
                    }
                    else
                    {
                        Console.WriteLine("Failure: Flight still exists after deletion:");
                        PrintFlight(deletedFlight);
                    }
                }
                else
                {
                    Console.WriteLine("Failure: Could not retrieve the newly added flight. Did the Add operation fail?");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred during testing: {ex.Message}");
            }
            
            Console.WriteLine("\nTests Completed.");
        }

        private static void PrintFlight(Flight f)
        {
            if (f == null) 
            {
                Console.WriteLine("     [Flight object is null]");
                return;
            }
            Console.WriteLine($"     [ID: {f.FlightId} | Number: {f.FlightNumber} | Status: {f.Status} | Distance: {f.DistanceKm}km | Departure Time: {f.DepartureTime}]");
        }

        private static void PrintAirport(Airport a)
        {
            if (a == null) return;
            Console.WriteLine($"     [Airport] ID: {a.AirportId} | Name: {a.Name} | City: {a.City} | IATA: {a.Iatacode}");
        }

        private static void PrintAircraft(Aircraft a)
        {
            if (a == null) return;
            Console.WriteLine($"     [Aircraft] ID: {a.AircraftId} | Model: {a.Model} | Capacity: {a.PassengerCapacity}");
        }
    }
}
