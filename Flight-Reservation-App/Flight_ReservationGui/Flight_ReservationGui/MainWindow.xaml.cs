using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Flight_ReservationGui.Pages;

namespace Flight_ReservationGui
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Set window size
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(1400, 900));

            // Navigate to home on startup
            ContentFrame.Navigated += ContentFrame_Navigated;
            ContentFrame.Navigate(typeof(HomePage));
        }

        private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            var pageType = e.SourcePageType;
            string tag = "home";

            if (pageType == typeof(HomePage)) tag = "home";
            else if (pageType == typeof(FlightsPage)) tag = "flights";
            else if (pageType == typeof(PassengersPage)) tag = "passengers";
            else if (pageType == typeof(BookingsPage)) tag = "bookings";
            else if (pageType == typeof(TicketsPage)) tag = "tickets";
            else if (pageType == typeof(PaymentsPage)) tag = "payments";

            foreach (var item in NavView.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == tag)
                {
                    NavView.SelectedItem = navItem;
                    break;
                }
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer is NavigationViewItem item)
            {
                var tag = item.Tag?.ToString();
                switch (tag)
                {
                    case "home":
                        ContentFrame.Navigate(typeof(HomePage));
                        break;
                    case "flights":
                        ContentFrame.Navigate(typeof(FlightsPage));
                        break;
                    case "passengers":
                        ContentFrame.Navigate(typeof(PassengersPage));
                        break;
                    case "bookings":
                        ContentFrame.Navigate(typeof(BookingsPage));
                        break;
                    case "tickets":
                        ContentFrame.Navigate(typeof(TicketsPage));
                        break;
                    case "payments":
                        ContentFrame.Navigate(typeof(PaymentsPage));
                        break;
                }
            }
        }
    }
}
