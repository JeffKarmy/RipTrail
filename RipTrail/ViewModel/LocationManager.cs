using System;
using System.Windows;
using Windows.Devices.Geolocation;
using System.Device.Location;
using Microsoft.Phone.Maps.Controls;

using RipTrail.Models;
using RipTrail.Resources;

namespace RipTrail.ViewModel
{
    public class LocationManager
    {
        private readonly MiscFunctions _miscFunctions = null;

        // Consturctor
        public LocationManager()
        {
            _miscFunctions = new MiscFunctions();

            if(App.Trk == null)
            {
                App.Trk = new Track();
            }           
        }

        /// <summary>
        /// Initialize a geolocator
        /// </summary>
        /// <param name="threshold"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        public Boolean InitGeoLocator(int threshold, uint report)
        {
            try
            {
                if (App.Geolocator == null)
                {
                    App.Geolocator = new Geolocator();
                    App.Geolocator.DesiredAccuracy = PositionAccuracy.High;
                    App.Geolocator.MovementThreshold = threshold;
                    App.Geolocator.ReportInterval = report;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Asynchronous method, get location from device geolocation
        /// </summary>
        //public async Task<Boolean> InitGeoposition()
        //{
        //    try
        //    {
        //        if (App.Geolocator != null)
        //        {
        //            App.Geoposition = await App.Geolocator.GetGeopositionAsync();
        //            App.GeoCoordinate = ConvertGeocoordinate(_geoposition.Coordinate);
        //            return true;
        //        }
        //        return false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        /// <summary>
        /// Converts a Geocoordinate to a GeoCordinate.
        /// </summary>
        /// <param name="geocoordinate"></param>
        /// <returns></returns>
        public GeoCoordinate ConvertGeocoordinate(Geocoordinate geocoordinate)
        {
            return new GeoCoordinate
                (
                geocoordinate.Latitude,
                geocoordinate.Longitude,
                geocoordinate.Altitude ?? Double.NaN,
                geocoordinate.Accuracy,
                geocoordinate.AltitudeAccuracy ?? Double.NaN,
                geocoordinate.Speed ?? Double.NaN,
                geocoordinate.Heading ?? Double.NaN
                );
        }

        /// <summary>
        /// Get user consent.
        /// </summary>
        /// <returns>Boolean</returns>
        public void UserConcent()
        {
            if (App.UserSettings.ContainsSetting(AppResources.UserLocationConsent))
            {
                return;
            }
            else if (_miscFunctions.ShowMessage("This application needs access to your phones location services.", "Location Consent"))
            {
                App.UserSettings.AddOrUpdateSetting(AppResources.UserLocationConsent, true);
            }
            else
            {
                App.UserSettings.AddOrUpdateSetting(AppResources.UserLocationConsent, false);
            }

        }

        /// <summary>
        /// Recheck user concent
        /// </summary>
        /// <returns>boolean</returns>
        public bool CheckUserConcent()
        {
            if(App.UserSettings.GetSetting<bool>(AppResources.UserLocationConsent, false))
            {
                CheckLocationStatus();
                return true;
            }
            else
            {
                MessageBox.Show("This function requires access to your phones location, please allow access to you location in app settings.");
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async void CheckLocationStatus()
        {
            if (App.Geolocator.LocationStatus == PositionStatus.Disabled)
            {
                if (_miscFunctions.ShowMessage("Your phones location services are switched off.  Please turn on location services located in your phones settings", "Location Services"))
                {
                    await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-location:"));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearTrackValues()
        {
            App.Trk.MaxAltitude = 0;
            App.Trk.MaxSpeed = 0;
            App.Trk.SpeedCount = 0;
            App.Trk.TotalMeters = 0;
            App.Trk.AVGSpeed = 0;
            App.Trk.line = new MapPolyline();
        }

    } // End class
}// End namespace
