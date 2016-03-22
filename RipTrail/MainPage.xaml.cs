using System;
using System.Linq;
using System.Windows;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Toolkit;
using Microsoft.Phone.Shell;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Maps.Controls;
using System.Windows.Media;
using System.Net;
using System.Collections.Generic;
using System.Device.Location;
using Microsoft.Phone.Info;

using RipTrail.ViewModel;
using RipTrail.Resources;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace RipTrail
{
    public partial class MainPage : PhoneApplicationPage
    {
        private readonly LocationManager _loc;
        private readonly MiscFunctions _miscFunctions;
        private readonly UnitConversions _unitConversions;
        private ApplicationBarMenuItem _recordTrackBarMenuItem;
        private int _mapUnitIndex;
        private readonly ProgressIndicator _prog;
        private readonly RotateTransform _transformCompass;

        // Constructor
        public MainPage()
        {           
            InitializeComponent();
            BuildLocalizedApplicationBar();

            // Initialize objects
            _loc = new LocationManager();
            _miscFunctions = new MiscFunctions();
            _unitConversions = new UnitConversions();
            _mapUnitIndex = App.UserSettings.GetSetting(AppResources.UserSettingsMapUnits, 0);
            _prog = new ProgressIndicator();

            SystemTray.SetIsVisible(this, true);            
            SystemTray.SetProgressIndicator(this, _prog);

            MapUnits(_mapUnitIndex);                        

            if (Compass.IsSupported)
            {
                App.DispatcherTimer.Tick += DispatcherTimer_Tick;
                App.DispatcherTimer.Start();
                _transformCompass = new RotateTransform();                
                imgCompass.Visibility = Visibility.Visible;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (App.GoToPoint)
            {
                imgCompass.RenderTransformOrigin = new Point(0.5, 0.5);
                _transformCompass.Angle = _unitConversions.Bearing(App.GeoCoordinate.Latitude, App.GeoCoordinate.Longitude, App.Pin.GeoCoordinate.Latitude, App.Pin.GeoCoordinate.Longitude) - App.TrueHeading;
                imgCompass.RenderTransform = _transformCompass;
                txtHeading.Text = (_unitConversions.Bearing(App.GeoCoordinate.Latitude, App.GeoCoordinate.Longitude, App.Pin.GeoCoordinate.Latitude, App.Pin.GeoCoordinate.Longitude) - App.TrueHeading).ToString("000");               
            }
            else
            {
                imgCompass.RenderTransformOrigin = new Point(0.5, 0.5);
                _transformCompass.Angle = 360 - App.TrueHeading;
                imgCompass.RenderTransform = _transformCompass;
                txtHeading.Text = (360 - App.TrueHeading).ToString("000");              
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="isVisible"></param>
        /// <param name="text"></param>
        private void SetProgressIndicator(bool isVisible, string text)
        {
            _prog.IsIndeterminate = isVisible;
            _prog.IsVisible = isVisible;
            _prog.Text = text;
        }


        #region "EVENT HANDLERS"

        /// <summary>
        /// Poisition changed event handler, executes code when position changes and updates map.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            Dispatcher.BeginInvoke(() =>
            {
                // Update global GeoCoordinate.
                App.GeoCoordinate = _loc.ConvertGeocoordinate(args.Position.Coordinate);

                // If position source if from satellite, update track line.
                if (args.Position.Coordinate.PositionSource == PositionSource.Satellite)
                {
                    App.MapFunctions.DrawTrackLine(App.GeoCoordinate, App.ShowTrackLine);
                }

                // Update global track object. TODO: make this a function.
                App.Trk.MaxAltitude = _miscFunctions.MaxDouble(App.GeoCoordinate.Altitude, App.Trk.MaxAltitude);
                App.Trk.MaxSpeed = _miscFunctions.MaxDouble(App.GeoCoordinate.Speed, App.Trk.MaxSpeed);
                App.Trk.SpeedCount++;
                App.Trk.TotalSpeed += App.GeoCoordinate.Speed;
                App.Trk.AVGSpeed = _miscFunctions.AverageDouble(App.Trk.TotalSpeed, App.Trk.SpeedCount);

                // Animate the map with the latest location.
                App.MapFunctions.CenterMapOnLocationAnimated(App.GeoCoordinate, App.Zoom, App.MyMap.Pitch, App.NorthUp, App.RunningInBackground);

                App.MapFunctions.ShowLocationArrow(App.GeoCoordinate);

               // Update text boxes on main page.
                txtSpeed.Text = _unitConversions.Speed(_mapUnitIndex, App.GeoCoordinate.Speed).ToString();
                txtDistance.Text = _unitConversions.DistanceLarge(_mapUnitIndex, App.Trk.TotalMeters).ToString();
                txtAltitude.Text = _unitConversions.DistanceSmall(_mapUnitIndex, (double)args.Position.Coordinate.Altitude).ToString();

                if (App.Pin != null)
                {
                    App.Pin.Content = string.Format("{0}, Distance to: {1}{2}", App.PinName, _unitConversions.DistanceLarge(_mapUnitIndex, App.GeoCoordinate.GetDistanceTo(App.Pin.GeoCoordinate)), _unitConversions.LargeMapUnits(_mapUnitIndex));
                }

                // Update live tile.
                ShellTile.ActiveTiles.First().Update(new IconicTileData()
                {
                    WideContent1 = App.ShowTrackLine ? string.Format("Distance: {0} {1}", _unitConversions.DistanceLarge(_mapUnitIndex, App.Trk.TotalMeters).ToString(), _unitConversions.LargeMapUnits(_mapUnitIndex)) : string.Format("Odometer: {0} {1}", _unitConversions.DistanceLarge(_mapUnitIndex, App.Odometer).ToString(), _unitConversions.LargeMapUnits(_mapUnitIndex)),
                    WideContent2 = string.Format("Speed: {0} {1}", _unitConversions.Speed(_mapUnitIndex, App.GeoCoordinate.Speed), _unitConversions.SpeedMapUnits(_mapUnitIndex)),
                    WideContent3 = App.Timer.TotalTime()
                });
            });    
        }


        /// <summary>
        /// when map center changed event handler.  Turns off the centermaplock.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MyMap_CenterChanged(object sender, MapCenterChangedEventArgs e)
        {
            if(!App.ViewChanging && App.GeoCoordinate != null)
            {
                App.CenterMapLock = false;
            }
            else
            {
                App.CenterMapLock = true;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MyMap_Tap(object sender, GestureEventArgs e)
        {
            App.MyMap.Layers.Remove(App.PinMapLayer);
            App.Pin = null;
            App.PinMapOverlay = null;
            App.PinMapLayer = null;
            App.GoToPoint = false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MyMap_DoubleTap(object sender, GestureEventArgs e)
        {
            App.Zoom++;
            App.Zoom++;
        }


        /// <summary>
        /// Hold gesture on map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MyMap_Hold(object sender, GestureEventArgs e)
        {
            if (App.GeoCoordinate != null && App.Geolocator.LocationStatus == PositionStatus.Ready)
            {
                if (App.MyMap.Layers.Contains(App.PinMapLayer))
                {
                    App.MyMap.Layers.Remove(App.PinMapLayer);
                    App.Pin = null;
                    App.PinMapOverlay = null;
                    App.PinMapLayer = null;
                    App.GoToPoint = false;
                }

                if (App.Pin == null)
                {
                    GeoCoordinate coord = App.MyMap.ConvertViewportPointToGeoCoordinate(e.GetPosition(App.MyMap));
                    if (coord != null)
                    {
                        App.Pin = new Pushpin();
                        App.Pin.Background = new SolidColorBrush(Colors.Black);
                        App.Pin.GeoCoordinate = coord;
                        App.Pin.Tap += _pin_Tap;

                        App.PinMapOverlay = new MapOverlay();
                        App.PinMapOverlay.PositionOrigin = new Point(0, 1);
                        App.PinMapOverlay.GeoCoordinate = coord;
                        App.PinMapOverlay.Content = App.Pin;

                        App.PinMapLayer = new MapLayer();
                        App.PinMapLayer.Add(App.PinMapOverlay);
                        App.PinName = "Pin Drop";
                        App.Pin.Content = string.Format("{0}, Distance to: {1} {2}", App.PinName, _unitConversions.DistanceLarge(_mapUnitIndex, App.GeoCoordinate.GetDistanceTo(coord)), _unitConversions.LargeMapUnits(_mapUnitIndex));
                        App.MyMap.Layers.Add(App.PinMapLayer);                       
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _pin_Tap(object sender, GestureEventArgs e)
        {
            string uri = string.Format("/ShareLocation.xaml?lat={0}&lon={1}", App.Pin.GeoCoordinate.Latitude, App.Pin.GeoCoordinate.Longitude);
            NavigationService.Navigate(new Uri(uri, UriKind.Relative));
        }


        /// <summary>
        /// Recond track menu item click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecordTrack(object sender, EventArgs e)
        {
            App.MapFunctions.RecordTrack();
            if (App.ShowTrackLine)
            {
                _recordTrackBarMenuItem.Text = "recording track: on";
                if (!App.ShowTopPanel)
                {
                    sbTopPanelShow.Begin();
                    App.ShowTopPanel = true;
                }
            }
            else
            {
                _recordTrackBarMenuItem.Text = "recording track: off";
                App.ShowTopPanel = false;
                sbTopPanelHide.Begin();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TopPanel_OnTap(object sender, GestureEventArgs e)
        {
            if (!App.ShowTopPanel)
            {
                sbTopPanelShow.Begin();
                App.ShowTopPanel = true;
            }
            else
            {
                sbTopPanelHide.Begin();
                App.ShowTopPanel = false;
            }
        }

        #endregion

        #region "TEXT BOXES"

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        private void MapUnits(int index)
        {
            txtDistanceUnit.Text = _unitConversions.LargeMapUnits(index);
            txtSpeedUnit.Text = _unitConversions.SpeedMapUnits(index);
            txtAltUnit.Text = _unitConversions.SmallMapUnits(index);
        }


        /// <summary>
        /// 
        /// </summary>
        private void ShowCompassRose()
        {
            if(Compass.IsSupported && !App.HideCompass)
            {
                imgCompass.Visibility = Visibility.Visible;
                txtHeading.Visibility = Visibility.Visible;
                txtDegree.Visibility = Visibility.Visible;
            }
            else
            {
                imgCompass.Visibility = Visibility.Collapsed;
                txtHeading.Visibility = Visibility.Collapsed;
                txtDegree.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region "APPLICATIONBAR"

        /// <summary>
        /// Build application bar, the thing you pull up from the bottom of the screen.
        /// </summary>
        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 0.6;

            // Settings menu button
            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Images/trail_icon_sml.png", UriKind.Relative));
            appBarButton.Text = AppResources.MapTrack;
            appBarButton.Click += TracksPiv;
            ApplicationBar.Buttons.Add(appBarButton);

            // Location menu button
            appBarButton = new ApplicationBarIconButton(new Uri("/Images/location_76x80.png", UriKind.Relative));
            appBarButton.Text = AppResources.Location;
            appBarButton.Click += App.MapFunctions.ToggleLocation;
            ApplicationBar.Buttons.Add(appBarButton);

            // Zoom In button.
            appBarButton = new ApplicationBarIconButton(new Uri("/Images/add.png", UriKind.Relative));
            appBarButton.Text = AppResources.ZoomInText;
            appBarButton.Click += App.MapFunctions.ZoomIn;
            ApplicationBar.Buttons.Add(appBarButton);

            // Zoom Out button.
            appBarButton = new ApplicationBarIconButton(new Uri("/Images/minus.png", UriKind.Relative));
            appBarButton.Text = AppResources.ZoomOutText;
            appBarButton.Click += App.MapFunctions.ZoomOut;
            ApplicationBar.Buttons.Add(appBarButton);

            if (App.ShowTrackLine)
            {
                _recordTrackBarMenuItem = new ApplicationBarMenuItem("recording track: on");
            }
            else
            {
                _recordTrackBarMenuItem = new ApplicationBarMenuItem("recording track: off");
            }
            _recordTrackBarMenuItem.Click += RecordTrack;
            ApplicationBar.MenuItems.Add(_recordTrackBarMenuItem);

            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem("settings");
            appBarMenuItem.Click += Map_Settings;
            ApplicationBar.MenuItems.Add(appBarMenuItem);

            appBarMenuItem = new ApplicationBarMenuItem("route statistics");
            appBarMenuItem.Click += Statistics;
            ApplicationBar.MenuItems.Add(appBarMenuItem);

            appBarMenuItem = new ApplicationBarMenuItem("save route");
            appBarMenuItem.Click += SaveTrack;
            ApplicationBar.MenuItems.Add(appBarMenuItem);

            appBarMenuItem = new ApplicationBarMenuItem("load route");
            appBarMenuItem.Click += LoadTrack;
            ApplicationBar.MenuItems.Add(appBarMenuItem);

            //appBarMenuItem = new ApplicationBarMenuItem("share your location");
            //appBarMenuItem.Click += Share;
            //ApplicationBar.MenuItems.Add(appBarMenuItem);

            appBarMenuItem = new ApplicationBarMenuItem("about");
            appBarMenuItem.Click += About;
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }

        #endregion

        #region "NAVIGATION EVENTS"

        /// <summary>
        /// Event handler for share location menu item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Share(object sender, EventArgs e)
        {
            if (_loc.CheckUserConcent() && App.Geolocator != null && App.Geolocator.LocationStatus == PositionStatus.Ready)
            {
                NavigationService.Navigate(new Uri("/ShareLocation.xaml", UriKind.Relative));
            } 
        }

        private void About(object sender, EventArgs e)
        {
            if (_loc.CheckUserConcent() && App.Geolocator != null && App.Geolocator.LocationStatus == PositionStatus.Ready)
            {
                NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Statistics(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/TracksPiv.xaml?index=0", UriKind.Relative));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveTrack(object sender, EventArgs e)
        {
            if (App.Trk.line == null || App.Trk.line.Path.Count <= 0)
            {
                MessageBox.Show("You have not recorded a route. Press the record button on the map page or track page and shred some trail!");
            }
            else
            {
                App.ShowTrackLine = false;
                NavigationService.Navigate(new Uri("/Save.xaml", UriKind.Relative));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadTrack(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/TracksPiv.xaml?index=1", UriKind.Relative));
        }

        /// <summary>
        /// Navigates to Map settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Map_Settings(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AppSettings.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Navigates to TracksPano.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TracksPiv(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/TracksPiv.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Override for back button to send query string parameters.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (_miscFunctions.ShowMessage("Exit Rip Trail?", "Exit Rip Trail"))
            {
                App.UserSettings.AddOrUpdateSetting(AppResources.UserSettingsOdometer, App.Odometer);
                Application.Current.Terminate();
            }
            else
            {
                e.Cancel = true;
            }   
        }

        /// <summary>
        /// Navigation event handler, Returning to MainPage.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            GetLocation();

            if (!ContentPanel.Children.Contains(App.MyMap))
            {
                ContentPanel.Children.Add(App.MyMap);
            }
          
            _mapUnitIndex = App.UserSettings.GetSetting(AppResources.UserSettingsMapUnits, 0);
            MapUnits(_mapUnitIndex);

            if(App.GeoCoordinate != null && !App.ViewChanging && App.CenterMapLock)
            {
                App.MyMap.SetView(App.GeoCoordinate, App.Zoom, MapAnimationKind.Parabolic);
            }

            if(App.Geolocator != null && App.GeoCoordinate != null)
            {
                App.Geolocator.PositionChanged += Geolocator_PositionChanged;
                App.MapFunctions.ShowLocationArrow(App.GeoCoordinate);
                txtSpeed.Text = _unitConversions.Speed(_mapUnitIndex, App.GeoCoordinate.Speed).ToString();
                txtDistance.Text = _unitConversions.DistanceLarge(_mapUnitIndex, App.Trk.TotalMeters).ToString(); ;
                txtAltitude.Text = _unitConversions.DistanceSmall(_mapUnitIndex, App.GeoCoordinate.Altitude).ToString();
            }

            if (App.MyMap != null)
            {
                App.MyMap.DoubleTap += MyMap_DoubleTap;
                App.MyMap.Tap += MyMap_Tap;
                App.MyMap.CenterChanged += MyMap_CenterChanged;
                App.MyMap.Hold += MyMap_Hold;
            }
            if (App.ShowTopPanel)
            {
                TopPanel.Opacity = .7;
            }
            if (App.Pin != null)
            {
                App.Pin.Content = string.Format("{0}, Distance to: {1} {2}", App.PinName, _unitConversions.DistanceLarge(_mapUnitIndex, App.GeoCoordinate.GetDistanceTo(App.Pin.GeoCoordinate)), _unitConversions.LargeMapUnits(_mapUnitIndex));
            }

            ShowCompassRose();

            DeviceStatus.PowerSourceChanged += new EventHandler(DeviceStatus_PowerSourceChanged);

        }

        /// <summary>
        /// Navigtion event handler, Leaving MainPage.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (ContentPanel.Children.Contains(App.MyMap))
            {
                ContentPanel.Children.Remove(App.MyMap);
            }

            App.MyMap.Layers.Remove(App.LocationLayer);
            App.LocationLayer = null;
            App.Zoom = App.MyMap.ZoomLevel;

            DeviceStatus.PowerSourceChanged -= new EventHandler(DeviceStatus_PowerSourceChanged);
        }

        /// <summary>
        /// Device power source status changed event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DeviceStatus_PowerSourceChanged(object sender, EventArgs e)
        {
            // The PowerSourceChanged event is not raised on the UI thread, 
            // so the Dispatcher must be invoked to update the Text property.
            this.Dispatcher.BeginInvoke(() =>
            {
                switch (DeviceStatus.PowerSource)
                {
                    case PowerSource.Battery:
                        PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
                        break;
                    case PowerSource.External:
                        PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
                        break;
                }
            }
            );
        }

        #endregion

        /// <summary>
        /// Asynchronous method, get location from device geolocation
        /// </summary>
        private void GetLocation()
        {              
            _loc.UserConcent();
            if (App.UserSettings.GetSetting<Boolean>(AppResources.UserLocationConsent, false))
            {
                if(App.Geolocator == null)
                {                 
                    try
                    {
                        if (App.Geolocator == null)
                        {
                            App.Geolocator = new Geolocator();
                            App.Geolocator.DesiredAccuracy = PositionAccuracy.High;
                            //App.Geolocator.MovementThreshold = 3;
                            App.Geolocator.ReportInterval = 500;
                            App.Geolocator.StatusChanged += Geolocator_StatusChanged;
                        }
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            else if(App.Geolocator != null)
            {
                App.Geolocator.PositionChanged -= Geolocator_PositionChanged;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            Dispatcher.BeginInvoke(() =>
            {
                switch (args.Status)
                {
                    case PositionStatus.Disabled:
                        if (_miscFunctions.ShowMessage("Location in settings is off.", "Location Services"))
                        {
                            Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-location:"));
                        }
                        break;
                    case PositionStatus.Initializing:
                        // the geolocator started the tracking operation
                        SetProgressIndicator(true, "Acquiring position from location services");
                        if (App.MyMap.ZoomLevel < 12)
                        {
                            App.Zoom = 12;
                        } 
                        App.Geolocator.PositionChanged += Geolocator_PositionChanged;
                        break;
                    case PositionStatus.NoData:
                        // the location service was not able to acquire the location
                        break;
                    case PositionStatus.Ready:
                        // the location service is generating geopositions as specified by the tracking parameters
                        if (_prog.IsVisible)
                        {
                            SetProgressIndicator(false, "");
                        }
                        break;
                    case PositionStatus.NotInitialized:
                        // the initial state of the geolocator, once the tracking operation is stopped by the user the geolocator moves back to this state
                        //MessageBox.Show("Not Initialized");
                        break;
                }
            });
        }

    } // end class
} // end namespace