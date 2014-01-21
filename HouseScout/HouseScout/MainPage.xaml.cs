using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Device.Location;
using System.Diagnostics;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HouseScout.Resources;
using Microsoft.Phone.Maps;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Maps.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Maps.Services;
using System.Collections.ObjectModel;
using Microsoft.WindowsAzure.MobileServices;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;


namespace HouseScout
{
    public partial class MainPage : PhoneApplicationPage
    {
        private IsolatedStorageSettings userSettings = IsolatedStorageSettings.ApplicationSettings;
        MapLayer myLocationLayer = new MapLayer();
        Geolocator eyBBLocateMe;
        GeoCoordinate myLocation;
        bool first = true;

        // Constructor
        public MainPage()
        {
            SystemTray.SetOpacity(this, 0);
            InitializeComponent();

            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "f637ab6b-bb9a-418b-bf31-1ac6667894c4";
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "Wqr37yrpSvPWjzKdoHtaqw";

            Debug.WriteLine(checkConnection());
            /*if (checkConnection() == false)
            {
                Debug.WriteLine("if-statement");
                
                //NavigationService.Navigate(new Uri("/HomesNearYou.xaml", UriKind.Relative));
                //ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
                //connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.WiFi;
                //connectionSettingsTask.Show();

            }*/
            //else
            //{
                goToLocation();
                TrackLocation();
                PullFromDatabase();
            //}
            
            if (App.goHere != "")
            {
                ApplicationBarIconButton searchbtn = (ApplicationBarIconButton)ApplicationBar.Buttons[3];
                SearchBar.Text = App.goHere;
                App.goHere = "";
                SearchBar.Visibility = Visibility.Visible;
                GoButton.Visibility = Visibility.Visible;
                searchbtn.IconUri = new Uri("/Assets/AppBar/cancel.png", UriKind.Relative);
                searchbtn.Text = "cancel";
            }

        }

        // Purpose: Check if there is active data connection
        public static bool checkConnection()
        {
            return Microsoft.Phone.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }

        // Purpose: prompt the user to allow location services or not
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            //delete everything in the backstack
            while (this.NavigationService.BackStack.Any())
            {
                this.NavigationService.RemoveBackEntry();
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            {
                // User has opted in or out of Location
                return;
            }
            else
            {
                MessageBoxResult result =
                    MessageBox.Show("This app accesses your phone's location. Is that cool with you?",
                    "Location",
                    MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                else
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;

                IsolatedStorageSettings.ApplicationSettings.Save();
            }

            
        }

        // Purpose: set the map center to current location
        private async void goToLocation()
        {
            Geoposition geoposition;
            eyBBLocateMe = new Geolocator();
            eyBBLocateMe.DesiredAccuracyInMeters = 20;

            try
            {
                geoposition = await eyBBLocateMe.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                    );

                myLocation = new GeoCoordinate(geoposition.Coordinate.Latitude, geoposition.Coordinate.Longitude);
                if (first)
                {
                    myMap.Center = myLocation;
                    myMap.ZoomLevel = 17;
                    first = false;
                }
                else
                    myMap.SetView(myLocation, 17, 0, 0, MapAnimationKind.Parabolic);
                Debug.WriteLine("GOTOLOCATION: "+myLocation.Latitude + "    " + myLocation.Longitude);

            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    //location services disabled
                }
            }

        }

        //purpose: track location in real time
        private void TrackLocation()
        {
            RefreshProgressBar.IsIndeterminate = true;
            RefreshProgressBar.Visibility = Visibility.Visible;
            RefreshProgressBar.IsEnabled = true;

            eyBBLocateMe = new Geolocator();
            eyBBLocateMe.DesiredAccuracy = PositionAccuracy.High;
            eyBBLocateMe.MovementThreshold = 1; // The units are meters.

            eyBBLocateMe.PositionChanged += eyBBLocateMe_PositionChanged;


        }

