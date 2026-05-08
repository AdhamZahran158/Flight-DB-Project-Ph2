using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Flight_Reservation_App.Models;
using Flight_Reservation_App.Services;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flight_ReservationGui.Pages
{
    public sealed partial class PassengersPage : Page
    {
        private readonly PassengerDomain _passengerDomain = new PassengerDomain(Flight_Reservation_App.GlobalUsing.connectionString);
        private ObservableCollection<Tenant> _passengers = new();
        private bool _isEditMode = false;

        public PassengersPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPassengers();
        }

        private async Task LoadPassengers()
        {
            try
            {
                var list = await _passengerDomain.GetPassengersAsync();
                _passengers = new ObservableCollection<Tenant>(list);
                PassengersListView.ItemsSource = _passengers;
                ShowSuccess($"Loaded {_passengers.Count} passengers.");
            }
            catch (Exception ex)
            {
                ShowError("Failed to load passengers: " + ex.Message);
            }
        }

        private bool ValidateInput(out string error)
        {
            error = "";

            if (string.IsNullOrWhiteSpace(TxtPassport.Text))
            {
                error = "Passport Number is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtFname.Text))
            {
                error = "First Name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtLname.Text))
            {
                error = "Last Name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtEmail.Text))
            {
                error = "Email is required.";
                return false;
            }

            // Email format validation
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(TxtEmail.Text.Trim(), emailPattern))
            {
                error = "Please enter a valid email address (e.g. user@example.com).";
                return false;
            }

            return true;
        }

        private Tenant BuildTenantFromForm()
        {
            return new Tenant
            {
                PassportNum = TxtPassport.Text.Trim(),
                Fname = TxtFname.Text.Trim(),
                Mname = string.IsNullOrWhiteSpace(TxtMname.Text) ? null : TxtMname.Text.Trim(),
                Lname = TxtLname.Text.Trim(),
                Email = TxtEmail.Text.Trim(),
                NationalId = string.IsNullOrWhiteSpace(TxtNationalId.Text) ? null : TxtNationalId.Text.Trim(),
                Nationality = string.IsNullOrWhiteSpace(TxtNationality.Text) ? null : TxtNationality.Text.Trim()
            };
        }

        private async void Btn_Add_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput(out var error)) { ShowError(error); return; }

            try
            {
                var tenant = BuildTenantFromForm();
                await _passengerDomain.AddPassengerAsync(tenant);
                ShowSuccess("Passenger added successfully!");
                ClearForm();
                await LoadPassengers();
            }
            catch (Exception ex)
            {
                ShowError("Failed to add passenger: " + ex.Message);
            }
        }

        private async void Btn_Update_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditMode)
            {
                ShowError("Please select a passenger from the list to update.");
                return;
            }
            if (!ValidateInput(out var error)) { ShowError(error); return; }

            try
            {
                var tenant = BuildTenantFromForm();
                await _passengerDomain.UpdatePassengerAsync(tenant);
                ShowSuccess("Passenger updated successfully!");
                await LoadPassengers();
            }
            catch (Exception ex)
            {
                ShowError("Failed to update passenger: " + ex.Message);
            }
        }

        private async void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtPassport.Text))
            {
                ShowError("Please select a passenger to delete.");
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Confirm Delete",
                Content = $"Delete passenger with passport '{TxtPassport.Text}'?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                try
                {
                    await _passengerDomain.DeletePassengerAsync(TxtPassport.Text.Trim());
                    ShowSuccess("Passenger deleted successfully!");
                    ClearForm();
                    await LoadPassengers();
                }
                catch (Exception ex)
                {
                    ShowError("Failed to delete passenger: " + ex.Message);
                }
            }
        }

        private async void Btn_Refresh_Click(object sender, RoutedEventArgs e) => await LoadPassengers();

        private void Btn_Clear_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            TxtPassport.Text = "";
            TxtFname.Text = "";
            TxtMname.Text = "";
            TxtLname.Text = "";
            TxtEmail.Text = "";
            TxtNationalId.Text = "";
            TxtNationality.Text = "";
            TxtPassport.IsEnabled = true;
            _isEditMode = false;
            PassengersListView.SelectedItem = null;
        }

        private void PassengersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PassengersListView.SelectedItem is Tenant t)
            {
                TxtPassport.Text = t.PassportNum ?? "";
                TxtFname.Text = t.Fname ?? "";
                TxtMname.Text = t.Mname ?? "";
                TxtLname.Text = t.Lname ?? "";
                TxtEmail.Text = t.Email ?? "";
                TxtNationalId.Text = t.NationalId ?? "";
                TxtNationality.Text = t.Nationality ?? "";
                TxtPassport.IsEnabled = false; // PK — can't change on update
                _isEditMode = true;
            }
        }

        private void ShowError(string msg)
        {
            StatusInfoBar.Message = msg;
            StatusInfoBar.Severity = InfoBarSeverity.Error;
            StatusInfoBar.IsOpen = true;
        }

        private void ShowSuccess(string msg)
        {
            StatusInfoBar.Message = msg;
            StatusInfoBar.Severity = InfoBarSeverity.Success;
            StatusInfoBar.IsOpen = true;
        }
    }
}
