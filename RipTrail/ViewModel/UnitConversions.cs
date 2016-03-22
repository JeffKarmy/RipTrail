using System;
using System.Device.Location;

namespace RipTrail.ViewModel
{
    class UnitConversions
    {

        const double METERS_TO_FEET = 3.28084;
        const double MPS_TO_MPH = 2.23694;
        const double MPS_TO_KNOTS = 1.94384;
        const double MPS_TO_KM = 3.6;
        const double METERS_TO_MILES = .000621371;
        const double METERS_TO_NAUTICAL_MILES = .000539957;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public double DistanceSmall(int index, Double distance)
        {
            double value = 0.00;
            switch(index)
            {
                case 0:
                    value = Math.Round(METERS_TO_FEET * distance, 0);
                    break;
                case 1:
                    value = Math.Round(distance , 0);
                    break;
                case 2:
                    value = Math.Round(METERS_TO_FEET * distance, 0);
                    break;
            }
            return value;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public double DistanceLarge(int index, Double distance)
        {
            double value = 0.00;
            switch (index)
            {
                case 0:
                    if(distance <= 16077.35)
                    {
                        value = Math.Round(METERS_TO_MILES * distance, 2);
                    }
                    else if (distance <= 160773.5)
                    {
                        value = Math.Round(METERS_TO_MILES * distance, 1);
                    }
                    else
                    {
                        value = Math.Round(METERS_TO_MILES * distance, 0);
                    }
                    break;
                case 1:
                    if(distance <= 9990)
                    {
                        value = Math.Round(0.001 * distance, 2);
                    }
                    else if( distance <= 99900)
                    {
                        value = Math.Round(0.001 * distance, 1);
                    } 
                    else
                    {
                        value = Math.Round(0.001 * distance, 0);
                    }
                    break;
                case 2:
                    if (distance <= 18501.48)
                    {
                        value = Math.Round(METERS_TO_NAUTICAL_MILES * distance, 2);
                    }
                    else if (distance <= 185014.8)
                    {
                        value = Math.Round(METERS_TO_NAUTICAL_MILES * distance, 1);
                    }
                    else
                    {
                        value = Math.Round(METERS_TO_NAUTICAL_MILES * distance, 0);
                    }
                    break;
            }
            return value;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="totalDist"></param>
        //public string DistanceTextBox(int index, double totalDist)
        //{
        //    string rt = "0.00";
        //    if (totalDist != 0)
        //    {
        //        double distance = DistanceLarge(index, totalDist);

        //        if (distance < 999)
        //        {
        //            rt = distance.ToString();
        //        }
        //        else
        //        {
        //            rt = (distance - 999).ToString();
        //        }
        //    }
        //    else
        //    {
        //        rt = "0.00";
        //    }
        //    return rt;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        public double Speed(int index, Double speed)
        {
            double value = 0.00;
            switch (index)
            {
                case 0:
                    value = Math.Round(MPS_TO_MPH * speed, 1);
                    break;
                case 1:
                    value = Math.Round(MPS_TO_KM * speed, 1);
                    break;
                case 2:
                    value = Math.Round(MPS_TO_KNOTS * speed, 1);
                    break;
            }
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string SpeedMapUnits(int index)
        {
            string unit = "";
            switch (index)
            {
                case 0:
                    unit = "mph";
                    break;
                case 1:
                    unit = "km/h";
                    break;
                case 2:
                    unit = "knot";
                    break;
            }
            return unit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string SmallMapUnits(int index)
        {
            string unit = "";
            switch (index)
            {
                case 0:
                    unit = "ft";
                    break;
                case 1:
                    unit = "m";
                    break;
                case 2:
                    unit = "ft";
                    break;
            }
            return unit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string LargeMapUnits(int index)
        {
            string unit = "";
            switch (index)
            {
                case 0:
                    unit = "mi";
                    break;
                case 1:
                    unit = "km";
                    break;
                case 2:
                    unit = "nm";
                    break;
            }
            return unit;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public double RadToDeg(double radians)
        {
            return radians * (180 / Math.PI);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public double DegToRad(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        /// <summary>
        /// Calculates the the degrees between two degraphical points
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="long1"></param>
        /// <param name="lat2"></param>
        /// <param name="long2"></param>
        /// <returns>returns a bearing </returns>
        public double Bearing(double lat1, double long1, double lat2, double long2)
        {
            //Convert input values to radians  
            lat1 = DegToRad(lat1);
            long1 = DegToRad(long1);
            lat2 = DegToRad(lat2);
            long2 = DegToRad(long2);

            double deltaLong = long2 - long1;

            double y = Math.Sin(deltaLong) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) -
                    Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(deltaLong);
            double bearing = Math.Atan2(y, x);
            return ConvertToBearing(RadToDeg(bearing));
        }

        /// <summary>
        /// Converts a degree to bearing.
        /// </summary>
        /// <param name="deg"></param>
        /// <returns></returns>
        public double ConvertToBearing(double deg)
        {
            return (deg + 360) % 360;
        }

    }
}
