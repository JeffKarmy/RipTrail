using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;

namespace RipTrail.ViewModel
{
    public class Settings
    {

        private IsolatedStorageSettings MySettings;

        public Settings()
        {
            if (MySettings == null)
            {
                MySettings = IsolatedStorageSettings.ApplicationSettings;
            }
        }

        /// <summary>
        /// Adds a setting to storage.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>If the key value changed</returns>
        public Boolean AddOrUpdateSetting(string key, object value)
        {
            Boolean changed = false;
            if (!MySettings.Contains(key))
            {
                MySettings.Add(key, value);
                changed = true;
            }
            else
            {
                MySettings[key] = value;
                changed = true;
            }
            MySettings.Save();
            return changed;
        }

        /// <summary>
        /// Removes a setting from storage
        /// </summary>
        /// <param name="key"></param>
        public void RemoveSetting(string key)
        {
            if (MySettings.Contains(key))
            {
                MySettings.Remove(key);
            }
        }

        /// <summary>
        /// Returns a setting object.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetSetting<T>(string key, T defaultValue)
        {
            T value;
            if (MySettings.Contains(key))
            {
                value = (T)MySettings[key];
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }
        /// <summary>
        /// returns true of MySettings contains key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Boolean ContainsSetting(string key)
        {
            if (MySettings.Contains(key))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    } // End class
} // End namespace 
