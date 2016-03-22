using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using RipTrail.Resources;
using Microsoft.Phone.Maps.Controls;
using System.Device.Location;
using Windows.Devices.Geolocation;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Maps.Toolkit;
using Microsoft.Phone.Marketplace;
using Microsoft.Phone.Info;

using RipTrail.ViewModel;
using RipTrail.Models;

namespace RipTrail
{
    public partial class App : Application
    {
        public static Settings UserSettings { set; get; }
        public static MapFunctions MapFunctions { set; get; }
        public static CompasSensor CompasSensor { set; get; }
        public static Timer Timer { set; get; }
        public static Timer TimerMoving { set; get; }
        public static string StartTime { set; get; }
        
        public static Compass Compass { set; get; }
        public static DispatcherTimer DispatcherTimer { set; get; }
        public static long MovingTime { set; get; }

        public static Map MyMap { set; get; }
        public static MapLayer LocationLayer { set; get; }
        public static MapOverlay LocationOverlay { set; get; }
        public static Image LocationImage { set; get; }
        public static RotateTransform TransformArrow { get; set; }
        public static Pushpin Pin { get; set; }
        public static MapOverlay PinMapOverlay { get; set; }
        public static MapLayer PinMapLayer { get; set; }
        public static string PinName { get; set; }

        public static Track Trk { set; get; } // For the current track being creted
        public static Track ImportTrk { set; get; } // Tracks being imported from SD card.

        public static GeoCoordinate GeoCoordinate { set; get; }
        public static Geolocator Geolocator { set; get; }

        public static Boolean ShowTrackLine = false;
        public static Boolean RunningInBackground { get; set; }
        public static Boolean NorthUp = true;
        public static Boolean CenterMapLock = true;
        public static Boolean ViewChanging = false;
        public static Boolean ShowTopPanel { set; get; }
        public static Boolean Calibrating { set; get; }
        public static Boolean IsDataValid { set; get; }
        public static Boolean GoToPoint { set; get; }
        public static Boolean IsTrial { set; get; }
        public static Boolean HideCompass = false;

        public static Double Zoom { get; set; }
        public static Double Odometer = 0;
        public static Double MagneticHeading { set; get; }
        public static Double TrueHeading { set; get; }
        public static Double HeadingAccuracy { set; get; }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Prevent the screen from turning off while under the debugger by disabling
                // the application's idle detection.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            using (TrackDataContext db = new TrackDataContext(TrackDataContext.DBConnectionString))
            {
                if (db.DatabaseExists() == false)
                {
                    //Create the database
                    db.CreateDatabase();
                }
            }

            //Initialize user settings.
            MapFunctions = new MapFunctions();
            UserSettings = new Settings();
            Timer = new Timer();
            TimerMoving = new Timer();
            TimerMoving.timer.Tick += Timer_Tick;
            TimerMoving.StartTimer();
            CompasSensor = new CompasSensor();

            if (Compass.IsSupported)
            {
                Compass.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<CompassReading>>(compass_CurrentValueChanged);
                Compass.Start();
            }
            //LicenseInformation license = new LicenseInformation();
            //IsTrial = license.IsTrial();

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (ShowTrackLine)
            {
                if( GeoCoordinate != null && GeoCoordinate.Speed > 0)
                {
                    MovingTime += 1000;
                }
            }
        }

        private static void InitMap()
        {
            if (MyMap == null)
            {
                MyMap = new Map();
                MyMap.LandmarksEnabled = true;
                MyMap.PedestrianFeaturesEnabled = true;
                MyMap.ColorMode = UserSettings.GetSetting<MapColorMode>("MapColorMode", MapColorMode.Light);
                MyMap.CartographicMode = UserSettings.GetSetting<MapCartographicMode>("MapCartographicMode", MapCartographicMode.Road);
                MyMap.Loaded += myMapControl_Loaded;
                MyMap.ViewChanged += MyMap_ViewChanged;
                MyMap.ViewChanging += MyMap_ViewChanging;
                MyMap.ZoomLevel = 2;
            }
        }

        /// <summary>
        /// Compass event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void compass_CurrentValueChanged(object sender, SensorReadingEventArgs<CompassReading> e)
        {
            IsDataValid = Compass.IsDataValid;
            TrueHeading = e.SensorReading.TrueHeading;
            MagneticHeading = e.SensorReading.MagneticHeading;
            HeadingAccuracy = e.SensorReading.HeadingAccuracy;
        }

        /// <summary>
        /// Map loaded event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void myMapControl_Loaded(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = AppResources.MapApplicationID;
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = AppResources.MapAuthenticationToken;
        }

        private static void MyMap_ViewChanging(object sender, MapViewChangingEventArgs e)
        {
            ViewChanging = true;
        }

        private static void MyMap_ViewChanged(object sender, MapViewChangedEventArgs e)
        {
            ViewChanging = false;
        }


        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            InitMap();
            Odometer = UserSettings.GetSetting<Double>(AppResources.UserSettingsOdometer, 0);
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            InitMap();
            RunningInBackground = false;
            App.MapFunctions.ShowLocationArrow(App.GeoCoordinate);
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        public void Application_Closing(object sender, ClosingEventArgs e)
        {           
            
        }
        private void Application_RunningInBackground(object sender, RunningInBackgroundEventArgs args)
        {
            RunningInBackground = true;
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new TransitionFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Assign the URI-mapper class to the application frame.
            RootFrame.UriMapper = new GPXAutoLauncher();

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // do nothing
            }
        }

        #endregion

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage()
        {
            try
            {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }
    }
}