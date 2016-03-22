using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using Microsoft.Phone.Storage;
using RipTrail.Models;
using RipTrail.Resources;
using RipTrail.ViewModel;

namespace RipTrail
{
    public partial class ImportTracks : PhoneApplicationPage
    {
        public ObservableCollection<ExternalStorageFile> Routes { get; set; }
        private readonly ProgressIndicator _prog;
        private readonly FileIO _fileIO;
        private readonly Serializer _serializer;
        private readonly MiscFunctions _miscFunctions;

        public ImportTracks()
        {
            InitializeComponent();

            _fileIO = new FileIO();
            _serializer = new Serializer();
            _miscFunctions = new MiscFunctions();
            SystemTray.SetIsVisible(this, true);
            _prog = new ProgressIndicator();
            SystemTray.SetProgressIndicator(this, _prog);
           
            Routes = new ObservableCollection<ExternalStorageFile>();
            GetUserFolder();
            DataContext = this;
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

        /// <summary>
        /// 
        /// </summary>
        private async void GetUserFolder()
        {
            if (App.UserSettings.ContainsSetting(AppResources.SDCardFolder))
            {
                txtFolderName.Text = App.UserSettings.GetSetting<string>(AppResources.SDCardFolder, "Routes");
                if (await _fileIO.SDCard_CheckFolder(App.UserSettings.GetSetting<string>(AppResources.SDCardFolder, "Routes")))
                {
                    GetTracksFromSD(AppResources.SDCardFolder);
                }
               
            }
            else
            {
                txtFolderName.Text = "Routes";
                txtFolderName.Focus();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        private async void GetTracksFromSD(string folder)
        {
            try
            {
                SetProgressIndicator(true, "Accessing SD Card.");

                Routes.Clear();
                if (txtFolderName.Text != string.Empty)
                {
                    if (App.UserSettings.GetSetting(folder, "Routes").Equals(txtFolderName.Text))
                    {
                        App.UserSettings.AddOrUpdateSetting(folder, txtFolderName.Text);
                    }
                    gpxFilesListBox.DataContext = await _fileIO.GetExternal_GPXFiles(txtFolderName.Text, ".gpx");
                }
                else
                {
                    MessageBox.Show("Please provide the root folder on the SD card containing the GPX files.");
                }
                SetProgressIndicator(false, "");
            }
            catch (Exception)
            {
                MessageBox.Show("Please check SD Card.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ListBoxImportGPX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = (ListBox)sender;

            if (lb.SelectedItem != null)
            {
                try
                {
                    SetProgressIndicator(true, "");

                    ExternalStorageFile esf = (ExternalStorageFile)lb.SelectedItem;
                    Stream fileStream = await _fileIO.SDCard_ReadFile(esf.Path);

                    if (chkImport.IsChecked == true)
                    {
                        SetProgressIndicator(true, "Importing route to local memory.");
                        App.ImportTrk = _serializer.ReadGpxFileStastics(await _fileIO.SDCard_ReadFile(esf.Path), _miscFunctions.RemoveFileExtension(esf.Name));
                        NavigationService.Navigate(new Uri("/Save.xaml?import=true", UriKind.Relative));
                    }
                    else
                    {
                        SetProgressIndicator(true, "Loading route in map.");
                        App.MapFunctions.LoadTrackInMap(_serializer.ReadGPXFile(fileStream));
                        App.CenterMapLock = false;
                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    }
                    SetProgressIndicator(false, "");
                }
                catch (Exception ex)
                {
                    SetProgressIndicator(false, "");
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// Check box import checked event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkImport_OnChecked(object sender, RoutedEventArgs e)
        {
            App.UserSettings.AddOrUpdateSetting("ImportTrack", true);
        }

        /// <summary>
        /// Check box import uncheck event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkImport_OnUnchecked(object sender, RoutedEventArgs e)
        {
            App.UserSettings.AddOrUpdateSetting("ImportTrack", false);
        }

        /// <summary>
        /// On navigation to event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (App.UserSettings.GetSetting("ImportTrack", false))
            {
                chkImport.IsChecked = true;
            }
        }

        /// <summary>
        /// Buton Scan SD Card event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnScanSDCard_Click(object sender, RoutedEventArgs e)
        {
            if (txtFolderName.Text != String.Empty)
            {
                GetTracksFromSD(AppResources.SDCardFolder);
            }
            else
            {
                MessageBox.Show("Please enter the name of a root folder containing .gpx files.");
            }
        }
    }
}