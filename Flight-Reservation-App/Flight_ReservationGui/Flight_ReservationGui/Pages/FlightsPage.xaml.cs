using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Flight_Reservation_App.Models;
using Flight_Reservation_App.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Flight_ReservationGui.Pages
{
    public sealed partial class FlightsPage : Page
    {
        private readonly FlightDomain _flightDomain = new FlightDomain(Flight_Reservation_App.GlobalUsing.connectionString);
        private ObservableCollection<Flight> _flights = new();
        private System.Collections.Generic.List<Airport> _airports = new();
        private System.Collections.Generic.List<Aircraft> _aircrafts = new();

        public FlightsPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLookupData();
            await LoadFlights();
        }

        private async Task LoadLookupData()
        {
            try
            {
                _airports = await _flightDomain.GetAirportsAsync();
                _aircrafts = await _flightDomain.GetAircraftsAsync();

                CmbDepartureAirport.Items.Clear();
                CmbArrivalAirport.Items.Clear();
                CmbAircraft.ItemsSource = _aircrafts;

                foreach (var a in _airports)
                {
                    CmbDepartureAirport.Items.Add(new ComboBoxItem { Content = $"{a.Iatacode} - {a.Name}", Tag = a.AirportId });
                    CmbArrivalAirport.Items.Add(new ComboBoxItem { Content = $"{a.Iatacode} - {a.Name}", Tag = a.AirportId });
                }
            }
            catch (Exception ex)
            {
                ShowError("Failed to load lookup data: " + ex.Message);
            }
        }

        private async Task LoadFlights()
        {
            try
            {
                var flights = await _flightDomain.GetFlightsAsync();
                _flights = new ObservableCollection<Flight>(flights);
                FlightsListView.ItemsSource = _flights;
                ShowSuccess($"Loaded {_flights.Count} flights.");
            }
            catch (Exception ex)
            {
                ShowError("Failed to load flights: " + ex.Message);
            }
        }

        private bool ValidateInput(out string error)
        {
            error = "";

            if (string.IsNullOrWhiteSpace(TxtFlightNumber.Text))
            {
                error = "Flight Number is required.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(TxtDistanceKm.Text) && !decimal.TryParse(TxtDistanceKm.Text, out var dist))
            {
                error = "Distance must be a valid number.";
                return false;
            }
            else if (decimal.TryParse(TxtDistanceKm.Text, out var distVal) && distVal < 0)
            {
                error = "Distance cannot be negative.";
                return false;
            }

            if (CmbStatus.SelectedItem == null)
            {
                error = "Status is required.";
                return false;
            }

            if (CmbDepartureAirport.SelectedItem == null)
            {
                error = "Departure Airport is required.";
                return false;
            }

            if (CmbArrivalAirport.SelectedItem == null)
            {
                error = "Arrival Airport is required.";
                return false;
            }

            var depAirportId = (int)((ComboBoxItem)CmbDepartureAirport.SelectedItem).Tag;
            var arrAirportId = (int)((ComboBoxItem)CmbArrivalAirport.SelectedItem).Tag;
            if (depAirportId == arrAirportId)
            {
                error = "Departure and Arrival airports cannot be the same.";
                return false;
            }

            if (DpDeparture.Date == null)
            {
                error = "Departure Date is required.";
                return false;
            }

            if (DpArrival.Date == null)
            {
                error = "Arrival Date is required.";
                return false;
            }

            var depDateTime = DpDeparture.Date.Value.Date + TpDeparture.Time;
            var arrDateTime = DpArrival.Date.Value.Date + TpArrival.Time;

            if (arrDateTime <= depDateTime)
            {
                error = "Arrival time must be after departure time.";
                return false;
            }

            return true;
        }

        private Flight BuildFlightFromForm()
        {
            var nextId = _flights.Count > 0 ? _flights.Max(f => f.FlightId) + 1 : 1;

            var flight = new Flight
            {
                FlightId = nextId,
                FlightNumber = TxtFlightNumber.Text.Trim(),
                Status = (CmbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString()
            };

            if (decimal.TryParse(TxtDistanceKm.Text, out var dist))
                flight.DistanceKm = dist;

            if (CmbAircraft.SelectedItem is Aircraft ac)
                flight.AircraftId = ac.AircraftId;

            if (CmbDepartureAirport.SelectedItem is ComboBoxItem depItem)
                flight.DepartureAirportId = (int)depItem.Tag;

            if (CmbArrivalAirport.SelectedItem is ComboBoxItem arrItem)
                flight.ArrivalAirportId = (int)arrItem.Tag;

            if (DpDeparture.Date != null)
                flight.DepartureTime = DpDeparture.Date.Value.Date + TpDeparture.Time;

            if (DpArrival.Date != null)
                flight.ArrivalTime = DpArrival.Date.Value.Date + TpArrival.Time;

            return flight;
        }

        private async void Btn_Add_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput(out var error))
            {
                ShowError(error);
                return;
            }

            try
            {
                var flight = BuildFlightFromForm();
                await _flightDomain.AddFlightAsync(flight);
                ShowSuccess("Flight added successfully!");
                ClearForm();
                await LoadFlights();
            }
            catch (Exception ex)
            {
                ShowError("Failed to add flight: " + ex.Message);
            }
        }

        private async void Btn_Update_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFlightId.Text) || !int.TryParse(TxtFlightId.Text, out _))
            {
                ShowError("Please select a flight from the list to update.");
                return;
            }

            if (!ValidateInput(out var error))
            {
                ShowError(error);
                return;
            }

            try
            {
                var flight = BuildFlightFromForm();
                if (int.TryParse(TxtFlightId.Text, out var fid))
                    flight.FlightId = fid;
                await _flightDomain.UpdateFlightAsync(flight);
                ShowSuccess("Flight updated successfully!");
                await LoadFlights();
            }
            catch (Exception ex)
            {
                ShowError("Failed to update flight: " + ex.Message);
            }
        }

        private async void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFlightId.Text) || !int.TryParse(TxtFlightId.Text, out var flightId))
            {
                ShowError("Please select a flight from the list to delete.");
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Confirm Delete",
                Content = $"Are you sure you want to delete Flight #{flightId}?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                try
                {
                    await _flightDomain.DeleteFlightAsync(flightId);
                    ShowSuccess("Flight deleted successfully!");
                    ClearForm();
                    await LoadFlights();
                }
                catch (Exception ex)
                {
                    ShowError("Failed to delete flight: " + ex.Message);
                }
            }
        }

        private async void Btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadFlights();
        }

        private void Btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            TxtFlightId.Text = "";
            TxtFlightNumber.Text = "";
            TxtDistanceKm.Text = "";
            CmbStatus.SelectedItem = null;
            CmbAircraft.SelectedItem = null;
            CmbDepartureAirport.SelectedItem = null;
            CmbArrivalAirport.SelectedItem = null;
            DpDeparture.Date = null;
            DpArrival.Date = null;
            FlightsListView.SelectedItem = null;
        }

        private void FlightsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FlightsListView.SelectedItem is Flight f)
            {
                TxtFlightId.Text = f.FlightId.ToString();
                TxtFlightNumber.Text = f.FlightNumber ?? "";
                TxtDistanceKm.Text = f.DistanceKm?.ToString() ?? "";

                // Select status
                foreach (ComboBoxItem item in CmbStatus.Items)
                {
                    if (item.Content?.ToString() == f.Status)
                    {
                        CmbStatus.SelectedItem = item;
                        break;
                    }
                }

                // Select aircraft
                CmbAircraft.SelectedItem = _aircrafts.FirstOrDefault(a => a.AircraftId == f.AircraftId);

                // Select airports
                foreach (ComboBoxItem item in CmbDepartureAirport.Items)
                {
                    if (item.Tag is int id && id == f.DepartureAirportId)
                    {
                        CmbDepartureAirport.SelectedItem = item;
                        break;
                    }
                }
                foreach (ComboBoxItem item in CmbArrivalAirport.Items)
                {
                    if (item.Tag is int id && id == f.ArrivalAirportId)
                    {
                        CmbArrivalAirport.SelectedItem = item;
                        break;
                    }
                }

                if (f.DepartureTime.HasValue)
                {
                    DpDeparture.Date = new DateTimeOffset(f.DepartureTime.Value);
                    TpDeparture.Time = f.DepartureTime.Value.TimeOfDay;
                }

                if (f.ArrivalTime.HasValue)
                {
                    DpArrival.Date = new DateTimeOffset(f.ArrivalTime.Value);
                    TpArrival.Time = f.ArrivalTime.Value.TimeOfDay;
                }
            }
        }

        private void ShowError(string message)
        {
            StatusInfoBar.Message = message;
            StatusInfoBar.Severity = InfoBarSeverity.Error;
            StatusInfoBar.IsOpen = true;
        }

        private void ShowSuccess(string message)
        {
            StatusInfoBar.Message = message;
            StatusInfoBar.Severity = InfoBarSeverity.Success;
            StatusInfoBar.IsOpen = true;
        }
    }
}
