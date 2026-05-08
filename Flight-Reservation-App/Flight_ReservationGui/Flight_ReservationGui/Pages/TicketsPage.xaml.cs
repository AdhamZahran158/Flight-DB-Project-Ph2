using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Flight_Reservation_App.Models;
using Flight_Reservation_App.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Flight_ReservationGui.Pages
{
    public sealed partial class TicketsPage : Page
    {
        private readonly TicketDomain _ticketDomain = new TicketDomain(Flight_Reservation_App.GlobalUsing.connectionString);
        private readonly FlightDomain _flightDomain = new FlightDomain(Flight_Reservation_App.GlobalUsing.connectionString);
        private List<Seat> _availableSeats = new();
        private List<Aircraft> _aircrafts = new();

        public TicketsPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAircrafts();
            await LoadAvailableSeats();
        }

        private async Task LoadAircrafts()
        {
            try
            {
                _aircrafts = await _flightDomain.GetAircraftsAsync();
                CmbAircraft.ItemsSource = _aircrafts;
            }
            catch (Exception ex)
            {
                ShowError("Failed to load aircrafts: " + ex.Message);
            }
        }

        private async Task LoadAvailableSeats()
        {
            try
            {
                _availableSeats = await _ticketDomain.GetAvailableSeats();
                SeatsListView.ItemsSource = new ObservableCollection<Seat>(_availableSeats);
                UpdateSeatComboBox();
                ShowSuccess($"Found {_availableSeats.Count} available seats.");
            }
            catch (Exception ex)
            {
                ShowError("Failed to load available seats: " + ex.Message);
            }
        }

        private void UpdateSeatComboBox()
        {
            CmbSeat.Items.Clear();
            var filteredSeats = _availableSeats;

            // Filter by selected aircraft if any
            if (CmbAircraft.SelectedItem is Aircraft ac)
            {
                filteredSeats = _availableSeats.Where(s => s.AircraftId == ac.AircraftId).ToList();
            }

            foreach (var seat in filteredSeats)
            {
                CmbSeat.Items.Add(new ComboBoxItem
                {
                    Content = $"{seat.SeatNumber} ({seat.ClassType})",
                    Tag = seat
                });
            }
        }

        private void CmbAircraft_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSeatComboBox();
        }

        private bool ValidateInput(out string error)
        {
            error = "";

            if (string.IsNullOrWhiteSpace(TxtTicketPrice.Text) || !decimal.TryParse(TxtTicketPrice.Text, out var price))
            {
                error = "Ticket Price must be a valid number.";
                return false;
            }
            if (price < 0)
            {
                error = "Ticket Price cannot be negative.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtBookingId.Text) || !int.TryParse(TxtBookingId.Text, out _))
            {
                error = "Booking ID must be a valid integer.";
                return false;
            }

            if (CmbSeat.SelectedItem == null)
            {
                error = "Please select an available seat.";
                return false;
            }

            return true;
        }

        private async void Btn_Add_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput(out var error)) { ShowError(error); return; }

            try
            {
                var nextId = await _ticketDomain.GetMaxTicketId() + 1;

                var selectedSeat = (Seat)((ComboBoxItem)CmbSeat.SelectedItem).Tag;
                var ticket = new Ticket
                {
                    TicketId = nextId,
                    TicketPrice = decimal.Parse(TxtTicketPrice.Text),
                    BookingId = int.Parse(TxtBookingId.Text),
                    AircraftId = selectedSeat.AircraftId,
                    SeatNumber = selectedSeat.SeatNumber
                };

                if (!string.IsNullOrWhiteSpace(TxtTripId.Text) && int.TryParse(TxtTripId.Text, out var tripId))
                    ticket.TripId = tripId;

                await _ticketDomain.AddTicket(ticket);
                ShowSuccess("Ticket created successfully!");
                ClearForm();
                await LoadAvailableSeats();
            }
            catch (Exception ex)
            {
                ShowError("Failed to add ticket: " + ex.Message);
            }
        }

        private async void Btn_Refresh_Click(object sender, RoutedEventArgs e) => await LoadAvailableSeats();
        private void Btn_Clear_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            TxtTicketId.Text = "";
            TxtTicketPrice.Text = "";
            TxtBookingId.Text = "";
            TxtTripId.Text = "";
            CmbAircraft.SelectedItem = null;
            CmbSeat.SelectedItem = null;
        }

        private void SeatsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SeatsListView.SelectedItem is Seat seat)
            {
                // Auto-select the aircraft and seat
                CmbAircraft.SelectedItem = _aircrafts.FirstOrDefault(a => a.AircraftId == seat.AircraftId);
                foreach (ComboBoxItem item in CmbSeat.Items)
                {
                    if (item.Tag is Seat s && s.SeatNumber == seat.SeatNumber && s.AircraftId == seat.AircraftId)
                    {
                        CmbSeat.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void ShowError(string msg) { StatusInfoBar.Message = msg; StatusInfoBar.Severity = InfoBarSeverity.Error; StatusInfoBar.IsOpen = true; }
        private void ShowSuccess(string msg) { StatusInfoBar.Message = msg; StatusInfoBar.Severity = InfoBarSeverity.Success; StatusInfoBar.IsOpen = true; }
    }
}
