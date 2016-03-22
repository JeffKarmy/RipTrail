using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Devices.Geolocation;

using RipTrail.Resources;
using RipTrail.ViewModel;
using RipTrail.Models;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace RipTrail
{
    public partial class TracksPiv : PhoneApplicationPage, INotifyPropertyChanged
    {

        private readonly FileIO _fileio;
        private Serializer _serializer;
        private readonly UnitConversions _unitConversions;
        private readonly MiscFunctions _miscFunctions;
        private int _mapUnitIndex;
        private ApplicationBarIconButton _appBarButton;
        private readonly LocationManager _loc;
        private readonly ProgressIndicator _prog;
        private ApplicationBarMenuItem _appBarMenuItem;
        private TrackDataBase _query;

       

        // Data context for the local database
        private readonly TrackDataContext _trackDb;

        #region INotifyPropertyChanged Members

        private ObservableCollection<TrackDataBase> _trackDataBases;
        public ObservableCollection<TrackDataBase> TrackDataBases
        {
            get
            {
                return _trackDataBases;
            }
            set
            {
                if (_trackDataBases != value)
                {
                    _trackDataBases = value;
                    NotifyPropertyChanged("TrackDataBases");
                }
            }
        }

        #endregion

        //Constructor
        public TracksPiv()
        {
            _miscFunctions = new MiscFunctions();
            _unitConversions = new UnitConversions();
            _fileio = new FileIO();
            _mapUnitIndex = App.UserSettings.GetSetting(AppResources.UserSettingsMapUnits, 0);
            _loc = new LocationManager();

            _trackDb = new TrackDataContext(TrackDataContext.DBConnectionString);

            DataContext = this;
            
            InitializeComponent();
            BuildLocalizedApplicationBar();

            SystemTray.SetIsVisible(this, true);
            _prog = new ProgressIndicator();
            SystemTray.SetProgressIndicator(this, _prog);

            App.Timer.timer.Tick += Timer_Tick;
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

        # region "TEXT BOXES"

        /// <summary>
        /// txtOdometer textbox
        /// </summary>
        /// <param name="odometer"></param>
        /// <param name="index"></param>
        private void OdometerTextBox(double odometer, int index)
        {
            txtOdometer.Text = _unitConversions.DistanceLarge(index, odometer).ToString();
            txtOdoUnit.Text = _unitConversions.LargeMapUnits(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alt"></param>
        /// <param name="maxAlt"></param>
        /// <param name="index"></param>
        private void Altitude(double alt, double maxAlt, int index)
        {
            txtAltitude.Text = _unitConversions.DistanceSmall(index, alt).ToString();
            txtAltUnit.Text = _unitConversions.SmallMapUnits(index);

            txtMAXAltitude.Text = _unitConversions.DistanceSmall(index, maxAlt).ToString();
            txtMAXAltUnit.Text = _unitConversions.SmallMapUnits(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spd"></param>
        /// <param name="maxSpd"></param>
        ///  /// <param name="avgSpd"></param>
        /// <param name="index"></param>
        private void Speed(double spd, double maxSpd, double avgSpd, int index)
        {
            txtSpeed.Text =  _unitConversions.Speed(index, spd).ToString();
            txtSpdUnit.Text = _unitConversions.SpeedMapUnits(index);

            txtAVGSpd.Text = _unitConversions.Speed(index, avgSpd).ToString();
            txtAVGSpdUnit.Text = _unitConversions.SpeedMapUnits(index);

            txtMAXSpd.Text = _unitConversions.Speed(index, maxSpd).ToString();
            txtMAXSpdUnit.Text =  _unitConversions.SpeedMapUnits(index);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the app that a property has changed.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region "NAVIGATION"

        /// <summary>
        /// Override for back button to send query string parameters.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnBackKeyPress(CancelEventArgs e)
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
            // Define the query to gather all of the to-do items.
            var tracksInDb = from TrackDataBase trk in _trackDb.TrackTable select trk;

            // Execute the query and place the results into a collection.
            TrackDataBases = new ObservableCollection<TrackDataBase>(tracksInDb);
           
            base.OnNavigatedTo(e);

            if (App.ShowTrackLine)
            {
                txtRecord.Text = "Recording: On";
                txtRecord.Foreground = new SolidColorBrush(_miscFunctions.GetAccentColor());
            }
            else
            {
                txtRecord.Text = "Recording: Off";
                txtRecord.Foreground = new SolidColorBrush(_miscFunctions.GetPhoneBackgroundColor());
            }

            if (NavigationContext.QueryString.ContainsKey("index"))
            {
                var index = NavigationContext.QueryString["index"];
                var parsIndex = int.Parse(index);
                pivTrack.SelectedIndex = parsIndex;
            }
            _mapUnitIndex = App.UserSettings.GetSetting(AppResources.UserSettingsMapUnits, 0);
            OdometerTextBox(App.Odometer, _mapUnitIndex);
            if(App.GeoCoordinate != null)
            {
                Altitude(App.GeoCoordinate.Altitude, App.Trk.MaxAltitude, _mapUnitIndex);
                Speed(App.GeoCoordinate.Speed, App.Trk.MaxSpeed, App.Trk.AVGSpeed, _mapUnitIndex);
                txtDistance.Text = _unitConversions.DistanceLarge(_mapUnitIndex, App.Trk.TotalMeters).ToString();
                txtDistUnit.Text = _unitConversions.LargeMapUnits(_mapUnitIndex);
            }

            if (App.UserSettings.GetSetting(AppResources.UserLocationConsent, false))
            {
                if (App.Geolocator != null &&
                    (App.Geolocator.LocationStatus == PositionStatus.Ready ||
                     App.Geolocator.LocationStatus == PositionStatus.Initializing ||
                     App.Geolocator.LocationStatus == PositionStatus.NotInitialized))
                {
                    App.Geolocator.PositionChanged += Geolocator_PositionChanged;
                }
            }
            else if (App.Geolocator != null)
            {
                App.Geolocator.PositionChanged -= Geolocator_PositionChanged;               
                App.Geolocator = null;
                App.GeoCoordinate = null;
            }

            if (App.ShowTrackLine)
            {
                txtTimer.Text = txtTimer.Text = TimeSpan.FromMilliseconds(App.MovingTime).ToString(@"hh\:mm\:ss");
                txtTotalTime.Text = App.Timer.TotalTime();
                txtStartTime.Text = App.StartTime;
            }
        }

        /// <summary>
        /// Code runs when leaving page.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            
        }

        #endregion

        #region "APPLICATIONBAR"

        /// <summary>
        /// Build application bar, the thing you pull up from the bottom of the screen.
        /// </summary>
        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 0.75;

            // Settings menu button
            _appBarButton = new ApplicationBarIconButton(new Uri("/Images/feature.settings.png", UriKind.Relative));
            _appBarButton.Text = AppResources.MapSettings;
            _appBarButton.Click += Map_Settings;
            ApplicationBar.Buttons.Add(_appBarButton);

            _appBarButton = new ApplicationBarIconButton(new Uri("/Images/rec.png", UriKind.Relative));
            _appBarButton.Text = AppResources.Record;        
            _appBarButton.Click += RecordTrack;
            ApplicationBar.Buttons.Add(_appBarButton);

            // Zoom Out button.
            _appBarButton = new ApplicationBarIconButton(new Uri("/Images/save.png", UriKind.Relative));
            _appBarButton.Text = AppResources.SaveTrack;
            _appBarButton.Click += SaveTrack;
            ApplicationBar.Buttons.Add(_appBarButton);

            _appBarButton = new ApplicationBarIconButton(new Uri("/Images/Eraser.png", UriKind.Relative));
            _appBarButton.Text = AppResources.EraseTrack;
            _appBarButton.Click += EraseTrack;
            ApplicationBar.Buttons.Add(_appBarButton);

            // This is the menu under the application bar.
            _appBarMenuItem = new ApplicationBarMenuItem("Import GPX Files");
            _appBarMenuItem.Click += ImportGPXFiles;
            ApplicationBar.MenuItems.Add(_appBarMenuItem);

        }

        #endregion

        #region "BUTTON CONTROLS"

        /// <summary>
        /// Record Button event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecordTrack(object sender, EventArgs e)
        {
            App.MapFunctions.RecordTrack();
            if (App.ShowTrackLine)
            {
                txtRecord.Text = "Recording: On";
                txtRecord.Foreground = new SolidColorBrush(_miscFunctions.GetAccentColor());
                txtStartTime.Text = App.StartTime;
            }
            else
            {
                txtRecord.Text = "Recording: Off";
                txtRecord.Foreground = new SolidColorBrush(_miscFunctions.GetPhoneBackgroundColor());
            }
        }

        /// <summary>
        /// Delete Button event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnDeleteTrack_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button != null && _miscFunctions.ShowMessage("Please confirm delete.", "Delete Route"))
            {
                SetProgressIndicator(true,"Deleting Entry...");
                TrackDataBase trackToDelete = button.DataContext as TrackDataBase;
                if (trackToDelete != null)
                {
                    TrackDataBases.Remove(trackToDelete);
                    _trackDb.TrackTable.DeleteOnSubmit(trackToDelete);
                    await _fileio.DeleteFileFromLocal(trackToDelete.FileName, AppResources.LocalFolder);
                    _trackDb.SubmitChanges();
                }
                SetProgressIndicator(false,"");
            }
        }

        /// <summary>
        /// Save track Button event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveTrack(object sender, EventArgs e)
        {
            if(App.Trk.line == null || App.Trk.line.Path.Count <= 0)
            {
                MessageBox.Show("You have not recorded a route. Press the record button on the map page or track page and shred some trail!");
            }
            else
            {
                App.ShowTrackLine = false;
                App.Timer.StopTimer();
                NavigationService.Navigate(new Uri("/Save.xaml", UriKind.Relative));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EraseTrack(object sender, EventArgs e)
        {
            if(_miscFunctions.ShowMessage("This will remove your track from the map.","Clear Map"))
            {
                App.ShowTrackLine = false;
                App.MyMap.MapElements.Clear();
                if(App.Trk != null)
                { 
                    App.Trk.ClearTrack();
                }
                txtRecord.Text = "Recording: Off";
                txtRecord.Foreground = new SolidColorBrush(_miscFunctions.GetPhoneBackgroundColor());
                App.MovingTime = 0;
                
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportGPXFiles(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ImportTracks.xaml", UriKind.Relative));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Map_Settings(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AppSettings.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Load Track Button event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnLoadTrack_Click(object sender, RoutedEventArgs e)
        {
            // Cast the parameter as a button.
            var button = sender as Button;

            if (App.Trk.line != null && App.Trk.line.Path.Count > 0)
            {
                if (_miscFunctions.ShowMessage("Are you sure, this operation will delete you current track?", "Load Track"))
                {
                    App.ShowTrackLine = false;
                    App.MyMap.MapElements.Clear();
                    App.Trk.ClearTrack();
                    if (App.Trk != null)
                    {
                        App.Trk.line = null;
                        App.Trk = null;
                    }
                }
                else
                {
                    return;
                }
            }

            if (button != null && App.Trk != null)
            {
                SetProgressIndicator(true, "Loading Route...");
                _serializer = new Serializer();
                _query = button.DataContext as TrackDataBase;

                if (_query != null)
                {
                    Stream file = await _fileio.ReadFileFromFolder(_query.FileName, AppResources.LocalFolder);
                    if (file != null)
                    {

                        App.MapFunctions.LoadTrackInMap(_serializer.ReadGPXFile(file));
                        App.CenterMapLock = false;
                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    }
                    else
                    {
                       MessageBox.Show(string.Format("File {0} not found.", _query.FileName));                      
                    }
                }
            }
            SetProgressIndicator(false, "");
        }

        /// <summary>
        /// Route preview event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPreview_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button != null)
            {
                _query = button.DataContext as TrackDataBase;

                if (_query != null)
                {
                    NavigationService.Navigate(new Uri("/ViewRoute.xaml?fileName=" + _query.FileName, UriKind.Relative));
                }
            }
        }

        #endregion

        #region "EVENT HANDLERS"

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if(App.ShowTrackLine)
            {
                txtTotalTime.Text = App.Timer.TotalTime();
            }
           
            //txtTimer.Text = App.Timer.MovingTimeCalc(1000, App.ShowTrackLine, App.GeoCoordinate);
            txtTimer.Text = TimeSpan.FromMilliseconds(App.MovingTime).ToString(@"hh\:mm\:ss");
        }

        /// <summary>
        /// Poisition changed event handler, executes code when position changes and updates map.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            if (App.Geolocator != null && App.Geolocator.LocationStatus == PositionStatus.Ready)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    if (App.GeoCoordinate != null)
                    {
                        Altitude(App.GeoCoordinate.Altitude, App.Trk.MaxAltitude, _mapUnitIndex);
                        Speed(App.GeoCoordinate.Speed, App.Trk.MaxSpeed, App.Trk.AVGSpeed, _mapUnitIndex);
                        txtDistance.Text = _unitConversions.DistanceLarge(_mapUnitIndex, App.Trk.TotalMeters).ToString();
                    }
                    OdometerTextBox(App.Odometer, _mapUnitIndex);
                });
            }
        }

        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdRouteEntry_OnTap(object sender, GestureEventArgs e)
        {
            var grid = sender as Grid;

            if (grid != null)
            {
                _query = grid.DataContext as TrackDataBase;

                if (_query != null)
                {
                    NavigationService.Navigate(new Uri("/ViewRoute.xaml?trackId=" + _query.TrackId, UriKind.Relative));
                }
            }
        }
    }
}