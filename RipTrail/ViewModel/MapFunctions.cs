using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Phone.Maps.Controls;
using System.Device.Location;
using Windows.Devices.Geolocation;
using Microsoft.Devices.Sensors;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Info;

namespace RipTrail.ViewModel
{
    public class MapFunctions
    {
        const int MIN_ZOOM_LEVEL = 1;
        const int MAX_ZOOM_LEVEL = 20;      

        private readonly MiscFunctions _miscFunctions = new MiscFunctions();
        private readonly LocationManager _loc = new LocationManager();

        #region "MAP CONTROLS"

        /// <summary>
        /// Zooms the map in.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ZoomIn(object sender, EventArgs e)
        {
            if (App.MyMap.ZoomLevel < MAX_ZOOM_LEVEL)
            {
                try
                {
                    App.MyMap.ZoomLevel++;
                    App.Zoom = App.MyMap.ZoomLevel;
                }
                catch
                {
                    App.MyMap.ZoomLevel--;
                }
            }
        }

        /// <summary>
        /// Zooms the map out.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ZoomOut(object sender, EventArgs e)
        {
            if (App.MyMap.ZoomLevel > MIN_ZOOM_LEVEL)
            {
                try
                {
                    App.MyMap.ZoomLevel--;
                    App.Zoom = App.MyMap.ZoomLevel;
                }
                catch
                {
                    App.MyMap.ZoomLevel++;
                }
            }
        }

        /// <summary>
        /// Recond track menu item click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RecordTrack()
        {
            if (_loc.CheckUserConcent() && App.Geolocator.LocationStatus == PositionStatus.Ready)
            {
                if (!App.ShowTrackLine)
                {
                    App.ShowTrackLine = true;
                    App.MapFunctions.InitTrackLine(App.GeoCoordinate);
                    App.StartTime = string.Format(@"hh\:mm\:ss", DateTime.Now);
                    App.Timer.StartTimer();
                    App.StartTime = string.Format("{0:hh:mm tt}", DateTime.Now);
                }
                else
                {
                    App.ShowTrackLine = false;
                    App.Timer.StopTimer();
                }
            }
        }

        #endregion

        #region "MAP TYPE"

        /// <summary>
        /// Sets map cartographicMode to hybird.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Hybrid(object sender, EventArgs e)
        {
            App.MyMap.CartographicMode = MapCartographicMode.Hybrid;
        }

