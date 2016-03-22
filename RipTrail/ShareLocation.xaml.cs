using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Phone.Net.NetworkInformation;

using RipTrail.ViewModel;
using RipTrail.Resources;


namespace RipTrail
{
    public partial class ShareLocation : PhoneApplicationPage
    {

        private string _address;
        private StringBuilder _sb;
        private WebClient _client;
        private bool _isAddressGood = false;
        private UnitConversions _unitConversions;
        private int _mapUnitIndex;
        private StringBuilder _SBPinLocation;


        public ShareLocation()
        {
            InitializeComponent();
            _mapUnitIndex = App.UserSettings.GetSetting(AppResources.UserSettingsMapUnits, 0);
            
        }

        private void MakeJsonCall(double lat, double lon)
        {
            try
            {
                _client = new WebClient();
                _client.DownloadStringCompleted += client_DownloadedStringCompleted;
                string Url = "http://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat.ToString() + "," + lon.ToString() + "&sensor=true";
                _client.DownloadStringAsync(new Uri(Url, UriKind.RelativeOrAbsolute));
            }
            catch (Exception)
            {
                _SBPinLocation.AppendLine("Did not get any data back from server.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_DownloadedStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                JObject jsonObject = JObject.Parse(e.Result);
                List<JToken> myAddressList = jsonObject.SelectTokens("results[*].formatted_address").ToList();

                if(myAddressList.Count > 1)
                {
                    _SBPinLocation.AppendLine(myAddressList[0].ToString());
                    _address = myAddressList[0].ToString();
                    _isAddressGood = true;
                }
                else
                {
                    _SBPinLocation.AppendLine(string.Format("Latitude: {0}, Longitude: {1}", App.GeoCoordinate.Latitude, App.GeoCoordinate.Longitude));
                    _isAddressGood = false;
                }

                btnEmail.IsEnabled = true;
                btnSms.IsEnabled = true;
                btnGoToPin.IsEnabled = true;
                txtAddress.Text = _SBPinLocation.ToString();
            }
            catch(Exception ex)
            {
                _SBPinLocation.AppendLine("Did not get any data back from server.");
            }
        }

        /// <summary>
        /// Create 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private string Body(string address, bool goodAddress)
        {
            if(goodAddress)
            {
                _sb = new StringBuilder();
                _sb.AppendLine("My pin drop: " + address + ".  ");         
            }
            else
            {
                _sb = new StringBuilder();
                _unitConversions = new UnitConversions();
                _sb.AppendLine(string.Format("Latitude: {0}, Longitude: {1}", App.Pin.GeoCoordinate.Latitude, App.Pin.GeoCoordinate.Longitude));
            }
            _sb.AppendLine(string.Format("https://maps.google.com/maps?q={0},{1}", App.Pin.GeoCoordinate.Latitude, App.Pin.GeoCoordinate.Longitude));
            _sb.AppendLine("  RipTrail.com/riptrail.html");
            return _sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEmail_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask email = new EmailComposeTask();
            email.Subject = "My Current Location.";
            email.Body = Body(_address, _isAddressGood);
            email.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSms_Click(object sender, RoutedEventArgs e)
        {
            SmsComposeTask sms = new SmsComposeTask();
            sms.Body = Body(_address, _isAddressGood);
            sms.Show();
        }

        /// <summary>
        /// On Navigated to event handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            base.OnNavigatedTo(e);

            double lat = 0;
            double lon = 0;

            if (NavigationContext.QueryString.ContainsKey("lat"))
            {
                lat = double.Parse(NavigationContext.QueryString["lat"]);
            }

            if (NavigationContext.QueryString.ContainsKey("lon"))
            {
                lon = double.Parse(NavigationContext.QueryString["lon"]);
            }

            if (DeviceNetworkInformation.IsNetworkAvailable)
            {
                MakeJsonCall(lat, lon);
            }

            _SBPinLocation = new StringBuilder();

            txtBoxPinName.Text = App.PinName;
            txtPinTitle.Text = App.PinName;
            _SBPinLocation.AppendLine(String.Format("Latitude: {0}, Longitude{1}",App.Pin.GeoCoordinate.Latitude, App.Pin.GeoCoordinate.Longitude));
            txtAddress.Text = _SBPinLocation.ToString();
            btnEmail.IsEnabled = true;
            btnSms.IsEnabled = true;
            btnGoToPin.IsEnabled = true;

        }


        /// <summary>
        /// Go to pin btn event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGoToPin_Click(object sender, RoutedEventArgs e)
        {
            App.GoToPoint = true;        
            App.PinName = txtBoxPinName.Text;
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void txtBoxPinName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }


    }// end class
}// Namespage end

