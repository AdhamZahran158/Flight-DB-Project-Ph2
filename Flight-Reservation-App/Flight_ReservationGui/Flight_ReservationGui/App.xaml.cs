using Microsoft.UI.Xaml;
using System;

namespace Flight_ReservationGui
{
    public partial class App : Application
    {
        private Window? _window;

        public App()
        {
            InitializeComponent();
            UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            System.Diagnostics.Debug.WriteLine("UNHANDLED EXCEPTION: " + e.Exception?.ToString());
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                _window = new MainWindow();
                _window.Activate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LAUNCH ERROR: " + ex.ToString());
                // Show a basic error window
                _window = new Window();
                _window.Title = "Error";
                _window.Content = new Microsoft.UI.Xaml.Controls.TextBlock
                {
                    Text = "Launch Error:\n" + ex.Message + "\n\n" + ex.StackTrace,
                    TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                    Margin = new Thickness(20),
                    IsTextSelectionEnabled = true
                };
                _window.Activate();
            }
        }
    }
}
