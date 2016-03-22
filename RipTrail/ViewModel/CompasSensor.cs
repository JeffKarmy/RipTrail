using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework;
using System.Windows.Threading;

using RipTrail.Models;
using RipTrail.Resources;
using RipTrail.ViewModel;

namespace RipTrail.ViewModel
{
    public class CompasSensor
    {
        public CompasSensor()
        {
            if (Compass.IsSupported)
            {
                App.Compass = new Compass();
                App.Compass.TimeBetweenUpdates = TimeSpan.FromMilliseconds(20);

                App.DispatcherTimer = new DispatcherTimer();
                App.DispatcherTimer.Interval = TimeSpan.FromMilliseconds(30);
            }
        }
    }
}
