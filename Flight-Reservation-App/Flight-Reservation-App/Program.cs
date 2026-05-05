using Flight_Reservation_App.Test;

namespace Flight_Reservation_App
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await TestFlightDomain.RunTestsAsync(); // Test Flight Functionalities
        }
    }
}
