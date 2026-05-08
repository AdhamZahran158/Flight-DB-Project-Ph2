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
    public sealed partial class BookingsPage : Page
    {
        private readonly BookingDomain _bookingDomain = new BookingDomain(Flight_Reservation_App.GlobalUsing.connectionString);
        private readonly PassengerDomain _passengerDomain = new PassengerDomain(Flight_Reservation_App.GlobalUsing.connectionString);
        private ObservableCollection<Booking> _bookings = new();
        private bool _isEditMode = false;

        public BookingsPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPassengers();
            await LoadBookings();
            SetAddMode();
        }

        private async Task LoadPassengers()
        {
            try
            {
                var passengers = await _passengerDomain.GetPassengersAsync();
                CmbPassport.Items.Clear();
                foreach (var p in passengers)
                {
                    CmbPassport.Items.Add(new ComboBoxItem
                    {
                        Content = $"{p.PassportNum} — {p.Fname} {p.Lname}",
                        Tag = p.PassportNum
                    });
                }
            }
            catch (Exception ex)
            {
                ShowError("Failed to load passengers: " + ex.Message);
            }
        }

        private async Task LoadBookings()
        {
            try
            {
                var list = await _bookingDomain.GetBookings();
                _bookings = new ObservableCollection<Booking>(list);
                BookingsListView.ItemsSource = _bookings;
                ShowSuccess($"Loaded {_bookings.Count} bookings.");
            }
            catch (Exception ex)
            {
                ShowError("Failed to load bookings: " + ex.Message);
            }
        }

        private void SetAddMode()
        {
            _isEditMode = false;
            BtnAdd.IsEnabled = true;
            BtnUpdate.IsEnabled = false;
            BtnDelete.IsEnabled = false;
        }

        private void SetEditMode()
        {
            _isEditMode = true;
            BtnAdd.IsEnabled = false;
            BtnUpdate.IsEnabled = true;
            BtnDelete.IsEnabled = true;
        }

        private bool ValidateInput(out string error)
        {
            error = "";

            if (CmbBookingStatus.SelectedItem == null)
            {
                error = "Booking Status is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtTotalPrice.Text) || !decimal.TryParse(TxtTotalPrice.Text, out var price))
            {
                error = "Total Price must be a valid number.";
                return false;
            }

            if (price < 0)
            {
                error = "Total Price cannot be negative.";
                return false;
            }

            if (CmbPassport.SelectedItem == null)
            {
                error = "Please select a passenger.";
                return false;
            }

            return true;
        }

        private async void Btn_Add_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditMode)
            {
                ShowError("A record is selected. You can only Update or Delete it. Click Clear to add a new one.");
                return;
            }

            if (!ValidateInput(out var error)) { ShowError(error); return; }

            try
            {
                var nextId = _bookings.Count > 0 ? _bookings.Max(b => b.BookingId) + 1 : 1;

                var booking = new Booking
                {
                    BookingId = nextId,
                    BookingStatus = (CmbBookingStatus.SelectedItem as ComboBoxItem)?.Content?.ToString(),
                    TotalPrice = decimal.Parse(TxtTotalPrice.Text),
                    PassportNum = ((ComboBoxItem)CmbPassport.SelectedItem).Tag?.ToString()
                };

                await _bookingDomain.AddBooking(booking);
                ShowSuccess("Booking added successfully!");
                ClearForm();
                await LoadBookings();
            }
            catch (Exception ex)
            {
                ShowError("Failed to add booking: " + ex.Message);
            }
        }

        private async void Btn_Update_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditMode || string.IsNullOrWhiteSpace(TxtBookingId.Text) || !int.TryParse(TxtBookingId.Text, out var bookingId))
            {
                ShowError("Please select a booking from the list to update.");
                return;
            }

            if (CmbBookingStatus.SelectedItem == null)
            {
                ShowError("Please select a new status.");
                return;
            }

            try
            {
                var booking = new Booking
                {
                    BookingId = bookingId,
                    BookingStatus = (CmbBookingStatus.SelectedItem as ComboBoxItem)?.Content?.ToString()
                };

                await _bookingDomain.UpdateBooking(booking);
                ShowSuccess("Booking status updated!");
                await LoadBookings();
            }
            catch (Exception ex)
            {
                ShowError("Failed to update booking: " + ex.Message);
            }
        }

        private async void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditMode || string.IsNullOrWhiteSpace(TxtBookingId.Text) || !int.TryParse(TxtBookingId.Text, out var bookingId))
            {
                ShowError("Please select a booking from the list to delete.");
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Confirm Delete",
                Content = $"Delete booking #{bookingId}?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                try
                {
                    await _bookingDomain.DeleteBooking(bookingId);
                    ShowSuccess("Booking deleted!");
                    ClearForm();
                    await LoadBookings();
                }
                catch (Exception ex)
                {
                    ShowError("Failed to delete booking: " + ex.Message);
                }
            }
        }

        private async void Btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadPassengers();
            await LoadBookings();
        }

        private void Btn_Clear_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            TxtBookingId.Text = "";
            CmbBookingStatus.SelectedItem = null;
            TxtTotalPrice.Text = "";
            CmbPassport.SelectedItem = null;
            BookingsListView.SelectedItem = null;
            SetAddMode();
        }

        private void BookingsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BookingsListView.SelectedItem is Booking b)
            {
                TxtBookingId.Text = b.BookingId.ToString();
                TxtTotalPrice.Text = b.TotalPrice?.ToString() ?? "";

                foreach (ComboBoxItem item in CmbBookingStatus.Items)
                {
                    if (item.Content?.ToString() == b.BookingStatus)
                    {
                        CmbBookingStatus.SelectedItem = item;
                        break;
                    }
                }

                foreach (ComboBoxItem item in CmbPassport.Items)
                {
                    if (item.Tag?.ToString() == b.PassportNum)
                    {
                        CmbPassport.SelectedItem = item;
                        break;
                    }
                }

                SetEditMode();
            }
        }

        private void ShowError(string msg) { StatusInfoBar.Message = msg; StatusInfoBar.Severity = InfoBarSeverity.Error; StatusInfoBar.IsOpen = true; }
        private void ShowSuccess(string msg) { StatusInfoBar.Message = msg; StatusInfoBar.Severity = InfoBarSeverity.Success; StatusInfoBar.IsOpen = true; }
    }
}
