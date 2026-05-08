using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Flight_ReservationGui.Pages
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void Nav_Flights(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(FlightsPage));
        }

        private void Nav_Passengers(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PassengersPage));
        }

        private void Nav_Bookings(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BookingsPage));
        }

        private void Nav_Tickets(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(TicketsPage));
        }

        private void Nav_Payments(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PaymentsPage));
        }

        private void Nav_Aircrafts(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AircraftsPage));
        }
    }
}