        //event that your position has changed
        //purpose: set myLocation to the new geoCoordinate
        void eyBBLocateMe_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            myLocation = new GeoCoordinate(args.Position.Coordinate.Latitude, args.Position.Coordinate.Longitude);
            Debug.WriteLine("POSITIONCHANGED: "+myLocation.Latitude + "    " + myLocation.Longitude);
            ShowMyLocationOnTheMap(myLocation);
        }

        //purpose: show location on map with overlay
        private void ShowMyLocationOnTheMap(GeoCoordinate currentLocation)
        {
            //since this method is called in the PositionChanged event, any UI changes must be made in a separate thread
            Dispatcher.BeginInvoke(() =>
            {
                myMap.Layers.Remove(myLocationLayer);
                myLocationLayer = new MapLayer();

                Image me = new Image();
                me.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Assets/ghost.png", UriKind.Relative));

                // Create a MapOverlay to contain the circle.
                MapOverlay myLocationOverlay = new MapOverlay();
                myLocationOverlay.PositionOrigin = new Point(0.5, 0.5);
                myLocationOverlay.Content = me;
                myLocationOverlay.GeoCoordinate = currentLocation;
                
                // Create a MapLayer to contain the MapOverlay.
                //MapLayer myLocationLayer = new MapLayer();
                myLocationLayer.Add(myLocationOverlay);
                
                // Add the MapLayer to the Map.
                myMap.Layers.Add(myLocationLayer);

                RefreshProgressBar.IsIndeterminate = false;
                RefreshProgressBar.IsEnabled = false;
                RefreshProgressBar.Visibility = Visibility.Collapsed;
            });
        }

        //click event for the mapmode button
        //purpose: switches modes
        private void MapModeButton_Click(object sender, EventArgs e)
        {
            ApplicationBarIconButton btn = (ApplicationBarIconButton)ApplicationBar.Buttons[0];

            if (myMap.CartographicMode == MapCartographicMode.Road)
            {
                myMap.CartographicMode = MapCartographicMode.Aerial;
                btn.Text = "aerial";
                btn.IconUri = new Uri("/Assets/AppBar/appbar.map.aerial.png", UriKind.Relative);
            }
            else if (myMap.CartographicMode == MapCartographicMode.Aerial)
            {
                myMap.CartographicMode = MapCartographicMode.Hybrid;
                btn.Text = "hybrid";
                btn.IconUri = new Uri("/Assets/AppBar/appbar.map.satellite.png", UriKind.Relative);
            }
            else if (myMap.CartographicMode == MapCartographicMode.Hybrid)
            {
                myMap.CartographicMode = MapCartographicMode.Road;
                btn.Text = "road";
                btn.IconUri = new Uri("/Assets/AppBar/appbar.map.aerial.highway.png", UriKind.Relative);
            }

        }

        //click event for locate button
        //purpose: locate me
        private void LocationButton_Click(object sender, EventArgs e)
        {
            Geolocator locator = new Geolocator();

            if (locator.LocationStatus == PositionStatus.Disabled)
            {
                MessageBox.Show("Location Settings are disabled. Please enable them");
            }
            else
            {
                goToLocation();
                TrackLocation();
            }
        }

        //click event for search button
        //purpose: set searchbar and go button to visible
        private void SearchButton_Click(object sender, EventArgs e)
        {
            ApplicationBarIconButton searchbtn = (ApplicationBarIconButton)ApplicationBar.Buttons[3];

            if (SearchBar.Visibility == Visibility.Collapsed)
            {
                SearchBar.Visibility = Visibility.Visible;
                GoButton.Visibility = Visibility.Visible;
                searchbtn.IconUri = new Uri("/Assets/AppBar/cancel.png", UriKind.Relative);
                searchbtn.Text = "cancel";
            }
            else if (SearchBar.Visibility == Visibility.Visible)
            {
                SearchBar.Visibility = Visibility.Collapsed;
                GoButton.Visibility = Visibility.Collapsed;
                searchbtn.IconUri = new Uri("/Assets/AppBar/feature.search.png", UriKind.Relative);
                searchbtn.Text = "search";
            }
            
        }

        //click event for go button
        //purpose: go to address typed in searchbar
        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchBar.Text.Trim() == "")
            {
                // do nothing
            }
            else 
            {
                // Define search
                GeocodeQuery geoQuery = new GeocodeQuery();
                geoQuery.SearchTerm = SearchBar.Text;
                geoQuery.GeoCoordinate = new GeoCoordinate(myLocation.Latitude, myLocation.Longitude);
                geoQuery.QueryCompleted += (s, f) =>
                {
                    if (f.Error == null && f.Result.Count > 0)
                    {
                        // f.Result will contain a list of coordinates of matched places.
                        // You can show them on a map control , e.g.
                        myMap.SetView(f.Result[0].GeoCoordinate, 16, 0, 0, MapAnimationKind.Parabolic);
                    }
                };
                geoQuery.QueryAsync();            
            }
            
        }


        //onClick method for register your home option
        //purpose: navigate to registration page
        private void RegisterYourHome_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/RegistrationPage.xaml", UriKind.Relative));
        }

        //purpose: pull data from database, call addmarkers for each one
        private async void PullFromDatabase()
        {
            var allHouses = await App.MobileService.GetTable<HouseTable>().ToListAsync();

           
            foreach(var item in allHouses)
            {
                string address = item.Address;
                double lat = item.Latitude;
                double lon = item.Longitude;
                bool candy = item.Candy;
                Debug.WriteLine(address);
                AddMarkers(address, new GeoCoordinate(lat,lon), candy);
            }
            DrawMarkers();

        }

        private void PrivacyPolicy_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/PrivacyPolicyPage.xaml", UriKind.Relative));
        }

        //purpose: add a marker to the overlay
        private void AddMarkers(string address, GeoCoordinate coord, bool candy)
        {
            
            //Creating a Grid element.
            Grid MyGrid = new Grid();
            MyGrid.RowDefinitions.Add(new RowDefinition());
            MyGrid.RowDefinitions.Add(new RowDefinition());
            MyGrid.Background = new SolidColorBrush(Colors.Transparent);

            Image pic = new Image();

            //create and add marker to grid
            if (candy)
            {
                pic.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Assets/green-house-pin.png", UriKind.Relative));
            }
            else
            {
                pic.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Assets/red-house-pin.png", UriKind.Relative));
            }
            MyGrid.Children.Add(pic);

            //Creating a MapOverlay and adding the Grid to it.
            MapOverlay MyOverlay = new MapOverlay();
            MyOverlay.Content = MyGrid;
            MyOverlay.GeoCoordinate = coord;
            MyOverlay.PositionOrigin = new Point(0.5,1);

            MyLayer.Add(MyOverlay);
           
        }

        MapLayer MyLayer = new MapLayer();      //layer to be added to the map
        //purpose: add the layer to the map
        private void DrawMarkers()
        {
            myMap.Layers.Add(MyLayer);
        }

        //onClick method for refresh button
        //purpose: refresh house pins
        private void RefreshButton_Click(object sender, EventArgs e)
        {        
            RefreshProgressBar.IsIndeterminate = true;
            RefreshProgressBar.Visibility = Visibility.Visible;
            RefreshProgressBar.IsEnabled = true;

            if (checkConnection() == false)         // Checks for absense of connection
            {
                MessageBoxResult noConnectionError = MessageBox.Show("No Internet Connection Found. Cannot find registered homes.",
                    "Error", MessageBoxButton.OK);

            }
            else                                  // if connected, refresh
            {
                myMap.Layers.Remove(MyLayer);
                MyLayer = new MapLayer();
                PullFromDatabase();
            }

            RefreshProgressBar.IsIndeterminate = false;
            RefreshProgressBar.IsEnabled = false;
            RefreshProgressBar.Visibility = Visibility.Collapsed;
        
        }

        //onClick method for the homes near you button
        //purpose: bring up a listview of homes ranked from closest to furthest
        private async void NearYou_Click(object sender, EventArgs e)
        {
            
            TrackLocation();
     
            var allHouses = await App.MobileService.GetTable<HouseTable>().ToListAsync();
            App.HouseList = new List<HouseTable>(allHouses);
            SortedDictionary<double,HouseTable> HousesSortedByDist= new SortedDictionary<double,HouseTable>();
                   
            foreach (var item in allHouses)
            {
                
                string address = item.Address;
                double lat = item.Latitude;
                double lon = item.Longitude;
                bool candy = item.Candy;

                GeoCoordinate pin = new GeoCoordinate(lat, lon);
                double dist = myLocation.GetDistanceTo(pin);

                HousesSortedByDist.Add(dist,item);
                
            }

            App.listHouses = HousesSortedByDist.Values;
            
            NavigationService.Navigate(new Uri("/HomesNearYou.xaml", UriKind.Relative));

        }

        private async void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-location:"));
        }
        
    }
}