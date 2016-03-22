using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Device.Location;

namespace RipTrail.ViewModel
{
     public class Timer
    {
         public DispatcherTimer timer;
         public long startTime;
         public string endTime;

         public string RunTime { set; get; }

         public Timer()
         {
             timer = new DispatcherTimer();
             timer.Interval = TimeSpan.FromSeconds(1);
         }

         public void StartTimer()
         {
             if(!timer.IsEnabled)
             {
                 timer.Start();
                 startTime = System.Environment.TickCount;
             }
         }

         public void StopTimer()
         {
             if(timer.IsEnabled)
             {
                 timer.Stop();
             }           
         }

         public Boolean IsEnabled()
         {
             return timer.IsEnabled;
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="milliSeconds"></param>
         /// <param name="showTrackLine"></param>
         /// <param name="coord"></param>
         /// <returns></returns>
         public string MovingTimeCalc(long milliSeconds, bool showTrackLine, GeoCoordinate coord)
         {

             if (milliSeconds > 0 && coord != null && showTrackLine)
             {
                 if (coord.Speed > 0)
                 {
                     App.MovingTime += milliSeconds;
                 }
             }
             return TimeSpan.FromMilliseconds(App.MovingTime).ToString(@"hh\:mm\:ss");
         }

         /// <summary>
         /// Returns the moving time.
         /// </summary>
         /// <returns></returns>
         public string MovingTime()
         {
             return TimeSpan.FromMilliseconds(App.MovingTime).ToString(@"hh\:mm\:ss");
         }

         /// <summary>
         /// Returns the total time.
         /// </summary>
         /// <returns></returns>
         public string TotalTime()
         {
             return TimeSpan.FromMilliseconds(Environment.TickCount - App.Timer.startTime).ToString(@"hh\:mm\:ss");
         }

    }
}