        /// <summary>
        /// Sets map cartographicMode to road.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Road(object sender, EventArgs e)
        {
            App.MyMap.CartographicMode = MapCartographicMode.Road;
            App.UserSettings.AddOrUpdateSetting("MapCartographicMode", MapCartographicMode.Road);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public void MapType(MapCartographicMode type)
        {
            switch (type)
            {
                case MapCartographicMode.Aerial:
                    App.MyMap.CartographicMode = MapCartographicMode.Aerial;
                    App.UserSettings.AddOrUpdateSetting("MapCartographicMode", MapCartographicMode.Aerial);
                    break;
                case MapCartographicMode.Road:
                    App.MyMap.CartographicMode = MapCartographicMode.Road;
                    App.UserSettings.AddOrUpdateSetting("MapCartographicMode", MapCartographicMode.Road);
                    break;
                case MapCartographicMode.Terrain:
                    //if (App.MyMap.ColorMode == MapColorMode.Dark)
                    //{
                    //    App.MyMap.ColorMode = MapColorMode.Light;
                    //    App.UserSettings.AddOrUpdateSetting("MapColorMode", MapColorMode.Light);
                    //}
                    App.MyMap.CartographicMode = MapCartographicMode.Terrain;
                    App.UserSettings.AddOrUpdateSetting("MapCartographicMode", MapCartographicMode.Terrain);
                    break;
                case MapCartographicMode.Hybrid:
                    App.MyMap.CartographicMode = MapCartographicMode.Hybrid;
                    App.UserSettings.AddOrUpdateSetting("MapCartographicMode", MapCartographicMode.Hybrid);
                    break;
            }
        }

        /// <summary>
        /// Sets map cartographicMode to terrain.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Terrain(object sender, EventArgs e)
        {
            if (App.MyMap.ColorMode == MapColorMode.Dark)
            //{
            //    App.MyMap.ColorMode = MapColorMode.Light;
            //    App.UserSettings.AddOrUpdateSetting("MapColorMode", MapColorMode.Light);
            //}
            App.MyMap.CartographicMode = MapCartographicMode.Terrain;
            App.UserSettings.AddOrUpdateSetting("MapCartographicMode", MapCartographicMode.Terrain);
        }

        #endregion

        #region "MAP ANIMATIONS"

        /// <summary>
        /// Sets the map view
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="zoomLevel"></param>
        /// <param name="pitch"></param>
        /// /// <param name="northUp"></param>
        /// /// <param name="runInBackGround"></param>
        public void CenterMapOnLocationAnimated(GeoCoordinate loc, double zoomLevel, double pitch, Boolean northUp, Boolean runInBackGround)
        {
            if(loc != null && !runInBackGround && !App.ViewChanging && App.CenterMapLock)
            {
                double heading = loc.Course;
                if (northUp)
                {
                    heading = 0;
                }

                if (heading > 0 && pitch > 0)
                {
                    App.MyMap.SetView(loc, zoomLevel, heading, pitch, MapAnimationKind.Linear);
                }
                else if (heading > 0)
                {
                    App.MyMap.SetView(loc, zoomLevel, heading, MapAnimationKind.Linear);
                }
                else
                {
                    App.MyMap.SetView(loc, zoomLevel, MapAnimationKind.Parabolic);
                }
            }
        }

        /// <summary>
        /// Centers position on map.
        /// </summary>
        /// <param name="loc"></param>
        public void CenterMapOnLocation(GeoCoordinate loc)
        {
            App.MyMap.Center = loc;
        }

        #endregion

        #region "MAP DRAWING"

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ToggleLocation(object sender, EventArgs e)
        {
            if(_loc.CheckUserConcent() && App.Geolocator != null && App.Geolocator.LocationStatus == PositionStatus.Ready)
            {
                App.Zoom = App.MyMap.ZoomLevel;
                App.CenterMapLock = true;
                CenterMapOnLocationAnimated(App.GeoCoordinate, App.Zoom, App.MyMap.Pitch, App.NorthUp, App.RunningInBackground);
            }
        }

        /// <summary>
        /// Creates a small dot on a mapOverLay and add to Maplayer.
        /// </summary>
        public void ShowLocation(GeoCoordinate coordinate)
        {
            if (App.LocationLayer == null)
            {
                //Create a small circle to mark the current location.
                Ellipse myCircle = new Ellipse();
                myCircle.Fill = new SolidColorBrush(_miscFunctions.GetAccentColor());
                myCircle.Height = 8;
                myCircle.Width = 8;
                myCircle.Opacity = 100;

                // Create a MapOverlay to contain the circle.
                App.LocationOverlay = new MapOverlay();
                App.LocationOverlay.Content = myCircle;
                App.LocationOverlay.PositionOrigin = new Point(0.5, 0.5);
                App.LocationOverlay.GeoCoordinate = coordinate;

                // Create a MapLayer to contain the MapOverlay.
                App.LocationLayer = new MapLayer();
                App.LocationLayer.Add(App.LocationOverlay);

                // Add the MapLayer to the Map.
                App.MyMap.Layers.Add(App.LocationLayer);
            }
        }

        /// <summary>
        /// My location arrow rotation if compass is not supported.
        /// </summary>
        /// <param name="coord"></param>
        public void ShowLocationArrow(GeoCoordinate coord)
        {
            if (coord != null)
            {
                if (App.LocationLayer == null)
                {
                    // Create a new location image.
                    App.LocationImage = new Image();
                    App.LocationImage.Source = new BitmapImage(new Uri("Images/arrowbc.png", UriKind.Relative));

                    // Rotate transform provides image rotation
                    App.TransformArrow = new RotateTransform();
                    App.LocationImage.RenderTransformOrigin = new Point(0.5, 0.5);
                    if (coord.Course >= 0 && coord.Course <= 360)
                    {
                        App.TransformArrow.Angle = coord.Course;
                    }
                    App.LocationImage.RenderTransform = App.TransformArrow;

                    // Create a new location overlay
                    App.LocationOverlay = new MapOverlay();
                    App.LocationOverlay.Content = App.LocationImage;
                    App.LocationOverlay.PositionOrigin = new Point(0.5, 0.5);
                    App.LocationOverlay.GeoCoordinate = coord;

                    // Create a MapLayer to contain the MapOverlay.
                    App.LocationLayer = new MapLayer();
                    App.LocationLayer.Add(App.LocationOverlay);

                    // Add the MapLayer to the Map.
                    App.MyMap.Layers.Add(App.LocationLayer);
                }
                else
                {
                    App.LocationImage.RenderTransformOrigin = new Point(0.5, 0.5);
                    if (coord.Course >= 0 && coord.Course <= 360)
                    {
                        App.TransformArrow.Angle = coord.Course; 
                    }
                    App.LocationImage.RenderTransform = App.TransformArrow;
                    App.LocationOverlay.GeoCoordinate = coord;
                }
            }
        }
      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="coord"></param>
        public void InitTrackLine(GeoCoordinate coord)
        {
            if (App.Timer == null)
            {
                App.Timer = new Timer();              
            }
            if (App.Trk.line == null && coord != null)
            {
                App.Trk.line = new MapPolyline();
                App.Trk.UTC = new List<string>();
                App.Trk.line.StrokeColor = _miscFunctions.GetAccentColor();
                App.Trk.line.StrokeThickness = 5;
                App.Trk.line.Path.Add(coord);
                App.MyMap.MapElements.Add(App.Trk.line);
                App.Trk.UTC.Add(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"));
                App.Timer.StartTimer();
            }
        }


        /// <summary>
        /// Draw a track line on the map
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="showTrackLine"></param>
        public void DrawTrackLine(GeoCoordinate coord, Boolean showTrackLine)
        {
            if (showTrackLine && coord != null)
            {               
                if (!App.Trk.line.Path.Last().Equals(coord))
                {
                    double dist = App.Trk.line.Path.Last().GetDistanceTo(coord);
                    App.Trk.TotalMeters += dist;
                    App.Odometer += dist;
                    App.Trk.line.Path.Add(coord);
                    App.Trk.UTC.Add(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        public void LoadTrackInMap(GeoCoordinateCollection track)
        {
            MapPolyline line = new MapPolyline();
            line.StrokeColor = _miscFunctions.GetAccentColor();
            line.StrokeThickness = 4;
            line.Path = track;

            LocationRectangle locRect = new LocationRectangle(track.Max((p) => p.Latitude),
                                                              track.Min((p) => p.Longitude),
                                                              track.Min((p) => p.Latitude),
                                                              track.Max((p) => p.Longitude));
            App.MyMap.MapElements.Add(line);
            App.MyMap.SetView(locRect);
        }

        #endregion

    }//End of Class
}// End of namespace

