using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Diagnostics;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Services;
using System.Device.Location;
using Microsoft.WindowsAzure.MobileServices;

namespace HouseScout
{
    public partial class RegistrationPage : PhoneApplicationPage
    {
        public RegistrationPage()
        {
            InitializeComponent();
            InitializeListPickers();
            SystemTray.SetOpacity(this, 0);
           
        }

        //purpose: populate the listPickers
        private void InitializeListPickers()
        {
            String[] states = {"State","AL","AK","AZ","AR","CA","CO","CT","DE","FL","GA","HI",
                              "ID","IL","IN","IA","KS","KY","LA","ME","MD","MA","MI",
                              "MN","MS","MO","MT","NE","NV","NH","NJ","NM","NY","NC",
                              "ND","OH","OK","OR","PA","RI","SC","SD","TN","TX","UT",
                              "VT","VA","WA","WV","WI","WY"};
            StateList.ItemsSource = states;
            CandyToggle.ItemsSource = new String[] {"Yes", "No"};
        }

        //click event method for submit button
        //purpose: checks if the address is valid
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)StateList.SelectedItem == "State") //no state selected
            { 
                //error message
                MessageBoxResult StateError = MessageBox.Show("Please Select a State", "Error", MessageBoxButton.OK); 
            }
            else if (StreetAddressBar.Text == "") //no street address entered
            {
                MessageBoxResult StreetAddressError = MessageBox.Show("Please enter an Address", "Error", MessageBoxButton.OK);
            }
            else if (CityBar.Text == "")         //no city entered
            {
                MessageBoxResult CityError = MessageBox.Show("Please enter a City", "Error", MessageBoxButton.OK);
            }
            else if (ZipCodeBar.Text == "")     //no zipcode entered
            {
                MessageBoxResult ZipCodeError = MessageBox.Show("Please enter a Zip Code", "Error", MessageBoxButton.OK);
            }
            else
            {

                string address = StreetAddressBar.Text + ", " + CityBar.Text + ", " + StateList.SelectedItem.ToString() + ", " + ZipCodeBar.Text;
                Debug.WriteLine(address);

                // Define search
                GeocodeQuery geoQuery = new GeocodeQuery();
                geoQuery.SearchTerm = address;
                geoQuery.GeoCoordinate = new GeoCoordinate();
                geoQuery.QueryCompleted += (s, f) =>
                {
                    if (f.Error != null || f.Result.Count <= 0)     //invalid address
                    {
                        MessageBoxResult InvalidAddress = MessageBox.Show("Invalid Address. Please try again", "Error", MessageBoxButton.OK);
                    }
                    else 
                    {
                        PostHouseData(address, f.Result[0].GeoCoordinate.Latitude, f.Result[0].GeoCoordinate.Longitude);
                    }

                };
                geoQuery.QueryAsync();

            }
        }

        //purpose: post the registered house's data to the server
        private async void PostHouseData(string address, double lat, double lon)
        {
            SubmitProgressBar.IsEnabled = true;             //enable the progress bar
            SubmitProgressBar.IsIndeterminate = true;
            SubmitProgressBar.Visibility = Visibility.Visible;

            //candy???
            bool CandyYes;
            if (CandyToggle.SelectedItem.ToString() == "Yes")
                CandyYes = true;  //yes
            else
                CandyYes = false;  //no

            //new House object to be sent to the database
            HouseTable regHouse = new HouseTable { Address = address.ToUpper(), Latitude = lat, Longitude = lon, Candy = CandyYes };

            //make list of lat and lon doubles
            var allHouses = await App.MobileService.GetTable<HouseTable>().ToListAsync();
            List<double> latList = new List<double>();
            List<double> lonList = new List<double>();
            foreach (var item in allHouses)
            {
                latList.Add(item.Latitude);
                lonList.Add(item.Longitude);
            }

            //dont post to database if the address is already registered
            if (latList.Contains(regHouse.Latitude) && lonList.Contains(regHouse.Longitude))
            {
                MessageBoxResult StateError = MessageBox.Show("This Address has already been registered", "Error", MessageBoxButton.OK);

                SubmitProgressBar.IsEnabled = false;         //disable the progress bar
                SubmitProgressBar.IsIndeterminate = false;
                SubmitProgressBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                await App.MobileService.GetTable<HouseTable>().InsertAsync(regHouse);   //attempt to send

                SubmitProgressBar.IsEnabled = false;         //disable the progress bar
                SubmitProgressBar.IsIndeterminate = false;
                SubmitProgressBar.Visibility = Visibility.Collapsed;

                NavigationService.Navigate(new Uri("/RegisterationDone.xaml", UriKind.Relative));
            }         
        }
    }
}