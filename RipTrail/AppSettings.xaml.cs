using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Shell;

using RipTrail.ViewModel;
using RipTrail.Resources;

namespace RipTrail
{
    public partial class AppSettings : PhoneApplicationPage
    {
        private readonly MapFunctions _mapFunctions;
        private readonly MiscFunctions _miscFunctions;

        public AppSettings()
        {
            InitializeComponent();
            _mapFunctions = new MapFunctions();
            _miscFunctions = new MiscFunctions();

            if (Compass.IsSupported)
            {
                App.DispatcherTimer.Tick += DispatcherTimer_Tick;
                App.DispatcherTimer.Start();
                btnCalibrateCompass.Visibility = Visibility.Visible;
                btnCalibrateCompass.IsEnabled = true;
                pivCompassOptions.Visibility = Visibility.Visible;         
            }
        }

        void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (App.HeadingAccuracy <= 10)
            {
                calibrationTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                btnCalibrateCompass.Content = "Compass Calibration";
                btnCalibrateCompass.Foreground = new SolidColorBrush(Colors.Green);
                calibrationTextBlock.Text = " Complete!";
            }
            else
            {
                calibrationTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                btnCalibrateCompass.Content = "Compass Calibration Required";
                btnCalibrateCompass.Foreground = new SolidColorBrush(Colors.Red);
                calibrationTextBlock.Text = App.HeadingAccuracy.ToString("0.0");
            }
        }



        #region "Page Events"

        /// <summary>
        /// Override for back button to send query string parameters.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Gets the query string parameter for map color to populate lstMapColor listPicker control.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            switch (App.MyMap.ColorMode)
            {
                case MapColorMode.Light:
                    lstMapColors.SelectedIndex = 0;
                    break;
                case MapColorMode.Dark:
                    lstMapColors.SelectedIndex = 1;
                    break;
            }

            switch (App.MyMap.CartographicMode)
            {
                case MapCartographicMode.Road:
                    lstMapTypes.SelectedIndex = 0;
                    break;
                case MapCartographicMode.Hybrid:
                    lstMapTypes.SelectedIndex = 1;
                    break;
                case MapCartographicMode.Aerial:
                    lstMapTypes.SelectedIndex = 2;
                    break;
                case MapCartographicMode.Terrain:
                    lstMapTypes.SelectedIndex = 3;
                    break;
            }

            lstMapUnits.SelectedIndex = App.UserSettings.GetSetting<int>("MapUnits", 0);

            if (App.UserSettings.GetSetting<Boolean>(AppResources.UserLocationConsent, false))
            {
                togMyLocation.IsChecked = true;
                togMyLocation.Content = "On";
            }
            else
            {
                togMyLocation.IsChecked = false;
                togMyLocation.Content = "Off";
            }

            if (PhoneApplicationService.Current.UserIdleDetectionMode == IdleDetectionMode.Enabled)
            {
                togScreenSleep.IsChecked = false;
            }
            if (PhoneApplicationService.Current.UserIdleDetectionMode == IdleDetectionMode.Disabled)
            {
                togScreenSleep.IsChecked = true;
            }

            if(App.HideCompass)
            {
                togHideCompassRose.Content = "On";
                togHideCompassRose.IsChecked = true;
            }
            else
            {
                togHideCompassRose.Content = "Off";
                togHideCompassRose.IsChecked = false;
            }

