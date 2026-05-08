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
    public sealed partial class AircraftsPage : Page
    {
        private readonly AircraftDomain _aircraftDomain = new AircraftDomain();
        private ObservableCollection<Aircraft> _aircrafts = new();

        public AircraftsPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAircrafts();
            SetAddMode();
        }

        private void SetAddMode()
        {
            BtnAdd.IsEnabled = true;
            BtnUpdate.IsEnabled = false;
            BtnDelete.IsEnabled = false;
        }

        private void SetEditMode()
        {
            BtnAdd.IsEnabled = false;
            BtnUpdate.IsEnabled = true;
            BtnDelete.IsEnabled = true;
        }

        private async Task LoadAircrafts()
        {
            try
            {
                var aircrafts = await _aircraftDomain.GetAircrafts();
                _aircrafts = new ObservableCollection<Aircraft>(aircrafts);
                AircraftsListView.ItemsSource = _aircrafts;
                ShowSuccess($"Loaded {_aircrafts.Count} aircrafts.");
            }
            catch (Exception ex)
            {
                ShowError("Failed to load aircrafts: " + ex.Message);
            }
        }

        private bool ValidateInput(out string error, bool isAdd)
        {
            error = "";

            if (string.IsNullOrWhiteSpace(TxtModel.Text))
            {
                error = "Model is required.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(TxtCargoCapacity.Text) && !decimal.TryParse(TxtCargoCapacity.Text, out _))
            {
                error = "Cargo Capacity must be a valid number.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(TxtMaxTakeOffWeight.Text) && !decimal.TryParse(TxtMaxTakeOffWeight.Text, out _))
            {
                error = "Max Takeoff Weight must be a valid number.";
                return false;
            }

            if (isAdd)
            {
                if (string.IsNullOrWhiteSpace(TxtEconomySeats.Text) || !int.TryParse(TxtEconomySeats.Text, out var eco) || eco < 0)
                {
                    error = "Economy Seats must be a valid non-negative integer.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(TxtBusinessSeats.Text) || !int.TryParse(TxtBusinessSeats.Text, out var bus) || bus < 0)
                {
                    error = "Business Seats must be a valid non-negative integer.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(TxtFirstClassSeats.Text) || !int.TryParse(TxtFirstClassSeats.Text, out var fc) || fc < 0)
                {
                    error = "First Class Seats must be a valid non-negative integer.";
                    return false;
                }
                if (eco + bus + fc == 0)
                {
                    error = "Aircraft must have at least one seat.";
                    return false;
                }
            }

            return true;
        }

        private async void Btn_Add_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtAircraftId.Text))
            {
                ShowError("A record is selected. You can only Update or Delete it. Click Clear to add a new one.");
                return;
            }

            if (!ValidateInput(out var error, isAdd: true))
            {
                ShowError(error);
                return;
            }

            try
            {
                var aircraft = new Aircraft
                {
                    Model = TxtModel.Text.Trim()
                };

                if (decimal.TryParse(TxtCargoCapacity.Text, out var cargo))
                    aircraft.CargoCapacity = cargo;

                if (decimal.TryParse(TxtMaxTakeOffWeight.Text, out var weight))
                    aircraft.MaxTakeOffWeight = weight;

                int ecoSeats = int.Parse(TxtEconomySeats.Text);
                int busSeats = int.Parse(TxtBusinessSeats.Text);
                int fcSeats = int.Parse(TxtFirstClassSeats.Text);

                await _aircraftDomain.AddAircraft(aircraft, ecoSeats, busSeats, fcSeats);
                ShowSuccess("Aircraft added successfully with seats configured!");
                ClearForm();
                await LoadAircrafts();
            }
            catch (Exception ex)
            {
                ShowError("Failed to add aircraft: " + ex.Message);
            }
        }

        private async void Btn_Update_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtAircraftId.Text) || !int.TryParse(TxtAircraftId.Text, out var aircraftId))
            {
                ShowError("Please select an aircraft from the list to update.");
                return;
            }

            if (!ValidateInput(out var error, isAdd: false))
            {
                ShowError(error);
                return;
            }

            try
            {
                var aircraft = new Aircraft
                {
                    AircraftID = aircraftId,
                    Model = TxtModel.Text.Trim()
                };

                if (decimal.TryParse(TxtCargoCapacity.Text, out var cargo))
                    aircraft.CargoCapacity = cargo;

                if (decimal.TryParse(TxtMaxTakeOffWeight.Text, out var weight))
                    aircraft.MaxTakeOffWeight = weight;

                await _aircraftDomain.UpdateAircraft(aircraft);
                ShowSuccess("Aircraft updated successfully!");
                await LoadAircrafts();
            }
            catch (Exception ex)
            {
                ShowError("Failed to update aircraft: " + ex.Message);
            }
        }

        private async void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtAircraftId.Text) || !int.TryParse(TxtAircraftId.Text, out var aircraftId))
            {
                ShowError("Please select an aircraft from the list to delete.");
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Confirm Delete",
                Content = $"Are you sure you want to delete Aircraft #{aircraftId}? This will also delete all associated seats.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                try
                {
                    await _aircraftDomain.DeleteAircraft(aircraftId);
                    ShowSuccess("Aircraft deleted successfully!");
                    ClearForm();
                    await LoadAircrafts();
                }
                catch (Exception ex)
                {
                    ShowError("Failed to delete aircraft: " + ex.Message);
                }
            }
        }

        private async void Btn_Refresh_Click(object sender, RoutedEventArgs e) => await LoadAircrafts();
        private void Btn_Clear_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            TxtAircraftId.Text = "";
            TxtModel.Text = "";
            TxtCargoCapacity.Text = "";
            TxtMaxTakeOffWeight.Text = "";
            TxtEconomySeats.Text = "";
            TxtBusinessSeats.Text = "";
            TxtFirstClassSeats.Text = "";
            SeatConfigPanel.Visibility = Visibility.Visible;
            AircraftsListView.SelectedItem = null;
            SetAddMode();
        }

        private void AircraftsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AircraftsListView.SelectedItem is Aircraft a)
            {
                TxtAircraftId.Text = a.AircraftID.ToString();
                TxtModel.Text = a.Model ?? "";
                TxtCargoCapacity.Text = a.CargoCapacity?.ToString() ?? "";
                TxtMaxTakeOffWeight.Text = a.MaxTakeOffWeight?.ToString() ?? "";

                // Hide seat config when editing (seats can't be changed via update)
                SeatConfigPanel.Visibility = Visibility.Collapsed;
                SetEditMode();
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
