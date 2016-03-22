using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using System.ComponentModel;
using RipTrail.Resources;
using RipTrail.ViewModel;
using RipTrail.Models;

namespace RipTrail
{
    public partial class Save : PhoneApplicationPage, INotifyPropertyChanged
    {
        private readonly FileIO _fileIO;
        private readonly Serializer _ser;
        private readonly ProgressIndicator _prog;
        private bool _importTrack;
        private Track _track { set; get; }

        // Data context for the local database
        private readonly TrackDataContext _trackDB;

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

        /// <summary>
        /// 
        /// </summary>
        public Save()
        {
            InitializeComponent();
            
            _fileIO = new FileIO();
            _ser = new Serializer();
            _trackDB = new TrackDataContext(TrackDataContext.DBConnectionString);

            // Data context and observable collection are children of the main page.
            DataContext = this;

            // Define the query to gather all of the to-do items.
            var tracksInDB = from TrackDataBase trk in _trackDB.TrackTable select trk;

            // Execute the query and place the results into a collection.
            TrackDataBases = new ObservableCollection<TrackDataBase>(tracksInDB);

            SystemTray.SetIsVisible(this, true);
            _prog = new ProgressIndicator();
            SystemTray.SetProgressIndicator(this, _prog);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isVisible"></param>
        /// /// <param name="text"></param>
        private void SetProgressIndicator(bool isVisible, string text)
        {
            _prog.IsIndeterminate = isVisible;
            _prog.IsVisible = isVisible;
            _prog.Text = text;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies when a property has changed
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
        /// Save button event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveTrack_Click(object sender, RoutedEventArgs e)
        {
            if (txtTrackName.Text != string.Empty)
            {
                if (TrackDataBases.Any(x => x.ItemName == txtTrackName.Text))
                {
                    txtTrackName.Focus();
                    MessageBox.Show("You are already using this name. Please provide a unique file name.");
                }
                else
                {
                    SetProgressIndicator(true, "Writing track to memory");
                    _track.name = txtTrackName.Text;
                    _track.description = txtDescription.Text;
                    SaveTrackToDb(_track);
                    
                   ;
                }
            }
            else
            {
                txtTrackName.Focus();
                MessageBox.Show("Please enter a name.");
            }                     
        }

        /// <summary>
        /// 
        /// </summary>
        private void ClearTrackData()
        {
            if (App.Trk != null)
            {
                App.Trk.ClearTrack();
            }
            if (App.ImportTrk != null)
            {
                App.ImportTrk.ClearTrack();
            }

            if (App.UserSettings.GetSetting(AppResources.UserClearMapSave, false))
            {
                App.MyMap.MapElements.Clear();
            }                                 
        }

        /// <summary>
        /// Save track to db/localstoreage
        /// </summary>
        /// <param name="trk"></param>
        private async void SaveTrackToDb(Track trk)
        {  
            try
            {
                SetProgressIndicator(true, "Writing track to memory");               
                TrackDataBase newItem = new TrackDataBase
                {
                    FileName = trk.name,
                    ItemName = trk.name,
                    Description = trk.description,
                };
                TrackDataBases.Add(newItem);
                _trackDB.TrackTable.InsertOnSubmit(newItem);

                await _fileIO.WriteToFile(trk.name + ".gpx", AppResources.LocalFolder, _ser.CreateGPX(trk).ToString());

                _trackDB.SubmitChanges();
                SetProgressIndicator(false, "");
                NavigationService.Navigate(new Uri("/TracksPiv.xaml?index=1", UriKind.Relative));
                ClearTrackData();
            }
            catch
            {
                SetProgressIndicator(false, "");
                MessageBox.Show("Something went very wrong.");
            }            
        }

        /// <summary>
        /// Override for back button to send query string   parameters.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            NavigationService.GoBack();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.ContainsKey("import"))
            {
                _importTrack = bool.Parse(NavigationContext.QueryString["import"]);
            }

            if (_importTrack)
            {
                _track = App.ImportTrk;
                txtTrackName.Text = App.ImportTrk.name;
                txtDescription.Text = App.ImportTrk.description;
            }
            else
            {
                _track = App.Trk;
                txtDescription.Text = DateTime.Now.ToString("dddd, MMMM M, yyyy");
            }

            if (App.UserSettings.GetSetting(AppResources.UserClearMapSave, false))
            {
                togClearMap.IsChecked = true;
            }
            else
            {
                togClearMap.IsChecked = false;
            }
        }

        private void TogClearMap_OnChecked(object sender, RoutedEventArgs e)
        {
            App.UserSettings.AddOrUpdateSetting(AppResources.UserClearMapSave, true);
        }

        private void TogClearMap_OnUnchecked(object sender, RoutedEventArgs e)
        {
            App.UserSettings.AddOrUpdateSetting(AppResources.UserClearMapSave, false);
        }
    }
}