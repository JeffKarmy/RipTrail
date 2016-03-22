using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Microsoft.Phone.Controls;
using System.Threading.Tasks;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Maps.Controls;
using System.Collections.ObjectModel;
using System.IO;
using Windows.Phone.Storage.SharedAccess;
using Windows.Phone.Storage;
using Windows.Storage;

using RipTrail.Models;
using RipTrail.Resources;
using RipTrail.ViewModel;

namespace RipTrail
{
    public partial class ViewRoute : PhoneApplicationPage
    {
        private Track _trk;

        public event PropertyChangedEventHandler PropertyChanged;
        private readonly MiscFunctions _miscFunctions;
        private readonly FileIO _fileIo;
        private int _trackId; 
        
        // Data context for the local database
        private readonly TrackDataContext _trackDb;

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

        public ViewRoute()
        {
            InitializeComponent();
            MapPreview.CartographicMode = App.UserSettings.GetSetting<MapCartographicMode>("MapCartographicMode", MapCartographicMode.Road);
            MapPreview.ColorMode = App.UserSettings.GetSetting<MapColorMode>("MapColorMode", MapColorMode.Light);
            _miscFunctions = new MiscFunctions();
            _fileIo = new FileIO();
            _trackDb = new TrackDataContext(TrackDataContext.DBConnectionString);
            MapPreview.DoubleTap += MapPreview_DoubleTap;
        }

        /// <summary>
        /// Used to notify the app that a property has changed.
        /// </summary>
        /// <param name="propertyName"></param>
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        /// <summary>
        /// On Navigated to event handler
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            var tracksInDb = from TrackDataBase trk in _trackDb.TrackTable select trk;
            TrackDataBases = new ObservableCollection<TrackDataBase>(tracksInDb);

            base.OnNavigatedTo(e);
           
            if (NavigationContext.QueryString.ContainsKey("trackId"))
            {
                _trackId = int.Parse(NavigationContext.QueryString["trackId"]);

                foreach (var route in TrackDataBases)
                {
                    if (route.TrackId == _trackId)
                    {
                        _trk = LoadPage(await LoadRoutePreview(route.FileName, AppResources.LocalFolder));
                        break;
                    }                
                }
            }
            else if (NavigationContext.QueryString.ContainsKey("fileToken"))
            {
                string fileName = await _fileIo.SharedStorageCopyFile(NavigationContext.QueryString["fileToken"]);

                _trk = LoadPage(await LoadRoutePreview(_miscFunctions.RemoveFileExtension(fileName)));

                btnSaveImportedTrack.Visibility = Visibility.Visible;
            }

            if (MapPreview.ZoomLevel == 2)  //TODO: Band Aid
            {
                LoadTrackInMap(_trk.line.Path);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapPreview_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (MapPreview.ZoomLevel == 2)
            {
                LoadTrackInMap(_trk.line.Path);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private async Task<Track> LoadRoutePreview(string fileName, string folder = "")
        {
            try
            {
                Serializer serializer = new Serializer();

                if (fileName != string.Empty && folder != string.Empty)
                {              
                    return serializer.ReadGpxFileStastics(await _fileIo.ReadFileFromFolder(fileName + ".gpx", folder), fileName);
                }
                else if(fileName != string.Empty && folder == string.Empty)
                {
                    return serializer.ReadGpxFileStastics(await _fileIo.ReadFileFromLocalStorage(fileName + ".gpx"), fileName);
                }

                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);  //TODO: No ex messages.
                return null;
            }
        }


        /// <summary>
        /// Loads all the text boxes on the page.
        /// </summary>
        /// <param name="trk"></param>
        private Track LoadPage(Track trk)
        {
            UnitConversions uc = new UnitConversions();
            int mapUnitIndex = App.UserSettings.GetSetting(AppResources.UserSettingsMapUnits, 0);

            if (trk != null)
            {
                txtTitle.Text = trk.name;
                
                txtAVGSpeed.Text = uc.Speed(mapUnitIndex, trk.AVGSpeed).ToString("00.0");
                txtDistance.Text = uc.DistanceLarge(mapUnitIndex, trk.TotalMeters).ToString();
                txtTotalTime.Text = trk.TotalTime;
                txtMAXAltitude.Text = uc.DistanceSmall(mapUnitIndex, trk.MaxAltitude).ToString();

                txtAVGSpdUnit.Text = uc.SpeedMapUnits(mapUnitIndex);
                txtDistanceUnit.Text = uc.LargeMapUnits(mapUnitIndex);
                txtMAXAltUnit.Text = uc.SmallMapUnits(mapUnitIndex);

                LoadTrackInMap(trk.line.Path);

                return trk;
            }
            return null;
        }


        /// <summary>
        /// loads the route in the map.
        /// </summary>
        /// <param name="track"></param>
        public void LoadTrackInMap(GeoCoordinateCollection track)
        {
            MiscFunctions miscFunctions = new MiscFunctions();
            
            MapPolyline line = new MapPolyline();
            line.StrokeColor = miscFunctions.GetAccentColor();
            line.StrokeThickness = 4;
            line.Path = track;

            LocationRectangle locRect = new LocationRectangle(track.Max((p) => p.Latitude),
                                                              track.Min((p) => p.Longitude),
                                                              track.Min((p) => p.Latitude),
                                                              track.Max((p) => p.Longitude));
            MapPreview.MapElements.Add(line);
            MapPreview.SetView(locRect);
        }


        /// <summary>
        /// Load track in main map button event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLoadTrack_Click(object sender, RoutedEventArgs e)
        {
            if(_trk != null)
            {
                App.MapFunctions.LoadTrackInMap(_trk.line.Path);
                App.CenterMapLock = false;
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative)); 
            }
        }


        /// <summary>
        /// Delete button event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (_miscFunctions.ShowMessage("Please confirm delete.", "Delete Route"))
            {
                TrackDataBase trackToDelete = TrackDataBases[_trackId - 1];
                if (trackToDelete != null)
                {
                    TrackDataBases.Remove(trackToDelete);
                    _trackDb.TrackTable.DeleteOnSubmit(trackToDelete);
                    await _fileIo.DeleteFileFromLocal(trackToDelete.FileName, AppResources.LocalFolder);
                    _trackDb.SubmitChanges();
                    NavigationService.Navigate(new Uri("/TracksPiv.xaml?index=1", UriKind.Relative));
                }
            }
        }


        /// <summary>
        /// Button SaveImportedTracks event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveImportedTrack_Click(object sender, RoutedEventArgs e)
        {
            App.ImportTrk = _trk;
            btnSaveImportedTrack.Visibility = Visibility.Collapsed;
            NavigationService.Navigate(new Uri("/Save.xaml?import=true", UriKind.Relative));
        }

    }// End of class
}// End of name space.