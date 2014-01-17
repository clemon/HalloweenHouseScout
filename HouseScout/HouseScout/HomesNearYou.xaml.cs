using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Device.Location;
using System.Diagnostics;
using System.Windows.Data;
using System.Xml.Linq;

namespace HouseScout
{
    public partial class HomesNearYou : PhoneApplicationPage
    {
        public HomesNearYou()
        {
            SystemTray.SetOpacity(this, 0);
            InitializeComponent();
            

            List<string> addresses = new List<string>();
            foreach (var item in App.listHouses.ToList())
            {
                addresses.Add(item.Address);
            }

            list.ItemsSource = addresses;
            
        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (list.SelectedItem != null)
            {
                string address = (string)list.SelectedItem;
                App.goHere = address;
 

                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                
                // Reset selected item to null
                list.SelectedItem = null;
            }

        }
    }
}