﻿#pragma checksum "D:\Users\jrkarmy\Documents\Visual Studio 2013\Projects\RipTrail\RipTrail\About.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "8B7BBBD2BD1B9E78D7D9B31B1C1B82E6"
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
    
    
    public partial class About : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.Button btnFeedBack;
        
        internal System.Windows.Controls.TextBlock txtVersion;
        
        internal System.Windows.Controls.TextBlock txtCopyRight;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/RipTrail;component/About.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.btnFeedBack = ((System.Windows.Controls.Button)(this.FindName("btnFeedBack")));
            this.txtVersion = ((System.Windows.Controls.TextBlock)(this.FindName("txtVersion")));
            this.txtCopyRight = ((System.Windows.Controls.TextBlock)(this.FindName("txtCopyRight")));
        }
    }
}