            if (App.NorthUp)
            {
                togCourse.IsChecked = false;
                togCourse.Content = "North Up";
            }
            else
            {
                togCourse.IsChecked = true;
                togCourse.Content = "Course Up";
            }
        }

        #endregion

        #region "Map Settings"

        /// <summary>
        /// Eventhandler for listpicker map types.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LstMapTypes_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstMapTypes != null)
            {
                switch (lstMapTypes.SelectedIndex)
                {
                    case 0:
                        _mapFunctions.MapType(MapCartographicMode.Road);
                        break;
                    case 1:
                        _mapFunctions.MapType(MapCartographicMode.Hybrid);
                        break;
                    case 2:
                        _mapFunctions.MapType(MapCartographicMode.Aerial);
                        break;
                    case 3:
                        _mapFunctions.MapType(MapCartographicMode.Terrain);
                        break;
                }
            }
        }

        /// <summary>
        /// ListPicker lstMapColors event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListPicker_MapColor(object sender, SelectionChangedEventArgs e)
        {
            if (lstMapColors != null)
            {
                switch (lstMapColors.SelectedIndex)
                {
                    case 0:
                        App.MyMap.ColorMode = MapColorMode.Light;
                        App.UserSettings.AddOrUpdateSetting("MapColorMode", MapColorMode.Light);
                        break;
                    case 1:
                        App.MyMap.ColorMode = MapColorMode.Dark;
                        App.UserSettings.AddOrUpdateSetting("MapColorMode", MapColorMode.Dark);
                        break;
                }
            }
        }

        /// <summary>
        ///  Event handler for course up toggle switch.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void togCourse_Checked(object sender, RoutedEventArgs e)
        {
            togCourse.IsChecked = true;
            togCourse.Content = "Course Up";
            //App.NorthUp = false;
        }

        /// <summary>
        /// Event handler for course up toggle switch.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void togCourse_UnChecked(object sender, RoutedEventArgs e)
        {
            togCourse.IsChecked = false;
            togCourse.Content = "North Up";
           // App.NorthUp = true;
        }

        #endregion

        #region "App Settings"

        /// <summary>
        /// Eventhandler for Reset Odometer Button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResetOdo_Click(object sender, RoutedEventArgs e)
        {
            if (_miscFunctions.ShowMessage("Are you sure you want to reset the odometer?", "Odometer Reset"))
            {
                App.Odometer = 0;
                App.UserSettings.AddOrUpdateSetting(AppResources.UserSettingsOdometer, App.Odometer);
            }
        }

        /// <summary>
        /// Event handler for checking my location.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void togMyLocation_Checked(object sender, RoutedEventArgs e)
        {
            App.UserSettings.AddOrUpdateSetting(AppResources.UserLocationConsent, true);
            togMyLocation.Content = "On";
        }

        /// <summary>
        /// Event handler for unchecking my locaiton.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void togMyLocation_UnChecked(object sender, RoutedEventArgs e)
        {
            App.UserSettings.AddOrUpdateSetting(AppResources.UserLocationConsent, false);
            togMyLocation.Content = "Off";
        }

        /// <summary>
        /// UnCheck event handler for Map Orientation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void togScreenSleep_Checked(object sender, RoutedEventArgs e)
        {
            if (PhoneApplicationService.Current.UserIdleDetectionMode == IdleDetectionMode.Enabled)
            {
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
        }

        /// <summary>
        /// Checked event handler for Map Orientation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void togScreenSleep_Unchecked(object sender, RoutedEventArgs e)
        {
            if (PhoneApplicationService.Current.UserIdleDetectionMode == IdleDetectionMode.Disabled)
            {
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
            }
        }

        /// <summary>
        /// L
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListPicker_MapUnits(object sender, SelectionChangedEventArgs e)
        {
            if (lstMapUnits != null)
            {
                int mapUnitsIndex = 0;

                switch (lstMapUnits.SelectedIndex)
                {
                    case 0:
                        // US Standard Units
                        mapUnitsIndex = 0;
                        break;
                    case 1:
                        //Metric Units
                        mapUnitsIndex = 1;
                        break;
                    case 2:
                        // Nautical Units
                        mapUnitsIndex = 2;
                        break;
                }
                App.UserSettings.AddOrUpdateSetting(AppResources.UserSettingsMapUnits, mapUnitsIndex);
            }
        }

        #endregion

        #region "Compass Settings"

        /// <summary>
        /// Event handler for Calibrate button, Hides compass calibrate button and shows calibrate UI image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calibrationButton_Click(object sender, RoutedEventArgs e)
        {
            calibrationStackPanel.Visibility = Visibility.Collapsed;
            grdCalibrationPanel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Event handler for Done button, Hides UI images and shows calibrate compass.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCalibrateCompass_Click(object sender, RoutedEventArgs e)
        {
            calibrationStackPanel.Visibility = Visibility.Visible;
            grdCalibrationPanel.Visibility = Visibility.Collapsed;
        }


        /// <summary>
        /// Event handler for hide compass
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void togHideCompass_Checked(object sender, RoutedEventArgs e)
        {
            App.HideCompass = true;
            togHideCompassRose.Content = "On";
        }


        /// <summary>
        /// Event handler for show compass
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void togHideCompass_Unchecked(object sender, RoutedEventArgs e)
        {
            App.HideCompass = false;
            togHideCompassRose.Content = "Off";
        }

        #endregion





    } // End Class
} // End namespace