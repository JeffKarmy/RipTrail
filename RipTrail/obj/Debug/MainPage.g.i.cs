﻿#pragma checksum "D:\Users\jrkarmy\Documents\Visual Studio 2013\Projects\RipTrail\RipTrail\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "7D603AD99A6CDC8294C857824F122424"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
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


namespace RipTrail {
    
    
    public partial class MainPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.Grid TopPanel;
        
        internal System.Windows.Media.Animation.Storyboard sbTopPanelShow;
        
        internal System.Windows.Media.Animation.Storyboard sbTopPanelHide;
        
        internal System.Windows.Controls.TextBlock txtDistTitle;
        
        internal System.Windows.Controls.TextBlock txtDistance;
        
        internal System.Windows.Controls.TextBlock txtSpeedTitle;
        
        internal System.Windows.Controls.TextBlock txtSpeed;
        
        internal System.Windows.Controls.TextBlock txtSpeedUnit;
        
        internal System.Windows.Controls.TextBlock txtDistanceUnit;
        
        internal System.Windows.Controls.TextBlock txtAltitude;
        
        internal System.Windows.Controls.TextBlock txtAltUnit;
        
        internal System.Windows.Controls.Image imgCompass;
        
        internal System.Windows.Controls.TextBlock txtHeading;
        
        internal System.Windows.Controls.TextBlock txtDegree;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/RipTrail;component/MainPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.TopPanel = ((System.Windows.Controls.Grid)(this.FindName("TopPanel")));
            this.sbTopPanelShow = ((System.Windows.Media.Animation.Storyboard)(this.FindName("sbTopPanelShow")));
            this.sbTopPanelHide = ((System.Windows.Media.Animation.Storyboard)(this.FindName("sbTopPanelHide")));
            this.txtDistTitle = ((System.Windows.Controls.TextBlock)(this.FindName("txtDistTitle")));
            this.txtDistance = ((System.Windows.Controls.TextBlock)(this.FindName("txtDistance")));
            this.txtSpeedTitle = ((System.Windows.Controls.TextBlock)(this.FindName("txtSpeedTitle")));
            this.txtSpeed = ((System.Windows.Controls.TextBlock)(this.FindName("txtSpeed")));
            this.txtSpeedUnit = ((System.Windows.Controls.TextBlock)(this.FindName("txtSpeedUnit")));
            this.txtDistanceUnit = ((System.Windows.Controls.TextBlock)(this.FindName("txtDistanceUnit")));
            this.txtAltitude = ((System.Windows.Controls.TextBlock)(this.FindName("txtAltitude")));
            this.txtAltUnit = ((System.Windows.Controls.TextBlock)(this.FindName("txtAltUnit")));
            this.imgCompass = ((System.Windows.Controls.Image)(this.FindName("imgCompass")));
            this.txtHeading = ((System.Windows.Controls.TextBlock)(this.FindName("txtHeading")));
            this.txtDegree = ((System.Windows.Controls.TextBlock)(this.FindName("txtDegree")));
        }
    }
}

