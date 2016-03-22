using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Xml;
using System.Xml.Linq;
using System.Device.Location;
using Microsoft.Phone.Maps.Controls;
using System.IO;
using System.Windows;

using RipTrail.Models;

namespace RipTrail.ViewModel
{
    public class Serializer
    {

        public XDocument CreateXML(Track geoTrack)
        {
            XDocument xDoc = new XDocument();                                
            XElement body = new XElement("root");

            XElement header = new XElement(new XElement("metadata",
                                            new XElement("link", "http://www.riptrail.com"),
                                            new XElement("text", "Rip Trail"),
                                            new XElement("time", System.DateTime.Now.ToString()),
                                            new XElement("name", geoTrack.name),
                                            new XElement("desc", geoTrack.description),
                                            new XElement("totaltm", geoTrack.TotalTime),
                                            new XElement("totalm", geoTrack.TotalMeters.ToString()),
                                            new XElement("maxalt", geoTrack.MaxAltitude.ToString()),
                                            new XElement("maxspd", geoTrack.MaxSpeed.ToString()),
                                            new XElement("avgspd", geoTrack.AVGSpeed.ToString())
                                            ));
            body.Add(header);
            XElement track = new XElement("trk");

            foreach (GeoCoordinate element in geoTrack.line.Path)
            {
                XElement segment = new XElement("trkseg", new XAttribute("lat", element.Latitude.ToString()),
                                                          new XAttribute("lon", element.Longitude.ToString()),
                                   new XElement("ele", element.Altitude.ToString())
                                   );
                track.Add(segment);
            }
            body.Add(track);
            xDoc.Add(body);
            return xDoc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public Track DeSerilaizeXML(string xml)
        {
            Track geoTrack;

            try
            {
                geoTrack = new Track();
                geoTrack.geoCoordinateCollection = new GeoCoordinateCollection();
                GeoCoordinate geo = null;

                // Create an XmlReader
                using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
                {
                    reader.ReadToFollowing("time");
                    geoTrack.Time = reader.ReadElementContentAsString();

                    reader.ReadToFollowing("name");
                    geoTrack.name = reader.ReadElementContentAsString();

                    reader.ReadToFollowing("desc");
                    geoTrack.description = reader.ReadElementContentAsString();

                    reader.ReadToFollowing("totaltm");
                    geoTrack.TotalTime = reader.ReadElementContentAsString();

                    reader.ReadToFollowing("totalm");
                    geoTrack.TotalMeters = Convert.ToDouble(reader.ReadElementContentAsString());

                    reader.ReadToFollowing("maxalt");
                    geoTrack.MaxAltitude = Convert.ToDouble(reader.ReadElementContentAsString());

                    reader.ReadToFollowing("maxspd");
                    geoTrack.MaxSpeed = Convert.ToDouble(reader.ReadElementContentAsString());

                    reader.ReadToFollowing("avgspd");
                    geoTrack.AVGSpeed = Convert.ToDouble(reader.ReadElementContentAsString());

                    while (reader.Read())
                    {
                        if (reader.ReadToFollowing("trkseg"))
                        {
                            geo = new GeoCoordinate();
                            reader.MoveToFirstAttribute();
                            geo.Latitude = Convert.ToDouble(reader.Value);

                            reader.MoveToNextAttribute();
                            geo.Longitude = Convert.ToDouble(reader.Value);

                            reader.ReadToFollowing("ele");
                            geo.Altitude = Convert.ToDouble(reader.ReadElementContentAsString());

                            geoTrack.geoCoordinateCollection.Add(geo);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            return geoTrack;
        }

        /// <summary>
        /// Serilize track object into a gpx file.
        /// </summary>
        /// <param name="geoTrack"></param>
        /// <returns>XDocument</returns>
        public XDocument CreateGPX(Track geoTrack)
        {
            XNamespace ns = "http://www.topografix.com/GPX/1/1";
            XNamespace xsiNs = "http://www.w3.org/2001/XMLSchema-instance";

            XDocument xDoc = new XDocument();

            //XElement body = new XElement("", new XDeclaration("1.0", "UTF-8", "no"));

            XElement header = new XElement(ns + "gpx", new XAttribute(XNamespace.Xmlns + "xsi", xsiNs),
                                                        new XAttribute(xsiNs + "schemaLocation", "http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd"),
                                                        new XAttribute("creator", "Rip Trail"),
                                                        new XAttribute("version", "1.1")
                                                        );

            XElement meta = new XElement(ns + "metadata",
                                new XElement(ns + "name", geoTrack.name),
                                new XElement(ns + "desc", geoTrack.description),
                                new XElement(ns + "link", new XAttribute("href", "http://www.riptrtail.com")),
                                new XElement(ns + "time", System.DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"))
                                );
            header.Add(meta);

            XElement trk = new XElement(ns + "trk");


            XElement trkseg = new XElement(ns + "trkseg");
            int count = 0;
            foreach (GeoCoordinate element in geoTrack.line.Path)
            {
                XElement trkpt;
                if (geoTrack.UTC.Count > 0)
                {
                    trkpt = new XElement(ns + "trkpt",
                            new XAttribute("lat", element.Latitude.ToString()),
                            new XAttribute("lon", element.Longitude.ToString()),
                            new XElement(ns + "ele", element.Altitude.ToString()),
                            new XElement(ns + "time", geoTrack.UTC[count])
                            );
                }
                else
                {
                    trkpt = new XElement(ns + "trkpt",
                            new XAttribute("lat", element.Latitude.ToString()),
                            new XAttribute("lon", element.Longitude.ToString()),
                            new XElement(ns + "ele", element.Altitude.ToString()));
                }

                trkseg.Add(trkpt);
                count++;
            }
            trk.Add(trkseg);
            header.Add(trk);
            xDoc.Add(header);
            return xDoc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gpx"></param>
        /// <returns></returns>
        public Track DeSerilaizeGPX(Stream gpx)
        {
            Track geoTrack;

            try
            {
                geoTrack = new Track();
                geoTrack.geoCoordinateCollection = new GeoCoordinateCollection();
                geoTrack.UTC = new List<string>();
                GeoCoordinate geo = null;

                // Create an XmlReader
                using (XmlReader reader = XmlReader.Create(gpx))
                {
                    if (reader.ReadToFollowing("name"))
                    {
                        geoTrack.name = reader.ReadElementContentAsString();
                    }


                    if (reader.ReadToFollowing("desc"))
                    {
                         geoTrack.description = reader.ReadElementContentAsString();
                    }

                    while (reader.Read())
                    {
                        if (reader.ReadToFollowing("trkpt"))
                        {
                            geo = new GeoCoordinate();
                            reader.MoveToFirstAttribute();
                            geo.Latitude = Convert.ToDouble(reader.Value);

                            reader.MoveToNextAttribute();
                            geo.Longitude = Convert.ToDouble(reader.Value);

                            reader.ReadToFollowing("ele");
                            geo.Altitude = Convert.ToDouble(reader.ReadElementContentAsString());

                            reader.ReadToFollowing("time");
                            geoTrack.UTC.Add(reader.ReadElementContentAsString());

                            geoTrack.geoCoordinateCollection.Add(geo);
                        }
                        else if (reader.ReadToFollowing("rtept"))
                        {
                            geo = new GeoCoordinate();
                            reader.MoveToFirstAttribute();
                            geo.Latitude = Convert.ToDouble(reader.Value);

                            reader.MoveToNextAttribute();
                            geo.Longitude = Convert.ToDouble(reader.Value);

                            reader.ReadToFollowing("ele");
                            geo.Altitude = Convert.ToDouble(reader.ReadElementContentAsString());

                            reader.ReadToFollowing("time");
                            geoTrack.UTC.Add(reader.ReadElementContentAsString());

                            geoTrack.geoCoordinateCollection.Add(geo);
                        }
                        else
                        {
                            MessageBox.Show("The GPX file contains no route data; missing the <rte> or <trk> element.");
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
            return geoTrack;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        ///  /// <param name="name"></param>
        /// <returns></returns>
        public Track ReadGpxFileStastics(Stream stream, string name)
        {
            GeoCoordinateCollection geoCollection = new GeoCoordinateCollection();
            List<DateTime> lstDateTime = new List<DateTime>();
            List<string> lstTimeString = new List<string>();
            Track track = new Track();
            XElement allData = XElement.Load(stream);
            track.name = name;
            track.description = "Imported: " + DateTime.Now.ToString("dddd, MMMM M, yyyy");

            XNamespace xn = allData.GetDefaultNamespace();

            // Look for route <rte> and track <trk> elements in the route data.
            var routesAndTracks = (from e in allData.DescendantsAndSelf()
                                    select new
                                    {
                                        RouteElements = e.Descendants(xn + "rte"),
                                        TrackElements = e.Descendants(xn + "trk")
                                    }).FirstOrDefault();

            // Create a list of map points from the route <rte> element, otherwise use the track <trk> element.
            List<XElement> mapPoints;
            if (routesAndTracks.RouteElements.Count() > 0)
            {
                mapPoints = (from p in routesAndTracks.RouteElements.First().Descendants(xn + "rtept")
                                select p).ToList();
            }
            else if (routesAndTracks.TrackElements.Count() > 0)
            {
                mapPoints = (from p in routesAndTracks.TrackElements.First().Descendants(xn + "trkpt")
                                select p).ToList();
            }
            else
            {
                // Route data contains no route <rte> or track <trk> elements.
                MessageBox.Show("The GPX file contains no route data; missing the <rte> or <trk> element.");
                return null;
            }

            // Convert the GPX map points to coordinates that can be mapped.
            for (int i = 0; i < mapPoints.Count(); i++)
            {
                XElement xe = mapPoints[i];

                if (xe.Attribute("lat") != null && xe.Attribute("lon") != null && xe.Element(xn + "ele") != null)
                {
                    geoCollection.Add(new GeoCoordinate(double.Parse(xe.Attribute("lat").Value),
                                                        double.Parse(xe.Attribute("lon").Value),
                                                        double.Parse(xe.Element(xn + "ele").Value)));
                }
                else if (xe.Attribute("lat") != null && xe.Attribute("lon") != null && xe.Element(xn + "ele") == null)
                {
                    geoCollection.Add(new GeoCoordinate(double.Parse(xe.Attribute("lat").Value),
                                                        double.Parse(xe.Attribute("lon").Value)));
                }
                else
                {
                    return null;
                }

                if (xe.Element(xn + "time") != null)
                {
                    lstDateTime.Add(DateTimeOffset.Parse(xe.Element(xn + "time").Value).UtcDateTime);
                    lstTimeString.Add(xe.Element(xn + "time").Value);
                }
            }

            if (geoCollection.Count > 0)
            {
                for (int i = 1; i < geoCollection.Count - 1; i++)
                {
                    track.TotalMeters += geoCollection[i - 1].GetDistanceTo(geoCollection[i]);

                    if (track.MaxAltitude < geoCollection[i].Altitude)
                    {
                        track.MaxAltitude = geoCollection[i].Altitude;
                    }
                }
            }
            track.line = new MapPolyline();
            track.line.Path = geoCollection;
            track.UTC = new List<string>();
            track.UTC = lstTimeString;
            TimeSpan t = lstDateTime.Last() - lstDateTime.First();
            if (track.TotalMeters > 0 && t.TotalSeconds > 0)
            {
                track.AVGSpeed = track.TotalMeters / t.TotalSeconds;
            }
            else
            {
                track.AVGSpeed = 0.0;
            }
            
            track.TotalTime = t.ToString(@"hh\:mm\:ss");

            if (geoCollection.Count != lstTimeString.Count)
            {
                throw new System.ArgumentException("Route is corrupt or missing elements.");
            }

            return track;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public GeoCoordinateCollection ReadGPXFile(Stream stream)
        {
            XElement allData = XElement.Load(stream);
            GeoCoordinateCollection geoCollection = new GeoCoordinateCollection();

            XNamespace xn = allData.GetDefaultNamespace();

            var routesAndTracks = (from e in allData.DescendantsAndSelf()
                                    select new
                                    {
                                        RouteElements = e.Descendants(xn + "rte"),
                                        TrackElements = e.Descendants(xn + "trk")
                                    }).FirstOrDefault();

            List<XElement> mapPoints;
            if (routesAndTracks.RouteElements.Count() > 0)
            {
                mapPoints = (from p in routesAndTracks.RouteElements.First().Descendants(xn + "rtept")
                                select p).ToList();
            }
            else if (routesAndTracks.TrackElements.Count() > 0)
            {
                mapPoints = (from p in routesAndTracks.TrackElements.First().Descendants(xn + "trkpt")
                                select p).ToList();
            }
            else
            {
                MessageBox.Show("The GPX file contains no route data; missing the <rte> or <trk> element.");
                return null;
            }

            for (int i = 0; i < mapPoints.Count(); i++)
            {
                XElement xe = mapPoints[i];
                double dLat = 0;
                double dLon = 0;

                if (xe.Attribute("lat") != null)
                {
                    dLat = double.Parse(xe.Attribute("lat").Value);
                }
                if (xe.Attribute("lon") != null)
                {
                    dLon = double.Parse(xe.Attribute("lon").Value);
                }

                geoCollection.Add(new GeoCoordinate(dLat, dLon));
            }
            return geoCollection;
        }


    }// End of class
}// End of namespace
