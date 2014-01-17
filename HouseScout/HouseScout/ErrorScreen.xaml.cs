using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Devices.Geolocation;
using System.Diagnostics;
using Microsoft.Phone.Tasks;

namespace HouseScout
{
    public partial class ErrorScreen : PhoneApplicationPage
    {

        public ErrorScreen()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Geolocator locator = new Geolocator();

            NavigationService.RemoveBackEntry();

            if (locator.LocationStatus == PositionStatus.Disabled && checkConnection() == false)
            {
                // Location and wifi off
                locText.Visibility = Visibility.Visible;
                dataText.Visibility = Visibility.Visible;

                wifiSettingsButton.Visibility = Visibility.Visible;
                cellularSettingsButton.Visibility = Visibility.Visible;
                locationSettingsButton.Visibility = Visibility.Visible;
            }
            else if (locator.LocationStatus == PositionStatus.Disabled)
            {
                // Location is turned off; Wifi is on
                locText.Visibility = Visibility.Visible;
                dataText.Visibility = Visibility.Collapsed;

                wifiSettingsButton.Visibility = Visibility.Collapsed;
                cellularSettingsButton.Visibility = Visibility.Collapsed;
                locationSettingsButton.Visibility = Visibility.Visible;
            }
            else if (checkConnection() == false)
            {
                // WiFi is off; Location is on
                locText.Visibility = Visibility.Collapsed;
                dataText.Visibility = Visibility.Visible;

                wifiSettingsButton.Visibility = Visibility.Visible;
                cellularSettingsButton.Visibility = Visibility.Visible;
                locationSettingsButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                NavigationService.Navigate(new Uri("/StartPage.xaml", UriKind.Relative));
            }
        }

        // checks for a data connection
        public static bool checkConnection()
        {
            return Microsoft.Phone.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }

        private void wifiSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
            connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.WiFi;
            connectionSettingsTask.Show();
        }

        private void cellularSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
            connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.Cellular;
            connectionSettingsTask.Show();
        }

        private async void locationSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-location:"));
        }

        private void PrivPolicyButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/PrivacyPolicyPage.xaml", UriKind.Relative));
        }

    }
}