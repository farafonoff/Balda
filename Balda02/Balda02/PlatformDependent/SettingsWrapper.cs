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
            foreach (string key in IsolatedStorageSettings.ApplicationSettings.Keys)
            {
                storage[key] = IsolatedStorageSettings.ApplicationSettings[key];
            }
#else
            var applicationData = Windows.Storage.ApplicationData.Current;

            var localSettings = applicationData.LocalSettings;

            var container = localSettings.CreateContainer("baldaContainer",
                    Windows.Storage.ApplicationDataCreateDisposition.Always);
            var keys = container.Values.Keys;
            foreach (var key in keys)
            {
                try
                {
                    if (key.StartsWith("@"))
                    {
                        string[] idx = key.Split('@');
                        string sk = idx[1];
                        string ik = idx[2];
                        Dictionary<string, object> val;
                        if (storage.ContainsKey(sk))
                        {
                            val = (Dictionary<string, object>)storage[sk];
                        }
                        else
                        {
                            val = new Dictionary<string, object>();
                            storage[sk] = val;
                        }
                        val[ik] = container.Values[key];
                    }
                    else
                    {
                        storage[key] = container.Values[key];
                    }
                } catch (Exception)
                {

                }
            }
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
            var applicationData = Windows.Storage.ApplicationData.Current;

            var localSettings = applicationData.LocalSettings;

            var container = localSettings.CreateContainer("baldaContainer",
                    Windows.Storage.ApplicationDataCreateDisposition.Always);
            var keys = storage.Keys;
            foreach (var key in keys)
            {
                if (storage[key] is Dictionary<string,object>)
                {
                    var val = (Dictionary<string, object>)storage[key];
                    string prefix = "@"+key+"@";
                    foreach (var iter in val)
                    {
                        container.Values[prefix + iter.Key] = iter.Value;
                    }
                    //container.Values[key] = new Dictionary<string, object>();
                } else
                container.Values[key] = storage[key];
            }
#endif
        }
    }
}
