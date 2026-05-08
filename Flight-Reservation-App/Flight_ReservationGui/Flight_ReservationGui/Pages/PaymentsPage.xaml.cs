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
        private ObservableCollection<Payment> _payments = new();

        public PaymentsPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPayments();
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

        private bool ValidateInput(out string error)
        {
            error = "";

            // Payment ID is now auto-generated, so we skip validating it
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

            if (string.IsNullOrWhiteSpace(TxtBookingId.Text) || !int.TryParse(TxtBookingId.Text, out _))
            {
                error = "Booking ID must be a valid integer.";
                return false;
            }

            return true;
        }

        private async void Btn_Add_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput(out var error)) { ShowError(error); return; }

            try
            {
                var nextId = _payments.Count > 0 ? _payments.Max(p => p.PaymentId) + 1 : 1;

                var payment = new Payment
                {
                    PaymentId = nextId,
                    PaymentMethod = (CmbPaymentMethod.SelectedItem as ComboBoxItem)?.Content?.ToString(),
                    PaymentStatus = (CmbPaymentStatus.SelectedItem as ComboBoxItem)?.Content?.ToString(),
                    BookingId = int.Parse(TxtBookingId.Text)
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
            if (string.IsNullOrWhiteSpace(TxtPaymentId.Text) || !int.TryParse(TxtPaymentId.Text, out var paymentId))
            {
                ShowError("Please select a payment to update.");
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
            if (string.IsNullOrWhiteSpace(TxtPaymentId.Text) || !int.TryParse(TxtPaymentId.Text, out var paymentId))
            {
                ShowError("Please select a payment to delete.");
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

        private async void Btn_Refresh_Click(object sender, RoutedEventArgs e) => await LoadPayments();
        private void Btn_Clear_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            TxtPaymentId.Text = "";
            CmbPaymentMethod.SelectedItem = null;
            CmbPaymentStatus.SelectedItem = null;
            TxtBookingId.Text = "";
            PaymentsListView.SelectedItem = null;
        }

        private void PaymentsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PaymentsListView.SelectedItem is Payment p)
            {
                TxtPaymentId.Text = p.PaymentId.ToString();
                TxtBookingId.Text = p.BookingId?.ToString() ?? "";

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
            }
        }

        private void ShowError(string msg) { StatusInfoBar.Message = msg; StatusInfoBar.Severity = InfoBarSeverity.Error; StatusInfoBar.IsOpen = true; }
        private void ShowSuccess(string msg) { StatusInfoBar.Message = msg; StatusInfoBar.Severity = InfoBarSeverity.Success; StatusInfoBar.IsOpen = true; }
    }
}
