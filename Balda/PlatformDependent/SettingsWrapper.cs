using System;
using System.Collections.Generic;
#if SILVERLIGHT
using System.IO.IsolatedStorage;
#else
#endif
namespace Balda.Logic
{
    public class SettingsWrapper
    {
        static SettingsWrapper instance = null;
        SettingsWrapper()
        {
#if SILVERLIGHT
            try
            {
                foreach (string key in IsolatedStorageSettings.ApplicationSettings.Keys)
                {
                    storage[key] = IsolatedStorageSettings.ApplicationSettings[key];
                }
            }
            catch (Exception ex)
            {
            }
#else
#endif
        }

        static Dictionary<string, object> storage = new Dictionary<string,object>();
        public static Dictionary<string, object> ApplicationSettings
        {
            get
            {
                if (instance == null) instance = new SettingsWrapper();
                return storage;
            }
        }
        public static void Save()
        {
#if SILVERLIGHT
            foreach(string key in storage.Keys)
            {
                IsolatedStorageSettings.ApplicationSettings[key] = storage[key];
            }
            IsolatedStorageSettings.ApplicationSettings.Save();
#else
#endif
        }
    }
}
