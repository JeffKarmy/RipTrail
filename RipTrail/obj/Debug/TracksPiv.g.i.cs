﻿#pragma checksum "D:\Users\jrkarmy\Documents\Visual Studio 2013\Projects\RipTrail\RipTrail\TracksPiv.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "C0C4776FB133765497BBBE3DA0285B89"
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
    
    
    public partial class TracksPiv : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal Microsoft.Phone.Controls.Pivot pivTrack;
        
        internal System.Windows.Controls.TextBlock txtOdometer;
        
        internal System.Windows.Controls.TextBlock txtOdoUnit;
        
        internal System.Windows.Controls.TextBlock txtTimer;
        
        internal System.Windows.Controls.TextBlock txtRecord;
        
        internal System.Windows.Controls.TextBlock txtAltitude;
        
        internal System.Windows.Controls.TextBlock txtAltUnit;
        
        internal System.Windows.Controls.TextBlock txtMAXAltitude;
        
        internal System.Windows.Controls.TextBlock txtMAXAltUnit;
        
        internal System.Windows.Controls.TextBlock txtSpeed;
        
        internal System.Windows.Controls.TextBlock txtSpdUnit;
        
        internal System.Windows.Controls.TextBlock txtAVGSpd;
        
        internal System.Windows.Controls.TextBlock txtAVGSpdUnit;
        
        internal System.Windows.Controls.TextBlock txtMAXSpd;
        
        internal System.Windows.Controls.TextBlock txtMAXSpdUnit;
        
        internal System.Windows.Controls.TextBlock txtDistance;
        
        internal System.Windows.Controls.TextBlock txtDistUnit;
        
        internal System.Windows.Controls.TextBlock txtStartTime;
        
        internal System.Windows.Controls.TextBlock txtTotalTime;
        
        internal System.Windows.Controls.Grid ContentPanelLoadTracks;
        
        internal System.Windows.Controls.ListBox tracksListBox;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/RipTrail;component/TracksPiv.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.pivTrack = ((Microsoft.Phone.Controls.Pivot)(this.FindName("pivTrack")));
            this.txtOdometer = ((System.Windows.Controls.TextBlock)(this.FindName("txtOdometer")));
            this.txtOdoUnit = ((System.Windows.Controls.TextBlock)(this.FindName("txtOdoUnit")));
            this.txtTimer = ((System.Windows.Controls.TextBlock)(this.FindName("txtTimer")));
            this.txtRecord = ((System.Windows.Controls.TextBlock)(this.FindName("txtRecord")));
            this.txtAltitude = ((System.Windows.Controls.TextBlock)(this.FindName("txtAltitude")));
            this.txtAltUnit = ((System.Windows.Controls.TextBlock)(this.FindName("txtAltUnit")));
            this.txtMAXAltitude = ((System.Windows.Controls.TextBlock)(this.FindName("txtMAXAltitude")));
            this.txtMAXAltUnit = ((System.Windows.Controls.TextBlock)(this.FindName("txtMAXAltUnit")));
            this.txtSpeed = ((System.Windows.Controls.TextBlock)(this.FindName("txtSpeed")));
            this.txtSpdUnit = ((System.Windows.Controls.TextBlock)(this.FindName("txtSpdUnit")));
            this.txtAVGSpd = ((System.Windows.Controls.TextBlock)(this.FindName("txtAVGSpd")));
            this.txtAVGSpdUnit = ((System.Windows.Controls.TextBlock)(this.FindName("txtAVGSpdUnit")));
            this.txtMAXSpd = ((System.Windows.Controls.TextBlock)(this.FindName("txtMAXSpd")));
            this.txtMAXSpdUnit = ((System.Windows.Controls.TextBlock)(this.FindName("txtMAXSpdUnit")));
            this.txtDistance = ((System.Windows.Controls.TextBlock)(this.FindName("txtDistance")));
            this.txtDistUnit = ((System.Windows.Controls.TextBlock)(this.FindName("txtDistUnit")));
            this.txtStartTime = ((System.Windows.Controls.TextBlock)(this.FindName("txtStartTime")));
            this.txtTotalTime = ((System.Windows.Controls.TextBlock)(this.FindName("txtTotalTime")));
            this.ContentPanelLoadTracks = ((System.Windows.Controls.Grid)(this.FindName("ContentPanelLoadTracks")));
            this.tracksListBox = ((System.Windows.Controls.ListBox)(this.FindName("tracksListBox")));
        }
    }
}

