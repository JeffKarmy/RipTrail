using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Phone.Maps.Controls;

namespace RipTrail.Models
{
    public class Track
    {
        public GeoCoordinateCollection geoCoordinateCollection { get; set; }

        public string name { get; set; }
        public string description { get; set; }
        public string TotalTime { get; set; }
        public string Time { get; set; }

        public MapPolyline line { get; set; }
        public List<string> UTC { get; set; }

        public Double TotalMeters { get; set; }
        public Double MaxAltitude { get; set; }
        public Double MaxSpeed { get; set; }
        public Double AVGSpeed { get; set; }
        public Double TotalSpeed { get; set; }

        public int SpeedCount { get; set; }

        public void ClearTrack()
        {
            name = "";
            description = "";
            TotalTime = "00:00:00";
            line = null;
            TotalMeters = 0;
            MaxAltitude = 0;
            MaxSpeed = 0;
            AVGSpeed = 0;
            TotalSpeed = 0;
            SpeedCount = 0;
        }
     
    }
}
