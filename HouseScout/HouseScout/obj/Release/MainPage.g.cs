﻿#pragma checksum "D:\Shared\Programming\Visual Studio Projects\HalloweenHouseScout\HouseScout\HouseScout\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "E6CD4F9D2354F3C755DABFD9CC1EF30D"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace HouseScout {
    
    
    public partial class MainPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.StackPanel TitlePanel;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal Microsoft.Phone.Maps.Controls.Map myMap;
        
        internal System.Windows.Controls.WatermarkTextBox SearchBar;
        
        internal System.Windows.Controls.Button GoButton;
        
        internal System.Windows.Controls.ProgressBar RefreshProgressBar;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/HouseScout;component/MainPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.TitlePanel = ((System.Windows.Controls.StackPanel)(this.FindName("TitlePanel")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.myMap = ((Microsoft.Phone.Maps.Controls.Map)(this.FindName("myMap")));
            this.SearchBar = ((System.Windows.Controls.WatermarkTextBox)(this.FindName("SearchBar")));
            this.GoButton = ((System.Windows.Controls.Button)(this.FindName("GoButton")));
            this.RefreshProgressBar = ((System.Windows.Controls.ProgressBar)(this.FindName("RefreshProgressBar")));
        }
    }
}

