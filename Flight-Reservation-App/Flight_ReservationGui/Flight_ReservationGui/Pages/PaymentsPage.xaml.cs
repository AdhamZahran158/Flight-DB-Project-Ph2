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
    public sealed partial class PaymentsPage : Page
    {
        private readonly PaymentDomain _paymentDomain = new PaymentDomain(Flight_Reservation_App.GlobalUsing.connectionString);
        private readonly BookingDomain _bookingDomain = new BookingDomain(Flight_Reservation_App.GlobalUsing.connectionString);
        private ObservableCollection<Payment> _payments = new();
        private bool _isEditMode = false;

        public PaymentsPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadBookings();
            await LoadPayments();
            SetAddMode();
        }

        private async Task LoadBookings()
        {
            try
            {
                var bookings = await _bookingDomain.GetBookings();
                CmbBooking.Items.Clear();
                foreach (var b in bookings)
                {
                    CmbBooking.Items.Add(new ComboBoxItem
                    {
                        Content = $"#{b.BookingId} — {b.BookingStatus} ({b.PassportNum})",
                        Tag = b.BookingId
                    });
                }
            }
            catch (Exception ex)
            {
                ShowError("Failed to load bookings: " + ex.Message);
            }
        }

        private async Task LoadPayments()
        {
            try
            {
                var list = await _paymentDomain.GetPaymentsAsync();
                _payments = new ObservableCollection<Payment>(list);
                PaymentsListView.ItemsSource = _payments;
                ShowSuccess($"Loaded {_payments.Count} payments.");
            }
            catch (Exception ex)
            {
                ShowError("Failed to load payments: " + ex.Message);
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

            if (CmbPaymentMethod.SelectedItem == null)
            {
                error = "Payment Method is required.";
                return false;
            }

            if (CmbPaymentStatus.SelectedItem == null)
            {
                error = "Payment Status is required.";
                return false;
            }

            if (CmbBooking.SelectedItem == null)
            {
                error = "Please select a booking.";
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
                var nextId = _payments.Count > 0 ? _payments.Max(p => p.PaymentId) + 1 : 1;
                var bookingId = (int)((ComboBoxItem)CmbBooking.SelectedItem).Tag;

                var payment = new Payment
                {
                    PaymentId = nextId,
                    PaymentMethod = (CmbPaymentMethod.SelectedItem as ComboBoxItem)?.Content?.ToString(),
                    PaymentStatus = (CmbPaymentStatus.SelectedItem as ComboBoxItem)?.Content?.ToString(),
                    BookingId = bookingId
                };

                await _paymentDomain.AddPaymentAsync(payment);
                ShowSuccess("Payment added successfully!");
                ClearForm();
                await LoadPayments();
            }
            catch (Exception ex)
            {
                ShowError("Failed to add payment: " + ex.Message);
            }
        }

        private async void Btn_Update_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditMode || string.IsNullOrWhiteSpace(TxtPaymentId.Text) || !int.TryParse(TxtPaymentId.Text, out var paymentId))
            {
                ShowError("Please select a payment from the list to update.");
                return;
            }

            if (CmbPaymentStatus.SelectedItem == null)
            {
                ShowError("Please select a new status.");
                return;
            }

            try
            {
                var newStatus = (CmbPaymentStatus.SelectedItem as ComboBoxItem)?.Content?.ToString();
                await _paymentDomain.UpdatePaymentStatusAsync(paymentId, newStatus!);
                ShowSuccess("Payment status updated!");
                await LoadPayments();
            }
            catch (Exception ex)
            {
                ShowError("Failed to update payment: " + ex.Message);
            }
        }

        private async void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditMode || string.IsNullOrWhiteSpace(TxtPaymentId.Text) || !int.TryParse(TxtPaymentId.Text, out var paymentId))
            {
                ShowError("Please select a payment from the list to delete.");
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Confirm Delete",
                Content = $"Delete payment #{paymentId}?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                try
                {
                    await _paymentDomain.DeletePaymentAsync(paymentId);
                    ShowSuccess("Payment deleted!");
                    ClearForm();
                    await LoadPayments();
                }
                catch (Exception ex)
                {
                    ShowError("Failed to delete payment: " + ex.Message);
                }
            }
        }

        private async void Btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadBookings();
            await LoadPayments();
        }

        private void Btn_Clear_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            TxtPaymentId.Text = "";
            CmbPaymentMethod.SelectedItem = null;
            CmbPaymentStatus.SelectedItem = null;
            CmbBooking.SelectedItem = null;
            PaymentsListView.SelectedItem = null;
            SetAddMode();
        }

        private void PaymentsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PaymentsListView.SelectedItem is Payment p)
            {
                TxtPaymentId.Text = p.PaymentId.ToString();

                // Select booking in dropdown
                foreach (ComboBoxItem item in CmbBooking.Items)
                {
                    if (item.Tag is int id && id == p.BookingId)
                    {
                        CmbBooking.SelectedItem = item;
                        break;
                    }
                }

                foreach (ComboBoxItem item in CmbPaymentMethod.Items)
                {
                    if (item.Content?.ToString() == p.PaymentMethod)
                    {
                        CmbPaymentMethod.SelectedItem = item;
                        break;
                    }
                }

                foreach (ComboBoxItem item in CmbPaymentStatus.Items)
                {
                    if (item.Content?.ToString() == p.PaymentStatus)
                    {
                        CmbPaymentStatus.SelectedItem = item;
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
